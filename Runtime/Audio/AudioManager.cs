using System;
using SketchEngine.Data;
using SketchEngine.Mono;
using UnityEngine;
using UnityEngine.Pool;

namespace SketchEngine.Audio
{
    public class AudioManager : MonoSingleton<AudioManager>
    {
        protected override bool PersistAcrossScenes => true;

        public static readonly DataValue<float> VolumnMusic = new DataValue<float>(1.0f);
        public static readonly DataValue<float> VolumeSound = new DataValue<float>(1.0f);

        ObjectPool<AudioScript> _pool;

        public static ObjectPool<AudioScript> Pool => SafeInstance._pool;

        public static event Action<SketchAudioType> EventStopAll;

        #region MonoBehaviour

        protected override void Awake()
        {
            base.Awake();
            InitPool();
        }

        #endregion

        #region Function -> Public

        public static AudioScript Play(AudioConfig config, bool loop = false)
        {
            if (config == null || config.Clip == null) return null;

            AudioScript audio = Pool.Get();
            audio.Play(config, loop);
            return audio;
        }

        public static AudioScript Play(AudioConfig config, Vector3 position, bool loop = false)
        {
            if (config == null || config.Clip == null) return null;

            AudioScript audio = Pool.Get();
            audio.TransformCached.position = position;
            audio.Play(config, loop);
            return audio;
        }

        public static void ForceStopAll(SketchAudioType type)
        {
            EventStopAll?.Invoke(type);
        }

        // Chỉ được gọi từ AudioScript.Stop(); AudioScript tự đảm bảo chỉ release một lần qua cờ _inUse.
        internal static void Release(AudioScript audio)
        {
            if (audio == null || !HasInstance) return;
            Pool.Release(audio);
        }

        #endregion

        #region Function -> Private

        void InitPool()
        {
            if (_pool != null) return;

            _pool = new ObjectPool<AudioScript>(
                createFunc: CreateAudioScript,
                actionOnGet: audio => audio.GameObjectCached.SetActive(true),
                actionOnRelease: audio => audio.GameObjectCached.SetActive(false),
                actionOnDestroy: audio => Destroy(audio.GameObjectCached),
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                collectionCheck: true,
#else
                collectionCheck: false,
#endif
                defaultCapacity: 16,
                maxSize: 50
            );
        }

        static AudioScript CreateAudioScript()
        {
            var go = new GameObject(nameof(AudioScript), typeof(AudioSource));
            return go.AddComponent<AudioScript>();
        }

        #endregion
    }
}
