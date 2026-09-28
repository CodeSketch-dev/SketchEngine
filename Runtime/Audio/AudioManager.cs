using UnityEngine;
using UnityEngine.Pool;
using System.Collections.Generic;
using SketchEngine.Data;
using SketchEngine.Diagnostics;
using SketchEngine.Mono;
using System;

namespace SketchEngine.Audio
{
    public class AudioManager : MonoSingleton<AudioManager>
    {
        protected override bool PersistAcrossScenes => true;

        public static readonly DataValue<float> VolumnMusic = new DataValue<float>(1.0f);
        public static readonly DataValue<float> VolumeSound = new DataValue<float>(1.0f);

        public static readonly HashSet<AudioScript> HashsetActives = new HashSet<AudioScript>(); // Lưu trữ các AudioScript đang active

        ObjectPool<AudioScript> _pool;
        public static ObjectPool<AudioScript> Pool => SafeInstance._pool;

        public static event Action<AudioType> EventStopAll;

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

            var audio = Pool.Get();
            audio.Play(config, loop);

            // Thêm vào danh sách đang sử dụng
            HashsetActives.Add(audio);

            return audio;
        }

        public static AudioScript Play(AudioConfig config, Vector3 position, bool loop = false)
        {
            if (config == null || config.Clip == null) return null;

            var audio = Pool.Get();
            audio.TransformCached.position = position;
            audio.Play(config, loop);

            // Thêm vào danh sách đang sử dụng
            HashsetActives.Add(audio);

            return audio;
        }

        public static void ForceStopAll(AudioType type)
        {
            EventStopAll?.Invoke(type);
        }

        public static void ReturnPool(AudioScript audio)
        {
            if (audio == null) return;

            // Kiểm tra xem object có đang trong danh sách active không
            if (HashsetActives.Contains(audio))
            {
                HashsetActives.Remove(audio);
                Pool.Release(audio);
            }
            else
            {
                CodeSketchDebug.LogWarning($"[AudioManager] Trying to release an object that has already been released: {audio.name}");
            }
        }

        #endregion

        #region Function -> Private

        void InitPool()
        {
            if (Pool != null) return;

            _pool = new ObjectPool<AudioScript>(
                createFunc: () =>
                {
                    GameObject go = new GameObject($"{typeof(AudioScript)}", typeof(AudioSource));
                    return go.AddComponent<AudioScript>();
                },
                actionOnGet: _audio =>
                {
                    _audio.GameObjectCached.SetActive(true);
                },
                actionOnRelease: (_audio) =>
                {
                    _audio.GameObjectCached.SetActive(false);
                },
                actionOnDestroy: (_audio) =>
                {
                    Destroy(_audio.GameObjectCached);
                },
                collectionCheck: true, // Kiểm tra lỗi nếu object đã được release trước đó (Unity 2022+)
                defaultCapacity: 25,
                maxSize: 50
            );
        }

        #endregion
    }
}
