using ExtenderApp.Abstract;
using ExtenderApp.Data;

namespace ExtenderApp.Common.IO.Binary.Formatters
{
    /// <summary>
    /// 用于数组格式化的扩展格式化器
    /// </summary>
    /// <typeparam name="T">数组元素的类型</typeparam>
    public sealed class ArrayFormatter<T> : BinaryFormatter<T[]?>
    {
        /// <summary>
        /// 二进制格式化器
        /// </summary>
        private readonly IBinaryFormatter<T> _binaryFormatter;

        /// <summary>
        /// 初始化ArrayFormatter对象
        /// </summary>
        /// <param name="formatter">二进制格式化器</param>
        /// <param name="binarybufferConvert">二进制写入转换器</param>
        /// <param name="binarybufferConvert">二进制读取转换器</param>
        /// <param name="options">二进制选项</param>
        public ArrayFormatter(IBinaryFormatterResolver resolver, ByteBufferConvert convert, BinaryOptions options) : base(convert, options)
        {
            _binaryFormatter = resolver.GetFormatter<T>();
        }

        public override int DefaultLength => 1;

        /// <summary>
        /// 反序列化数组
        /// </summary>
        /// <param name="buffer">扩展二进制读取器</param>
        /// <returns>反序列化后的数组</returns>
        public override T[]? Deserialize(ref ByteBuffer buffer)
        {
            if (_bufferConvert.TryReadNil(ref buffer))
            {
                return default;
            }

            var len = _bufferConvert.ReadArrayHeader(ref buffer);
            if (len == 0)
            {
                return Array.Empty<T>();
            }

            var array = new T[len];
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = _binaryFormatter.Deserialize(ref buffer);
            }
            return array;
        }

        /// <summary>
        /// 序列化数组
        /// </summary>
        /// <param name="buffer">扩展二进制写入器</param>
        /// <param name="value">要序列化的数组</param>
        public override void Serialize(ref ByteBuffer buffer, T[]? value)
        {
            if (value == null || value == Array.Empty<T>())
            {
                _bufferConvert.WriteNil(ref buffer);
            }
            else
            {
                _bufferConvert.WriteArrayHeader(ref buffer, value.Length);

                for (int i = 0; i < value.Length; i++)
                {
                    _binaryFormatter.Serialize(ref buffer, value[i]);
                }
            }
        }

        public override long GetLength(T[] value)
        {
            if (value == null)
            {
                return 1;
            }

            var result = DefaultLength;
            result += value.Length * _binaryFormatter.DefaultLength;
            return result;
        }
    }
}
