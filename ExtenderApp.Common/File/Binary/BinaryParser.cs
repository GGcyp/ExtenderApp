using ExtenderApp.Abstract;
using ExtenderApp.Common.File.Binary;

namespace ExtenderApp.Common.File
{
    public class BinaryParser : IBinaryParser
    {
        private readonly BinaryWriterCovert _binaryWriterCovert;
        private readonly IBinaryFormatterResolver _binaryFormatterResolver;

        public BinaryParser(BinaryWriterCovert binaryWriterCovert, IBinaryFormatterResolver binaryFormatterResolver)
        {
            _binaryWriterCovert = binaryWriterCovert;
            _binaryFormatterResolver = binaryFormatterResolver;
        }

        public byte[] Serialize<T>(T value)
        {
            //byte[]? array = scratchArray;
            //if (array == null)
            //{
            //    scratchArray = array = new byte[65536];
            //}

            //options = options ?? DefaultOptions;
            //var msgpackWriter = new MessagePackWriter(options.SequencePool, array)
            //{
            //    CancellationToken = cancellationToken,
            //};
            //Serialize(ref msgpackWriter, value, options);
            //return msgpackWriter.FlushAndGetArray();
            return null;
        }

        internal static void Serialize<T>(ref Data.BinaryWriter writer, T value)
        {
            try
            {
                //if (options.Compression.IsCompression() && !PrimitiveChecker<T>.IsMessagePackFixedSizePrimitive)
                //{
                //    using (var scratchRental = options.SequencePool.Rent())
                //    {
                //        var scratch = scratchRental.Value;
                //        MessagePackWriter scratchWriter = writer.Clone(scratch);
                //        options.Resolver.GetFormatterWithVerify<T>().Serialize(ref scratchWriter, value, options);
                //        scratchWriter.Flush();
                //        ToLZ4BinaryCore(scratch, ref writer, options.Compression, options.CompressionMinLength);
                //    }
                //}
                //else
                //{
                //    options.Resolver.GetFormatterWithVerify<T>().Serialize(ref writer, value, options);
                //}
            }
            catch (Exception ex)
            {
                //throw new MessagePackSerializationException($"Failed to serialize {typeof(T).FullName} value.", ex);
            }
        }

        private bool IsFixedSizePrimitiveType(Type type)
        {
            return type == typeof(short)
                || type == typeof(int)
                || type == typeof(long)
                || type == typeof(ushort)
                || type == typeof(uint)
                || type == typeof(ulong)
                || type == typeof(float)
                || type == typeof(double)
                || type == typeof(bool)
                || type == typeof(byte)
                || type == typeof(sbyte)
                || type == typeof(char)
            ;
        }
    }
}
