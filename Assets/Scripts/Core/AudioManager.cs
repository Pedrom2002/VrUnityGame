using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VRAimLab.Core
{
    /// Audio manager - handles all game audio (music, SFX, UI, voice)
    /// Singleton pattern with spatial 3D audio support for VR
    public class AudioManager : MonoBehaviour
    {
        #region Singleton
        public static AudioManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            InitializeAudioSources();
            LoadVolumeSettings();
        }
        #endregion

        #region Audio Clips
        [Header("Music")]
        [SerializeField] private AudioClip menuMusic;
        [SerializeField] private AudioClip gameplayMusic;
        [SerializeField] private AudioClip victoryMusic;

        [Header("SFX - Shooting")]
        [SerializeField] private AudioClip shootSound;
        [SerializeField] private AudioClip reloadSound;
        [SerializeField] private AudioClip emptyGunSound;

        [Header("SFX - Targets")]
        [SerializeField] private AudioClip targetHitSound;
        [SerializeField] private AudioClip perfectHitSound;
        [SerializeField] private AudioClip targetMissSound;
        [SerializeField] private AudioClip targetSpawnSound;

        [Header("SFX - Combos & Streaks")]
        [SerializeField] private AudioClip comboSound;
        [SerializeField] private AudioClip streakSound;
        [SerializeField] private AudioClip streakLostSound;

        [Header("UI Sounds")]
        [SerializeField] private AudioClip buttonClickSound;
        [SerializeField] private AudioClip buttonHoverSound;
        [SerializeField] private AudioClip modeSelectSound;

        [Header("Voice Lines (ARIA)")]
        [SerializeField] private AudioClip welcomeVoice;
        [SerializeField] private AudioClip goodJobVoice;
        [SerializeField] private AudioClip excellentVoice;
        [SerializeField] private AudioClip gameOverVoice;
        #endregion

        #region Audio Sources
        private AudioSource musicSource;
        private AudioSource sfxSource;
        private AudioSource uiSource;
        private AudioSource voiceSource;
        #endregion

        #region Volume Settings
        [Header("Volume Settings")]
        [SerializeField][Range(0f, 1f)] private float masterVolume = 1f;
        [SerializeField][Range(0f, 1f)] private float musicVolume = 0.7f;
        [SerializeField][Range(0f, 1f)] private float sfxVolume = 0.8f;
        [SerializeField][Range(0f, 1f)] private float uiVolume = 0.6f;
        [SerializeField][Range(0f, 1f)] private float voiceVolume = 0.9f;

        public float MasterVolume
        {
            get => masterVolume;
            set
            {
                masterVolume = Mathf.Clamp01(value);
                UpdateAllVolumes();
                SaveVolumeSettings();
            }
        }

        public float MusicVolume
        {
            get => musicVolume;
            set
            {
                musicVolume = Mathf.Clamp01(value);
                UpdateMusicVolume();
                SaveVolumeSettings();
            }
        }

        public float SFXVolume
        {
            get => sfxVolume;
            set
            {
                sfxVolume = Mathf.Clamp01(value);
                UpdateSFXVolume();
                SaveVolumeSettings();
            }
        }

        public float UIVolume
        {
            get => uiVolume;
            set
            {
                uiVolume = Mathf.Clamp01(value);
                UpdateUIVolume();
                SaveVolumeSettings();
            }
        }

        public float VoiceVolume
        {
            get => voiceVolume;
            set
            {
                voiceVolume = Mathf.Clamp01(value);
                UpdateVoiceVolume();
                SaveVolumeSettings();
            }
        }
        #endregion

        #region Initialization
        /// Initialize all audio sources with proper settings
        private void InitializeAudioSources()
        {
            // Music source (2D, looping)
            musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.loop = true;
            musicSource.playOnAwake = false;
            musicSource.spatialBlend = 0f; // 2D

            // SFX source (3D spatial)
            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.loop = false;
            sfxSource.playOnAwake = false;
            sfxSource.spatialBlend = 1f; // 3D
            sfxSource.rolloffMode = AudioRolloffMode.Linear;
            sfxSource.maxDistance = 50f;

            // UI source (2D)
            uiSource = gameObject.AddComponent<AudioSource>();
            uiSource.loop = false;
            uiSource.playOnAwake = false;
            uiSource.spatialBlend = 0f; // 2D

            // Voice source (2D, clear)
            voiceSource = gameObject.AddComponent<AudioSource>();
            voiceSource.loop = false;
            voiceSource.playOnAwake = false;
            voiceSource.spatialBlend = 0f; // 2D
            voiceSource.priority = 0; // Highest priority

            UpdateAllVolumes();
        }
        #endregion

        #region Volume Management
        private void UpdateAllVolumes()
        {
            UpdateMusicVolume();
            UpdateSFXVolume();
            UpdateUIVolume();
            UpdateVoiceVolume();
        }

        private void UpdateMusicVolume()
        {
            if (musicSource != null)
                musicSource.volume = musicVolume * masterVolume;
        }

        private void UpdateSFXVolume()
        {
            if (sfxSource != null)
                sfxSource.volume = sfxVolume * masterVolume;
        }

        private void UpdateUIVolume()
        {
            if (uiSource != null)
                uiSource.volume = uiVolume * masterVolume;
        }

        private void UpdateVoiceVolume()
        {
            if (voiceSource != null)
                voiceSource.volume = voiceVolume * masterVolume;
        }

        private void SaveVolumeSettings()
        {
            PlayerPrefs.SetFloat("MasterVolume", masterVolume);
            PlayerPrefs.SetFloat("MusicVolume", musicVolume);
            PlayerPrefs.SetFloat("SFXVolume", sfxVolume);
            PlayerPrefs.SetFloat("UIVolume", uiVolume);
            PlayerPrefs.SetFloat("VoiceVolume", voiceVolume);
            PlayerPrefs.Save();
        }

        private void LoadVolumeSettings()
        {
            masterVolume = PlayerPrefs.GetFloat("MasterVolume", 1f);
            musicVolume = PlayerPrefs.GetFloat("MusicVolume", 0.7f);
            sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 0.8f);
            uiVolume = PlayerPrefs.GetFloat("UIVolume", 0.6f);
            voiceVolume = PlayerPrefs.GetFloat("VoiceVolume", 0.9f);
            UpdateAllVolumes();
        }
        #endregion

        #region Music Methods
        /// Play music with optional fade in
        public void PlayMusic(AudioClip clip, bool fadeIn = true, float fadeDuration = 1f)
        {
            if (clip == null) return;

            if (fadeIn && musicSource.isPlaying)
            {
                StartCoroutine(CrossfadeMusic(clip, fadeDuration));
            }
            else
            {
                musicSource.clip = clip;
                musicSource.Play();
                if (fadeIn)
                {
                    StartCoroutine(FadeInMusic(fadeDuration));
                }
            }
        }

        /// Stop music with optional fade out
        public void StopMusic(bool fadeOut = true, float fadeDuration = 1f)
        {
            if (fadeOut)
            {
                StartCoroutine(FadeOutMusic(fadeDuration));
            }
            else
            {
                musicSource.Stop();
            }
        }

        /// Play menu music
        public void PlayMenuMusic()
        {
            PlayMusic(menuMusic);
        }

        /// Play gameplay music
        public void PlayGameplayMusic()
        {
            PlayMusic(gameplayMusic);
        }

        private IEnumerator FadeInMusic(float duration)
        {
            float targetVolume = musicVolume * masterVolume;
            musicSource.volume = 0f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                musicSource.volume = Mathf.Lerp(0f, targetVolume, elapsed / duration);
                yield return null;
            }

            musicSource.volume = targetVolume;
        }

        private IEnumerator FadeOutMusic(float duration)
        {
            float startVolume = musicSource.volume;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                musicSource.volume = Mathf.Lerp(startVolume, 0f, elapsed / duration);
                yield return null;
            }

            musicSource.Stop();
            musicSource.volume = startVolume;
        }

        private IEnumerator CrossfadeMusic(AudioClip newClip, float duration)
        {
            float halfDuration = duration / 2f;

            // Fade out current
            yield return StartCoroutine(FadeOutMusic(halfDuration));

            // Change clip
            musicSource.clip = newClip;
            musicSource.Play();

            // Fade in new
            yield return StartCoroutine(FadeInMusic(halfDuration));
        }
        #endregion

        #region SFX Methods - Shooting
        /// Play weapon shoot sound at position (3D spatial)
        public void PlayShootSound(Vector3 position)
        {
            PlaySoundAtPosition(shootSound, position);
        }

        /// Play reload sound
        public void PlayReloadSound()
        {
            PlaySFX(reloadSound);
        }

        /// Play empty gun sound
        public void PlayEmptyGunSound()
        {
            PlaySFX(emptyGunSound);
        }
        #endregion

        #region SFX Methods - Targets
        /// Play hit sound with perfect hit variation
        public void PlayHitSound(bool isPerfect = false)
        {
            AudioClip clip = isPerfect ? perfectHitSound : targetHitSound;
            PlaySFX(clip);
        }

        /// Play target hit sound at position (3D)
        public void PlayTargetHitSound(Vector3 position, bool isPerfect = false)
        {
            AudioClip clip = isPerfect ? perfectHitSound : targetHitSound;
            PlaySoundAtPosition(clip, position);
        }

        /// Play miss sound
        public void PlayMissSound()
        {
            PlaySFX(targetMissSound);
        }

        /// Play target spawn sound at position
        public void PlayTargetSpawnSound(Vector3 position)
        {
            PlaySoundAtPosition(targetSpawnSound, position);
        }
        #endregion

        #region SFX Methods - Combos
        /// Play combo sound
        public void PlayComboSound()
        {
            PlaySFX(comboSound);
        }

        /// Play streak sound
        public void PlayStreakSound()
        {
            PlaySFX(streakSound);
        }

        /// Play streak lost sound
        public void PlayStreakLostSound()
        {
            PlaySFX(streakLostSound);
        }
        #endregion

        #region UI Sound Methods
        /// Play button click sound
        public void PlayButtonClick()
        {
            PlayUI(buttonClickSound);
        }

        /// Play button hover sound
        public void PlayButtonHover()
        {
            PlayUI(buttonHoverSound);
        }

        /// Play mode select sound
        public void PlayModeSelect()
        {
            PlayUI(modeSelectSound);
        }
        #endregion

        #region Voice Methods
        /// Play ARIA welcome voice
        public void PlayWelcomeVoice()
        {
            PlayVoice(welcomeVoice);
        }

        /// Play encouragement voice based on performance
        public void PlayEncouragementVoice(float accuracy)
        {
            if (accuracy >= 0.9f)
            {
                PlayVoice(excellentVoice);
            }
            else if (accuracy >= 0.6f)
            {
                PlayVoice(goodJobVoice);
            }
        }

        /// Play game over voice
        public void PlayGameOverVoice()
        {
            PlayVoice(gameOverVoice);
        }
        #endregion

        #region Core Play Methods
        private void PlaySFX(AudioClip clip)
        {
            if (clip == null) return;
            sfxSource.PlayOneShot(clip, sfxVolume * masterVolume);
        }

        private void PlaySoundAtPosition(AudioClip clip, Vector3 position)
        {
            if (clip == null) return;
            AudioSource.PlayClipAtPoint(clip, position, sfxVolume * masterVolume);
        }

        private void PlayUI(AudioClip clip)
        {
            if (clip == null) return;
            uiSource.PlayOneShot(clip, uiVolume * masterVolume);
        }

        private void PlayVoice(AudioClip clip)
        {
            if (clip == null) return;

            // Stop current voice if playing
            if (voiceSource.isPlaying)
            {
                voiceSource.Stop();
            }

            voiceSource.PlayOneShot(clip, voiceVolume * masterVolume);
        }
        #endregion

        #region Public Utility Methods
        /// Mute/unmute all audio
        public void SetMute(bool muted)
        {
            AudioListener.volume = muted ? 0f : 1f;
        }

        /// Check if any voice is currently playing
        public bool IsVoicePlaying()
        {
            return voiceSource != null && voiceSource.isPlaying;
        }

        /// Stop all sounds immediately
        public void StopAllSounds()
        {
            musicSource?.Stop();
            sfxSource?.Stop();
            uiSource?.Stop();
            voiceSource?.Stop();
        }
        #endregion
    }
}
