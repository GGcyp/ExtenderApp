using ExtenderApp.Abstract;
using ExtenderApp.Common.DataBuffers;
using ExtenderApp.Common.IO.Binary;
using ExtenderApp.Common.IO.Binary.Formatters;
using ExtenderApp.Data;

namespace ExtenderApp.Common.IO.Binary.Formatters
{
    /// <summary>
    /// 抽象类 NodeFormatter，用于格式化 Node<TLinkClient> 类型的节点。
    /// </summary>
    /// <typeparam name="T">表示 Node<TLinkClient> 类型的泛型参数。</typeparam>
    public abstract class NodeFormatter<T> : BinaryFormatter<T> where T : Node<T>, IEnumerable<T>
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
        /// <param name="binarybufferConvert">二进制写入转换器。</param>
        /// <param name="binarybufferConvert">二进制读取转换器。</param>
        /// <param name="options">二进制选项。</param>
        protected NodeFormatter(IBinaryFormatterResolver resolver, ByteBufferConvert convert, BinaryOptions options) : base(convert, options)
        {
        }

        /// <summary>
        /// 从给定的 ByteBuffer 中反序列化一个 Node<TLinkClient> 类型的对象。
        /// </summary>
        /// <param name="buffer">ByteBuffer 对象，用于读取二进制数据。</param>
        /// <returns>返回反序列化后的 Node<TLinkClient> 类型的对象。</returns>
        public sealed override T Deserialize(ref ByteBuffer buffer)
        {
            if (_bufferConvert.TryReadNil(ref buffer))
            {
                return default;
            }

            T root = ProtectedDeserialize(ref buffer);
            int count = _bufferConvert.ReadInt32(ref buffer);
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
                    T child = ProtectedDeserialize(ref buffer);
                    int subCount = _bufferConvert.ReadInt32(ref buffer);
                    parent.Add(child);
                    if (subCount > 0)
                        queue.Push((child, subCount));
                }
            }
            return root;
        }

        /// <summary>
        /// 将给定的 Node<TLinkClient> 类型的对象序列化为二进制数据，并写入 ByteBuffer 中。
        /// </summary>
        /// <param name="buffer">ByteBuffer 对象，用于写入二进制数据。</param>
        /// <param name="value">要序列化的 Node<TLinkClient> 类型的对象。</param>
        public sealed override void Serialize(ref ByteBuffer buffer, T value)
        {
            if (value == null)
            {
                _bufferConvert.WriteNil(ref buffer);
                return;
            }

            // 广度优先遍历，序列化所有节点
            Queue<T> queue = new();
            queue.Enqueue(value);
            while (queue.Count > 0)
            {
                T node = queue.Dequeue();
                ProtectedSerialize(ref buffer, node);
                _bufferConvert.WriteInt32(ref buffer, node.Count);
                for (int i = 0; i < node.Count; i++)
                {
                    queue.Enqueue(node[i]);
                }
            }
        }

        /// <summary>
        /// 受保护的序列化方法，用于将给定的 Node<TLinkClient> 类型的对象序列化为二进制数据，并写入 ByteBuffer 中。
        /// </summary>
        /// <param name="buffer">ByteBuffer 对象，用于写入二进制数据。</param>
        /// <param name="value">要序列化的 Node<TLinkClient> 类型的对象。</param>
        protected abstract void ProtectedSerialize(ref ByteBuffer buffer, T value);

        /// <summary>
        /// 受保护的反序列化方法，用于从 ByteBuffer 中反序列化一个 Node<TLinkClient> 类型的对象。
        /// </summary>
        /// <param name="buffer">ByteBuffer 对象，用于读取二进制数据。</param>
        /// <returns>返回反序列化后的 Node<TLinkClient> 类型的对象。</returns>
        protected abstract T ProtectedDeserialize(ref ByteBuffer buffer);

        /// <summary>
        /// 获取给定值的长度。
        /// </summary>
        /// <param name="value">要获取长度的值。</param>
        /// <returns>给定值的长度。</returns>
        public override long GetLength(T value)
        {
            if (value == null)
                return 1;

            DataBuffer<long> dataBuffer = DataBuffer<long>.Get();
            ProtectedGetLength(value, dataBuffer);
            value.LoopAllChildNodes(ProtectedGetLength, dataBuffer);
            long length = dataBuffer.Item1;
            dataBuffer.Release();
            return length;
        }

        /// <summary>
        /// 受保护的抽象方法，用于获取指定值的长度，并将结果存储在指定的DataBuffer中。
        /// </summary>
        /// <param name="value">需要获取长度的值。</param>
        /// <param name="dataBuffer">用于存储长度结果的DataBuffer。</param>
        protected abstract void ProtectedGetLength(T value, DataBuffer<long> dataBuffer);
    }
}
