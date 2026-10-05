using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using SketchEngine.Diagnostics;
using UnityEngine;

namespace SketchEngine.Data
{
    public static class DataFileHandler
    {
        const string RootFolderName = "SketchEngine.Data";
        const int FlushTimeoutMs = 2000;

        // Hash nội dung đã đưa vào hàng đợi (hoặc đã ghi) theo file -> bỏ qua khi data không đổi.
        // Chỉ truy cập từ main thread.
        static readonly Dictionary<string, ulong> s_enqueuedHash = new Dictionary<string, ulong>();

        // Phần dùng chung với worker thread, luôn truy cập dưới s_lock.
        static readonly object s_lock = new object();
        static readonly Dictionary<string, byte[]> s_pending = new Dictionary<string, byte[]>();
        static readonly List<(string path, string error)> s_failed = new List<(string, string)>();
        static Thread s_worker;
        static bool s_busy;

        static string GetDevicePath(string filePath)
        {
            return Path.Combine(Application.persistentDataPath, RootFolderName, filePath);
        }

        static string GetProjectPath(string filePath)
        {
            return Path.Combine(Application.dataPath, filePath);
        }

        // ---- SAVE (main thread: serialize + hash; worker: ghi đĩa) ----

        static void Save<T>(T data, string filePath) where T : class
        {
            try
            {
                DrainFailures();

                byte[] bytes = DataSerializer.Serialize<T>(data);
                if (bytes == null) return;

                ulong hash = ComputeHash(bytes);
                if (s_enqueuedHash.TryGetValue(filePath, out ulong prev) && prev == hash)
                    return;

                s_enqueuedHash[filePath] = hash;

                lock (s_lock)
                {
                    // Ghi đè bản pending cũ: chỉ bản mới nhất được ghi xuống đĩa.
                    s_pending[filePath] = bytes;
                    EnsureWorker();
                    Monitor.PulseAll(s_lock);
                }
            }
            catch (Exception e)
            {
                SketchDebug.Log(typeof(DataFileHandler), $"Save failed: {e}");
            }
        }

        // Chờ toàn bộ ghi đang chờ/đang chạy xong (tối đa FlushTimeoutMs). Dùng lúc pause/quit để đảm bảo đã ghi xuống đĩa.
        public static void Flush()
        {
            lock (s_lock)
            {
                if (s_worker != null)
                {
                    int start = Environment.TickCount;
                    while (s_pending.Count > 0 || s_busy)
                    {
                        int remaining = FlushTimeoutMs - (Environment.TickCount - start);
                        if (remaining <= 0)
                        {
                            SketchDebug.LogWarning(typeof(DataFileHandler), "Flush timed out, some data may not be written yet");
                            break;
                        }

                        Monitor.Wait(s_lock, remaining);
                    }
                }
            }

            DrainFailures();
        }

        static void EnsureWorker()
        {
            if (s_worker != null) return;

            s_worker = new Thread(WorkerLoop) { IsBackground = true, Name = "SketchEngine.DataWriter" };
            s_worker.Start();
        }

        static void WorkerLoop()
        {
            var batch = new List<KeyValuePair<string, byte[]>>();

            while (true)
            {
                lock (s_lock)
                {
                    while (s_pending.Count == 0)
                        Monitor.Wait(s_lock);

                    foreach (var kv in s_pending)
                        batch.Add(kv);

                    s_pending.Clear();
                    s_busy = true;
                }

                for (int i = 0; i < batch.Count; i++)
                {
                    try
                    {
                        WriteAtomic(batch[i].Key, batch[i].Value);
                    }
                    catch (Exception e)
                    {
                        lock (s_lock) s_failed.Add((batch[i].Key, e.Message));
                    }
                }

                batch.Clear();

                lock (s_lock)
                {
                    s_busy = false;
                    Monitor.PulseAll(s_lock);
                }
            }
        }

