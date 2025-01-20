using ExtenderApp.Abstract;
using ExtenderApp.Data;
using ExtenderApp.Data.File;


namespace ExtenderApp.Common.File
{
    public class BinaryParser : IBinaryParser
    {
        private readonly IBinaryFormatterResolver _binaryFormatterResolver;
        private readonly SequencePool _sequencePool;
        private readonly Stack<byte[]> _arrayStack;

        public BinaryParser(IBinaryFormatterResolver binaryFormatterResolver)
        {
            _binaryFormatterResolver = binaryFormatterResolver;
            _sequencePool = new SequencePool();
            _arrayStack = new Stack<byte[]>();
        }

        #region Serialize

        public byte[] Serialize<T>(T value)
        {
            byte[] bytes = GetBytes();
            var writer = new ExtenderBinaryWriter(_sequencePool, bytes);
            Serialize(ref writer, value);

            var result = writer.FlushAndGetArray();
            _arrayStack.Push(bytes);
            return result;
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
            //暂时先这么写
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
            if (reader.Remaining == 0) 
                return _binaryFormatterResolver.GetFormatterWithVerify<T>().Default;
            return _binaryFormatterResolver.GetFormatterWithVerify<T>().Deserialize(ref reader);
        }

        public T? Deserialize<T>(FileOperate operate, object? options = null)
        {
            //暂时先这么写
            byte[] bytes = GetBytes();
            int length = -1;

            if (!operate.LocalFileInfo.FileInfo.Exists)
                return Deserialize<T>(ExtenderBinaryReader.Empty);

            using (FileStream stream = operate.OpenFile())
            {
                length = stream.Read(bytes);
            }

            ExtenderBinaryReader reader = new ExtenderBinaryReader(new ReadOnlyMemory<byte>(bytes, 0, length));

            return Deserialize<T>(reader);
        }

        #endregion

        private byte[] GetBytes()
        {
            return _arrayStack.Count > 0 ? _arrayStack.Pop() : new byte[65536];
        }
    }
}
