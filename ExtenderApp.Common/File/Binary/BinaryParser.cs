using System.Buffers;
using System;
using System.Threading;
using ExtenderApp.Abstract;
using ExtenderApp.Data;
using ExtenderApp.Data.File;


namespace ExtenderApp.Common.File
{
    public class BinaryParser : IBinaryParser
    {
        private readonly IBinaryFormatterResolver _binaryFormatterResolver;
        private readonly SequencePool _sequencePool;

        public BinaryParser(IBinaryFormatterResolver binaryFormatterResolver)
        {
            _binaryFormatterResolver = binaryFormatterResolver;
            _sequencePool = new SequencePool();
        }

        #region Serialize

        public byte[] Serialize<T>(T value)
        {
            byte[]? array = new byte[65536];
            var writer = new ExtenderBinaryWriter(_sequencePool, array);
            Serialize(ref writer, value);

            return writer.FlushAndGetArray();
        }

        public void Serialize<T>(ref ExtenderBinaryWriter writer, T value)
        {
            try
            {
                _binaryFormatterResolver.GetFormatterWithVerify<T>().Serialize(ref writer, value);
            }
            catch (Exception ex)
            {
                //throw new MessagePackSerializationException($"Failed to serialize {typeof(T).FullName} value.", ex);
                throw;
            }
        }

        public bool Serialize<T>(FileOperate operate, T value, object? options = null)
        {
            //using (SequencePool.Rental rental = _sequencePool.Rent())
            //{
            //    var writer = new ExtenderBinaryWriter(rental.Value);
            //    Serialize(ref writer, value);
            //    writer.Flush();
            //    using (FileStream stream = operate.OpenFile())
            //    {
            //        stream.Write(writer.GetSpan());
            //        foreach (ReadOnlyMemory<byte> segment in rental.Value.AsReadOnlySequence)
            //        {
            //            var sharedBuffer = ArrayPool<byte>.Shared.Rent(segment.Length);
            //            segment.CopyTo(sharedBuffer);
            //            stream.Write(sharedBuffer, 0, segment.Length);
            //        }
            //    }
            //}

            byte[] bytes = Serialize(value);
            using (FileStream stream = operate.OpenFile())
            {
                stream.Write(bytes);
            }


            return true;
        }

        #endregion

        #region Deserialize

        public T? Deserialize<T>(byte[] bytes)
        {
            if (bytes == null)
                throw new ArgumentNullException(nameof(bytes));


            ExtenderBinaryReader reader = new ExtenderBinaryReader(bytes);
            return Deserialize<T>(reader);
        }

        private T? Deserialize<T>(ExtenderBinaryReader reader)
        {
            return _binaryFormatterResolver.GetFormatterWithVerify<T>().Deserialize(ref reader);

        }


        public T? Deserialize<T>(FileOperate operate, object? options = null)
        {
            throw new NotImplementedException();
        }

        #endregion

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
