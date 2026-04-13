using UnityEngine;
using System.Collections;

namespace Matchmancer.Audio
{
    /// <summary>
    /// SFX types enumeration for the Matchmancer game.
    /// Each type corresponds to an indexed AudioClip in the AudioManager's _sfxClips array.
    /// </summary>
    public enum SFX
    {
        TileSelect = 0,
        ValidSwap = 1,
        InvalidSwap = 2,
        MatchClear = 3,
        Cascade = 4,
        SigilCreate = 5,
        SigilActivate = 6,
        BlockerCrack = 7,
        BlockerBreak = 8,
        UltimateCharge = 9,
        UltimateActivate = 10
    }

    /// <summary>
    /// AudioManager is a singleton MonoBehaviour that manages all audio playback for Matchmancer.
    /// It handles background music with fade transitions, SFX playback with combo-based pitch scaling,
    /// and volume persistence across play sessions.
    ///
    /// Integration Points:
    /// - BoardController: calls PlaySFXWithCombo() on cascade/match events
    /// - InputHandler: calls PlaySFX(SFX.TileSelect) on tile tap
    /// - SwapValidator: calls PlaySFX(ValidSwap/InvalidSwap) on swap validation
    /// - EnemyTurnController: can trigger victory/defeat music transitions
    /// - CharacterRuntime: calls PlaySFX(UltimateCharge) when ultimate meter fills
    ///
    /// Performance: Uses single AudioSources for music and SFX to minimize memory footprint on mobile.
    /// Pitch scaling via combo creates ascending musical sequences without additional object allocation.
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        private static AudioManager _instance;

