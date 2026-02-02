using ExtenderApp.Abstract;
using ExtenderApp.Data;

namespace ExtenderApp.Common.Serializations.Binary.Formatters
{
    /// <summary>
    /// 用于数组格式化的扩展格式化器
    /// </summary>
    /// <typeparam name="T">数组元素的类型</typeparam>
    public sealed class ArrayFormatter<T> : ResolverFormatter<T[]?>
    {
        /// <summary>
        /// 二进制格式化器
        /// </summary>
        private readonly IBinaryFormatter<T> _t;

        private readonly IBinaryFormatter<int> _int;

        /// <summary>
        /// 初始化ArrayFormatter对象
        /// </summary>
        /// <param name="formatter">二进制格式化器</param>
        /// <param name="binarybufferConvert">二进制写入转换器</param>
        /// <param name="binarybufferConvert">二进制读取转换器</param>
        /// <param name="options">二进制选项</param>
        public ArrayFormatter(IBinaryFormatterResolver resolver) : base(resolver)
        {
            _t = GetFormatter<T>();
            _int = GetFormatter<int>();
        }

        public override int DefaultLength => 1;

        /// <summary>
        /// 反序列化数组
        /// </summary>
        /// <param name="buffer">扩展二进制读取器</param>
        /// <returns>反序列化后的数组</returns>
        public override T[]? Deserialize(ref ByteBuffer buffer)
        {
            if (TryReadNil(ref buffer))
            {
                return default;
            }

            if (!TryReadArrayHeader(ref buffer))
            {
                throw new InvalidOperationException("数据格式不匹配，无法反序列化为数组");
            }

            var len = _int.Deserialize(ref buffer);
            if (len == 0)
            {
                return Array.Empty<T>();
            }

            var array = new T[len];
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = _t.Deserialize(ref buffer);
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
                WriteNil(ref buffer);
            }
            else
            {
                WriteArrayHeader(ref buffer);
                _int.Serialize(ref buffer, value.Length);
                for (int i = 0; i < value.Length; i++)
                {
                    _t.Serialize(ref buffer, value[i]);
                }
            }
        }

        public override long GetLength(T[]? value)
        {
            if (value == null)
            {
                return NilLength;
            }

            long result = _int.GetLength(value.Length) + 1;
            for (int i = 0; i < value.Length; i++)
            {
                result += _t.GetLength(value[i]);
            }
            return result;
        }
    }
}