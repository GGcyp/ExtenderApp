using ExtenderApp.Abstract;
using ExtenderApp.Data;

namespace ExtenderApp.Common.IO.Binaries.Formatter
{
    /// <summary>
    /// 用于数组格式化的扩展格式化器
    /// </summary>
    /// <typeparam name="T">数组元素的类型</typeparam>
    public sealed class ArrayFormatter<T> : ExtenderFormatter<T[]?>
    {
        /// <summary>
        /// 二进制格式化器
        /// </summary>
        private readonly IBinaryFormatter<T> _binaryFormatter;

        /// <summary>
        /// 初始化ArrayFormatter对象
        /// </summary>
        /// <param name="formatter">二进制格式化器</param>
        /// <param name="binaryWriterConvert">二进制写入转换器</param>
        /// <param name="binaryReaderConvert">二进制读取转换器</param>
        /// <param name="options">二进制选项</param>
        public ArrayFormatter(IBinaryFormatterResolver resolver, ExtenderBinaryWriterConvert binaryWriterConvert, ExtenderBinaryReaderConvert binaryReaderConvert, BinaryOptions options) : base(binaryWriterConvert, binaryReaderConvert, options)
        {
            _binaryFormatter = resolver.GetFormatter<T>();
        }

        public override int Length => 1;

        /// <summary>
        /// 反序列化数组
        /// </summary>
        /// <param name="reader">扩展二进制读取器</param>
        /// <returns>反序列化后的数组</returns>
        public override T[]? Deserialize(ref ExtenderBinaryReader reader)
        {
            if (_binaryReaderConvert.TryReadNil(ref reader))
            {
                return default;
            }

            var len = _binaryReaderConvert.ReadArrayHeader(ref reader);
            if (len == 0)
            {
                return Array.Empty<T>();
            }

            var array = new T[len];
            DepthStep(ref reader);
            try
            {
                for (int i = 0; i < array.Length; i++)
                {
                    reader.CancellationToken.ThrowIfCancellationRequested();
                    array[i] = _binaryFormatter.Deserialize(ref reader);
                }
            }
            finally
            {
                reader.Depth--;
            }

            return array;
        }

        /// <summary>
        /// 序列化数组
        /// </summary>
        /// <param name="writer">扩展二进制写入器</param>
        /// <param name="value">要序列化的数组</param>
        public override void Serialize(ref ExtenderBinaryWriter writer, T[]? value)
        {
            if (value == null || value == Array.Empty<T>())
            {
                _binaryWriterConvert.WriteNil(ref writer);
            }
            else
            {
                _binaryWriterConvert.WriteArrayHeader(ref writer, value.Length);

                for (int i = 0; i < value.Length; i++)
                {
                    _binaryFormatter.Serialize(ref writer, value[i]);
                }
            }
        }

        public override long GetLength(T[] value)
        {
            if (value == null)
            {
                return 1;
            }

            var result = Length;
            result += value.Length * _binaryFormatter.Length;
            return result;
        }
    }
}
