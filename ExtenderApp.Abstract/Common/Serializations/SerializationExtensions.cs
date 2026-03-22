using System.Buffers;
using ExtenderApp.Buffer;
using ExtenderApp.Buffer.Reader;
using ExtenderApp.Contracts;

namespace ExtenderApp.Abstract
{
    /// <summary>
    /// 序列化相关的扩展方法集合。
    /// <para>包含将序列化实现注册到依赖注入容器，以及针对 <see cref="ISerialization"/> 的便捷读写扩展方法。</para>
    /// </summary>
    public static class SerializationExtensions
    {
        /// <summary>
        /// 最小合并默认缓冲区大小（字节）。当序列化结果较小时，使用此默认大小的缓冲区以避免过度分配和内存碎片。
        /// </summary>
        private const int DefaultMinCombineBufferSize = 1024;

        #region Serialize

        /// <summary>
        /// 将对象序列化到指定的 <see cref="IBufferWriter{byte}"/> 实例中（同步）。
        /// </summary>
        /// <typeparam name="T">要序列化的对象类型。</typeparam>
        /// <typeparam name="TBuffer">目标缓冲写入器类型，必须实现 <see cref="IBufferWriter{byte}"/>。</typeparam>
        /// <param name="serialization">用于序列化的 <see cref="ISerialization"/> 实例（不可为空）。</param>
        /// <param name="obj">要序列化的对象实例。</param>
        /// <param name="buffer">用于接收序列化数据的缓冲写入器。</param>
        public static void Serialize<T, TBuffer>(this ISerialization serialization, T obj, TBuffer buffer) where TBuffer : IBufferWriter<byte>
        {
            ArgumentNullException.ThrowIfNull(serialization);

            BinaryWriterAdapter writer = new(buffer);
            serialization.Serialize(ref writer, obj);
        }

        /// <summary>
        /// 将对象序列化并写入由 <see cref="IFileOperateProvider"/> 根据 <see cref="FileOperateInfo"/> 创建的文件操作器中。
        /// </summary>
        /// <typeparam name="T">要序列化的对象类型。</typeparam>
        /// <param name="serialization">用于序列化的 <see cref="ISerialization"/> 实例。</param>
        /// <param name="obj">要序列化并写入的对象。</param>
        /// <param name="provider">用于获取 <see cref="IFileOperate"/> 的提供者。</param>
        /// <param name="info">用于创建文件操作器的文件操作配置信息。</param>
        /// <param name="compression">可选的压缩实现；若提供则在写入前尝试压缩序列化结果。</param>
        /// <param name="position">写入文件的起始位置（字节偏移）。</param>
        /// <exception cref="ArgumentNullException">当 <paramref name="provider"/> 为 <c>null</c> 时抛出。</exception>
        public static void Serialize<T>(this ISerialization serialization, T obj, IFileOperateProvider provider, FileOperateInfo info, ICompression? compression = null, long position = 0)
        {
            ArgumentNullException.ThrowIfNull(provider);
            serialization.Serialize(obj, provider.GetOperate(info), compression, position);
        }

        /// <summary>
        /// 将对象序列化并写入指定的 <see cref="IFileOperate"/> 中（同步）。
        /// </summary>
        /// <typeparam name="T">要序列化的对象类型。</typeparam>
        /// <param name="serialization">用于序列化的 <see cref="ISerialization"/> 实例。</param>
        /// <param name="obj">要序列化并写入的对象。</param>
        /// <param name="operate">用于执行写入操作的 <see cref="IFileOperate"/> 实例。</param>
        /// <param name="compression">可选的压缩实现；若提供则在写入前尝试压缩序列化结果。</param>
        /// <param name="position">写入文件的起始位置（字节偏移）。</param>
        /// <exception cref="ArgumentNullException">当 <paramref name="serialization"/> 或 <paramref name="operate"/> 为 <c>null</c> 时抛出。</exception>
        /// <remarks>
        /// 方法内部：调用 <see cref="ISerialization.Serialize{T}(T?, out ByteBuffer)"/> 获取序列化缓冲， 若提供 <paramref name="compression"/> 则尝试压缩缓冲并在必要时替换为压缩结果，最后使用
        /// <see cref="IFileOperate.Write(ByteBuffer)"/> 写入并释放缓冲。
        /// </remarks>
        public static void Serialize<T>(this ISerialization serialization, T obj, IFileOperate operate, ICompression? compression = null, long position = 0)
        {
            ArgumentNullException.ThrowIfNull(serialization);
            ArgumentNullException.ThrowIfNull(operate);

            serialization.Serialize(obj, out var buffer);
            if (compression != null && compression.TryCompress(buffer, out var compressedBuffer))
            {
                buffer.TryRelease();
                buffer = compressedBuffer;
            }

            var writeResult = operate.Write(buffer, position);
            buffer.TryRelease();
            if (!writeResult.IsSuccess)
            {
                throw new InvalidOperationException($"Write failed: {writeResult.Message}");
            }
        }

