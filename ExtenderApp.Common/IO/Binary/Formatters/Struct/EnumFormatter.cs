using System.Runtime.CompilerServices;
using ExtenderApp.Abstract;
using ExtenderApp.Data;

namespace ExtenderApp.Common.IO.Binary.Formatters
{
    internal class EnumFormatter<T> : ResolverFormatter<T>
        where T : struct, Enum
    {
        //原设计
        //public EnumFormatter(ByteBufferConvert convert, BinaryOptions options) : base(convert, options)
        //{
        //    var type = typeof(T).GetEnumUnderlyingType();

        //    switch (Type.GetTypeCode(type))
        //    {
        //        case TypeCode.Byte:
        //            serializer = (ref ByteBuffer buffer, ref T value) => _bufferConvert.Write(ref buffer, Unsafe.As<T, Byte>(ref value));
        //            deserializer = (ref ByteBuffer buffer) => { var v = _bufferConvert.ReadByte(ref buffer); return Unsafe.As<Byte, T>(ref v); };
        //            break;
        //        case TypeCode.Int16:
        //            serializer = (ref ByteBuffer buffer, ref T value) => _bufferConvert.Write(ref buffer, Unsafe.As<T, Int64>(ref value));
        //            deserializer = (ref ByteBuffer buffer) => { var v = _bufferConvert.ReadInt16(ref buffer); return Unsafe.As<Int16, T>(ref v); };
        //            break;
        //        case TypeCode.Int32:
        //            serializer = (ref ByteBuffer buffer, ref T value) => _bufferConvert.Write(ref buffer, Unsafe.As<T, Int32>(ref value));
        //            deserializer = (ref ByteBuffer buffer) => { var v = _bufferConvert.ReadInt32(ref buffer); return Unsafe.As<Int32, T>(ref v); };
        //            break;
        //        case TypeCode.Int64:
        //            serializer = (ref ByteBuffer buffer, ref T value) => _bufferConvert.Write(ref buffer, Unsafe.As<T, Int64>(ref value));
        //            deserializer = (ref ByteBuffer buffer) => { var v = _bufferConvert.ReadInt64(ref buffer); return Unsafe.As<Int64, T>(ref v); };
        //            break;
        //        case TypeCode.SByte:
        //            serializer = (ref ByteBuffer buffer, ref T value) => _bufferConvert.Write(ref buffer, Unsafe.As<T, SByte>(ref value));
        //            deserializer = (ref ByteBuffer buffer) => { var v = _bufferConvert.ReadSByte(ref buffer); return Unsafe.As<SByte, T>(ref v); };
        //            break;
        //        case TypeCode.UInt16:
        //            serializer = (ref ByteBuffer buffer, ref T value) => _bufferConvert.Write(ref buffer, Unsafe.As<T, UInt16>(ref value));
        //            deserializer = (ref ByteBuffer buffer) => { var v = _bufferConvert.ReadUInt16(ref buffer); return Unsafe.As<UInt16, T>(ref v); };
        //            break;
        //        case TypeCode.UInt32:
        //            serializer = (ref ByteBuffer buffer, ref T value) => _bufferConvert.Write(ref buffer, Unsafe.As<T, UInt32>(ref value));
        //            deserializer = (ref ByteBuffer buffer) => { var v = _bufferConvert.ReadUInt32(ref buffer); return Unsafe.As<UInt32, T>(ref v); };
        //            break;
        //        case TypeCode.UInt64:
        //            serializer = (ref ByteBuffer buffer, ref T value) => _bufferConvert.Write(ref buffer, Unsafe.As<T, UInt64>(ref value));
        //            deserializer = (ref ByteBuffer buffer) => { var v = _bufferConvert.ReadUInt64(ref buffer); return Unsafe.As<UInt64, T>(ref v); };
        //            break;
        //        default:
        //            throw new NotSupportedException(string.Format("这个枚举未找到转换类型{0}", type.FullName));
        //    }
        //}

        private delegate void EnumSerialize(ref ByteBuffer buffer, ref T value);
        private delegate T EnumDeserialize(ref ByteBuffer buffer);
        private delegate long EnumGetLength(T value);

        private EnumSerialize serializer;
        private EnumDeserialize deserializer;
        private EnumGetLength getLength;

        public override int DefaultLength { get; }

        public EnumFormatter(IBinaryFormatterResolver resolver) : base(resolver)
        {
            var type = typeof(T).GetEnumUnderlyingType();

            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Byte:
                    DefaultLength = CreateEnumSerialize(GetFormatter<byte>());
                    break;
                case TypeCode.Int16:
                    DefaultLength = CreateEnumSerialize(GetFormatter<Int16>());
                    break;
                case TypeCode.Int32:
                    DefaultLength = CreateEnumSerialize(GetFormatter<Int32>());
                    break;
                case TypeCode.Int64:
                    DefaultLength = CreateEnumSerialize(GetFormatter<Int64>());
                    break;
                case TypeCode.SByte:
                    DefaultLength = CreateEnumSerialize(GetFormatter<SByte>());
                    break;
                case TypeCode.UInt16:
                    DefaultLength = CreateEnumSerialize(GetFormatter<UInt16>());
                    break;
                case TypeCode.UInt32:
                    DefaultLength = CreateEnumSerialize(GetFormatter<UInt32>());
                    break;
                case TypeCode.UInt64:
                    DefaultLength = CreateEnumSerialize(GetFormatter<UInt64>());
                    break;
                default:
                    throw new NotSupportedException(string.Format("这个枚举未找到转换类型{0}", type.FullName));
            }
        }

        private int CreateEnumSerialize<TType>(IBinaryFormatter<TType> formatter)
        {
            serializer = (ref ByteBuffer buffer, ref T value) => formatter.Serialize(ref buffer, Unsafe.As<T, TType>(ref value));
            deserializer = (ref ByteBuffer buffer) => { var v = formatter.Deserialize(ref buffer); return Unsafe.As<TType, T>(ref v); };
            getLength = (T val) => formatter.GetLength(Unsafe.As<T, TType>(ref val));
            return formatter.DefaultLength;
        }

        public override T Deserialize(ref ByteBuffer buffer)
        {
            return deserializer.Invoke(ref buffer);
        }

        public override void Serialize(ref ByteBuffer buffer, T value)
        {
            serializer.Invoke(ref buffer, ref value);
        }

        public override long GetLength(T value)
        {
            return getLength.Invoke(value);
        }
    }
}
