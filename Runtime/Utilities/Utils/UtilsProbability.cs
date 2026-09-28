using System;

namespace SketchEngine.Utilities.Utils
{
    public static class UtilsProbability
    {
        static readonly Random _rnd = new Random();

        /// <summary>
        /// Trả về index ngẫu nhiên dựa trên mảng xác suất `double[]`, đảm bảo chính xác cho tỉ lệ cực nhỏ.
        /// Tổng các xác suất không cần bằng 1.
        /// </summary>
        public static int RandomWithProbability(double[] probs)
        {
            double total = 0;
            foreach (double p in probs)
                total += p;

            if (total <= 0)
                return 0; // fallback nếu không có tỉ lệ hợp lệ

            double r = _rnd.NextDouble() * total;
            double cumulative = 0;

            for (int i = 0; i < probs.Length; i++)
            {
                cumulative += probs[i];
                if (r < cumulative)
                    return i;
            }

            // fallback an toàn nếu cộng dồn bị lệch cực nhỏ
            return probs.Length - 1;
        }

        /// <summary>Trả về index ngẫu nhiên dựa trên mảng xác suất.</summary>
        public static int RandomWithProbably(float[] probs)
        {
            float total = 0f;
            foreach (var p in probs)
                total += p;

            if (total <= 0f) return 0; // tránh chia cho 0

            float r = RandomFloat(0f, total);
            float cumulative = 0f;

            for (int i = 0; i < probs.Length; i++)
            {
                cumulative += probs[i];
                if (r < cumulative)
                    return i;
            }

            // fallback an toàn nếu float cộng dồn bị lệch
            return probs.Length - 1;
        }
        
        /// <summary>Xác suất xảy ra theo phần trăm (0-100).</summary>
        public static bool IsOccurrence(float probabilityPercent)
        {
            return RandomFloat(0, 100) < probabilityPercent;
        }

        /// <summary>Trả về số thực ngẫu nhiên trong khoảng [from, to).</summary>
        public static float RandomFloat(float from, float to)
        {
            return UnityEngine.Random.Range(from, to);
        }
        
        /// <summary>Trả về số nguyên ngẫu nhiên trong khoảng [from, to).</summary>
        public static int RandomInt(int from, int to)
        {
            return _rnd.Next(from, to);
        }
        /// <summary>Trả về bool ngẫu nhiên.</summary>
        public static bool RandomBool() => UnityEngine.Random.value > 0.5f;
    }
}
