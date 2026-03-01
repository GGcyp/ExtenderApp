using ExtenderApp.Abstract;
using ExtenderApp.Buffer;
using ExtenderApp.Buffer.Reader;
using ExtenderApp.Contracts;

namespace ExtenderApp.Common.Compressions.LZ4
{
    /// <summary>
    /// LZ4 压缩实现类，提供基于 LZ4 算法的压缩和解压功能。
    /// </summary>
    /// <summary>
    /// 基于 LZ4 算法的压缩/解压实现。
    /// </summary>
    /// <remarks>
    /// 提供将原始字节数据压缩为 LZ4 格式的功能，并能将符合格式的数据解压回原始字节序列。该类使用内部的 LZ4Codec 进行实际的编码/解码操作，
    /// 并依赖于二进制序列化器（<see cref="IBinarySerialization"/>）对元数据（如长度、压缩类型）进行序列化与反序列化。
    /// </remarks>
    internal sealed class LZ4Compression : Compression, ILZ4Compression
    {
        /// <summary>
        /// 对输入数据进行压缩的最小长度阈值。小于此长度的数据将不会被压缩以避免压缩开销大于收益。
        /// </summary>
        private const int CompressionMinLength = 64;

        /// <summary>
        /// 压缩数据起始标记字节，用于标识数据使用 LZ4 压缩格式。
        /// </summary>
        private const byte LZ4CompressionMark = 64;

        /// <summary>
        /// 当以数组模式压缩多个块时用于标识数组结束的标志字节。
        /// </summary>
        private const byte LZ4CompressionArrayEndSign = 64;

        /// <summary>
        /// 表示 LZ4 编解码器的函数签名：接收输入字节并将结果写入提供的输出缓冲区，返回写入字节数。
        /// </summary>
        /// <param name="input">要处理的输入字节序列。</param>
        /// <param name="output">调用方提供的用于接收输出数据的缓冲区。</param>
        /// <returns>写入到 <paramref name="output"/> 的字节数。</returns>
        private delegate int LZ4Transform(ReadOnlySpan<byte> input, Span<byte> output);

        /// <summary>
        /// 指向 LZ4 编码实现的方法引用。
        /// </summary>
        private static readonly LZ4Transform LZ4CodecEncode = LZ4Codec.Encode;

        /// <summary>
        /// 指向 LZ4 解码实现的方法引用。
        /// </summary>
        private static readonly LZ4Transform LZ4CodecDecode = LZ4Codec.Decode;

        /// <summary>
        /// 用于对压缩元数据（如块长度、原始长度、压缩类型）进行序列化和反序列化的服务。
        /// </summary>
        private readonly IBinarySerialization _binarySerialization;

        /// <summary>
        /// 使用指定的二进制序列化器创建一个 <see cref="LZ4Compression"/> 实例。
        /// </summary>
        /// <param name="binarySerialization">用于序列化/反序列化元数据的序列化器，不能为空。</param>
        public LZ4Compression(IBinarySerialization binarySerialization)
        {
            _binarySerialization = binarySerialization;
        }

        #region Compress

        /// <summary>
        /// 尝试压缩指定的字节片段。若输入为空或长度小于阈值则不会执行压缩，而是直接返回合适的缓冲区表示。
        /// </summary>
        /// <param name="span">要压缩的输入字节片段。</param>
        /// <param name="output">输出的缓冲区，包含压缩后的数据或原始数据。</param>
        /// <returns>如果方法成功执行（不论是否实际进行了压缩）返回 <c>true</c>；否则返回 <c>false</c>。</returns>
        public override bool TryCompress(ReadOnlySpan<byte> span, out AbstractBuffer<byte> output)
        {
            if (span.IsEmpty)
            {
                output = AbstractBuffer<byte>.Empty;
                return true;
            }

            if (span.Length < CompressionMinLength)
            {
                // 数据太短，不做压缩，直接返回原始数据包装
                output = MemoryBlock<byte>.GetBuffer(span);
                return true;
            }

            var maxCompressedLength = LZ4Codec.MaximumOutputLength(span.Length);
            var compressBlock = MemoryBlock<byte>.GetBuffer(maxCompressedLength);
            var compressSpan = compressBlock.GetSpan(maxCompressedLength).Slice(0, maxCompressedLength);
            var compressLength = LZ4CodecEncode(span, compressSpan);
            compressBlock.Advance(compressLength);

            var sequence = SequenceBuffer<byte>.GetBuffer();
            Compress(span.Length, compressBlock, sequence);
            output = sequence;
            return true;
        }

