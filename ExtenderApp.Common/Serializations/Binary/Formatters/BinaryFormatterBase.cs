using ExtenderApp.Abstract;
using ExtenderApp.Common.Expressions;
using ExtenderApp.Data;

namespace ExtenderApp.Common.Serializations.Binary.Formatters
{
    /// <summary>
    /// 二进制转换器基类
    /// </summary>
    /// <typeparam name="T">需要被转换的类型</typeparam>
    public abstract class BinaryFormatterBase<T> : IBinaryFormatter<T>
    {
        private static readonly object _lock = new();

        /// <summary>
        /// 二进制格式化器的方法信息详情。
        /// </summary>
        private BinaryFormatterMethodInfoDetails _methodInfoDetails;

        public BinaryFormatterMethodInfoDetails MethodInfoDetails
        {
            get
            {
                if (_methodInfoDetails.IsEmpty)
                {
                    lock (_lock)
                    {
                        if (_methodInfoDetails.IsEmpty)
                        {
                            var serialieMethod = this.GetMethodInfo(Serialize);
                            var deserializeMethod = this.GetMethodInfo(Deserialize);
                            var getLengthMethod = this.GetMethodInfo(GetLength);

                            _methodInfoDetails = new BinaryFormatterMethodInfoDetails(serialieMethod, deserializeMethod, getLengthMethod);
                        }
                    }
                }
                return _methodInfoDetails;
            }
        }

        public abstract int DefaultLength { get; }

        public abstract T Deserialize(ref ByteBuffer buffer);

        public abstract void Serialize(ref ByteBuffer buffer, T value);

        public abstract long GetLength(T value);
    }
}