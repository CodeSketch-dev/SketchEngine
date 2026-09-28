using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace SketchEngine.SO.Editor
{
    public static class ScriptableObjectHelper
    {
        public static T CreateAsset<T>(string path, string fileName) where T : ScriptableObject
        { 
#if UNITY_EDITOR
            string filePath = $"{path}/{fileName}.asset";

            T asset = ScriptableObject.CreateInstance<T>();

            AssetDatabase.CreateAsset(asset, filePath);
            SaveAssetsDatabase();

            return AssetDatabase.LoadAssetAtPath(filePath, typeof(T)) as T;
#endif
            return null;
        }
        
        public static void SaveAsset(ScriptableObject asset)
        {
#if UNITY_EDITOR
            EditorUtility.SetDirty(asset);
            SaveAssetsDatabase();
#endif
        }
        
        public static T LoadOrCreateNewAsset<T>(string path, string fileName) where T : ScriptableObject
        {
#if UNITY_EDITOR
            string filePath = $"{path}/{fileName}.asset";

            T asset = AssetDatabase.LoadAssetAtPath(filePath, typeof(T)) as T;
            if (asset == null)
            {
                asset = CreateAsset<T>(path, fileName);
            }

            return asset;
#endif
            return null;
        }
        
        public static T CopyAsset<T>(string oldPath, string newPath) where T : ScriptableObject
        {
#if UNITY_EDITOR
            DeleteAsset<T>(newPath);

            AssetDatabase.CopyAsset(oldPath, newPath);
            SaveAssetsDatabase();

            return AssetDatabase.LoadAssetAtPath(newPath, typeof(T)) as T;
#endif
            return null;
        }

        public static void DeleteAsset<T>(string assetPath)
        {
#if UNITY_EDITOR
            if (AssetDatabase.LoadAssetAtPath(assetPath, typeof(T)) != null)
            {
                AssetDatabase.DeleteAsset(assetPath);
            }
#endif
        }
        
        public static List<T> FindAssetsByType<T>() where T : ScriptableObject
        {
            List<T> assets = new List<T>();
#if UNITY_EDITOR
            string[] guids = AssetDatabase.FindAssets($"t:{typeof(T)}");
            foreach (var t in guids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(t);
                T asset = AssetDatabase.LoadAssetAtPath<T>(assetPath);
                if (asset != null)
                {
                    assets.Add(asset);
                }
            }
#endif
            return assets;
        }
        
        static void SaveAssetsDatabase()
        {
#if UNITY_EDITOR
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
#endif
        }
    }
}
