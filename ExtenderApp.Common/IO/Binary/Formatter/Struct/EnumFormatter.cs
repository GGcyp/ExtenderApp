﻿using System.Runtime.CompilerServices;
using ExtenderApp.Abstract;
using ExtenderApp.Data;

namespace ExtenderApp.Common.IO.Binaries.Formatter
{
    internal class EnumFormatter<T> : ResolverFormatter<T> where T : struct, Enum
    {
        //原设计
        //public EnumFormatter(ExtenderBinaryWriterConvert binaryWriterConvert, ExtenderBinaryReaderConvert binaryReaderConvert, BinaryOptions options) : base(binaryWriterConvert, binaryReaderConvert, options)
        //{
        //    var type = typeof(T).GetEnumUnderlyingType();

        //    switch (Type.GetTypeCode(type))
        //    {
        //        case TypeCode.Byte:
        //            serializer = (ref ExtenderBinaryWriter writer, ref T value) => _binaryWriterConvert.Write(ref writer, Unsafe.As<T, Byte>(ref value));
        //            deserializer = (ref ExtenderBinaryReader reader) => { var v = _binaryReaderConvert.ReadByte(ref reader); return Unsafe.As<Byte, T>(ref v); };
        //            break;
        //        case TypeCode.Int16:
        //            serializer = (ref ExtenderBinaryWriter writer, ref T value) => _binaryWriterConvert.Write(ref writer, Unsafe.As<T, Int64>(ref value));
        //            deserializer = (ref ExtenderBinaryReader reader) => { var v = _binaryReaderConvert.ReadInt16(ref reader); return Unsafe.As<Int16, T>(ref v); };
        //            break;
        //        case TypeCode.Int32:
        //            serializer = (ref ExtenderBinaryWriter writer, ref T value) => _binaryWriterConvert.Write(ref writer, Unsafe.As<T, Int32>(ref value));
        //            deserializer = (ref ExtenderBinaryReader reader) => { var v = _binaryReaderConvert.ReadInt32(ref reader); return Unsafe.As<Int32, T>(ref v); };
        //            break;
        //        case TypeCode.Int64:
        //            serializer = (ref ExtenderBinaryWriter writer, ref T value) => _binaryWriterConvert.Write(ref writer, Unsafe.As<T, Int64>(ref value));
        //            deserializer = (ref ExtenderBinaryReader reader) => { var v = _binaryReaderConvert.ReadInt64(ref reader); return Unsafe.As<Int64, T>(ref v); };
        //            break;
        //        case TypeCode.SByte:
        //            serializer = (ref ExtenderBinaryWriter writer, ref T value) => _binaryWriterConvert.Write(ref writer, Unsafe.As<T, SByte>(ref value));
        //            deserializer = (ref ExtenderBinaryReader reader) => { var v = _binaryReaderConvert.ReadSByte(ref reader); return Unsafe.As<SByte, T>(ref v); };
        //            break;
        //        case TypeCode.UInt16:
        //            serializer = (ref ExtenderBinaryWriter writer, ref T value) => _binaryWriterConvert.Write(ref writer, Unsafe.As<T, UInt16>(ref value));
        //            deserializer = (ref ExtenderBinaryReader reader) => { var v = _binaryReaderConvert.ReadUInt16(ref reader); return Unsafe.As<UInt16, T>(ref v); };
        //            break;
        //        case TypeCode.UInt32:
        //            serializer = (ref ExtenderBinaryWriter writer, ref T value) => _binaryWriterConvert.Write(ref writer, Unsafe.As<T, UInt32>(ref value));
        //            deserializer = (ref ExtenderBinaryReader reader) => { var v = _binaryReaderConvert.ReadUInt32(ref reader); return Unsafe.As<UInt32, T>(ref v); };
        //            break;
        //        case TypeCode.UInt64:
        //            serializer = (ref ExtenderBinaryWriter writer, ref T value) => _binaryWriterConvert.Write(ref writer, Unsafe.As<T, UInt64>(ref value));
        //            deserializer = (ref ExtenderBinaryReader reader) => { var v = _binaryReaderConvert.ReadUInt64(ref reader); return Unsafe.As<UInt64, T>(ref v); };
        //            break;
        //        default:
        //            throw new NotSupportedException(string.Format("这个枚举未找到转换类型{0}", type.FullName));
        //    }
        //}

        private delegate void EnumSerialize(ref ExtenderBinaryWriter writer, ref T value);
        private delegate T EnumDeserialize(ref ExtenderBinaryReader reader);

        private EnumSerialize serializer;
        private EnumDeserialize deserializer;

        public override int Length { get; }

        public EnumFormatter(IBinaryFormatterResolver resolver) : base(resolver)
        {
            var type = typeof(T).GetEnumUnderlyingType();

            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Byte:
                    Length = CreateEnumSerialize(GetFormatter<byte>());
                    break;
                case TypeCode.Int16:
                    Length = CreateEnumSerialize(GetFormatter<Int16>());
                    break;
                case TypeCode.Int32:
                    Length = CreateEnumSerialize(GetFormatter<Int32>());
                    break;
                case TypeCode.Int64:
                    Length = CreateEnumSerialize(GetFormatter<Int64>());
                    break;
                case TypeCode.SByte:
                    Length = CreateEnumSerialize(GetFormatter<SByte>());
                    break;
                case TypeCode.UInt16:
                    Length = CreateEnumSerialize(GetFormatter<UInt16>());
                    break;
                case TypeCode.UInt32:
                    Length = CreateEnumSerialize(GetFormatter<UInt32>());
                    break;
                case TypeCode.UInt64:
                    Length = CreateEnumSerialize(GetFormatter<UInt64>());
                    break;
                default:
                    throw new NotSupportedException(string.Format("这个枚举未找到转换类型{0}", type.FullName));
            }
        }

        private int CreateEnumSerialize<TType>(IBinaryFormatter<TType> formatter)
        {
            serializer = (ref ExtenderBinaryWriter writer, ref T value) => formatter.Serialize(ref writer, Unsafe.As<T, TType>(ref value));
            deserializer = (ref ExtenderBinaryReader reader) => { var v = formatter.Deserialize(ref reader); return Unsafe.As<TType, T>(ref v); };
            return formatter.Length;
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
