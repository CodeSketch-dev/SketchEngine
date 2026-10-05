using System;
using System.Runtime.CompilerServices;

namespace SketchEngine.Utilities.Utils
{
    /// <summary>
    /// RNG cho gameplay, mutable, không cấp phát (allocation-free), dùng thuật toán PCG-XSH-RR 64/32
    /// (state 16 byte). Phải seed rõ ràng trước khi dùng; default(struct) sẽ cho 2 kết quả đầu bằng 0.
    /// Giữ nó trong field không-readonly và truyền bằng ref: copy struct này là copy luôn cả chuỗi số
    /// ngẫu nhiên. Mỗi owner/thread nên dùng 1 instance riêng; không phải RNG mật mã học. Các hàm random
    /// theo khoảng số nguyên là unbiased (không lệch) và chặn trên không bao gồm (upper-exclusive).
    /// </summary>
    [Serializable]
    public struct RNG
    {
        // Thuật toán PCG: https://www.pcg-random.org/ (M. E. O'Neill).
        const ulong Multiplier = 6364136223846793005UL;
        ulong _state;
        ulong _increment;

        /// <param name="stream">Bộ chọn chuỗi số độc lập; chỉ 63 bit thấp được dùng.</param>
        public RNG(ulong seed, ulong stream = 54UL)
        {
            _state = 0;
            _increment = unchecked((stream << 1) | 1UL);
            NextUInt();
            _state = unchecked(_state + seed);
            NextUInt();
        }
        public void SetSeed(ulong seed, ulong stream = 54UL) => this = new RNG(seed, stream);

