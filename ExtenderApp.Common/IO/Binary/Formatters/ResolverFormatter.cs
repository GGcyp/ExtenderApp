using ExtenderApp.Abstract;
using ExtenderApp.Data;

namespace ExtenderApp.Common.IO.Binaries.Formatters
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
        /// <summary>
        /// 用于序列化和反序列化Nil类型的二进制格式化器
        /// Nil类型通常表示空值或无效值的特殊标记
        /// </summary>
        /// <remarks>
        /// 1. 该字段由依赖注入系统初始化（通常在构造函数中注入）
        /// 2. 实现了IBinaryFormatter&lt;Nil&gt;接口，专门处理Nil类型的二进制转换
        /// 3. 在解析器中用于处理特殊空值标记场景
        /// </remarks>
        protected readonly IBinaryFormatter<Nil> _nil;

        public virtual T Default => default;

        public abstract int DefaultLength { get; }

        protected ResolverFormatter(IBinaryFormatterResolver resolver)
        {
            _resolver = resolver;
            _nil = GetFormatter<Nil>();
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

        /// <summary>
        /// 获取提供版本化数据序列化、反序列化及长度计算功能的接口
        /// </summary>
        /// <typeparam name="TValue"></typeparam>
        /// <returns></returns>
        protected IVersionDataFormatterMananger<TValue> GetVersionDataFormatter<TValue>()
        {
            return _resolver.GetFormatter<TValue>() as IVersionDataFormatterMananger<TValue>;
        }

        /// <summary>
        /// 将空值写入到二进制写入器中。
        /// </summary>
        /// <param name="writer">二进制写入器。</param>
        protected void WriteNil(ref ExtenderBinaryWriter writer)
        {
            _nil.Serialize(ref writer, true);
        }

        /// <summary>
        /// 尝试从二进制读取器中读取一个空值。
        /// </summary>
        /// <param name="reader">二进制读取器。</param>
        /// <returns>如果成功读取到一个空值，则返回true；否则返回false。</returns>
        protected bool TryReadNil(ref ExtenderBinaryReader reader)
        {
            return _nil.Deserialize(ref reader).IsNil;
        }

        public virtual long GetLength(T value)
        {
            return DefaultLength;
        }
    }
}
