using System.Runtime.CompilerServices;
using ExtenderApp.Abstract;
using ExtenderApp.Data;

namespace ExtenderApp.Common.Serializations.Binary.Formatters
{
    internal class EnumFormatter<T> : ResolverFormatter<T>
        where T : struct, Enum
    {
        //原设计
        //public EnumFormatter(ByteBufferConvert convert, BinaryOptions options) : base(convert, options)
        //{
        //    var type = typeof(TLinkClient).GetEnumUnderlyingType();

        //    switch (StartupType.GetTypeCode(type))
        //    {
        //        case TypeCode.Byte:
        //            serializer = (ref ByteBuffer Block, ref TLinkClient Value) => _bufferConvert.Write(ref Block, Unsafe.As<TLinkClient, Byte>(ref Value));
        //            deserializer = (ref ByteBuffer Block) => { var v = _bufferConvert.ReadByte(ref Block); return Unsafe.As<Byte, TLinkClient>(ref v); };
        //            break;
        //        case TypeCode.Int16:
        //            serializer = (ref ByteBuffer Block, ref TLinkClient Value) => _bufferConvert.Write(ref Block, Unsafe.As<TLinkClient, Int64>(ref Value));
        //            deserializer = (ref ByteBuffer Block) => { var v = _bufferConvert.ReadInt16(ref Block); return Unsafe.As<Int16, TLinkClient>(ref v); };
        //            break;
        //        case TypeCode.Int32:
        //            serializer = (ref ByteBuffer Block, ref TLinkClient Value) => _bufferConvert.Write(ref Block, Unsafe.As<TLinkClient, Int32>(ref Value));
        //            deserializer = (ref ByteBuffer Block) => { var v = _bufferConvert.ReadInt32(ref Block); return Unsafe.As<Int32, TLinkClient>(ref v); };
        //            break;
        //        case TypeCode.Int64:
        //            serializer = (ref ByteBuffer Block, ref TLinkClient Value) => _bufferConvert.Write(ref Block, Unsafe.As<TLinkClient, Int64>(ref Value));
        //            deserializer = (ref ByteBuffer Block) => { var v = _bufferConvert.ReadInt64(ref Block); return Unsafe.As<Int64, TLinkClient>(ref v); };
        //            break;
        //        case TypeCode.SByte:
        //            serializer = (ref ByteBuffer Block, ref TLinkClient Value) => _bufferConvert.Write(ref Block, Unsafe.As<TLinkClient, SByte>(ref Value));
        //            deserializer = (ref ByteBuffer Block) => { var v = _bufferConvert.ReadSByte(ref Block); return Unsafe.As<SByte, TLinkClient>(ref v); };
        //            break;
        //        case TypeCode.UInt16:
        //            serializer = (ref ByteBuffer Block, ref TLinkClient Value) => _bufferConvert.Write(ref Block, Unsafe.As<TLinkClient, UInt16>(ref Value));
        //            deserializer = (ref ByteBuffer Block) => { var v = _bufferConvert.ReadUInt16(ref Block); return Unsafe.As<UInt16, TLinkClient>(ref v); };
        //            break;
        //        case TypeCode.UInt32:
        //            serializer = (ref ByteBuffer Block, ref TLinkClient Value) => _bufferConvert.Write(ref Block, Unsafe.As<TLinkClient, UInt32>(ref Value));
        //            deserializer = (ref ByteBuffer Block) => { var v = _bufferConvert.ReadUInt32(ref Block); return Unsafe.As<UInt32, TLinkClient>(ref v); };
        //            break;
        //        case TypeCode.UInt64:
        //            serializer = (ref ByteBuffer Block, ref TLinkClient Value) => _bufferConvert.Write(ref Block, Unsafe.As<TLinkClient, UInt64>(ref Value));
        //            deserializer = (ref ByteBuffer Block) => { var v = _bufferConvert.ReadUInt64(ref Block); return Unsafe.As<UInt64, TLinkClient>(ref v); };
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
            deserializer = default!;
            serializer = default!;
            getLength = default!;

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