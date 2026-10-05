using System;
using System.Runtime.CompilerServices;
using SketchEngine.Core;

namespace SketchEngine.Utilities.Utils
{
    /// <summary>
    /// Allocation-free probability helpers backed by one deterministic PRNG.
    /// The shared generator is intended for Unity's main thread.
    /// </summary>
    public static class UtilityProbability
    {
        // PCG-XSH-RR 64/32: compact, fast and statistically robust for gameplay.
        const ulong Multiplier = 6364136223846793005UL;
        const ulong DefaultStream = 1442695040888963407UL;

        static ulong _state = 0x853C49E6748FEA9BUL;
        static ulong _increment = DefaultStream;

        /// <summary>Resets the shared generator to a reproducible sequence.</summary>
        public static void SetSeed(ulong seed, ulong stream = DefaultStream)
        {
            _state = 0UL;
            _increment = (stream << 1) | 1UL;
            NextUInt();
            _state = unchecked(_state + seed);
            NextUInt();
        }

        /// <summary>
        /// Selects an index using finite, non-negative weights. Weights do not need
        /// to add up to one. No allocation is performed for valid input.
        /// </summary>
        public static int RandomWithProbability(double[] weights)
        {
            if (weights == null)
                throw new ArgumentNullException(nameof(weights));

            return RandomWithProbability((ReadOnlySpan<double>)weights);
        }

        /// <inheritdoc cref="RandomWithProbability(double[])"/>
        public static int RandomWithProbability(ReadOnlySpan<double> weights)
        {
            if (weights.IsEmpty)
                throw new ArgumentException("At least one weight is required.", nameof(weights));

            double total = 0d;
            int lastPositiveIndex = -1;

            for (int i = 0; i < weights.Length; i++)
            {
                double weight = weights[i];
                if (weight < 0d || double.IsNaN(weight) || double.IsInfinity(weight))
                    throw new ArgumentOutOfRangeException(nameof(weights), "Weights must be finite and non-negative.");

                if (weight > 0d)
                    lastPositiveIndex = i;

                total += weight;
            }

            if (lastPositiveIndex < 0 || double.IsInfinity(total))
                throw new ArgumentException("Weights must have a finite, positive total.", nameof(weights));

            double sample = NextDouble() * total;
            double cumulative = 0d;

            for (int i = 0; i < weights.Length; i++)
            {
                cumulative += weights[i];
                if (sample < cumulative)
                    return i;
            }

            // Only reachable through floating-point accumulation error.
            return lastPositiveIndex;
        }

        /// <summary>Float overload of <see cref="RandomWithProbability(double[])"/>.</summary>
        public static int RandomWithProbability(float[] weights)
        {
            if (weights == null)
                throw new ArgumentNullException(nameof(weights));

            return RandomWithProbability((ReadOnlySpan<float>)weights);
        }

        /// <summary>Allocation-free float overload.</summary>
        public static int RandomWithProbability(ReadOnlySpan<float> weights)
        {
            if (weights.IsEmpty)
                throw new ArgumentException("At least one weight is required.", nameof(weights));

            // Accumulate floats as doubles to reduce drift for large collections.
            double total = 0d;
            int lastPositiveIndex = -1;

            for (int i = 0; i < weights.Length; i++)
            {
                float weight = weights[i];
                if (weight < 0f || float.IsNaN(weight) || float.IsInfinity(weight))
                    throw new ArgumentOutOfRangeException(nameof(weights), "Weights must be finite and non-negative.");

                if (weight > 0f)
                    lastPositiveIndex = i;

                total += weight;
            }

            if (lastPositiveIndex < 0 || double.IsInfinity(total))
                throw new ArgumentException("Weights must have a finite, positive total.", nameof(weights));

            double sample = NextDouble() * total;
            double cumulative = 0d;

            for (int i = 0; i < weights.Length; i++)
            {
                cumulative += weights[i];
                if (sample < cumulative)
                    return i;
            }

            return lastPositiveIndex;
        }

        [Obsolete("Use RandomWithProbability(float[]) instead.")]
        public static int RandomWithProbably(float[] weights) => RandomWithProbability(weights);

        /// <summary>Returns true with the supplied probability in the inclusive 0-100 range.</summary>
        public static bool IsOccurrence(float probabilityPercent)
        {
            if (probabilityPercent < 0f || probabilityPercent > 100f || float.IsNaN(probabilityPercent))
                throw new ArgumentOutOfRangeException(nameof(probabilityPercent), "Probability must be between 0 and 100.");

            return NextFloat01() * 100f < probabilityPercent;
        }

        /// <summary>Returns a random float in [from, to).</summary>
        public static float RandomFloat(float from, float to)
        {
            if (!(from < to) || float.IsNaN(from) || float.IsNaN(to) || float.IsInfinity(from) || float.IsInfinity(to))
                throw new ArgumentOutOfRangeException(nameof(to), "Bounds must be finite and from must be less than to.");

            // Interpolate as double so extreme finite bounds cannot overflow while
            // calculating their distance in float precision.
            return (float)(from + (((double)to - from) * NextFloat01()));
        }

        /// <summary>Returns a random integer in [from, to) without modulo bias.</summary>
        public static int RandomInt(int from, int to)
        {
            if (from >= to)
                throw new ArgumentOutOfRangeException(nameof(to), "to must be greater than from.");

            uint range = (uint)((long)to - from);
            uint threshold = unchecked(0u - range) % range;
            uint value;

            do
            {
                value = NextUInt();
            }
            while (value < threshold);

            return (int)(from + (long)(value % range));
        }

        /// <summary>Returns an unbiased random boolean.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool RandomBool() => (NextUInt() & 1u) != 0u;

        /// <summary>Returns an unbiased random 2D direction.</summary>
        public static Direction2D RandomDirection2D() => RandomBool() ? Direction2D.Left : Direction2D.Right;

        /// <summary>Returns an unbiased random 4D direction.</summary>
        public static Direction4D RandomDirection4D() => RandomInt(0, 4) switch
        {
            0 => Direction4D.Left,
            1 => Direction4D.Right,
            2 => Direction4D.Forward,
            _ => Direction4D.Backward,
        };

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static uint NextUInt()
        {
            ulong oldState = _state;
            _state = unchecked((oldState * Multiplier) + _increment);
            uint xorShifted = (uint)(((oldState >> 18) ^ oldState) >> 27);
            int rotation = (int)(oldState >> 59);
            return (xorShifted >> rotation) | (xorShifted << ((-rotation) & 31));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static float NextFloat01() => (NextUInt() >> 8) * (1f / 16777216f);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static double NextDouble()
        {
            ulong bits = ((ulong)(NextUInt() >> 5) << 26) | (NextUInt() >> 6);
            return bits * (1d / 9007199254740992d);
        }
    }
}
