using ExtenderApp.Abstract;
using ExtenderApp.Buffer;
using ExtenderApp.Common.Expressions;
using ExtenderApp.Contracts;

namespace ExtenderApp.Common.Serializations.Binary.Formatters
{
    /// <summary>
    /// 针对类型 <typeparamref name="T"/> 的二进制序列化/反序列化抽象基类（中间层）。 封装共享功能（如常用标记读写及 <see cref="BinaryOptions"/> 配置），由具体派生类实现类型特定的读写逻辑。
    /// </summary>
    /// <remarks>
    /// 本基类支持基于 <see cref="AbstractBuffer{byte}"/> 序列与基于 <see cref="SpanWriter{byte}"/> / <see cref="SpanReader{byte}"/> 的双重读写接口。
    /// 实现者可以直接操作栈上写入器（按引用传递）以实现高性能零拷贝解析，同时也提供对顺序缓冲的兼容性处理。 实现者在重写抽象方法时，应确保对位置推进语义（Advance/Consumed）的遵守。
    /// </remarks>
    /// <typeparam name="T">要序列化/反序列化的目标类型。</typeparam>
    public abstract class BinaryFormatter<T> : IBinaryFormatter<T>
    {
        private delegate void SerializeSpanMethod(ref SpanWriter<byte> writer, T value);

        private delegate T DeserializeSpanMethod(ref SpanReader<byte> reader);

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
        /// 获取当前格式化器的序列化/反序列化/长度计算方法的信息详情（线程安全，延迟初始化）。 使用双重检查锁定以避免重复初始化开销。
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
                            var serializeBuffer = this.GetMethodInfo((Action<AbstractBuffer<byte>, T>)Serialize);
                            var serializeSpan = this.GetMethodInfo((SerializeSpanMethod)Serialize);
                            var deserializeBuffer = this.GetMethodInfo((Func<AbstractBufferReader<byte>, T>)Deserialize);
                            var deserializeSpan = this.GetMethodInfo((DeserializeSpanMethod)Deserialize);
                            var getLength = this.GetMethodInfo(GetLength);

                            _methodInfoDetails = new BinaryFormatterMethodInfoDetails(serializeBuffer, serializeSpan, deserializeBuffer, deserializeSpan, getLength);
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
        /// 初始化一个新的 <see cref="BinaryFormatter{T}"/> 实例。 基类构造函数会把 <see cref="DefaultLength"/> 设为 Nil 标记长度的默认值。
        /// </summary>
        public BinaryFormatter()
        {
            DefaultLength = NilLength;
        }

        /// <inheritdoc/>
        /// <param name="buffer">目标顺序缓冲，数据将被写入该缓冲并推进其写入位置。</param>
        /// <param name="value">要序列化的实例值。</param>
        public abstract void Serialize(AbstractBuffer<byte> buffer, T value);

        /// <inheritdoc/>
        /// <param name="writer">目标栈上写入器（按引用），方法将在写入后推进其位置。</param>
        /// <param name="value">要序列化的实例值。</param>
        public abstract void Serialize(ref SpanWriter<byte> writer, T value);

        /// <inheritdoc/>
        /// <param name="reader">来源顺序缓冲读取器，方法将从中读取数据并推进其已消费位置。</param>
        /// <returns>反序列化得到的 <typeparamref name="T"/> 实例。</returns>
        public abstract T Deserialize(AbstractBufferReader<byte> reader);

        /// <inheritdoc/>
        /// <param name="reader">来源栈上读取器（按引用），方法将在读取后推进其位置。</param>
        /// <returns>反序列化得到的 <typeparamref name="T"/> 实例。</returns>
        public abstract T Deserialize(ref SpanReader<byte> reader);

        /// <summary>
        /// 获取序列化指定值所需的字节数（可以是精确值或合理估算）。 派生类必须实现此方法。
        /// </summary>
        /// <param name="value">要估算长度的值。</param>
        /// <returns>序列化该值所需的字节数。</returns>
        public abstract long GetLength(T value);

        /// <summary>
        /// 尝试从栈上读取器判断当前位置是否为数组头部标记；如果不是，则回退读取位置。
        /// </summary>
        /// <param name="reader">来源栈上读取器。</param>
        /// <returns>若匹配数组标记返回 true，否则返回 false 并回退。</returns>
        protected static bool TryReadArrayHeader(ref SpanReader<byte> reader)
        {
            return TryReadMark(ref reader, BinaryOptions.Array);
        }

        /// <summary>
        /// 尝试从栈上读取器判断当前位置是否为字典（Map）头部标记；如果不是，则回退读取位置。
        /// </summary>
        /// <param name="reader">来源栈上读取器。</param>
        /// <returns>若匹配字典头标记返回 true，否则返回 false 并回退。</returns>
        protected static bool TryReadMapHeader(ref SpanReader<byte> reader)
        {
            return TryReadMark(ref reader, BinaryOptions.MapHeader);
        }