        // Lỗi ghi từ worker được đẩy về main thread để log và cho phép lần Save sau thử lại.
        static void DrainFailures()
        {
            List<(string path, string error)> failed = null;

            lock (s_lock)
            {
                if (s_failed.Count == 0) return;

                failed = new List<(string, string)>(s_failed);
                s_failed.Clear();
            }

            for (int i = 0; i < failed.Count; i++)
            {
                s_enqueuedHash.Remove(failed[i].path);
                SketchDebug.Log(typeof(DataFileHandler), $"Save failed: {failed[i].path}: {failed[i].error}");
            }
        }

        // Ghi an toàn: tmp -> (cũ thành .bak) -> tmp thành file thật. Nếu lỗi giữa chừng thì khôi phục từ .bak.
        static void WriteAtomic(string filePath, byte[] bytes)
        {
            string dir = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            string tmp = filePath + ".tmp";
            string bak = filePath + ".bak";

            using (var stream = new FileStream(tmp, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                stream.Write(bytes, 0, bytes.Length);
                stream.Flush();
            }

            if (File.Exists(filePath))
            {
                if (File.Exists(bak)) File.Delete(bak);
                File.Move(filePath, bak);

                try
                {
                    File.Move(tmp, filePath);
                }
                catch
                {
                    File.Move(bak, filePath);
                    throw;
                }

                File.Delete(bak);
            }
            else
            {
                File.Move(tmp, filePath);
            }
        }

        // Nếu app bị kill giữa bước đổi tên thì file thật có thể đang thiếu, nhưng .bak vẫn còn -> khôi phục.
        static void RecoverIfNeeded(string filePath)
        {
            string bak = filePath + ".bak";
            if (!File.Exists(filePath) && File.Exists(bak))
                File.Move(bak, filePath);
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

            hash ^= (ulong)bytes.Length;
            return hash;
        }

        // ---- LOAD / DELETE (luôn Flush trước để không đọc/xóa khi worker đang ghi) ----

        static T Load<T>(string filePath) where T : class
        {
            try
            {
                Flush();
                RecoverIfNeeded(filePath);

                if (!File.Exists(filePath))
                {
                    SketchDebug.Log(typeof(DataFileHandler),
                        $"Can't load, file {filePath} does not exist! creating new file");
                    return null;
                }

                byte[] bytes = File.ReadAllBytes(filePath);

                // Đồng bộ hash ngay khi load -> Save đầu tiên mà data y hệt lúc load sẽ được bỏ qua.
                s_enqueuedHash[filePath] = ComputeHash(bytes);

                return DataSerializer.Deserialize<T>(bytes);
            }
            catch (Exception e)
            {
                SketchDebug.Log(typeof(DataFileHandler), $"Load failed: {e}");
                return null;
            }
        }

        static void Delete(string filePath)
        {
            try
            {
                Flush();
                s_enqueuedHash.Remove(filePath);

                DeleteIfExists(filePath);
                DeleteIfExists(filePath + ".tmp");
                DeleteIfExists(filePath + ".bak");
            }
            catch (Exception e)
            {
                SketchDebug.Log(typeof(DataFileHandler), $"Delete failed: {e}");
            }
        }

        static void DeleteIfExists(string path)
        {
            if (File.Exists(path)) File.Delete(path);
        }

        public static T Load<T>(TextAsset textAsset) where T : class
        {
            try
            {
                return DataSerializer.Deserialize<T>(textAsset.bytes);
            }
            catch (Exception e)
            {
                SketchDebug.Log(typeof(DataFileHandler), $"Load failed: {e}");
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
            try
            {
                Flush();

                string path = Path.Combine(Application.persistentDataPath, RootFolderName);
                var info = new DirectoryInfo(path);
                if (!info.Exists) return;

                FileInfo[] files = info.GetFiles("*", SearchOption.AllDirectories);
                for (int i = 0; i < files.Length; i++)
                    files[i].Delete();

                s_enqueuedHash.Clear();
            }
            catch (Exception e)
            {
                SketchDebug.Log(typeof(DataFileHandler), $"Delete all failed: {e}");
            }
        }
    }
}
