using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using OdinSerializer;

namespace SketchEngine.Data
{
    public static class DataSerializer
    {
        // Header 4 byte đánh dấu save ở dạng Odin, để phân biệt với save BinaryFormatter cũ khi đọc.
        static readonly byte[] s_odinMagic = { (byte)'S', (byte)'K', (byte)'O', (byte)'1' };

        // Chỉ dùng để đọc save cũ (migrate). Không thread-safe nên chỉ gọi từ main thread.
        static readonly BinaryFormatter s_legacyFormatter = new BinaryFormatter();

        public static byte[] Serialize<T>(T data) where T : class
        {
            if (data == null) return null;

            byte[] payload = SerializationUtility.SerializeValue(data, DataFormat.Binary);

            byte[] result = new byte[s_odinMagic.Length + payload.Length];
            Buffer.BlockCopy(s_odinMagic, 0, result, 0, s_odinMagic.Length);
            Buffer.BlockCopy(payload, 0, result, s_odinMagic.Length, payload.Length);
            return result;
        }

        public static T Deserialize<T>(byte[] byteArray)
        {
            if (byteArray == null) return default;

            if (HasOdinMagic(byteArray))
            {
                byte[] payload = new byte[byteArray.Length - s_odinMagic.Length];
                Buffer.BlockCopy(byteArray, s_odinMagic.Length, payload, 0, payload.Length);
                return SerializationUtility.DeserializeValue<T>(payload, DataFormat.Binary);
            }

            // Save cũ ghi bằng BinaryFormatter: đọc để không mất dữ liệu; lần Save kế tiếp sẽ ghi lại dạng Odin.
            using var stream = new MemoryStream(byteArray, writable: false);
            return (T)s_legacyFormatter.Deserialize(stream);
        }

        static bool HasOdinMagic(byte[] bytes)
        {
            if (bytes.Length < s_odinMagic.Length) return false;

            for (int i = 0; i < s_odinMagic.Length; i++)
            {
                if (bytes[i] != s_odinMagic[i]) return false;
            }

            return true;
        }
    }
}