        /// <summary>
        /// 尝试对泛型缓冲区类型进行压缩并输出为 <see cref="AbstractBuffer{byte}"/>。
        /// </summary>
        /// <typeparam name="TBuffer">输入缓冲区类型，必须实现相关缓冲区语义（包含 <see cref="Committed"/> 等）。</typeparam>
        /// <param name="input">要压缩的输入缓冲区实例。</param>
        /// <param name="output">输出的缓冲区（可能是序列或块），由方法返回。</param>
        /// <param name="compressionType">压缩模式，默认按块压缩（单块）。</param>
        /// <returns>方法是否成功执行（<c>true</c> 表示已生成 output）。</returns>
        public override bool TryCompress<TBuffer>(TBuffer input, out AbstractBuffer<byte> output, CompressionType compressionType = CompressionType.Block)
        {
            if (input == null || input.Committed == 0)
            {
                output = SequenceBuffer<byte>.Empty;
                return true;
            }

            var sequence = SequenceBuffer<byte>.GetBuffer();
            output = sequence;
            if (input is MemoryBlock<byte> memoryBlock)
            {
                Compress(memoryBlock, sequence);
            }
            else
            {
                if (compressionType == CompressionType.Block)
                {
                    Compress(input.ToMemoryBlock(), sequence);
                }
                else
                {
                    Compress(input, sequence);
                }
                return true;
            }
            return true;
        }

        /// <summary>
        /// 将指定的内存块压缩并将压缩结果写入目标序列缓冲区，方法会负责释放输入块的资源。
        /// </summary>
        /// <param name="input">包含原始（未压缩）数据的内存块。</param>
        /// <param name="output">用于追加压缩结果的序列缓冲区。</param>
        private void Compress(MemoryBlock<byte> input, SequenceBuffer<byte> output)
        {
            var maxCompressedLength = LZ4Codec.MaximumOutputLength((int)input.Committed);
            var compressBlock = MemoryBlock<byte>.GetBuffer(maxCompressedLength);
            var compressSpan = compressBlock.GetSpan(maxCompressedLength).Slice(0, maxCompressedLength);
            var compressLength = LZ4CodecEncode(input.CommittedSpan, compressSpan);
            compressBlock.Advance(compressLength);

            Compress(input.Committed, compressBlock, output);
            input.TryRelease();
        }

        /// <summary>
        /// 将已压缩块与元数据写入目标序列。输出格式包含起始标记、压缩类型、压缩后长度、原始长度以及实际压缩数据块。
        /// </summary>
        /// <param name="inputLength">原始数据长度（未压缩）。</param>
        /// <param name="compress">包含压缩数据的内存块。</param>
        /// <param name="output">用以接收最终序列的目标缓冲区。</param>
        private void Compress(long inputLength, MemoryBlock<byte> compress, SequenceBuffer<byte> output)
        {
            output.Write(LZ4CompressionMark);
            _binarySerialization.Serialize(CompressionType.Block, output);
            _binarySerialization.Serialize(compress.Committed, output);
            _binarySerialization.Serialize(inputLength, output);
            output.Append(compress);
            compress.TryRelease();
        }

        /// <summary>
        /// 将一个可能由多段组成的缓冲区压缩为数组模式，并把结果写入目标序列。每个段会单独压缩并写入长度信息，最后写入数组结束标志。
        /// </summary>
        /// <param name="input">可能由多段内存组成的输入缓冲区。</param>
        /// <param name="output">输出序列缓冲区，用于接收整个压缩数组的内容。</param>
        private void Compress(AbstractBuffer<byte> input, SequenceBuffer<byte> output)
        {
            output.Write(LZ4CompressionMark);
            _binarySerialization.Serialize(CompressionType.BlockArray, output);
            foreach (var memory in input)
            {
                CompressArray(memory.Span, output);
            }
            _binarySerialization.Serialize(LZ4CompressionArrayEndSign, output);
        }

        /// <summary>
        /// 将单个内存段压缩并把压缩长度与原始长度写入目标序列，随后追加压缩数据块本身。
        /// </summary>
        /// <param name="span">要压缩的连续字节片段。</param>
        /// <param name="buffer">目标序列缓冲区。</param>
        private void CompressArray(ReadOnlySpan<byte> span, SequenceBuffer<byte> buffer)
        {
            var maxCompressedLength = LZ4Codec.MaximumOutputLength(span.Length);
            MemoryBlock<byte> compressBlock = MemoryBlock<byte>.GetBuffer(maxCompressedLength);
            var compressSpan = compressBlock.GetSpan(maxCompressedLength).Slice(0, maxCompressedLength);
            var compressLength = LZ4CodecEncode(span, compressSpan);
            compressBlock.Advance(compressLength);

            _binarySerialization.Serialize(compressLength, buffer);
            _binarySerialization.Serialize(span.Length, buffer);
            buffer.Append(compressBlock);
            compressBlock.Dispose();
        }

        #endregion Compress