        /// <summary>
        /// 尝试从栈上读取器判断当前是否为 Nil 标记；如果不是，则回退读取位置。
        /// </summary>
        /// <param name="reader">数据来源栈上读取器。</param>
        /// <returns>若匹配 Nil 标记返回 true，否则返回 false 并回退。</returns>
        protected static bool TryReadNil(ref SpanReader<byte> reader)
        {
            return TryReadMark(ref reader, BinaryOptions.Nil);
        }

        /// <summary>
        /// 尝试从栈上读取器读取指定标记位；匹配失败时自动回退一个字节。
        /// </summary>
        /// <param name="reader">栈上读取器。</param>
        /// <param name="mark">期望的标记字节。</param>
        /// <returns>匹配成功返回 true，否则返回 false。</returns>
        protected static bool TryReadMark(ref SpanReader<byte> reader, byte mark)
        {
            if (reader.Read() != mark)
            {
                reader.Rewind(1);
                return false;
            }
            return true;
        }

        /// <summary>
        /// 尝试从顺序缓冲读取器判断当前位置是否为数组头部标记；如果不是，则回退读取位置。
        /// </summary>
        /// <param name="reader">来源顺序缓冲读取器。</param>
        /// <returns>匹配成功返回 true，否则返回 false。</returns>
        protected static bool TryReadArrayHeader(AbstractBufferReader<byte> reader)
        {
            return TryReadMark(reader, BinaryOptions.Array);
        }

        /// <summary>
        /// 尝试从顺序缓冲读取器判断当前位置是否为字典（Map）头部标记；如果不是，则回退读取位置。
        /// </summary>
        /// <param name="reader">来源顺序缓冲读取器。</param>
        /// <returns>匹配成功返回 true，否则返回 false。</returns>
        protected static bool TryReadMapHeader(AbstractBufferReader<byte> reader)
        {
            return TryReadMark(reader, BinaryOptions.MapHeader);
        }

        /// <summary>
        /// 尝试从顺序缓冲读取器判断当前位置是否为 Nil 标记；如果不是，则回退读取位置。
        /// </summary>
        /// <param name="reader">来源顺序缓冲读取器。</param>
        /// <returns>匹配成功返回 true，否则返回 false。</returns>
        protected static bool TryReadNil(AbstractBufferReader<byte> reader)
        {
            return TryReadMark(reader, BinaryOptions.Nil);
        }

        /// <summary>
        /// 尝试从顺序缓冲读取器读取指定标记位；匹配失败时回退位置。
        /// </summary>
        /// <param name="reader">顺序缓冲读取器。</param>
        /// <param name="mark">期望的标记字节。</param>
        /// <returns>匹配成功返回 true，否则返回 false。</returns>
        protected static bool TryReadMark(AbstractBufferReader<byte> reader, byte mark)
        {
            ArgumentNullException.ThrowIfNull(reader, nameof(reader));
            if (reader.TryPeek(out byte value) && value == mark)
            {
                reader.Read();
                return true;
            }
            return false;
        }

        /// <summary>
        /// 向栈上写入器写入数组头标记并推进位置。
        /// </summary>
        /// <param name="writer">目标栈上写入器。</param>
        protected static void WriteArrayHeader(ref SpanWriter<byte> writer)
        {
            writer.Write(BinaryOptions.Array);
        }

        /// <summary>
        /// 向栈上写入器写入字典（Map）头标记并推进位置。
        /// </summary>
        /// <param name="writer">目标栈上写入器。</param>
        protected static void WriteMapHeader(ref SpanWriter<byte> writer)
        {
            writer.Write(BinaryOptions.MapHeader);
        }

        /// <summary>
        /// 向栈上写入器写入 Nil 标记位并推进位置。
        /// </summary>
        /// <param name="writer">目标栈上写入器。</param>
        protected static void WriteNil(ref SpanWriter<byte> writer)
        {
            writer.Write(BinaryOptions.Nil);
        }

        /// <summary>
        /// 向顺序缓冲写入数组头标记并推进位置。
        /// </summary>
        /// <param name="writer">目标顺序缓冲。</param>
        protected static void WriteArrayHeader(AbstractBuffer<byte> writer)
        {
            writer.Write(BinaryOptions.Array);
        }

        /// <summary>
        /// 向顺序缓冲写入字典（Map）头标记并推进位置。
        /// </summary>
        /// <param name="writer">目标顺序缓冲。</param>
        protected static void WriteMapHeader(AbstractBuffer<byte> writer)
        {
            writer.Write(BinaryOptions.MapHeader);
        }

        /// <summary>
        /// 向顺序缓冲写入 Nil 标记位并推进位置。
        /// </summary>
        /// <param name="writer">目标顺序缓冲。</param>
        protected static void WriteNil(AbstractBuffer<byte> writer)
        {
            writer.Write(BinaryOptions.Nil);
        }

        /// <summary>
        /// 抛出一个操作相关的 <see cref="InvalidOperationException"/>。
        /// </summary>
        /// <param name="message">异常消息。</param>
        protected static void ThrowOperationException(string message)
        {
            throw new InvalidOperationException(message);
        }
    }
}