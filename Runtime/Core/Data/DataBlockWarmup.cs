using System;
using System.Reflection;
using UnityEngine;

namespace SketchEngine.Data
{
    // Công tắc tổng: Enabled = false thì bỏ qua toàn bộ warmup lúc khởi động (cả danh sách sinh sẵn lẫn quét reflection).
    // Gán trước BeforeSceneLoad, vd trong [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)].
    public static class DataWarmupConfig
    {
        public static bool Enabled { get; set; } = true;
    }

    public static partial class DataBlockWarmup
    {
        // File DataBlockRegistry.g.cs chỉ được sinh khi build game (xem DataBlockRegistryBuilder trong Editor) và sẽ implement hàm này.
        // Editor không có file đó nên hàm không tồn tại -> compiler bỏ qua lời gọi, và dùng quét reflection bên dưới.
        static bool s_generatedRan;
        static partial void WarmupGenerated();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void WarmupAll()
        {
            if (!DataWarmupConfig.Enabled) return;

            WarmupGenerated();

            if (!s_generatedRan)
                ScanAssemblies();
        }

        static void ScanAssemblies()
        {
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

            for (int i = 0; i < assemblies.Length; i++)
            {
                if (IsEngineAssembly(assemblies[i])) continue;

                Type[] types = GetTypesSafe(assemblies[i]);
                for (int j = 0; j < types.Length; j++)
                {
                    Type type = types[j];
                    if (type == null || !type.IsClass || type.IsAbstract) continue;

                    Type blockBase = FindDataBlockBase(type);
                    if (blockBase == null) continue;

                    blockBase.GetMethod("Warmup", BindingFlags.Public | BindingFlags.Static)?.Invoke(null, null);
                }
            }
        }

        // Bỏ qua assembly của engine/Unity/.NET để giảm thời gian quét trên máy yếu.
        static bool IsEngineAssembly(Assembly assembly)
        {
            string name = assembly.GetName().Name;
            return name.StartsWith("System", StringComparison.Ordinal)
                || name.StartsWith("Unity", StringComparison.Ordinal)
                || name.StartsWith("mscorlib", StringComparison.Ordinal)
                || name.StartsWith("netstandard", StringComparison.Ordinal);
        }

        static Type[] GetTypesSafe(Assembly assembly)
        {
            try
            {
                return assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException e)
            {
                return Array.FindAll(e.Types, t => t != null);
            }
        }

        static Type FindDataBlockBase(Type type)
        {
            Type current = type.BaseType;
            while (current != null)
            {
                if (current.IsGenericType && current.GetGenericTypeDefinition() == typeof(DataBlock<>))
                    return current;

                current = current.BaseType;
            }

            return null;
        }
    }
}
