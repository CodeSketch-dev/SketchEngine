using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

using UnityEngine;

namespace SketchEngine.Utilities
{
    public static class UtilityAssetDatabase
    {
        public static GameObject CloneAsPrefab(GameObject source, Transform parent = null, bool worldPositionStay = false)
        {
            if (source == null)
                return null;

#if UNITY_EDITOR
            var prefabAsset = PrefabUtility.GetCorrespondingObjectFromSource(source);
            if (prefabAsset == null)
            {
                Debug.LogWarning(
                    $"[UtilsAssetDatabase] Object '{source.name}' is not a prefab instance."
                );
                return null;
            }

            var ins = PrefabUtility.InstantiatePrefab(prefabAsset) as GameObject;
            if (ins == null)
                return null;

            if (parent != null)
                ins.transform.SetParent(parent, worldPositionStay);
            return ins;
#else
            return null;
#endif
        }

        public static List<T> LoadAllAssetsInFolder<T>(string folderPath) where T : Object
        {
            var list = new List<T>();
#if UNITY_EDITOR
            string[] guids = AssetDatabase.FindAssets($"t:{typeof(T).Name}", new[] { folderPath });
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                T asset = AssetDatabase.LoadAssetAtPath<T>(path);
                if (asset != null)
                    list.Add(asset);
            }
#endif
            return list;
        }
    }
}
