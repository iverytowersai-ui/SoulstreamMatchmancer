using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

namespace Matchmancer.UI
{
    /// <summary>
    /// MVP guest-only login. Plays a looping animated background via
    /// VideoPlayer and renders it to a full-screen RawImage. Shows a
    /// "Play as Guest" button. Tap it → replace with MainHub.
    /// </summary>
    public class LoginScreenController : ScreenController
    {
        [Header("References")]
        [SerializeField] private ScreenNavigatorController navigator;
        [SerializeField] private Button playAsGuestButton;

        [Header("Animated Background")]
        [Tooltip("Assign the LoginAnimation.mp4 video clip here.")]
        [SerializeField] private VideoClip loginVideo;

        [Tooltip("Full-screen RawImage that displays the video.")]
        [SerializeField] private RawImage videoDisplay;

        [Header("Static Fallback")]
        [Tooltip("Optional static background if no video is assigned.")]
        [SerializeField] private Image backgroundImage;

        private VideoPlayer _videoPlayer;
        private RenderTexture _renderTexture;

        private void OnEnable()
        {
            if (playAsGuestButton != null)
                playAsGuestButton.onClick.AddListener(HandleGuestLogin);

            SetupVideo();
        }

        private void OnDisable()
        {
            if (playAsGuestButton != null)
                playAsGuestButton.onClick.RemoveListener(HandleGuestLogin);

            CleanupVideo();
        }

        private void SetupVideo()
        {
            if (loginVideo == null || videoDisplay == null) return;

            // Hide static fallback when video is available
            if (backgroundImage != null)
                backgroundImage.gameObject.SetActive(false);

            _videoPlayer = gameObject.GetComponent<VideoPlayer>();
            if (_videoPlayer == null)
                _videoPlayer = gameObject.AddComponent<VideoPlayer>();

            _renderTexture = new RenderTexture(
                (int)loginVideo.width,
                (int)loginVideo.height,
                0);

            _videoPlayer.clip = loginVideo;
            _videoPlayer.renderMode = VideoRenderMode.RenderTexture;
            _videoPlayer.targetTexture = _renderTexture;
            _videoPlayer.isLooping = true;
            _videoPlayer.playOnAwake = false;
            _videoPlayer.audioOutputMode = VideoAudioOutputMode.None;

            videoDisplay.texture = _renderTexture;
            _videoPlayer.Play();
        }

        private void CleanupVideo()
        {
            if (_videoPlayer != null)
            {
                _videoPlayer.Stop();
                _videoPlayer.targetTexture = null;
            }

            if (_renderTexture != null)
            {
                _renderTexture.Release();
                Destroy(_renderTexture);
                _renderTexture = null;
            }
        }

        private void HandleGuestLogin()
        {
            if (navigator == null) return;
            navigator.Navigator.Replace(ScreenId.MainHub);
        }
    }
}
