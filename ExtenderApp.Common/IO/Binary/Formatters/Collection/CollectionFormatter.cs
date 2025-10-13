using ExtenderApp.Abstract;
using ExtenderApp.Data;

namespace ExtenderApp.Common.IO.Binary.Formatters
{
    /// <summary>
    /// 一个抽象类，用于将集合类型的数据序列化和反序列化。
    /// </summary>
    /// <typeparam name="T">集合中元素的类型。</typeparam>
    /// <typeparam name="TCollection">集合的类型。</typeparam>
    public abstract class CollectionFormatter<T, TCollection> : BinaryFormatter<TCollection?>
        where TCollection : IEnumerable<T>
    {
        /// <summary>
        /// 用于序列化和反序列化集合中单个元素的格式化器。
        /// </summary>
        private readonly IBinaryFormatter<T> _formatter;

        public override int DefaultLength => 5;

        /// <summary>
        /// 初始化 CollectionFormatter 实例。
        /// </summary>
        /// <param name="formatter">用于序列化和反序列化集合中单个元素的格式化器。</param>
        /// <param name="binarybufferConvert">二进制写入转换器。</param>
        /// <param name="binarybufferConvert">二进制读取转换器。</param>
        /// <param name="options">二进制选项。</param>
        protected CollectionFormatter(IBinaryFormatterResolver resolver, ByteBufferConvert convert, BinaryOptions options) : base(convert, options)
        {
            _formatter = resolver.GetFormatter<T>();
        }

        /// <summary>
        /// 将集合对象序列化为二进制数据。
        /// </summary>
        /// <param name="buffer">二进制写入器。</param>
        /// <param name="value">要序列化的集合对象。</param>
        public override void Serialize(ref ByteBuffer buffer, TCollection? value)
        {
            if (value == null)
            {
                _bufferConvert.WriteNil(ref buffer);
                return;
            }

            var count = value.Count();

            _bufferConvert.WriteArrayHeader(ref buffer, count);

            foreach (T item in value)
            {
                _formatter.Serialize(ref buffer, item);
            }
        }

        /// <summary>
        /// 从二进制数据中反序列化出集合对象。
        /// </summary>
        /// <param name="buffer">二进制读取器。</param>
        /// <returns>反序列化出的集合对象。</returns>
        public override TCollection? Deserialize(ref ByteBuffer buffer)
        {
            if (_bufferConvert.TryReadNil(ref buffer))
            {
                return default(TCollection);
            }

            var len = _bufferConvert.ReadArrayHeader(ref buffer);


            var result = Create(len);
            for (int i = 0; i < len; i++)
            {
                Add(result, _formatter.Deserialize(ref buffer));
            }

            return result;
        }

        public override long GetLength(TCollection value)
        {
            if (value == null)
            {
                return 1;
            }

            long result = DefaultLength;
            foreach (var item in value)
            {
                result += _formatter.GetLength(item);
            }
            return result;
        }

        /// <summary>
        /// 将元素添加到集合中。
        /// </summary>
        /// <param name="collection">目标集合。</param>
        /// <param name="value">要添加的元素。</param>
        protected abstract void Add(TCollection collection, T value);

        /// <summary>
        /// 创建一个指定大小的集合。
        /// </summary>
        /// <param name="count">集合的大小。</param>
        /// <returns>创建的集合对象。</returns>
        protected abstract TCollection Create(int count);
    }
}