        #endregion Serialize

        #region Deserialize

        /// <summary>
        /// 从给定的只读字节切片反序列化为指定类型的对象。
        /// </summary>
        /// <typeparam name="T">目标类型。</typeparam>
        /// <param name="serialization">序列化实现实例（不能为 <c>null</c>）。</param>
        /// <param name="span">包含序列化数据的字节切片。</param>
        /// <returns>反序列化得到的对象实例，若解析失败或数据为空返回 <c>null</c>（由具体实现决定）。</returns>
        public static T? Deserialize<T>(this ISerialization serialization, ReadOnlySpan<byte> span)
        {
            ArgumentNullException.ThrowIfNull(serialization);
            SpanReader<byte> readerAdapter = new(span);
            return serialization.Deserialize<T>(ref readerAdapter);
        }

        /// <summary>
        /// 从给定的 <see cref="AbstractBuffer{byte}"/> 中反序列化对象。方法在读取时会短暂冻结缓冲区以防止回收/写入冲突。
        /// </summary>
        /// <typeparam name="T">目标类型。</typeparam>
        /// <param name="serialization">序列化实现实例（不能为 <c>null</c>）。</param>
        /// <param name="buffer">包含序列化数据的缓冲区。</param>
        /// <returns>反序列化得到的对象实例，若解析失败或数据为空返回 <c>null</c>（由具体实现决定）。</returns>
        public static T? Deserialize<T>(this ISerialization serialization, AbstractBuffer<byte> buffer)
        {
            ArgumentNullException.ThrowIfNull(serialization);
            ArgumentNullException.ThrowIfNull(buffer);

            buffer.Freeze();
            buffer.FreezeWrite();
            T? result = default;
            try
            {
                if (buffer is MemoryBlock<byte> memoryBlock)
                {
                    SpanReader<byte> spanReader = new(memoryBlock.CommittedSpan);
                    result = serialization.Deserialize<T>(ref spanReader);
                }
                else
                {
                    if (buffer.Committed < DefaultMinCombineBufferSize)
                    {
                        memoryBlock = buffer.ToMemoryBlock();
                        SpanReader<byte> spanReader = new(memoryBlock.CommittedSpan);
                        result = serialization.Deserialize<T>(ref spanReader);
                        memoryBlock.TryRelease();
                    }
                    else
                    {
                        BinaryReaderAdapter readerAdapter = new(buffer.CommittedSequence);
                        result = serialization.Deserialize<T>(ref readerAdapter);
                    }
                }
                return result;
            }
            finally
            {
                buffer.Unfreeze();
                buffer.UnfreezeWrite();
            }
        }

        /// <summary>
        /// 从提供的 <see cref="AbstractBufferReader{byte}"/> 中反序列化对象，并推进读取器位置到已消费的字节数处。
        /// </summary>
        /// <typeparam name="T">目标类型。</typeparam>
        /// <param name="serialization">序列化实现实例（不能为 <c>null</c>）。</param>
        /// <param name="reader">用于读取数据的抽象缓冲读取器。</param>
        /// <returns>反序列化得到的对象实例。</returns>
        public static T? Deserialize<T>(this ISerialization serialization, AbstractBufferReader<byte> reader)
        {
            ArgumentNullException.ThrowIfNull(serialization);
            ArgumentNullException.ThrowIfNull(reader);

            T? result = default;

            if (reader is MemoryBlockReader<byte> memoryReader)
            {
                SpanReader<byte> spanReader = new(memoryReader.UnreadSpan);
                result = serialization.Deserialize<T>(ref spanReader);
                reader.Advance(spanReader.Consumed);
            }
            else
            {
                BinaryReaderAdapter readerAdapter = new(reader);
                result = serialization.Deserialize<T>(ref readerAdapter);
                reader.Advance(readerAdapter.Consumed);
            }
            return result;
        }

