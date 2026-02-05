using ExtenderApp.Abstract;
using ExtenderApp.Data;

namespace ExtenderApp.Common.Serializations.Binary.Formatters
{
    /// <summary>
    /// 通用可枚举集合格式化器基类。
    /// <para>为实现了 <see cref="IEnumerable{T}"/> 的集合类型提供标准的序列化和反序列化实现。</para>
    /// </summary>
    /// <typeparam name="T">集合元素的类型。</typeparam>
    /// <typeparam name="TEnumerable">具体的集合类型。</typeparam>
    public abstract class InterfaceEnumerableFormatter<T, TEnumerable> : ResolverFormatter<TEnumerable>
        where TEnumerable : IEnumerable<T>
    {
        private readonly IBinaryFormatter<T> _t;
        private readonly IBinaryFormatter<int> _int;

        /// <summary>
        /// 初始化 <see cref="InterfaceEnumerableFormatter{T, TEnumerable}"/> 类的新实例。
        /// </summary>
        /// <param name="resolver">用于获取依赖格式化器的解析器实例。</param>
        public InterfaceEnumerableFormatter(IBinaryFormatterResolver resolver) : base(resolver)
        {
            _t = GetFormatter<T>();
            _int = GetFormatter<int>();
        }

        public override TEnumerable Deserialize(ref ByteBuffer buffer)
        {
            if (TryReadNil(ref buffer))
            {
                return default!;
            }

            if (!TryReadArrayHeader(ref buffer))
            {
                throw new InvalidOperationException("数据类型不匹配。");
            }

            var len = _int.Deserialize(ref buffer);

            var result = Create(len);
            for (int i = 0; i < len; i++)
            {
                Add(result, _t.Deserialize(ref buffer));
            }

            return result;
        }

        public override void Serialize(ref ByteBuffer buffer, TEnumerable value)
        {
            if (value == null)
            {
                WriteNil(ref buffer);
                return;
            }

            var count = value.Count();
            WriteArrayHeader(ref buffer);
            _int.Serialize(ref buffer, count);

            foreach (T item in value)
            {
                _t.Serialize(ref buffer, item);
            }
        }

        public override long GetLength(TEnumerable value)
        {
            if (value == null)
            {
                return NilLength;
            }

            long result = _int.GetLength(value.Count()) + 1;
            foreach (var item in value)
            {
                result += _t.GetLength(item);
            }
            return result;
        }

        /// <summary>
        /// 将反序列化得到的元素添加到目标集合中。
        /// </summary>
        /// <param name="enumerable">目标集合实例。</param>
        /// <param name="value">要添加的元素。</param>
        protected abstract void Add(TEnumerable enumerable, T value);

        /// <summary>
        /// 创建一个指定容量的新集合实例。
        /// </summary>
        /// <param name="count">集合的预期元素数量。</param>
        /// <returns>新创建的集合对象。</returns>
        protected abstract TEnumerable Create(int count);
    }
}