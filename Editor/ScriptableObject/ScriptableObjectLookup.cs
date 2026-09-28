using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace SketchEngine.Editor.Scriptable
{
    public class ScriptableObjectLookup
    {
        [MenuItem("Assets/Create/Scriptable Object", false, 0)]
        public static void CreateAssembly()
        {
            var allScriptableObjects = new List<Type>();

            var assemblies = AppDomain.CurrentDomain.GetAssemblies();

            foreach (var assembly in assemblies)
            {
                string assemblyName = assembly.GetName().Name;

                bool isGameAssembly = assemblyName == "Assembly-CSharp";

                bool isSketchEngineRuntime =
                    assemblyName.StartsWith("SketchEngine") &&
                    !assemblyName.Contains("Editor");
                bool isInstaller = assemblyName.Contains("CodeSketch.Installer");

                if (!isGameAssembly && !isSketchEngineRuntime || isInstaller)
                    continue;

                try
                {
                    var types = assembly.GetTypes()
                        .Where(t =>
                            typeof(ScriptableObject).IsAssignableFrom(t) &&
                            !t.IsAbstract &&
                            !t.IsGenericType
                        );

                    allScriptableObjects.AddRange(types);
                }
                catch
                {
                    // ignore broken assembly
                }
            }

            if (allScriptableObjects.Count == 0)
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
                .ToArray();

            window.ShowPopup();
        }
    }
}