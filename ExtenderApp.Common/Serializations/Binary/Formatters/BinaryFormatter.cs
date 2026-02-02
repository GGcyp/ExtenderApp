using ExtenderApp.Abstract;
using ExtenderApp.Common.Expressions;
using ExtenderApp.Data;

namespace ExtenderApp.Common.Serializations.Binary.Formatters
{
    /// <summary>
    /// 针对类型 <typeparamref name="T"/> 的二进制序列化/反序列化抽象基类（中间层）。
    /// 封装共享功能（例如 <see cref="ByteBufferConvert"/> 与 <see cref="BinaryOptions"/>），
    /// 由具体派生类实现类型特定的读写逻辑。
    /// </summary>
    /// <typeparam name="T">要序列化/反序列化的目标类型。</typeparam>
    public abstract class BinaryFormatter<T> : IBinaryFormatter<T>
    {
        /// <summary>
        /// 用于保护延迟初始化 <see cref="_methodInfoDetails"/> 的锁对象。
        /// </summary>
        private static readonly object _lock = new();

        protected const int NilLength = 1;

        /// <summary>
        /// 缓存的当前格式化器方法信息详情（懒初始化）。
        /// </summary>
        private BinaryFormatterMethodInfoDetails _methodInfoDetails;

        /// <summary>
        /// 获取当前格式化器的序列化/反序列化/长度计算方法的信息详情（线程安全，延迟初始化）。
        /// 使用双重检查锁定以避免重复初始化开销。
        /// </summary>
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

        /// <summary>
        /// 序列化的默认预估长度（字节数）。派生类可重写以提供更准确的默认值。
        /// </summary>
        public virtual int DefaultLength { get; }

        /// <summary>
        /// 初始化一个新的 <see cref="BinaryFormatter{T}"/> 实例。
        /// 基类构造函数会把 <see cref="DefaultLength"/> 设为 Nil 标记长度的默认值。
        /// </summary>
        public BinaryFormatter()
        {
            DefaultLength = NilLength;
        }

        /// <summary>
        /// 从指定的 <see cref="ByteBuffer"/> 中反序列化出 <typeparamref name="T"/> 实例。
        /// 派生类必须实现此方法。
        /// </summary>
        /// <param name="buffer">来源缓冲区（按引用传递以避免拷贝）。</param>
        /// <returns>反序列化得到的 <typeparamref name="T"/> 值。</returns>
        public abstract T Deserialize(ref ByteBuffer buffer);

        /// <summary>
        /// 将指定的 <typeparamref name="T"/> 值序列化写入到 <see cref="ByteBuffer"/>。
        /// 派生类必须实现此方法。
        /// </summary>
        /// <param name="buffer">目标缓冲区（按引用传递以避免拷贝）。</param>
        /// <param name="value">要序列化的值。</param>
        public abstract void Serialize(ref ByteBuffer buffer, T value);

        /// <summary>
        /// 获取序列化指定值所需的字节数（可以是精确值或合理估算）。
        /// 派生类必须实现此方法。
        /// </summary>
        /// <param name="value">要估算长度的值。</param>
        /// <returns>序列化该值所需的字节数。</returns>
        public abstract long GetLength(T value);

        /// <summary>
        /// 判断缓冲区当前位置是否为数组头部标记；如果不是，会回退读取位置。
        /// </summary>
        /// <param name="buffer">来源缓冲区（按引用）。</param>
        /// <returns>如果当前位置为数组头部标记返回 true，否则返回 false。</returns>
        protected static bool TryReadArrayHeader(ref ByteBuffer buffer)
        {
            return TryReadMark(ref buffer, BinaryOptions.Array);
        }

        /// <summary>
        /// 判断缓冲区当前位置是否为 MapHeader 标记；如果不是，会回退读取位置。
        /// </summary>
        /// <param name="buffer">来源缓冲区（按引用）。</param>
        /// <returns>如果当前位置为 MapHeader 标记返回 true，否则返回 false。</returns>
        protected static bool TryReadMapHeader(ref ByteBuffer buffer)
        {
            return TryReadMark(ref buffer, BinaryOptions.MapHeader);
        }

        /// <summary>
        /// 尝试读取指定的标记字节；若不是预期标记则回退读取位置并返回 false。
        /// </summary>
        /// <param name="buffer">来源缓冲区（按引用）。</param>
        /// <param name="mark">预期的标记字节值。</param>
        /// <returns>若当前位置字节与 <paramref name="mark"/> 相等返回 true，否则返回 false（并回退）。</returns>
        protected static bool TryReadMark(ref ByteBuffer buffer, byte mark)
        {
            if (buffer.Read() != mark)
            {
                buffer.Rewind(1);
                return false;
            }
            return true;
        }

        /// <summary>
        /// 向目标缓冲区写入数组头标记。
        /// </summary>
        /// <param name="buffer">目标缓冲区（按引用）。</param>
        protected static void WriteArrayHeader(ref ByteBuffer buffer)
        {
            buffer.Write(BinaryOptions.Array);
        }

        /// <summary>
        /// 向目标缓冲区写入 MapHeader 标记。
        /// </summary>
        /// <param name="buffer">目标缓冲区（按引用）。</param>
        protected static void WriteMapHeader(ref ByteBuffer buffer)
        {
            buffer.Write(BinaryOptions.MapHeader);
        }

        /// <summary>
        /// 抛出一个操作相关的 <see cref="InvalidOperationException"/>。
        /// </summary>
        /// <param name="message">异常消息。</param>
        protected static void ThrowOperationException(string message)
        {
            throw new InvalidOperationException(message);
        }

        /// <summary>
        /// 将 Nil 标记写入到目标 <see cref="ByteBuffer"/>。
        /// </summary>
        /// <param name="buffer">目标写入器（按引用）。</param>
        protected static void WriteNil(ref ByteBuffer buffer)
        {
            buffer.Write(BinaryOptions.Nil);
        }

        /// <summary>
        /// 尝试读取 Nil 标记并返回是否为 Nil；如果不是 Nil，会回退读取位置。
        /// </summary>
        /// <param name="buffer">数据来源（按引用）。</param>
        /// <returns>若为 Nil 返回 true，否则返回 false。</returns>
        protected static bool TryReadNil(ref ByteBuffer buffer)
        {
            return TryReadMark(ref buffer, BinaryOptions.Nil);
        }
    }
}