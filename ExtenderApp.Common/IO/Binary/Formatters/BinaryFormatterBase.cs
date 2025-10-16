using ExtenderApp.Abstract;
using ExtenderApp.Data;

namespace ExtenderApp.Common.IO.Binary.Formatters
{
    /// <summary>
    /// 二进制转换器基类
    /// </summary>
    /// <typeparam name="T">需要被转换的类型</typeparam>
    public abstract class BinaryFormatterBase<T> : IBinaryFormatter<T>
    {

        private static readonly object _lock = new();

        /// <summary>
        /// 复用的“序列化方法”参数类型数组：ref ByteBuffer + T。
        /// 通过复用数组减少临时分配；在锁内写入第 2 个元素为具体类型。
        /// </summary>
        private static readonly Type[] _serializeTypes = new Type[2] { typeof(ByteBuffer).MakeByRefType(), null };

        /// <summary>
        /// 复用的“反序列化方法”参数类型数组：ref ByteBuffer。
        /// </summary>
        private static readonly Type[] _deserializeTypes = new Type[1] { typeof(ByteBuffer).MakeByRefType() };

        /// <summary>
        /// 复用的“获取长度方法”参数类型数组：T。
        /// 通过复用数组减少临时分配；在锁内写入第 1 个元素为具体类型。
        /// </summary>
        private static readonly Type[] _getLengthTypes = new Type[1] { null! };

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
                            Type type = GetType();
                            Type valueType = typeof(T);
                            _serializeTypes[1] = valueType;
                            _getLengthTypes[0] = valueType;

                            var serialieMethod = type.GetMethod(nameof(Serialize), _serializeTypes)!;
                            var deserializeMethod = type.GetMethod(nameof(Deserialize), _deserializeTypes)!;
                            var getLengthMethod = type.GetMethod(nameof(GetLength), _getLengthTypes)!;

                            _methodInfoDetails = new BinaryFormatterMethodInfoDetails(serialieMethod, deserializeMethod, getLengthMethod);

                            _serializeTypes[1] = null!;
                            _getLengthTypes[0] = null!;
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
