using System.Runtime.CompilerServices;
using ExtenderApp.Abstract;
using ExtenderApp.Buffer;

namespace ExtenderApp.Common.Serializations.Binary.Formatters
{
    internal class EnumFormatter<T> : ResolverFormatter<T>
        where T : struct, Enum
    {
        private delegate void EnumAbstractBufferSerialize(AbstractBuffer<byte> buffer, T value);

        private delegate void EnumSpanWriterSerialize(ref SpanWriter<byte> writer, T value);

        private delegate T EnumAbstractBufferReaderDeserialize(AbstractBufferReader<byte> reader);

        private delegate T EnumSpanReaderDeserialize(ref SpanReader<byte> reader);

        private delegate long EnumGetLength(T value);

        private EnumAbstractBufferSerialize abstractBufferSerialize = default!;
        private EnumSpanWriterSerialize spanWriterSerializer = default!;
        private EnumAbstractBufferReaderDeserialize abstractBufferReaderDeserialize = default!;
        private EnumSpanReaderDeserialize spanReaderDeserialize = default!;
        private EnumGetLength getLength = default!;

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
            where TType : struct
        {
            abstractBufferSerialize = (buffer, value) => formatter.Serialize(buffer, Unsafe.As<T, TType>(ref value));
            spanWriterSerializer = (ref SpanWriter<byte> writer, T value) => formatter.Serialize(ref writer, Unsafe.As<T, TType>(ref value));
            abstractBufferReaderDeserialize = (reader) => { var v = formatter.Deserialize(reader); return Unsafe.As<TType, T>(ref v); };
            spanReaderDeserialize = (ref SpanReader<byte> reader) => { var v = formatter.Deserialize(ref reader); return Unsafe.As<TType, T>(ref v); };
            getLength = (T val) => formatter.GetLength(Unsafe.As<T, TType>(ref val));
            return formatter.DefaultLength;
        }

        public override void Serialize(AbstractBuffer<byte> buffer, T value)
        {
            abstractBufferSerialize.Invoke(buffer, value);
        }

        public override void Serialize(ref SpanWriter<byte> writer, T value)
        {
            spanWriterSerializer.Invoke(ref writer, value);
        }

        public override T Deserialize(AbstractBufferReader<byte> reader)
        {
            return abstractBufferReaderDeserialize.Invoke(reader);
        }

        public override T Deserialize(ref SpanReader<byte> reader)
        {
            return spanReaderDeserialize.Invoke(ref reader);
        }

        public override long GetLength(T value)
        {
            return getLength.Invoke(value);
        }
    }
}