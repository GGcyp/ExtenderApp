using ExtenderApp.Abstract;
using ExtenderApp.Data;

namespace ExtenderApp.Common.Files.Binary.Formatter
{
    /// <summary>
    /// 表示一个抽象类，实现了 <see cref="IBinaryFormatter{T}"/> 接口，用于序列化和反序列化对象。
    /// </summary>
    /// <typeparam name="T">需要序列化和反序列化的对象类型。</typeparam>
    public abstract class ExtenderFormatter<T> : IBinaryFormatter<T>
    {
        /// <summary>
        /// 获取 <see cref="ExtenderBinaryWriterConvert"/> 对象，用于转换二进制数据。
        /// </summary>
        protected readonly ExtenderBinaryWriterConvert _binaryWriterConvert;
        protected readonly ExtenderBinaryReaderConvert _binaryReaderConvert;
        protected readonly BinaryOptions _binaryOptions;

        public abstract int Count { get; }

        public virtual T Default => default(T);

        /// <summary>
        /// 初始化 <see cref="ExtenderFormatter{T}"/> 类的新实例。
        /// </summary>
        /// <param name="binaryWriterConvert">用于转换二进制数据的 <see cref="ExtenderBinaryWriterConvert"/> 对象。</param>
        public ExtenderFormatter(ExtenderBinaryWriterConvert binaryWriterConvert, ExtenderBinaryReaderConvert binaryReaderConvert, BinaryOptions options)
        {
            _binaryWriterConvert = binaryWriterConvert;
            _binaryReaderConvert = binaryReaderConvert;
            _binaryOptions = options;
        }

        /// <summary>
        /// 将对象序列化为二进制数据。
        /// </summary>
        /// <param name="writer">用于写入二进制数据的 <see cref="ExtenderBinaryWriter"/> 对象。</param>
        /// <param name="value">要序列化的对象。</param>
        public abstract void Serialize(ref ExtenderBinaryWriter writer, T value);

        /// <summary>
        /// 反序列化二进制数据为对象。
        /// </summary>
        /// <param name="reader">用于读取二进制数据的 <see cref="ExtenderBinaryReader"/> 对象。</param>
        /// <returns>反序列化后的对象。</returns>
        public abstract T Deserialize(ref ExtenderBinaryReader reader);

        protected void DepthStep(ref ExtenderBinaryReader reader)
        {
            if (reader.Depth >= _binaryOptions.MaximumObjectGraphDepth)
            {
                throw new InsufficientExecutionStackException("这个类型的对象深度超过了最大深度！");
            }

            reader.Depth++;
        }

        public virtual int GetCount(T value)
        {
            if (typeof(T).IsClass)
            {
                if (value == null)
                {
                    return 1;
                }
            }

            return Count;
        }
    }
}
