using System.IO;
using OdinSerializer;
using SketchEngine.Data;
using UnityEngine.Scripting;

[assembly: RegisterFormatter(typeof(SketchEngine.Utilities.CompactValueFormatter))]
[assembly: RegisterFormatter(typeof(SketchEngine.Utilities.CompactDataValueFormatter))]

namespace SketchEngine.Utilities
{
    /// <summary>Stores two primitive numbers. No display strings or reflection into readonly fields.</summary>
    [Preserve]
    public sealed class CompactValueFormatter : MinimalBaseFormatter<CompactValue>
    {
        protected override void Write(ref CompactValue value, IDataWriter writer)
        {
            writer.WriteDouble("mantissa", value.Mantissa);
            writer.WriteInt32("exponent", value.Exponent);
        }

        protected override void Read(ref CompactValue value, IDataReader reader)
        {
            if (!reader.ReadDouble(out double mantissa) || !reader.ReadInt32(out int exponent))
                throw new InvalidDataException("Incomplete CompactValue save.");
            value = new CompactValue(mantissa, exponent);
        }
    }

    /// <summary>Direct serialization of the currency wrapper, without emitting a generic field formatter.</summary>
    [Preserve]
    public sealed class CompactDataValueFormatter : MinimalBaseFormatter<DataValue<CompactValue>>
    {
        static readonly Serializer<CompactValue> ValueSerializer = Serializer.Get<CompactValue>();

        protected override void Write(ref DataValue<CompactValue> value, IDataWriter writer)
            => ValueSerializer.WriteValue("_value", value.Value, writer);

        protected override void Read(ref DataValue<CompactValue> value, IDataReader reader)
            => value.Value = ValueSerializer.ReadValue(reader);

        // Kept by the linker so IL2CPP sees the concrete serializer constructors.
        // This method is never called and adds no startup work.
        [Preserve]
        static void PreserveAotSerializers()
        {
            new ComplexTypeSerializer<CompactValue>();
            new ComplexTypeSerializer<DataValue<CompactValue>>();
        }
    }
}
