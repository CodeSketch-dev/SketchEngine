using Sirenix.OdinInspector;
using UnityEngine;

using SketchEngine.SO.Editor;

namespace SketchEngine.SO
{
    public class SerializedScriptableObjectSingleton<T> : SerializedScriptableObject where T : SerializedScriptableObject
    {
        static string s_rootFolderName => "SketchEngine/ScriptableObjectSingletons";

        static T s_instance = null;

        public static T Instance
        {
            get
            {
                if (s_instance == null)
                {
                    s_instance = Resources.Load<T>($"{s_rootFolderName}/{typeof(T)}");

#if UNITY_EDITOR
                    if (s_instance == null)
                    {
                        string configPath = $"Assets/Resources/{s_rootFolderName}/";

                        if (!System.IO.Directory.Exists(configPath))
                            System.IO.Directory.CreateDirectory(configPath);

                        s_instance = ScriptableObjectHelper.CreateAsset<T>(configPath, typeof(T).ToString());
                    }
#endif
                    (s_instance as SerializedScriptableObjectSingleton<T>)?.OnInitialize();
                }

                return s_instance;
            }
        }
        
        public static void Release()
        {
            (Instance as SerializedScriptableObjectSingleton<T>)?.OnRelease();
            s_instance = null;
            Resources.UnloadUnusedAssets();
        }
        
        protected virtual void OnInitialize()
        {
            
        }
        
        protected virtual void OnRelease()
        {
            
        }
    }
}