        /// <summary>
        /// 从 <see cref="SequenceReader{byte}"/>（按引用）中反序列化对象，并将读取器向前推进到已消费位置。
        /// </summary>
        /// <typeparam name="T">目标类型。</typeparam>
        /// <param name="serialization">序列化实现实例（不能为 <c>null</c>）。</param>
        /// <param name="reader">按引用传入的序列读取器。</param>
        /// <returns>反序列化得到的对象实例。</returns>
        public static T? Deserialize<T>(this ISerialization serialization, ref SequenceReader<byte> reader)
        {
            ArgumentNullException.ThrowIfNull(serialization);

            BinaryReaderAdapter readerAdapter = new(reader);
            T? result = serialization.Deserialize<T>(ref readerAdapter);
            reader.Advance(readerAdapter.Consumed);
            return result;
        }

        /// <summary>
        /// 从值语义的 <see cref="SequenceReader{byte}"/> 中反序列化对象（不会修改外部传入的读取器）。
        /// </summary>
        /// <typeparam name="T">目标类型。</typeparam>
        /// <param name="serialization">序列化实现实例（不能为 <c>null</c>）。</param>
        /// <param name="reader">序列读取器的副本，用于读取数据。</param>
        /// <returns>反序列化得到的对象实例。</returns>
        public static T? Deserialize<T>(this ISerialization serialization, SequenceReader<byte> reader)
        {
            ArgumentNullException.ThrowIfNull(serialization);

            BinaryReaderAdapter readerAdapter = new(reader);
            T? result = serialization.Deserialize<T>(ref readerAdapter);
            return result;
        }

        /// <summary>
        /// 从 <see cref="ReadOnlySequence{byte}"/> 中反序列化对象（支持多段数据）。
        /// </summary>
        /// <typeparam name="T">目标类型。</typeparam>
        /// <param name="serialization">序列化实现实例（不能为 <c>null</c>）。</param>
        /// <param name="input">包含序列化数据的只读序列（可能由多段组成）。</param>
        /// <returns>反序列化得到的对象实例。</returns>
        public static T? Deserialize<T>(this ISerialization serialization, ReadOnlySequence<byte> input)
        {
            ArgumentNullException.ThrowIfNull(serialization);

            BinaryReaderAdapter readerAdapter = new(input);
            T? result = serialization.Deserialize<T>(ref readerAdapter);
            return result;
        }

        /// <summary>
        /// 从指定的 <see cref="Memory{byte}"/> 中反序列化对象。
        /// </summary>
        /// <typeparam name="T">目标类型。</typeparam>
        /// <param name="serialization">序列化实现实例（不能为 <c>null</c>）。</param>
        /// <param name="memory">包含序列化数据的内存片。</param>
        /// <returns>反序列化得到的对象实例。</returns>
        public static T? Deserialize<T>(this ISerialization serialization, Memory<byte> memory)
        {
            ArgumentNullException.ThrowIfNull(serialization);

            var buffer = MemoryBlock<byte>.GetBuffer(memory);
            buffer.Unfreeze();
            try
            {
                BinaryReaderAdapter readerAdapter = new(buffer);
                T? result = serialization.Deserialize<T>(ref readerAdapter);
                return result;
            }
            finally
            {
                buffer.TryRelease();
            }
        }

        /// <summary>
        /// 从指定的 <see cref="ReadOnlyMemory{byte}"/> 中反序列化对象。
        /// </summary>
        /// <typeparam name="T">目标类型。</typeparam>
        /// <param name="serialization">序列化实现实例（不能为 <c>null</c>）。</param>
        /// <param name="memory">包含序列化数据的只读内存片。</param>
        /// <returns>反序列化得到的对象实例。</returns>
        public static T? Deserialize<T>(this ISerialization serialization, ReadOnlyMemory<byte> memory)
        {
            ArgumentNullException.ThrowIfNull(serialization);

            var buffer = MemoryBlock<byte>.GetBuffer(memory);
            try
            {
                BinaryReaderAdapter readerAdapter = new(buffer);
                T? result = serialization.Deserialize<T>(ref readerAdapter);
                return result;
            }
            finally
            {
                buffer.TryRelease();
            }
        }

        #endregion Deserialize

        #region Deserialize And Compression

