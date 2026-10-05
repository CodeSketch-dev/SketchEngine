using PrimeTween;
using SketchEngine.Mono;
using UnityEngine;
using UnityEngine.Audio;

namespace SketchEngine.Audio
{
    public class AudioScript : MonoCached
    {
        AudioConfig _config;
        AudioSource _audioSource;

        Tween _tweenDelay;
        Tween _tweenVolume;

        // Delegate tạo 1 lần trong Awake; tránh cấp phát mỗi lần phát.
        System.Action _onFinished;
        System.Action<SketchAudioType> _onStopAll;
        System.Action<float> _onVolumeChanged;

        // Đang được lấy ra khỏi pool. Dùng cờ này thay vì HashSet để Stop() idempotent và không GC.
        bool _inUse;

        public AudioConfig Config => _config;

        public AudioSource AudioSource
        {
            get
            {
                if (_audioSource == null)
                    _audioSource = GetComponent<AudioSource>();
                return _audioSource;
            }
        }

        #region MonoBehaviour

        void Awake()
        {
            _audioSource = GetComponent<AudioSource>();

            _onFinished = Stop;
            _onStopAll = OnStopAll;
            _onVolumeChanged = OnVolumeChanged;

            AudioManager.VolumeSound.OnValueChanged += _onVolumeChanged;
            AudioManager.VolumnMusic.OnValueChanged += _onVolumeChanged;
            AudioManager.EventStopAll += _onStopAll;

            AudioManager.Attach(transform);
        }

        void OnDisable()
        {
            _tweenDelay.Stop();
            _tweenVolume.Stop();
            _inUse = false;
        }

        void OnDestroy()
        {
            _tweenDelay.Stop();
            _tweenVolume.Stop();
            _inUse = false;

            AudioManager.EventStopAll -= _onStopAll;
            AudioManager.VolumeSound.OnValueChanged -= _onVolumeChanged;
            AudioManager.VolumnMusic.OnValueChanged -= _onVolumeChanged;
        }

        #endregion

        #region Function -> Public

        public void Play(AudioConfig config, bool loop = false)
        {
            _inUse = true;
            Init(config, loop);

            _tweenDelay.Stop();

            if (!loop && _config != null && _config.Clip != null)
            {
                // Thời lượng thực = độ dài clip chia pitch, nếu không pitch khác 1 sẽ dừng sớm/muộn.
                float speed = Mathf.Max(0.01f, Mathf.Abs(AudioSource.pitch));
                _tweenDelay = Tween.Delay(_config.Clip.length / speed, _onFinished, false, false);
            }
        }

        public void Stop()
        {
            // Idempotent: tween hết giờ, ForceStopAll, TryStop có thể gọi cùng lúc; chỉ release đúng một lần.
            if (!_inUse) return;
            _inUse = false;

            _tweenDelay.Stop();
            _tweenVolume.Stop();

            if (_audioSource != null)
                _audioSource.Stop();

            AudioManager.Release(this);
        }

        public void TryStop(AudioConfig config)
        {
            if (config == null) return;
            TryStop(config.Clip);
        }

        public void TryStop(AudioClip clip)
        {
            if (clip == null || !_inUse || AudioSource == null || !AudioSource.isPlaying) return;

            if (AudioSource.clip == clip)
                Stop();
        }

        #endregion

        #region Function -> Private

        void OnStopAll(SketchAudioType type)
        {
            if (!_inUse || _config == null) return;

            if (_config.Type == type)
                Stop();
        }

        void OnVolumeChanged(float _)
        {
            UpdateVolume();
        }

        float GetVolume()
        {
            var channel = _config.Type == SketchAudioType.Music
                ? AudioManager.VolumnMusic.Value
                : AudioManager.VolumeSound.Value;

            return _config.Volume * channel;
        }

        void UpdateVolume()
        {
            if (_config == null || AudioSource == null) return;

            float volume = GetVolume();
            AudioSource.mute = volume <= 0f;

            if (Mathf.Approximately(AudioSource.volume, volume))
                return;

            _tweenVolume.Stop();
            _tweenVolume = Tween.AudioVolume(AudioSource, volume, 0.1f);
        }

        void Init(AudioConfig config, bool loop)
        {
            if (config == null || config.Clip == null)
            {
                SketchEngine.Diagnostics.SketchDebug.LogWarning("[AudioScript] Invalid config or clip!");
                return;
            }

            if (AudioSource == null) return;

            _config = config;

            AudioSource.clip = config.Clip;
            AudioSource.loop = loop;
            AudioSource.minDistance = config.EarsDistance.x;
            AudioSource.maxDistance = config.EarsDistance.y;
            AudioSource.spatialBlend = config.Mode == AudioMode.Mode3D ? 1f : 0f;
            AudioSource.rolloffMode = AudioRolloffMode.Logarithmic;

            // Tắt doppler để tránh méo âm khi nguồn/listener di chuyển.
            AudioSource.dopplerLevel = 0f;

            AudioMixerGroup group = config.Bus == AudioBus.Master ? null : AudioMixerFactory.GetGroup(config.Bus);
            if (AudioSource.outputAudioMixerGroup != group)
                AudioSource.outputAudioMixerGroup = group;

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
