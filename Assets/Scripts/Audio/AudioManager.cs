using UnityEngine;
using System.Collections;

namespace Matchmancer.Audio
{
    /// <summary>
    /// SFX types enumeration for Matchmancer. Each value indexes into AudioManager._sfxClips.
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
    /// Singleton AudioManager. Plays BGM with fade, SFX with combo-based pitch scaling,
    /// and persists volume via PlayerPrefs.
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        private static AudioManager _instance;

        public static AudioManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindAnyObjectByType<AudioManager>();
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
        [Tooltip("SFX clips indexed by SFX enum (0=TileSelect ... 10=UltimateActivate)")]
        private AudioClip[] _sfxClips = new AudioClip[11];

        [SerializeField]
        [Tooltip("Looping background music track")]
        private AudioClip _backgroundMusic;

        private AudioSource _musicSource;
        private AudioSource _sfxSource;

        [SerializeField, Range(0f, 1f)]
        private float _musicVolume = 0.7f;

        [SerializeField, Range(0f, 1f)]
        private float _sfxVolume = 0.7f;

        private int _currentComboDepth = 0;
        private const float BasePitch = 1.0f;
        private const float ComboDepthPitchIncrement = 0.08f;
        private const float MaxComboPitch = 1.6f;

        public delegate void OnSFXPlayedDelegate(SFX sfxType);
        public event OnSFXPlayedDelegate OnSFXPlayed;

        private Coroutine _musicFadeCoroutine;

        private void Awake()
        {
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

        private void InitializeAudioSources()
        {
            _musicSource = GetComponent<AudioSource>();
            if (_musicSource == null)
            {
                _musicSource = gameObject.AddComponent<AudioSource>();
            }
            _musicSource.loop = true;
            _musicSource.playOnAwake = false;
            _musicSource.volume = _musicVolume;

            _sfxSource = gameObject.AddComponent<AudioSource>();
            _sfxSource.loop = false;
            _sfxSource.playOnAwake = false;
            _sfxSource.volume = _sfxVolume;
        }

        /// <summary>Plays an SFX clip at current combo-adjusted pitch without changing combo depth.</summary>
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

        /// <summary>Plays an SFX with pitch scaled by comboDepth (ascending musical feedback).</summary>
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

        public void ResetComboPitch()
        {
            _currentComboDepth = 0;
        }

        public int GetCurrentComboDepth()
        {
            return _currentComboDepth;
        }

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

        public void StopMusic()
        {
            _musicSource.Stop();
        }

        public void FadeMusic(float targetVolume, float duration)
        {
            if (_musicFadeCoroutine != null)
            {
                StopCoroutine(_musicFadeCoroutine);
            }

            targetVolume = Mathf.Clamp01(targetVolume);
            _musicFadeCoroutine = StartCoroutine(MusicFadeCoroutine(targetVolume, duration));
        }

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

        public void SetMusicVolume(float volume)
        {
            _musicVolume = Mathf.Clamp01(volume);
            _musicSource.volume = _musicVolume;
            PlayerPrefs.SetFloat("MusicVolume", _musicVolume);
            PlayerPrefs.Save();
        }

        public void SetSFXVolume(float volume)
        {
            _sfxVolume = Mathf.Clamp01(volume);
            _sfxSource.volume = _sfxVolume;
            PlayerPrefs.SetFloat("SFXVolume", _sfxVolume);
            PlayerPrefs.Save();
        }

        public float GetMusicVolume()
        {
            return _musicVolume;
        }

        public float GetSFXVolume()
        {
            return _sfxVolume;
        }

        private void LoadVolumePrefs()
        {
            _musicVolume = PlayerPrefs.GetFloat("MusicVolume", 0.7f);
            _sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 0.7f);

            _musicSource.volume = _musicVolume;
            _sfxSource.volume = _sfxVolume;
        }

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