        /// <summary>
        /// 从 <paramref name="span"/> 中读取数据；若提供的 <paramref name="compression"/> 能解压数据则先解压再反序列化，否则直接反序列化原始数据（适用于内存块）。
        /// </summary>
        /// <typeparam name="T">目标对象类型。</typeparam>
        /// <param name="serialization">用于反序列化的 <see cref="ISerialization"/> 实例（不可为空）。</param>
        /// <param name="span">输入的字节切片。</param>
        /// <param name="compression">用于尝试解压的压缩实现（不可为空）。</param>
        /// <returns>反序列化得到的对象；若输入为空或解析失败由具体实现决定是否返回 <c>null</c>。</returns>
        /// <exception cref="ArgumentNullException">当 <paramref name="serialization"/> 或 <paramref name="compression"/> 为 <c>null</c> 时抛出。</exception>
        public static T? Deserialize<T>(this ISerialization serialization, ICompression compression, ReadOnlySpan<byte> span)
        {
            ArgumentNullException.ThrowIfNull(serialization);
            ArgumentNullException.ThrowIfNull(compression);
            if (span.IsEmpty) return default;

            if (compression.TryDecompress(span, out var buffer))
            {
                var reader = buffer.GetReader();
                var result = serialization.Deserialize<T>(reader);
                buffer.TryRelease();
                return result;
            }
            return serialization.Deserialize<T>(span);
        }

        /// <summary>
        /// 使用提供的压缩器先尝试解压内存数据，然后反序列化为目标类型。
        /// </summary>
        /// <typeparam name="T">目标对象类型。</typeparam>
        /// <param name="serialization">用于反序列化的 <see cref="ISerialization"/> 实例（不可为空）。</param>
        /// <param name="compression">用于解压的 <see cref="ICompression"/> 实例（不可为空）。</param>
        /// <param name="memory">输入的只读内存。</param>
        /// <returns>反序列化得到的对象实例；若数据为空或解析失败可返回 <c>null</c>（由具体实现决定）。</returns>
        /// <exception cref="ArgumentNullException">当 <paramref name="serialization"/> 或 <paramref name="compression"/> 为 <c>null</c> 时抛出。</exception>
        public static T? Deserialize<T>(this ISerialization serialization, ICompression compression, ReadOnlyMemory<byte> memory)
        {
            ArgumentNullException.ThrowIfNull(serialization);
            ArgumentNullException.ThrowIfNull(compression);
            if (memory.IsEmpty) return default;

            if (compression.TryDecompress(memory.Span, out var buffer))
            {
                var result = serialization.Deserialize<T>(buffer);
                buffer.TryRelease();
                return result;
            }

            return serialization.Deserialize<T>(memory);
        }

        /// <summary>
        /// 使用提供的压缩器先尝试解压序列数据，然后反序列化为目标类型。
        /// </summary>
        /// <typeparam name="T">目标对象类型。</typeparam>
        /// <param name="serialization">用于反序列化的 <see cref="ISerialization"/> 实例（不可为空）。</param>
        /// <param name="compression">用于解压的 <see cref="ICompression"/> 实例（不可为空）。</param>
        /// <param name="input">输入的 <see cref="ReadOnlySequence{byte}"/>。</param>
        /// <returns>反序列化得到的对象实例；若数据为空或解析失败可返回 <c>null</c>（由具体实现决定）。</returns>
        /// <exception cref="ArgumentNullException">当 <paramref name="serialization"/> 或 <paramref name="compression"/> 为 <c>null</c> 时抛出。</exception>
        public static T? Deserialize<T>(this ISerialization serialization, ICompression compression, ReadOnlySequence<byte> input)
        {
            ArgumentNullException.ThrowIfNull(serialization);
            ArgumentNullException.ThrowIfNull(compression);
            if (input.IsEmpty) return default;

            if (compression.TryDecompress(input, out var buffer))
            {
                var result = serialization.Deserialize<T>(buffer);
                buffer.TryRelease();
                return result;
            }
            return serialization.Deserialize<T>(input);
        }

        public static T? Deserialize<T>(this ISerialization serialization, ICompression compression, AbstractBufferReader<byte> reader)
        {
            ArgumentNullException.ThrowIfNull(serialization);
            ArgumentNullException.ThrowIfNull(compression);
            ArgumentNullException.ThrowIfNull(reader);

            T? result = default;
            if (compression.TryDecompress(reader.UnreadSequence, out var buffer))
            {
                result = serialization.Deserialize<T>(buffer);
                buffer.TryRelease();
                return result;
            }

            BinaryReaderAdapter readerAdapter = new(reader);
            result = serialization.Deserialize<T>(ref readerAdapter);
            reader.Advance(readerAdapter.Consumed);
            return result;
        }

