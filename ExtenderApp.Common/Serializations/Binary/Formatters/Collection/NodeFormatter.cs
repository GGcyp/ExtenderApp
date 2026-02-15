using ExtenderApp.Abstract;
using ExtenderApp.Buffer;
using ExtenderApp.Contracts;

namespace ExtenderApp.Common.Serializations.Binary.Formatters
{
    /// <summary>
    /// 抽象类 NodeFormatter，用于格式化 <see cref="Node{T}"/> 类型的节点。
    /// </summary>
    /// <typeparam name="T">表示 <see cref="Node{T}"/> 类型的泛型参数。</typeparam>
    public abstract class NodeFormatter<T> : ResolverFormatter<T> where T : Node<T>, IEnumerable<T>
    {
        private readonly IBinaryFormatter<int> _int;

        protected NodeFormatter(IBinaryFormatterResolver resolver) : base(resolver)
        {
            _int = GetFormatter<int>();
        }

        /// <summary>
        /// 从顺序缓冲读取器反序列化一个 <see cref="Node{T}"/>。
        /// </summary>
        /// <param name="reader">来源顺序缓冲读取器。</param>
        /// <returns>反序列化得到的节点对象。</returns>
        public override sealed T Deserialize(AbstractBufferReader<byte> reader)
        {
            if (TryReadNil(reader))
            {
                return default!;
            }

            T root = ProtectedDeserialize(reader);
            int count = _int.Deserialize(reader);
            if (count == 0)
                return root;

            Stack<(T Node, int Count)> queue = new();
            queue.Push((root, count));

            while (queue.Count > 0)
            {
                var (parent, childCount) = queue.Pop();
                for (int i = 0; i < childCount; i++)
                {
                    T child = ProtectedDeserialize(reader);
                    int subCount = _int.Deserialize(reader);
                    parent.Add(child);
                    if (subCount > 0)
                        queue.Push((child, subCount));
                }
            }
            return root;
        }

        /// <summary>
        /// 从栈上读取器反序列化一个 <see cref="Node{T}"/>。
        /// </summary>
        /// <param name="reader">来源栈上读取器。</param>
        /// <returns>反序列化得到的节点对象。</returns>
        public override sealed T Deserialize(ref SpanReader<byte> reader)
        {
            if (TryReadNil(ref reader))
            {
                return default!;
            }

            T root = ProtectedDeserialize(ref reader);
            int count = _int.Deserialize(ref reader);
            if (count == 0)
                return root;

            Stack<(T Node, int Count)> queue = new();
            queue.Push((root, count));

            while (queue.Count > 0)
            {
                var (parent, childCount) = queue.Pop();
                for (int i = 0; i < childCount; i++)
                {
                    T child = ProtectedDeserialize(ref reader);
                    int subCount = _int.Deserialize(ref reader);
                    parent.Add(child);
                    if (subCount > 0)
                        queue.Push((child, subCount));
                }
            }
            return root;
        }

        /// <summary>
        /// 将 <see cref="Node{T}"/> 序列化并写入顺序缓冲。
        /// </summary>
        /// <param name="buffer">目标顺序缓冲。</param>
        /// <param name="value">要序列化的节点。</param>
        public override sealed void Serialize(AbstractBuffer<byte> buffer, T value)
        {
            if (value == null)
            {
                WriteNil(buffer);
                return;
            }

            Queue<T> queue = new();
            queue.Enqueue(value);
            while (queue.Count > 0)
            {
                T node = queue.Dequeue();
                ProtectedSerialize(buffer, node);
                _int.Serialize(buffer, node.Count);
                for (int i = 0; i < node.Count; i++)
                {
                    queue.Enqueue(node[i]);
                }
            }
        }

        /// <summary>
        /// 将 <see cref="Node{T}"/> 序列化并写入栈上写入器。
        /// </summary>
        /// <param name="writer">目标栈上写入器。</param>
        /// <param name="value">要序列化的节点。</param>
        public override sealed void Serialize(ref SpanWriter<byte> writer, T value)
        {
            if (value == null)
            {
                WriteNil(ref writer);
                return;
            }

            Queue<T> queue = new();
            queue.Enqueue(value);
            while (queue.Count > 0)
            {
                T node = queue.Dequeue();
                ProtectedSerialize(ref writer, node);
                _int.Serialize(ref writer, node.Count);
                for (int i = 0; i < node.Count; i++)
                {
                    queue.Enqueue(node[i]);
                }
            }
        }

        /// <summary>
        /// 获取给定值的序列化长度。
        /// </summary>
        /// <param name="value">要获取长度的值。</param>
        /// <returns>给定值的长度。</returns>
        public override long GetLength(T value)
        {
            if (value == null)
                return NilLength;

            var chache = ValueCache.FromValue(0L);
            ProtectedGetLength(value, chache);
            value.LoopAllChildNodes(ProtectedGetLength, chache);
            chache.TryGetValue(out long length);
            chache.Release();
            return length;
        }

        /// <summary>
        /// 受保护的序列化方法（顺序缓冲路径）。
        /// </summary>
        /// <param name="buffer">目标顺序缓冲。</param>
        /// <param name="value">要序列化的节点。</param>
        protected abstract void ProtectedSerialize(AbstractBuffer<byte> buffer, T value);

        /// <summary>
        /// 受保护的序列化方法（栈上写入器路径）。
        /// </summary>
        /// <param name="writer">目标栈上写入器。</param>
        /// <param name="value">要序列化的节点。</param>
        protected abstract void ProtectedSerialize(ref SpanWriter<byte> writer, T value);

        /// <summary>
        /// 受保护的反序列化方法（顺序缓冲读取器路径）。
        /// </summary>
        /// <param name="reader">来源顺序缓冲读取器。</param>
        /// <returns>反序列化得到的节点。</returns>
        protected abstract T ProtectedDeserialize(AbstractBufferReader<byte> reader);

        /// <summary>
        /// 受保护的反序列化方法（栈上读取器路径）。
        /// </summary>
        /// <param name="reader">来源栈上读取器。</param>
        /// <returns>反序列化得到的节点。</returns>
        protected abstract T ProtectedDeserialize(ref SpanReader<byte> reader);

        /// <summary>
        /// 受保护的抽象方法，用于获取指定值的长度，并将结果存储在指定的 ValueCache 中。
        /// </summary>
        /// <param name="value">需要获取长度的值。</param>
        /// <param name="cahce">用于存储长度结果的 ValueCache。</param>
        protected abstract void ProtectedGetLength(T value, ValueCache cahce);
    }
}