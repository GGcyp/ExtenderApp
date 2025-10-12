using ExtenderApp.Abstract;
using ExtenderApp.Data;

namespace ExtenderApp.Common.IO.Binary.Formatters
{
    /// <summary>
    /// 基于解析器 <see cref="IBinaryFormatterResolver"/> 的序列化/反序列化抽象基类。
    /// 提供按需获取其它类型格式化器的能力，并内置对 Nil 标记的写入与检测辅助方法。
    /// </summary>
    /// <typeparam name="T">目标序列化/反序列化的类型。</typeparam>
    public abstract class ResolverFormatter<T> : BaseBinaryFormatter<T>
    {
        /// <summary>
        /// 格式化器解析器，用于解析并获取指定类型的 <see cref="IBinaryFormatter{T}"/>。
        /// </summary>
        private readonly IBinaryFormatterResolver _resolver;

        /// <summary>
        /// Nil 值的格式化器，供写入/检测空值编码时复用。
        /// </summary>
        protected readonly IBinaryFormatter<Nil> _nil;

        /// <summary>
        /// 使用给定的解析器初始化实例，并缓存 Nil 格式化器。
        /// </summary>
        /// <param name="resolver">格式化器解析器。</param>
        protected ResolverFormatter(IBinaryFormatterResolver resolver)
        {
            _resolver = resolver;
            _nil = GetFormatter<Nil>();
        }

        /// <summary>
        /// 从解析器获取指定类型的格式化器（若不存在则抛出）。
        /// </summary>
        /// <typeparam name="TValue">需要获取格式化器的类型。</typeparam>
        /// <returns><typeparamref name="TValue"/> 的格式化器实例。</returns>
        protected IBinaryFormatter<TValue> GetFormatter<TValue>()
        {
            return _resolver.GetFormatterWithVerify<TValue>();
        }

        /// <summary>
        /// 将 Nil 标记写入到目标 <see cref="ByteBlock"/>。
        /// </summary>
        /// <param name="block">目标写入器。</param>
        protected void WriteNil(ref ByteBlock block)
        {
            _nil.Serialize(ref block, true);
        }

        /// <summary>
        /// 尝试读取 Nil 标记并返回是否为 Nil。
        /// 具体是否消耗输入取决于 Nil 格式化器的实现。
        /// </summary>
        /// <param name="block">数据来源。</param>
        /// <returns>若为 Nil 返回 true，否则返回 false。</returns>
        protected bool TryReadNil(ref ByteBlock block)
        {
            return _nil.Deserialize(ref block).IsNil;
        }
    }
}