        public static T? Deserialize<T>(this ISerialization serialization, ICompression compression, ref SequenceReader<byte> reader)
        {
            ArgumentNullException.ThrowIfNull(serialization);
            ArgumentNullException.ThrowIfNull(compression);
            if (reader.Remaining == 0) return default;

            T? result;
            if (compression.TryDecompress(reader.UnreadSequence, out var buffer))
            {
                result = serialization.Deserialize<T>(buffer);
                buffer.TryRelease();
                return result;
            }

            BinaryReaderAdapter readerAdapter = new(reader);
            result = serialization.Deserialize<T>(ref readerAdapter);
            reader.Advance(readerAdapter.Consumed);
            return result;
        }

        #endregion Deserialize And Compression

        #region Deserialize And FileOperate

        /// <summary>
        /// 从由 <see cref="IFileOperateProvider"/> 根据 <see cref="FileOperateInfo"/> 创建的文件操作器中读取数据并反序列化为目标类型（从指定偏移到文件末尾）。
        /// </summary>
        /// <typeparam name="T">目标对象类型。</typeparam>
        /// <param name="serialization">用于反序列化的 <see cref="ISerialization"/> 实例。</param>
        /// <param name="provider">用于获取 <see cref="IFileOperate"/> 的提供者。</param>
        /// <param name="info">用于创建文件操作器的文件操作配置信息。</param>
        /// <param name="position">读取的起始位置（字节偏移）。</param>
        /// <returns>反序列化得到的对象实例；若数据为空或解析失败可返回 <c>null</c>（由具体实现决定）。</returns>
        /// <exception cref="ArgumentNullException">当 <paramref name="provider"/> 为 <c>null</c> 时抛出。</exception>
        public static T? Deserialize<T>(this ISerialization serialization, IFileOperateProvider provider, FileOperateInfo info, long position = 0)
        {
            return serialization.Deserialize<T>(provider.GetOperate(info), position);
        }

        /// <summary>
        /// 从由 <see cref="IFileOperateProvider"/> 根据 <see cref="FileOperateInfo"/> 创建的文件操作器中读取指定长度的数据并反序列化为目标类型（同步）。
        /// </summary>
        /// <typeparam name="T">目标对象类型。</typeparam>
        /// <param name="serialization">用于反序列化的 <see cref="ISerialization"/> 实例。</param>
        /// <param name="provider">用于获取 <see cref="IFileOperate"/> 的提供者。</param>
        /// <param name="info">用于创建文件操作器的文件操作配置信息。</param>
        /// <param name="position">读取的起始位置（字节偏移）。</param>
        /// <param name="length">要读取的字节数。</param>
        /// <returns>反序列化得到的对象实例；若数据为空或解析失败可返回 <c>null</c>（由具体实现决定）。</returns>
        /// <exception cref="ArgumentNullException">当 <paramref name="provider"/> 为 <c>null</c> 时抛出。</exception>
        public static T? Deserialize<T>(this ISerialization serialization, IFileOperateProvider provider, FileOperateInfo info, long position, int length)
        {
            return serialization.Deserialize<T>(provider.GetOperate(info), position, length);
        }

        /// <summary>
        /// 从指定的 <see cref="IFileOperate"/> 中读取（从 <paramref name="position"/> 到文件末尾）并反序列化为目标类型。
        /// </summary>
        /// <typeparam name="T">目标对象类型。</typeparam>
        /// <param name="serialization">用于反序列化的 <see cref="ISerialization"/> 实例。</param>
        /// <param name="operate">用于执行读取操作的 <see cref="IFileOperate"/> 实例。</param>
        /// <param name="position">起始读取位置（字节偏移）。</param>
        /// <returns>反序列化得到的对象实例；若数据为空或解析失败可返回 <c>null</c>（由具体实现决定）。</returns>
        /// <exception cref="ArgumentNullException">当 <paramref name="serialization"/> 或 <paramref name="operate"/> 为 <c>null</c> 时抛出。</exception>
        public static T? Deserialize<T>(this ISerialization serialization, IFileOperate operate, long position = 0)
        {
            ArgumentNullException.ThrowIfNull(operate);

            return serialization.Deserialize<T>(operate, position, (int)(operate.Info.Length - position));
        }

