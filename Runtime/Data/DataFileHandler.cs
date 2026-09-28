using System;
using System.Collections.Generic;
using System.IO;
using SketchEngine.Diagnostics;
using UnityEngine;

namespace SketchEngine.Data
{
    public static class DataFileHandler
    {
        const string RootFolderName = "CodeSketch.Data";

        // Cache hash của lần ghi gần nhất theo từng file -> để bỏ qua ghi khi data không đổi.
        static readonly Dictionary<string, ulong> s_lastWrittenHash = new Dictionary<string, ulong>();

        static string GetDevicePath(string filePath)
        {
            return Path.Combine(Application.persistentDataPath, RootFolderName, filePath);
        }

        static string GetProjectPath(string filePath)
        {
            return Path.Combine(Application.dataPath, filePath);
        }

        static void Save<T>(T data, string filePath) where T : class
        {
            try
            {
                byte[] bytes = DataSerializer.Serialize<T>(data);
                if (bytes == null) return;

                // ---- SKIP-IF-UNCHANGED ----
                // Nếu nội dung y hệt lần ghi trước -> không đụng đĩa (tránh double-write lúc pause,
                // tránh ghi các block không thay đổi). Phần tốn kém là I/O đĩa, không phải serialize.
                ulong hash = ComputeHash(bytes);
                if (s_lastWrittenHash.TryGetValue(filePath, out ulong prev) && prev == hash)
                    return;

                // Create folder if needed
                {
                    string dir = Path.GetDirectoryName(filePath);
                    if (dir != null && !Directory.Exists(dir))
                        Directory.CreateDirectory(dir);
                }

                // ---- ATOMIC WRITE ----
                // Ghi ra file tạm rồi đổi tên đè lên file thật. Rename là thao tác nguyên tử của OS:
                // bị kill giữa chừng thì file thật vẫn nguyên vẹn (không bao giờ corrupt nửa vời).
                WriteAtomic(filePath, bytes);

                s_lastWrittenHash[filePath] = hash;
            }
            catch (Exception e)
            {
                CodeSketchDebug.Log(typeof(DataFileHandler), $"Save failed: {e}");
            }
        }

        static void WriteAtomic(string filePath, byte[] bytes)
        {
            string tmp = filePath + ".tmp";

            // 1) Ghi toàn bộ vào file tạm (file thật chưa bị động tới).
            File.WriteAllBytes(tmp, bytes);

            // 2) Đổi tên tmp -> file thật.
            if (File.Exists(filePath))
            {
                // File.Replace nguyên tử nhất, nhưng vài filesystem trên Android có thể kén
                // -> fallback delete + move.
                try
                {
                    File.Replace(tmp, filePath, null);
                }
                catch
                {
                    File.Delete(filePath);
                    File.Move(tmp, filePath);
                }
            }
            else
            {
                File.Move(tmp, filePath);
            }
        }

        // FNV-1a 64-bit: nhanh, đủ để phát hiện "data có đổi hay không".
        static ulong ComputeHash(byte[] bytes)
        {
            const ulong offset = 14695981039346656037UL;
            const ulong prime = 1099511628211UL;

            ulong hash = offset;
            for (int i = 0; i < bytes.Length; i++)
            {
                hash ^= bytes[i];
                hash *= prime;
            }
            // Trộn thêm length để chống va chạm khi cùng hash khác độ dài.
            hash ^= (ulong)bytes.Length;
            return hash;
        }

        static T Load<T>(string filePath) where T : class
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    CodeSketchDebug.Log(typeof(DataFileHandler),
                        $"Can't load, file {filePath} does not exist! creating new file");
                    return null;
                }

                byte[] bytes = File.ReadAllBytes(filePath);

                // Đồng bộ cache hash ngay khi load -> lần Save đầu tiên mà data y hệt lúc load
                // cũng được skip luôn (không ghi oan).
                s_lastWrittenHash[filePath] = ComputeHash(bytes);

                return DataSerializer.Deserialize<T>(bytes);
            }
            catch (Exception e)
            {
                CodeSketchDebug.Log(typeof(DataFileHandler), $"Load failed: {e}");
                return null;
            }
        }

        static void Delete(string filePath)
        {
            try
            {
                s_lastWrittenHash.Remove(filePath);

                if (!File.Exists(filePath))
                {
                    CodeSketchDebug.Log(typeof(DataFileHandler), $"Can't delete, file {filePath} does not exist!");
                    return;
                }

                File.Delete(filePath);
            }
            catch (Exception e)
            {
                CodeSketchDebug.Log(typeof(DataFileHandler), $"Delete failed: {e}");
            }
        }

        public static T Load<T>(TextAsset textAsset) where T : class
        {
            try
            {
                return DataSerializer.Deserialize<T>(textAsset.bytes);
            }
            catch (Exception e)
            {
                CodeSketchDebug.Log(typeof(DataFileHandler), $"Load failed: {e}");
                return null;
            }
        }

        public static void SaveToDevice<T>(T data, string filePath) where T : class
        {
            Save(data, GetDevicePath(filePath));
        }

        public static void SaveToProject<T>(T data, string filePath) where T : class
        {
            Save(data, GetProjectPath(filePath));
        }

        public static T LoadFromDevice<T>(string filePath) where T : class
        {
            return Load<T>(GetDevicePath(filePath));
        }

        public static T LoadFromProject<T>(string filePath) where T : class
        {
            return Load<T>(GetProjectPath(filePath));
        }

        public static void DeleteInDevice(string filePath)
        {
            Delete(GetDevicePath(filePath));
        }

        public static void DeleteInProject(string filePath)
        {
            Delete(GetProjectPath(filePath));
        }

        public static void DeleteAllInDevice()
        {
            string path = Path.Combine(Application.persistentDataPath, RootFolderName);

            var info = new DirectoryInfo(path);
            if (!info.Exists)
                return;

            var files = info.GetFiles();
            for (int i = 0; i < files.Length; i++)
            {
                files[i].Delete();
            }

            s_lastWrittenHash.Clear();
        }
    }
}