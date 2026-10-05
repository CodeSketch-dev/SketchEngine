using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace SketchEngine.Editor.Scriptable
{
    public class ScriptableObjectLookup
    {
        [MenuItem("Assets/Create/SketchEngine_ScriptableLookup", false, -1000)]
        public static void CreateAssembly()
        {
            var allScriptableObjects = TypeCache.GetTypesDerivedFrom<ScriptableObject>()
                .Where(type => IsSupportedAssembly(type.Assembly.GetName().Name))
                .Where(type => !type.IsAbstract && !type.IsGenericType)
                .ToArray();

            if (allScriptableObjects.Length == 0)
            {
                Debug.LogWarning("No ScriptableObject types found in Assembly-CSharp or SketchEngine assemblies.");
                return;
            }

            var window = EditorWindow.GetWindow<ScriptableObjectWindow>(
                true,
                "Create a new ScriptableObject",
                true
            );

            window.Types = allScriptableObjects
                .OrderBy(t => t.Name)
                .ThenBy(t => t.Namespace)
                .ToArray();

            window.ShowPopup();
        }

        static bool IsSupportedAssembly(string assemblyName)
        {
            if (assemblyName.Contains("SketchEngine.Installer"))
                return false;

            if (assemblyName == "Assembly-CSharp")
                return true;

            return assemblyName.StartsWith("SketchEngine") &&
                   !assemblyName.Contains("Editor");
        }
    }
}
