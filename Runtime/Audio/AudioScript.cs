using SketchEngine.Diagnostics;
using UnityEngine;
using PrimeTween;

using SketchEngine.Mono;

namespace SketchEngine.Audio
{
    public class AudioScript : MonoCached
    {
        #region Properties

        AudioConfig _config;
        AudioSource _audioSource;

        Tween _tweendelay;
        Tween _tweenVolume;

        public AudioConfig Config => _config;

        public AudioSource AudioSource
        {
            get
            {
                if (_audioSource == null)
                    _audioSource = GameObjectCached.GetComponent<AudioSource>();

                return _audioSource;
            }
        }

        #endregion

        #region MonoBehaviour

        void Awake()
        {
            if (_audioSource == null)
                _audioSource = GetComponent<AudioSource>();

            AudioManager.VolumeSound.OnValueChanged += VolumeSound_EventValueChanged;
            AudioManager.VolumnMusic.OnValueChanged += VolumeMusic_EventValueChanged;

            AudioManager.EventStopAll += EventStopAll;

            AudioManager.Attach(transform);
        }

        private void OnDestroy()
        {
            _tweendelay.Stop();
            _tweenVolume.Stop();

            AudioManager.EventStopAll -= EventStopAll;

            if (!AudioManager.HasInstance)
                return;

            AudioManager.VolumeSound.OnValueChanged -= VolumeSound_EventValueChanged;
            AudioManager.VolumnMusic.OnValueChanged -= VolumeMusic_EventValueChanged;
        }

        void OnDisable()
        {
            _tweendelay.Stop();
            _tweenVolume.Stop();
        }

        #endregion

        #region Function -> Public

        public void Play(AudioConfig config, bool loop = false)
        {
            Init(config, loop);

            _tweendelay.Stop();

            if (!loop)
                _tweendelay = Tween.Delay(config.Clip.length, Stop, false, false);
        }

        public void Stop()
        {
            if (!AudioManager.HasInstance)
                return;

            _tweendelay.Stop();
            _tweenVolume.Stop();
            AudioSource.Stop();

            AudioManager.ReturnPool(this);
        }

        public void TryStop(AudioConfig config)
        {
            if (config == null) return;
            TryStop(config.Clip);
        }

        public void TryStop(AudioClip clip)
        {
            if (clip == null || !AudioSource.isPlaying) return;
            if (AudioSource.clip == clip)
                Stop();
        }

        #endregion

        #region Function -> Private

        void EventStopAll(AudioType type)
        {
            if (_config == null) return;
            if (_config.Type == type && AudioSource.isPlaying)
                Stop();
        }

        float GetVolume()
        {
            return _config.Volume * (_config.Type == AudioType.Music ? AudioManager.VolumnMusic.Value : AudioManager.VolumeSound.Value);
        }

        void UpdateVolume()
        {
            if (_config == null) return;

            float volume = GetVolume();
            AudioSource.mute = volume <= 0;

            if (Mathf.Approximately(AudioSource.volume, volume))
                return;

            _tweenVolume.Stop();
            _tweenVolume = Tween.AudioVolume(AudioSource, volume, 0.1f);
        }


        void VolumeSound_EventValueChanged(float volume)
        {
            UpdateVolume();
        }

        void VolumeMusic_EventValueChanged(float volume)
        {
            UpdateVolume();
        }

        void Init(AudioConfig config, bool loop = false)
        {
            if (config == null || config.Clip == null)
            {
                CodeSketchDebug.LogWarning("[AudioScript] Invalid config or clip!");
                return;
            }

            if (_audioSource == null)
                _audioSource = AudioSource;
            if (AudioSource == null) return;

            _config = config;

            AudioSource.clip = config.Clip;
            AudioSource.loop = loop;
            AudioSource.minDistance = config.EarsDistance.x;
            AudioSource.maxDistance = config.EarsDistance.y;
            AudioSource.spatialBlend = config.Mode == AudioMode.Mode3D ? 1f : 0f;

            AudioSource.rolloffMode = AudioRolloffMode.Logarithmic;

            // Tắt bỏ logic quyết định mức độ thay đổi cao độ (pitch) khi nguồn âm hoặc AudioListener di chuyển so với nhau.
            // Tránh trường hợp méo âm thanh
            AudioSource.dopplerLevel = 0;

            AudioSource.outputAudioMixerGroup = config.Bus == AudioBus.Master ? null : AudioMixerFactory.GetGroup(config.Bus);

            UpdateVolume();
            AudioSource.Play();
        }

        #endregion
    }

    public static class AudioExtensions
    {
        public static void TryStop(this AudioScript audio, AudioConfig config)
        {
            if (audio == null) return;
            audio.TryStop(config);
        }

        public static bool IsClip(this AudioScript audio, AudioConfig config)
        {
            if (audio == null || config == null || audio.AudioSource.clip == null || config.Clip == null) return false;
            return audio.AudioSource.clip == config.Clip;
        }
    }
}