        /// <summary>
        /// Singleton accessor for the AudioManager instance.
        /// </summary>
        public static AudioManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<AudioManager>();
                    if (_instance == null)
                    {
                        GameObject audioManagerObject = new GameObject("AudioManager");
                        _instance = audioManagerObject.AddComponent<AudioManager>();
                    }
                }
                return _instance;
            }
        }

        [SerializeField]
        [Tooltip("Array of SFX clips indexed by SFX enum values (0=TileSelect, 1=ValidSwap, etc.)")]
        private AudioClip[] _sfxClips = new AudioClip[11];

        [SerializeField]
        [Tooltip("Background music track to loop during gameplay")]
        private AudioClip _backgroundMusic;

        private AudioSource _musicSource;
        private AudioSource _sfxSource;

        [SerializeField]
        [Range(0f, 1f)]
        [Tooltip("Master volume for background music (0-1)")]
        private float _musicVolume = 0.7f;

        [SerializeField]
        [Range(0f, 1f)]
        [Tooltip("Master volume for sound effects (0-1)")]
        private float _sfxVolume = 0.7f;

        /// <summary>
        /// Tracks the current combo depth for pitch scaling. Incremented on cascades,
        /// reset when player makes a manual swap action.
        /// </summary>
        private int _currentComboDepth = 0;

        /// <summary>
        /// Minimum pitch multiplier for combo-scaled sounds (base pitch without scaling).
        /// </summary>
        private const float BasePitch = 1.0f;

        /// <summary>
        /// Pitch increment per combo level. Each cascade adds this to the pitch multiplier.
        /// Example: comboDepth=1 yields pitch 1.08, comboDepth=5 yields pitch 1.40.
        /// </summary>
        private const float ComboDepthPitchIncrement = 0.08f;

        /// <summary>
        /// Maximum pitch cap to prevent sounds becoming too high-pitched at deep combos.
        /// </summary>
        private const float MaxComboPitch = 1.6f;

        /// <summary>
        /// Unity event invoked whenever an SFX is played. Subscribers can use this for UI feedback,
        /// visual effects sync, or other event-driven audio responses.
        /// </summary>
        public delegate void OnSFXPlayedDelegate(SFX sfxType);
        public event OnSFXPlayedDelegate OnSFXPlayed;

        private Coroutine _musicFadeCoroutine;

        private void Awake()
        {
            // Implement singleton pattern with DontDestroyOnLoad
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);

            InitializeAudioSources();
            LoadVolumePrefs();
        }

        /// <summary>
        /// Initializes the music and SFX AudioSource components.
        /// Both sources are configured on the same GameObject for simplicity.
        /// </summary>
        private void InitializeAudioSources()
        {
            // Create or get music AudioSource
            _musicSource = GetComponent<AudioSource>();
            if (_musicSource == null)
            {
                _musicSource = gameObject.AddComponent<AudioSource>();
            }
            _musicSource.loop = true;
            _musicSource.playOnAwake = false;
            _musicSource.volume = _musicVolume;

            // Create SFX AudioSource (one-shot playback)
            _sfxSource = gameObject.AddComponent<AudioSource>();
            _sfxSource.loop = false;
            _sfxSource.playOnAwake = false;
            _sfxSource.volume = _sfxVolume;
        }

        /// <summary>
        /// Plays a single SFX clip at the current combo-adjusted pitch without incrementing combo depth.
        /// Used for discrete sound events (tile select, swap validation, etc.) that don't trigger cascades.
        /// </summary>
        /// <param name="sfxType">The SFX enum type to play.</param>
        public void PlaySFX(SFX sfxType)
        {
            if (!IsValidSFXIndex((int)sfxType))
            {
                Debug.LogWarning($"AudioManager.PlaySFX: Invalid or missing SFX clip for {sfxType}");
                return;
            }

            float pitch = BasePitch + (_currentComboDepth * ComboDepthPitchIncrement);
            pitch = Mathf.Clamp(pitch, BasePitch, MaxComboPitch);

            _sfxSource.pitch = pitch;
            _sfxSource.PlayOneShot(_sfxClips[(int)sfxType], _sfxVolume);

            OnSFXPlayed?.Invoke(sfxType);
        }

        /// <summary>
        /// Plays an SFX clip with automatic pitch scaling based on combo depth.
        /// This method is used for cascade and match events to create an ascending musical sequence.
        /// The pitch increases with each level of combo depth, creating a musical feedback loop.
        /// </summary>
        /// <param name="sfxType">The SFX enum type to play (typically Cascade or MatchClear).</param>
        /// <param name="comboDepth">The current cascade/combo depth (0-based). Each level adds ComboDepthPitchIncrement to pitch.</param>
        public void PlaySFXWithCombo(SFX sfxType, int comboDepth)
        {
            if (!IsValidSFXIndex((int)sfxType))
            {
                Debug.LogWarning($"AudioManager.PlaySFXWithCombo: Invalid or missing SFX clip for {sfxType}");
                return;
            }

            _currentComboDepth = comboDepth;

            float pitch = BasePitch + (comboDepth * ComboDepthPitchIncrement);
            pitch = Mathf.Clamp(pitch, BasePitch, MaxComboPitch);

            _sfxSource.pitch = pitch;
            _sfxSource.PlayOneShot(_sfxClips[(int)sfxType], _sfxVolume);

            OnSFXPlayed?.Invoke(sfxType);
        }

        /// <summary>
        /// Resets the combo depth to zero. Called by InputHandler or SwapValidator when the player
        /// initiates a new manual swap action, returning the pitch to baseline.
        /// </summary>
        public void ResetComboPitch()
        {
            _currentComboDepth = 0;
        }

        /// <summary>
        /// Gets the current combo depth for external systems that may need to query combo state.
        /// </summary>
        /// <returns>The current combo depth value.</returns>
        public int GetCurrentComboDepth()
        {
            return _currentComboDepth;
        }

        /// <summary>
        /// Starts playback of the background music track. If music is already playing, this does nothing.
        /// Music loops continuously until StopMusic() is called.
        /// </summary>
        public void PlayMusic()
        {
            if (_backgroundMusic == null)
            {
                Debug.LogWarning("AudioManager.PlayMusic: No background music clip assigned");
                return;
            }

            if (_musicSource.isPlaying)
                return;

            _musicSource.clip = _backgroundMusic;
            _musicSource.Play();
        }

        /// <summary>
        /// Stops music playback immediately.
        /// </summary>
        public void StopMusic()
        {
            _musicSource.Stop();
        }

        /// <summary>
        /// Fades the background music volume to a target level over a specified duration.
        /// If a fade is already in progress, it will be interrupted and replaced with this new fade.
        /// Useful for smooth transitions between gameplay states or scene changes.
        /// </summary>
        /// <param name="targetVolume">Target volume level (0-1).</param>
        /// <param name="duration">Duration of fade in seconds.</param>
        public void FadeMusic(float targetVolume, float duration)
        {
            // Kill any existing fade coroutine
            if (_musicFadeCoroutine != null)
            {
                StopCoroutine(_musicFadeCoroutine);
            }

            targetVolume = Mathf.Clamp01(targetVolume);
            _musicFadeCoroutine = StartCoroutine(MusicFadeCoroutine(targetVolume, duration));
        }

        /// <summary>
        /// Coroutine that smoothly transitions music volume over time.
        /// </summary>
        private IEnumerator MusicFadeCoroutine(float targetVolume, float duration)
        {
            float startVolume = _musicSource.volume;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                _musicSource.volume = Mathf.Lerp(startVolume, targetVolume, t);
                yield return null;
            }

            _musicSource.volume = targetVolume;
            _musicVolume = targetVolume;
        }

        /// <summary>
        /// Sets the master volume for all background music playback.
        /// Saves the preference to PlayerPrefs for persistence across sessions.
        /// </summary>
        /// <param name="volume">Volume level (0-1).</param>
        public void SetMusicVolume(float volume)
        {
            _musicVolume = Mathf.Clamp01(volume);
            _musicSource.volume = _musicVolume;
            PlayerPrefs.SetFloat("MusicVolume", _musicVolume);
            PlayerPrefs.Save();
        }

        /// <summary>
        /// Sets the master volume for all sound effects.
        /// Saves the preference to PlayerPrefs for persistence across sessions.
        /// </summary>
        /// <param name="volume">Volume level (0-1).</param>
        public void SetSFXVolume(float volume)
        {
            _sfxVolume = Mathf.Clamp01(volume);
            _sfxSource.volume = _sfxVolume;
            PlayerPrefs.SetFloat("SFXVolume", _sfxVolume);
            PlayerPrefs.Save();
        }

        /// <summary>
        /// Gets the current master volume for background music.
        /// </summary>
        /// <returns>Current music volume (0-1).</returns>
        public float GetMusicVolume()
        {
            return _musicVolume;
        }

        /// <summary>
        /// Gets the current master volume for sound effects.
        /// </summary>
        /// <returns>Current SFX volume (0-1).</returns>
        public float GetSFXVolume()
        {
            return _sfxVolume;
        }

        /// <summary>
        /// Loads volume preferences from PlayerPrefs. Called during Awake initialization.
        /// If no preferences are found, uses default values (0.7 for both).
        /// </summary>
        private void LoadVolumePrefs()
        {
            _musicVolume = PlayerPrefs.GetFloat("MusicVolume", 0.7f);
            _sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 0.7f);

            _musicSource.volume = _musicVolume;
            _sfxSource.volume = _sfxVolume;
        }

        /// <summary>
        /// Validates that an SFX index is within the valid range and has a clip assigned.
        /// </summary>
        /// <param name="index">The SFX enum index to validate.</param>
        /// <returns>True if the index is valid and has a clip assigned, false otherwise.</returns>
        private bool IsValidSFXIndex(int index)
        {
            return index >= 0 && index < _sfxClips.Length && _sfxClips[index] != null;
        }

        private void OnDestroy()
        {
            if (_musicFadeCoroutine != null)
            {
                StopCoroutine(_musicFadeCoroutine);
            }
        }
    }
}
