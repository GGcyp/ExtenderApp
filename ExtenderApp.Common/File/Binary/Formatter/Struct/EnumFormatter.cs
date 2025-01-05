using System.Runtime.CompilerServices;
using ExtenderApp.Data;

namespace ExtenderApp.Common.File.Binary.Formatter
{
    internal class EnumFormatter<T> : ExtenderFormatter<T> where T : struct, Enum
    {
        private delegate void EnumSerialize(ref ExtenderBinaryWriter writer, ref T value);

        private delegate T EnumDeserialize(ref ExtenderBinaryReader reader);

        private readonly EnumSerialize serializer;
        private readonly EnumDeserialize deserializer;

        public EnumFormatter(ExtenderBinaryWriterConvert binaryWriterConvert, ExtenderBinaryReaderConvert binaryReaderConvert, BinaryOptions options) : base(binaryWriterConvert, binaryReaderConvert, options)
        {
            var type = typeof(T).GetEnumUnderlyingType();

            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Byte:
                    serializer = (ref ExtenderBinaryWriter writer, ref T value) => _binaryWriterConvert.Write(ref writer, Unsafe.As<T, Byte>(ref value));
                    deserializer = (ref ExtenderBinaryReader reader) => { var v = _binaryReaderConvert.ReadByte(ref reader); return Unsafe.As<Byte, T>(ref v); };
                    break;
                case TypeCode.Int16:
                    serializer = (ref ExtenderBinaryWriter writer, ref T value) => _binaryWriterConvert.Write(ref writer, Unsafe.As<T, Int64>(ref value));
                    deserializer = (ref ExtenderBinaryReader reader) => { var v = _binaryReaderConvert.ReadInt16(ref reader); return Unsafe.As<Int16, T>(ref v); };
                    break;
                case TypeCode.Int32:
                    serializer = (ref ExtenderBinaryWriter writer, ref T value) => _binaryWriterConvert.Write(ref writer, Unsafe.As<T, Int32>(ref value));
                    deserializer = (ref ExtenderBinaryReader reader) => { var v = _binaryReaderConvert.ReadInt32(ref reader); return Unsafe.As<Int32, T>(ref v); };
                    break;
                case TypeCode.Int64:
                    serializer = (ref ExtenderBinaryWriter writer, ref T value) => _binaryWriterConvert.Write(ref writer, Unsafe.As<T, Int64>(ref value));
                    deserializer = (ref ExtenderBinaryReader reader) => { var v = _binaryReaderConvert.ReadInt64(ref reader); return Unsafe.As<Int64, T>(ref v); };
                    break;
                case TypeCode.SByte:
                    serializer = (ref ExtenderBinaryWriter writer, ref T value) => _binaryWriterConvert.Write(ref writer, Unsafe.As<T, SByte>(ref value));
                    deserializer = (ref ExtenderBinaryReader reader) => { var v = _binaryReaderConvert.ReadSByte(ref reader); return Unsafe.As<SByte, T>(ref v); };
                    break;
                case TypeCode.UInt16:
                    serializer = (ref ExtenderBinaryWriter writer, ref T value) => _binaryWriterConvert.Write(ref writer, Unsafe.As<T, UInt16>(ref value));
                    deserializer = (ref ExtenderBinaryReader reader) => { var v = _binaryReaderConvert.ReadUInt16(ref reader); return Unsafe.As<UInt16, T>(ref v); };
                    break;
                case TypeCode.UInt32:
                    serializer = (ref ExtenderBinaryWriter writer, ref T value) => _binaryWriterConvert.Write(ref writer, Unsafe.As<T, UInt32>(ref value));
                    deserializer = (ref ExtenderBinaryReader reader) => { var v = _binaryReaderConvert.ReadUInt32(ref reader); return Unsafe.As<UInt32, T>(ref v); };
                    break;
                case TypeCode.UInt64:
                    serializer = (ref ExtenderBinaryWriter writer, ref T value) => _binaryWriterConvert.Write(ref writer, Unsafe.As<T, UInt64>(ref value));
                    deserializer = (ref ExtenderBinaryReader reader) => { var v = _binaryReaderConvert.ReadUInt64(ref reader); return Unsafe.As<UInt64, T>(ref v); };
                    break;
                default:
                    throw new NotSupportedException(string.Format("这个枚举未找到转换类型{0}", type.FullName));
            }
        }

        public override T Deserialize(ref ExtenderBinaryReader reader)
        {
            return deserializer.Invoke(ref reader);
        }

        public override void Serialize(ref ExtenderBinaryWriter writer, T value)
        {
            serializer.Invoke(ref writer, ref value);
        }
    }
}