        /// <summary>Chỉ cần 2 số uint này là đủ lưu lại chính xác lần quay kế tiếp. Nếu nơi lưu không giữ được UInt64, hãy lưu dưới dạng hex/string.</summary>
        public void GetState(out ulong state, out ulong increment)
        {
            state = _state;
            increment = _increment | 1UL;
        }
        public void RestoreState(ulong state, ulong increment)
        {
            if ((increment & 1UL) == 0) throw new ArgumentException("PCG increment must be odd.", nameof(increment));
            _state = state;
            _increment = increment;
        }
        /// <summary>Nhảy trước delta lần quay 32-bit nguyên thủy với độ phức tạp O(log delta), không phải bằng cách gọi API cấp cao delta lần.</summary>
        public void Advance(ulong delta)
        {
            unchecked
            {
                ulong currentMultiplier = Multiplier, currentIncrement = _increment | 1UL;
                ulong accumulatedMultiplier = 1, accumulatedIncrement = 0;
                while (delta > 0)
                {
                    if ((delta & 1UL) != 0)
                    {
                        accumulatedMultiplier *= currentMultiplier;
                        accumulatedIncrement = accumulatedIncrement * currentMultiplier + currentIncrement;
                    }
                    currentIncrement *= currentMultiplier + 1;
                    currentMultiplier *= currentMultiplier;
                    delta >>= 1;
                }
                _state = accumulatedMultiplier * _state + accumulatedIncrement;
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint NextUInt()
        {
            unchecked
            {
                ulong old = _state;
                _state = old * Multiplier + (_increment | 1UL);
                uint output = (uint)(((old >> 18) ^ old) >> 27);
                int rotation = (int)(old >> 59);
                return (output >> rotation) | (output << ((-rotation) & 31));
            }
        }
        /// <summary>Phân bố đều trong [0, exclusiveMax), dùng kỹ thuật multiply-and-reject để tránh lệch do modulo.</summary>
        public uint NextUInt(uint exclusiveMax)
        {
            if (exclusiveMax == 0) throw new ArgumentOutOfRangeException(nameof(exclusiveMax));
            ulong product = (ulong)NextUInt() * exclusiveMax;
            uint low = unchecked((uint)product);
            if (low < exclusiveMax)
            {
                uint threshold = unchecked(0u - exclusiveMax) % exclusiveMax;
                while (low < threshold)
                {
                    product = (ulong)NextUInt() * exclusiveMax;
                    low = unchecked((uint)product);
                }
            }
            return (uint)(product >> 32);
        }
        public uint NextUInt(uint inclusiveMin, uint exclusiveMax)
        {
            if (inclusiveMin > exclusiveMax) throw new ArgumentOutOfRangeException(nameof(exclusiveMax));
            return inclusiveMin == exclusiveMax ? inclusiveMin : inclusiveMin + NextUInt(exclusiveMax - inclusiveMin);
        }
        public int NextInt(int exclusiveMax)
        {
            if (exclusiveMax <= 0) throw new ArgumentOutOfRangeException(nameof(exclusiveMax));
            return (int)NextUInt((uint)exclusiveMax);
        }
        public int NextInt(int inclusiveMin, int exclusiveMax)
        {
            if (inclusiveMin > exclusiveMax) throw new ArgumentOutOfRangeException(nameof(exclusiveMax));
            if (inclusiveMin == exclusiveMax) return inclusiveMin;
            uint width = (uint)((long)exclusiveMax - inclusiveMin);
            return (int)(inclusiveMin + (long)NextUInt(width));
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ulong NextULong() => ((ulong)NextUInt() << 32) | NextUInt();
        public ulong NextULong(ulong exclusiveMax)
        {
            if (exclusiveMax == 0) throw new ArgumentOutOfRangeException(nameof(exclusiveMax));
            ulong threshold = unchecked(0UL - exclusiveMax) % exclusiveMax;
            ulong value;
            do { value = NextULong(); } while (value < threshold);
            return value % exclusiveMax;
        }
        public ulong NextULong(ulong inclusiveMin, ulong exclusiveMax)
        {
            if (inclusiveMin > exclusiveMax) throw new ArgumentOutOfRangeException(nameof(exclusiveMax));
            return inclusiveMin == exclusiveMax ? inclusiveMin : inclusiveMin + NextULong(exclusiveMax - inclusiveMin);
        }
        public long NextLong(long exclusiveMax)
        {
            if (exclusiveMax <= 0) throw new ArgumentOutOfRangeException(nameof(exclusiveMax));
            return (long)NextULong((ulong)exclusiveMax);
        }
        public long NextLong(long inclusiveMin, long exclusiveMax)
        {
            if (inclusiveMin > exclusiveMax) throw new ArgumentOutOfRangeException(nameof(exclusiveMax));
            if (inclusiveMin == exclusiveMax) return inclusiveMin;
            unchecked { return (long)((ulong)inclusiveMin + NextULong((ulong)exclusiveMax - (ulong)inclusiveMin)); }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float NextFloat() => (NextUInt() >> 8) * (1f / 16777216f);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double NextDouble() => (NextULong() >> 11) * (1d / 9007199254740992d);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool NextBool() => (NextUInt() & 1u) != 0;
        /// <summary>Xác suất trong [0,1]. Hai đầu mút và khoảng giá trị bằng nhau sẽ không tiêu tốn một lần quay nào.</summary>
        public bool Chance(double probability)
        {
            if (!(probability >= 0 && probability <= 1)) throw new ArgumentOutOfRangeException(nameof(probability));
            return probability == 1 || (probability != 0 && NextDouble() < probability);
        }
        /// <summary>Độ phân giải xác suất 24-bit, nhanh hơn vì chỉ dùng 1 lần quay nguyên thủy thay vì 2.</summary>
        public bool Chance(float probability)
        {
            if (!(probability >= 0 && probability <= 1)) throw new ArgumentOutOfRangeException(nameof(probability));
            return probability == 1 || (probability != 0 && NextFloat() < probability);
        }
        public bool ChancePercent(float percent)
        {
            if (!(percent >= 0 && percent <= 100)) throw new ArgumentOutOfRangeException(nameof(percent));
            return Chance(percent * 0.01f);
        }
        public bool ChancePercent(double percent)
        {
            if (!(percent >= 0 && percent <= 100)) throw new ArgumentOutOfRangeException(nameof(percent));
            return Chance(percent * 0.01);
        }
        /// <summary>Khoảng [min,max) hữu hạn; được kẹp để tránh làm tròn float vượt ra ngoài chặn trên bị loại trừ.</summary>
        public float NextFloat(float inclusiveMin, float exclusiveMax)
        {
            if (!Finite(inclusiveMin) || !Finite(exclusiveMax) || inclusiveMin > exclusiveMax)
                throw new ArgumentOutOfRangeException(nameof(exclusiveMax));
            if (inclusiveMin == exclusiveMax) return inclusiveMin;
            float value = (float)(inclusiveMin + ((double)exclusiveMax - inclusiveMin) * NextFloat());
            return value >= exclusiveMax ? Previous(exclusiveMax) : value < inclusiveMin ? inclusiveMin : value;
        }
        public double NextDouble(double inclusiveMin, double exclusiveMax)
        {
            if (!Finite(inclusiveMin) || !Finite(exclusiveMax) || inclusiveMin > exclusiveMax)
                throw new ArgumentOutOfRangeException(nameof(exclusiveMax));
            if (inclusiveMin == exclusiveMax) return inclusiveMin;
            double sample = NextDouble(), width = exclusiveMax - inclusiveMin;
            double value = double.IsInfinity(width)
                ? inclusiveMin * (1 - sample) + exclusiveMax * sample
                : inclusiveMin + width * sample;
            return value >= exclusiveMax ? Previous(exclusiveMax) : value < inclusiveMin ? inclusiveMin : value;
        }
        static bool Finite(double value) => !double.IsNaN(value) && !double.IsInfinity(value);
        static float Previous(float value)
        {
            if (value == 0) return -float.Epsilon;
            int bits = BitConverter.SingleToInt32Bits(value);
            return BitConverter.Int32BitsToSingle(value > 0 ? bits - 1 : bits + 1);
        }
        static double Previous(double value)
        {
            if (value == 0) return -double.Epsilon;
            long bits = BitConverter.DoubleToInt64Bits(value);
            return BitConverter.Int64BitsToDouble(value > 0 ? bits - 1 : bits + 1);
        }
        /// <summary>Chọn ngẫu nhiên một phần tử với phân bố đều. Mảng có thể truyền trực tiếp dưới dạng span.</summary>
        public T Choose<T>(ReadOnlySpan<T> items)
        {
            if (items.IsEmpty) throw new ArgumentException("At least one item is required.", nameof(items));
            return items[NextInt(items.Length)];
        }
        public T Choose<T>(T[] items)
        {
            if (items == null) throw new ArgumentNullException(nameof(items));
            return Choose((ReadOnlySpan<T>)items);
        }
        /// <summary>Xáo trộn Fisher-Yates ngay tại chỗ (in-place); không cần mảng tạm.</summary>
        public void Shuffle<T>(Span<T> items)
        {
            for (int i = items.Length - 1; i > 0; i--)
            {
                int index = NextInt(i + 1);
                T temp = items[i]; items[i] = items[index]; items[index] = temp;
            }
        }
        public void Shuffle<T>(T[] items)
        {
            if (items == null) throw new ArgumentNullException(nameof(items));
            Shuffle(items.AsSpan());
        }
        /// <summary>Chọn theo trọng số kiểu dùng một lần, O(n). Nếu cần quay lặp lại nhiều lần với cùng bộ trọng số cố định, hãy dùng RngAliasTable.</summary>
        public int WeightedIndex(ReadOnlySpan<double> weights)
        {
            double maximum = RngAliasTable.ValidateWeights(weights);
            double total = 0;
            for (int i = 0; i < weights.Length; i++) total += weights[i] / maximum;
            double sample = NextDouble() * total;
            double cumulative = 0;
            int lastPositive = 0;
            for (int i = 0; i < weights.Length; i++)
            {
                if (weights[i] > 0) lastPositive = i;
                cumulative += weights[i] / maximum;
                if (sample < cumulative) return i;
            }
            return lastPositive;
        }
        /// <summary>Phân phối chuẩn (normal) dùng Box-Muller; không cache state, không cấp phát. Tốn chi phí hơn các lần quay đều (uniform).</summary>
        public double NextNormal(double mean = 0, double standardDeviation = 1)
        {
            if (!Finite(mean) || !Finite(standardDeviation) || standardDeviation < 0)
                throw new ArgumentOutOfRangeException(nameof(standardDeviation));
            if (standardDeviation == 0) return mean;
            double radius = Math.Sqrt(-2 * Math.Log(1 - NextDouble()));
            return mean + standardDeviation * radius * Math.Cos(2 * Math.PI * NextDouble());
        }
        /// <summary>Phân phối mũ (exponential) với tốc độ lambda hữu hạn, dương.</summary>
        public double NextExponential(double lambda = 1)
        {
            if (!Finite(lambda) || lambda <= 0) throw new ArgumentOutOfRangeException(nameof(lambda));
            return -Math.Log(1 - NextDouble()) / lambda;
        }
        /// <summary>Điền dữ liệu vào buffer do caller cấp. Lần quay cuối (nếu lẻ) vẫn tiêu tốn trọn một UInt32.</summary>
        public void FillBytes(Span<byte> destination)
        {
            int i = 0;
            while (i <= destination.Length - 4)
            {
                uint value = NextUInt();
                destination[i++] = unchecked((byte)value);
                destination[i++] = unchecked((byte)(value >> 8));
                destination[i++] = unchecked((byte)(value >> 16));
                destination[i++] = unchecked((byte)(value >> 24));
            }
            if (i < destination.Length)
            {
                uint value = NextUInt();
                while (i < destination.Length) { destination[i++] = unchecked((byte)value); value >>= 8; }
            }
        }
    }

    /// <summary>
    /// Cho phép quay theo trọng số lặp lại nhiều lần với độ phức tạp O(1), đổi lại tiền xử lý O(n).
    /// Struct này mượn các mảng do caller cấp phát. Chỉ cấp phát buffer một lần, sau đó giữ private
    /// và không thay đổi cho tới khi build lại. Không cấp phát managed memory trong Build/NextIndex.
    /// </summary>
    public readonly struct RngAliasTable
    {
        readonly double[] _probabilities;
        readonly int[] _aliases;
        public int Count { get; }
        RngAliasTable(double[] probabilities, int[] aliases, int count)
        { _probabilities = probabilities; _aliases = aliases; Count = count; }

        /// <summary>Cả 3 buffer phải có ít nhất weights.Length phần tử; scratch có thể tái sử dụng sau khi build xong.</summary>
        public static RngAliasTable Build(ReadOnlySpan<double> weights, double[] probabilities, int[] aliases, int[] scratch)
        {
            if (probabilities == null) throw new ArgumentNullException(nameof(probabilities));
            if (aliases == null) throw new ArgumentNullException(nameof(aliases));
            if (scratch == null) throw new ArgumentNullException(nameof(scratch));
            int count = weights.Length;
            if (probabilities.Length < count || aliases.Length < count || scratch.Length < count)
                throw new ArgumentException("Buffers must have at least weights.Length elements.");
            if (weights.Overlaps(probabilities.AsSpan()) || ReferenceEquals(aliases, scratch))
                throw new ArgumentException("Weights and output/scratch storage must not overlap.");
            double maximum = ValidateWeights(weights), total = 0;
            for (int i = 0; i < count; i++) total += weights[i] / maximum;
            double scale = count / total;
            int small = 0, large = count;
            for (int i = 0; i < count; i++)
            {
                probabilities[i] = (weights[i] / maximum) * scale;
                if (probabilities[i] < 1) scratch[small++] = i;
                else scratch[--large] = i;
            }
            while (small > 0 && large < count)
            {
                int low = scratch[--small], high = scratch[large++];
                aliases[low] = high;
                probabilities[high] = (probabilities[high] - 1) + probabilities[low];
                if (probabilities[high] < 1) scratch[small++] = high;
                else scratch[--large] = high;
            }
            while (large < count) { int i = scratch[large++]; probabilities[i] = 1; aliases[i] = i; }
            while (small > 0) { int i = scratch[--small]; probabilities[i] = 1; aliases[i] = i; }
            return new RngAliasTable(probabilities, aliases, count);
        }
        internal static double ValidateWeights(ReadOnlySpan<double> weights)
        {
            double maximum = 0;
            foreach (double weight in weights)
            {
                if (weight < 0 || double.IsNaN(weight) || double.IsInfinity(weight))
                    throw new ArgumentOutOfRangeException(nameof(weights), "Weights must be finite and nonnegative.");
                if (weight > maximum) maximum = weight;
            }
            if (maximum == 0) throw new ArgumentException("At least one positive weight is required.", nameof(weights));
            return maximum;
        }
        public int NextIndex(ref RNG rng)
        {
            if (Count == 0) throw new InvalidOperationException("Build the table before sampling.");
            int column = rng.NextInt(Count);
            return rng.NextDouble() < _probabilities[column] ? column : _aliases[column];
        }
    }
}