        #region Decompress

        /// <inheritdoc/>
        /// <summary>
        /// 尝试把给定的字节片段按 LZ4 格式解压为原始缓冲区。方法会根据头部元数据识别是单块压缩还是数组模式并执行相应解压。
        /// </summary>
        /// <param name="span">包含压缩数据的字节序列。</param>
        /// <param name="output">解压后的缓冲区（如果成功则为实际数据，否则为 Empty）。</param>
        /// <returns>如果解压成功返回 <c>true</c>；否则返回 <c>false</c>。</returns>
        public override bool TryDecompress(ReadOnlySpan<byte> span, out AbstractBuffer<byte> output)
        {
            if (span.IsEmpty)
            {
                output = AbstractBuffer<byte>.Empty;
                return false;
            }

            SpanReader<byte> reader = span;
            if (reader.Read() != LZ4CompressionMark)
            {
                output = AbstractBuffer<byte>.Empty;
                return false;
            }

            if (_binarySerialization.Deserialize<CompressionType>(ref reader) == CompressionType.Block)
            {
                return TryDecompressBlock(reader, out output);
            }
            else
            {
                return TryDecompressArray(reader, out output);
            }
        }

        /// <inheritdoc/>
        /// <summary>
        /// 泛型缓冲区解压入口。若输入为空则返回空缓冲区；若输入为 <see cref="MemoryBlock{byte}"/> 则直接基于其已提交的跨度执行解压。
        /// </summary>
        /// <typeparam name="TBuffer">输入缓冲类型。</typeparam>
        /// <param name="input">要解压的输入缓冲实例。</param>
        /// <param name="output">解压结果缓冲区。</param>
        /// <returns>方法是否成功执行并返回了结果缓冲区。</returns>
        public override bool TryDecompress<TBuffer>(TBuffer input, out AbstractBuffer<byte> output)
        {
            if (input == null || input.Committed == 0)
            {
                output = AbstractBuffer<byte>.Empty;
                return true;
            }

            if (input is MemoryBlock<byte> memoryBlock)
            {
                return TryDecompress(memoryBlock.CommittedSpan, out output);
            }

            return TryDecompress(input, out output);
        }

        /// <summary>
        /// 解压单个压缩块的数据。方法会读取压缩块长度与原始长度，然后解码数据到新的内存块并返回。
        /// </summary>
        /// <param name="input">用于读取压缩数据的 <see cref="SpanReader{byte}"/>。</param>
        /// <param name="output">解压后的缓冲区（若失败为 Empty）。</param>
        /// <returns>成功返回 <c>true</c>，否则返回 <c>false</c>。</returns>
        private bool TryDecompressBlock(SpanReader<byte> input, out AbstractBuffer<byte> output)
        {
            output = AbstractBuffer<byte>.Empty;
            int compressedLength = _binarySerialization.Deserialize<int>(ref input);
            if (input.Remaining < compressedLength)
            {
                return false;
            }

            int length = _binarySerialization.Deserialize<int>(ref input);
            output = MemoryBlock<byte>.GetBuffer(length);
            int decopressLength = LZ4CodecDecode(input, output.GetSpan(length).Slice(0, length));
            if (decopressLength != length)
            {
                output.TryRelease();
                output = AbstractBuffer<byte>.Empty;
                return false;
            }
            output.Advance(decopressLength);
            return true;
        }

        /// <summary>
        /// 解压以数组形式存储的多个压缩块，依次读取每个块的压缩长度和原始长度并追加解压数据到结果序列。
        /// </summary>
        /// <param name="input">用于读取压缩数组数据的 <see cref="SpanReader{byte}"/>。</param>
        /// <param name="output">解压结果（若失败返回 Empty）。</param>
        /// <returns>若成功返回 <c>true</c>，否则返回 <c>false</c>。</returns>
        private bool TryDecompressArray(SpanReader<byte> input, out AbstractBuffer<byte> output)
        {
            var sequence = SequenceBuffer<byte>.GetBuffer();
            output = sequence;

            while (input.TryPeek(out byte next) && next != LZ4CompressionArrayEndSign)
            {
                int compressedLength = _binarySerialization.Deserialize<int>(ref input);
                if (input.Remaining < compressedLength)
                {
                    sequence.TryRelease();
                    output = AbstractBuffer<byte>.Empty;
                    return false;
                }

                int length = _binarySerialization.Deserialize<int>(ref input);
                int decopressLength = LZ4CodecDecode(input, sequence.GetSpan(length).Slice(0, length));
                if (decopressLength != length)
                {
                    sequence.TryRelease();
                    output = AbstractBuffer<byte>.Empty;
                    return false;
                }
                sequence.Advance(decopressLength);
            }

            return true;
        }

        #endregion Decompress
    }
}