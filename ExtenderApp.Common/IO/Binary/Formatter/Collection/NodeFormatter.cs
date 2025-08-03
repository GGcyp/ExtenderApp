using ExtenderApp.Abstract;
using ExtenderApp.Common.DataBuffers;
using ExtenderApp.Common.IO.Binaries;
using ExtenderApp.Common.IO.Binaries.Formatter;
using ExtenderApp.Data;

namespace ExtenderApp.Common.IO.Binaries.Formatter
{
    /// <summary>
    /// 抽象类 NodeFormatter，用于格式化 Node<T> 类型的节点。
    /// </summary>
    /// <typeparam name="T">表示 Node<T> 类型的泛型参数。</typeparam>
    public abstract class NodeFormatter<T> : ExtenderFormatter<T> where T : Node<T>, IEnumerable<T>
    {
        /// <summary>
        /// 获取节点格式化的长度。
        /// </summary>
        /// <returns>返回值为 1。</returns>
        public override int DefaultLength => 1;

        /// <summary>
        /// 使用指定的参数初始化 NodeFormatter 实例。
        /// </summary>
        /// <param name="resolver">二进制格式化解析器。</param>
        /// <param name="binaryWriterConvert">二进制写入转换器。</param>
        /// <param name="binaryReaderConvert">二进制读取转换器。</param>
        /// <param name="options">二进制选项。</param>
        protected NodeFormatter(IBinaryFormatterResolver resolver, ExtenderBinaryWriterConvert binaryWriterConvert, ExtenderBinaryReaderConvert binaryReaderConvert, BinaryOptions options) : base(binaryWriterConvert, binaryReaderConvert, options)
        {
        }

        /// <summary>
        /// 从给定的 ExtenderBinaryReader 中反序列化一个 Node<T> 类型的对象。
        /// </summary>
        /// <param name="reader">ExtenderBinaryReader 对象，用于读取二进制数据。</param>
        /// <returns>返回反序列化后的 Node<T> 类型的对象。</returns>
        public sealed override T Deserialize(ref ExtenderBinaryReader reader)
        {
            if (_binaryReaderConvert.TryReadNil(ref reader))
            {
                return default;
            }

            T root = ProtectedDeserialize(ref reader);
            int count = _binaryReaderConvert.ReadInt32(ref reader);
            if (count == 0)
                return root;

            // 广度优先还原树结构
            Stack<(T, int)> queue = new();
            queue.Push((root, count));

            while (queue.Count > 0)
            {
                var (parent, childCount) = queue.Pop();
                for (int i = 0; i < childCount; i++)
                {
                    T child = ProtectedDeserialize(ref reader);
                    int subCount = _binaryReaderConvert.ReadInt32(ref reader);
                    parent.Add(child);
                    if (subCount > 0)
                        queue.Push((child, subCount));
                }
            }
            return root;
        }

        /// <summary>
        /// 将给定的 Node<T> 类型的对象序列化为二进制数据，并写入 ExtenderBinaryWriter 中。
        /// </summary>
        /// <param name="writer">ExtenderBinaryWriter 对象，用于写入二进制数据。</param>
        /// <param name="value">要序列化的 Node<T> 类型的对象。</param>
        public sealed override void Serialize(ref ExtenderBinaryWriter writer, T value)
        {
            if (value == null)
            {
                _binaryWriterConvert.WriteNil(ref writer);
                return;
            }

            // 广度优先遍历，序列化所有节点
            Queue<T> queue = new();
            queue.Enqueue(value);
            while (queue.Count > 0)
            {
                T node = queue.Dequeue();
                ProtectedSerialize(ref writer, node);
                _binaryWriterConvert.WriteInt32(ref writer, node.Count);
                for (int i = 0; i < node.Count; i++)
                {
                    queue.Enqueue(node[i]);
                }
            }
        }

        /// <summary>
        /// 受保护的序列化方法，用于将给定的 Node<T> 类型的对象序列化为二进制数据，并写入 ExtenderBinaryWriter 中。
        /// </summary>
        /// <param name="writer">ExtenderBinaryWriter 对象，用于写入二进制数据。</param>
        /// <param name="value">要序列化的 Node<T> 类型的对象。</param>
        protected abstract void ProtectedSerialize(ref ExtenderBinaryWriter writer, T value);

        /// <summary>
        /// 受保护的反序列化方法，用于从 ExtenderBinaryReader 中反序列化一个 Node<T> 类型的对象。
        /// </summary>
        /// <param name="reader">ExtenderBinaryReader 对象，用于读取二进制数据。</param>
        /// <returns>返回反序列化后的 Node<T> 类型的对象。</returns>
        protected abstract T ProtectedDeserialize(ref ExtenderBinaryReader reader);

        /// <summary>
        /// 获取给定值的长度。
        /// </summary>
        /// <param name="value">要获取长度的值。</param>
        /// <returns>给定值的长度。</returns>
        public override long GetLength(T value)
        {
            if (value == null)
                return 1;

            DataBuffer<long> dataBuffer = DataBuffer<long>.GetDataBuffer();
            ProtectedGetLength(value, dataBuffer);
            value.LoopAllChildNodes(ProtectedGetLength, dataBuffer);
            return dataBuffer.Item1;
        }

        /// <summary>
        /// 受保护的抽象方法，用于获取指定值的长度，并将结果存储在指定的DataBuffer中。
        /// </summary>
        /// <param name="value">需要获取长度的值。</param>
        /// <param name="dataBuffer">用于存储长度结果的DataBuffer。</param>
        protected abstract void ProtectedGetLength(T value, DataBuffer<long> dataBuffer);
    }
}
