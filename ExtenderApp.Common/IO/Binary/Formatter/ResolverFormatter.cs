using ExtenderApp.Abstract;
using ExtenderApp.Data;

namespace ExtenderApp.Common.IO.Binaries.Formatter
{
    /// <summary>
    /// 解析格式化器基类，用于处理二进制格式化操作。
    /// </summary>
    /// <typeparam name="T">要处理的数据类型。</typeparam>
    public abstract class ResolverFormatter<T> : IBinaryFormatter<T>
    {
        /// <summary>
        /// 二进制格式化解析器。
        /// </summary>
        private readonly IBinaryFormatterResolver _resolver;

        public virtual T Default => default;

        public abstract int Count { get; }

        protected ResolverFormatter(IBinaryFormatterResolver resolver)
        {
            _resolver = resolver;
        }

        public abstract T Deserialize(ref ExtenderBinaryReader reader);

        public abstract void Serialize(ref ExtenderBinaryWriter writer, T value);

        /// <summary>
        /// 获取指定类型的格式化器。
        /// </summary>
        /// <typeparam name="T">要获取格式化器的类型。</typeparam>
        /// <returns>返回指定类型的格式化器。</returns>
        protected IBinaryFormatter<TValue> GetFormatter<TValue>()
        {
            return _resolver.GetFormatterWithVerify<TValue>();
        }

        public virtual int GetCount(T value)
        {
            return Count;
        }
    }
}
