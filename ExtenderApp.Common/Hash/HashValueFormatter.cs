using ExtenderApp.Abstract;
using ExtenderApp.Common.Serializations.Binary.Formatters;
using ExtenderApp.Data;

namespace ExtenderApp.Common.Hash
{
    /// <summary>
    /// 哈希值格式化类
    /// </summary>
    internal class HashValueFormatter : ResolverFormatter<HashValue>
    {
        private readonly IBinaryFormatter<ByteBlock> _block;

        public HashValueFormatter(IBinaryFormatterResolver resolver) : base(resolver)
        {
            _block = GetFormatter<ByteBlock>();
        }

        public override void Serialize(ref ByteBuffer buffer, HashValue value)
        {
            if (value.IsEmpty)
            {
                WriteNil(ref buffer);
                return;
            }

            ByteBlock tempBlock = value;
            _block.Serialize(ref buffer, tempBlock);
            tempBlock.Dispose();
        }

        public override HashValue Deserialize(ref ByteBuffer buffer)
        {
            if (TryReadNil(ref buffer))
            {
                return HashValue.SHA1Empty;
            }

            return _block.Deserialize(ref buffer);
        }

        public override long GetLength(HashValue value)
        {
            if (value.IsEmpty)
            {
                return NilLength;
            }

            ByteBlock tempBlock = value;
            long result = _block.GetLength(tempBlock);
            tempBlock.Dispose();
            return result;
        }
    }
}