        /// <summary>
        /// 从指定的 <see cref="IFileOperate"/> 中读取指定长度的数据并反序列化为目标类型（同步）。
        /// </summary>
        /// <typeparam name="T">目标对象类型。</typeparam>
        /// <param name="serialization">用于反序列化的 <see cref="ISerialization"/> 实例。</param>
        /// <param name="operate">用于执行读取操作的 <see cref="IFileOperate"/> 实例。</param>
        /// <param name="position">起始读取位置（字节偏移）。</param>
        /// <param name="length">要读取的字节数。</param>
        /// <returns>反序列化得到的对象实例；若数据为空或解析失败可返回 <c>null</c>（由具体实现决定）。</returns>
        /// <exception cref="ArgumentNullException">当 <paramref name="serialization"/> 或 <paramref name="operate"/> 为 <c>null</c> 时抛出。</exception>
        /// <remarks>
        /// 内部流程：从 <paramref name="operate"/> 读取到 <see cref="ByteBuffer"/>，调用 <see cref="ISerialization.Deserialize{T}(ReadOnlySequence{byte})"/> 反序列化，并释放缓冲。
        /// </remarks>
        public static T? Deserialize<T>(this ISerialization serialization, IFileOperate operate, long position, int length)
        {
            ArgumentNullException.ThrowIfNull(serialization);
            ArgumentNullException.ThrowIfNull(operate);

            var result = operate.Read(length, out var buffer, position);
            try
            {
                if (!result)
                    return default;
                return serialization.Deserialize<T>(buffer);
            }
            finally
            {
                buffer.TryRelease();
            }
        }

        /// <summary>
        /// 从指定的 <see cref="IFileOperate"/> 中读取数据（从 <paramref name="position"/> 到文件末尾），先尝试解压再反序列化为目标类型。
        /// </summary>
        /// <typeparam name="T">目标对象类型。</typeparam>
        /// <param name="serialization">用于反序列化的 <see cref="ISerialization"/> 实例。</param>
        /// <param name="compression">用于解压的 <see cref="ICompression"/> 实例。</param>
        /// <param name="operate">用于执行读取操作的 <see cref="IFileOperate"/> 实例。</param>
        /// <param name="position">起始读取位置（字节偏移）。</param>
        /// <returns>反序列化得到的对象实例；若数据为空或解析失败可返回 <c>null</c>（由具体实现决定）。</returns>
        public static T? Deserialize<T>(this ISerialization serialization, ICompression compression, IFileOperate operate, long position = 0)
        {
            ArgumentNullException.ThrowIfNull(operate);

            return serialization.Deserialize<T>(compression, operate, position, (int)(operate.Info.Length - position));
        }

        /// <summary>
        /// 从指定的 <see cref="IFileOperate"/> 中读取指定长度的数据，先尝试解压再反序列化为目标类型。
        /// </summary>
        /// <typeparam name="T">目标对象类型。</typeparam>
        /// <param name="serialization">用于反序列化的 <see cref="ISerialization"/> 实例。</param>
        /// <param name="compression">用于解压的 <see cref="ICompression"/> 实例。</param>
        /// <param name="operate">用于执行读取操作的 <see cref="IFileOperate"/> 实例。</param>
        /// <param name="position">起始读取位置（字节偏移）。</param>
        /// <param name="length">要读取的字节数。</param>
        /// <returns>反序列化得到的对象实例；若数据为空或解析失败可返回 <c>null</c>（由具体实现决定）。</returns>
        public static T? Deserialize<T>(this ISerialization serialization, ICompression compression, IFileOperate operate, long position, int length)
        {
            ArgumentNullException.ThrowIfNull(operate);

            var buffer = MemoryBlock<byte>.GetBuffer(length);
            try
            {
                operate.Read(length, buffer, position);

                if (compression.TryDecompress(buffer.CommittedSpan, out var decompressed))
                {
                    try
                    {
                        var result = serialization.Deserialize<T>(decompressed);
                        return result;
                    }
                    finally
                    {
                        decompressed.TryRelease();
                    }
                }

                return serialization.Deserialize<T>(buffer.CommittedSpan);
            }
            finally
            {
                buffer.TryRelease();
            }
        }

        #endregion Deserialize And FileOperate
    }
}