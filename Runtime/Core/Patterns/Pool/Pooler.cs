using System.Collections.Generic;
using SketchEngine.Mono;
using UnityEngine;

namespace SketchEngine.Patterns.Pool
{
    public class Pooler : MonoSingleton<Pooler>
    {
        protected override bool PersistAcrossScenes => true;

        Dictionary<PoolPrefabConfig, GameObject> _attachers = new();

        public static void Attach(PoolPrefabItem item)
        {
            if (item == null || item.Config == null) return;
            
            if (!SafeInstance._attachers.ContainsKey(item.Config))
            {
                var go = new GameObject(item.Config.name);
                
                if (item.Config.PersistAcrossScenes)
                    go.transform.SetParent(SafeInstance.TransformCached);
                SafeInstance._attachers.Add(item.Config, go);
            }

            var attacher = SafeInstance._attachers.GetValueOrDefault(item.Config);
            if (attacher)
            {
                item.TransformCached.SetParent(attacher.transform);
            }
        }

        public static void Release(PoolPrefabConfig config)
        {
            if (config == null) return;

            if (SafeInstance._attachers.ContainsKey(config))
            {
                var attacher = SafeInstance._attachers[config];
                
                SafeInstance._attachers.Remove(config);
                Destroy(attacher);
            }
        }
    }
}
