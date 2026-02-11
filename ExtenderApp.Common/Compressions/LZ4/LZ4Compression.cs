using ExtenderApp.Abstract;
using ExtenderApp.Buffer;
using ExtenderApp.Buffer.Reader;
using ExtenderApp.Contracts;

namespace ExtenderApp.Common.Compressions.LZ4
{
    /// <summary>
    /// LZ4 压缩实现类，提供基于 LZ4 算法的压缩和解压功能。
    /// </summary>
    internal class LZ4Compression : Compression, ILZ4Compression
    {
        /// <summary>
        /// 压缩文件的最小长度
        /// </summary>
        private const int CompressionMinLength = 64;

        private const byte LZ4CompressionMark = 64;
        private const byte LZ4CompressionArrayEndSign = 64;

        /// <summary>
        /// LZ4 变换委托签名。
        /// </summary>
        /// <param name="input">输入只读字节序列。</param>
        /// <param name="output">输出目标缓冲区（由调用方预先分配）。</param>
        /// <returns>实际写入到 <paramref name="output"/> 的字节数。</returns>
        private delegate int LZ4Transform(ReadOnlySpan<byte> input, Span<byte> output);

        /// <summary>
        /// LZ4 编码实现（压缩）。
        /// </summary>
        private static readonly LZ4Transform LZ4CodecEncode = LZ4Codec.Encode;

        /// <summary>
        /// LZ4 解码实现（解压）。
        /// </summary>
        private static readonly LZ4Transform LZ4CodecDecode = LZ4Codec.Decode;

        private readonly IBinarySerialization _binarySerialization;

        public LZ4Compression(IBinarySerialization binarySerialization)
        {
            _binarySerialization = binarySerialization;
        }

        #region Compress

        /// <inheritdoc/>
        public override bool TryCompress(ReadOnlySpan<byte> span, out AbstractBuffer<byte> output)
        {
            if (span.IsEmpty)
            {
                output = AbstractBuffer<byte>.Empty;
                return true;
            }

            if (span.Length < CompressionMinLength)
            {
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

        /// <inheritdoc/>
        public override bool TryCompress(AbstractBuffer<byte> input, out AbstractBuffer<byte> output, CompressionType compressionType = CompressionType.Block)
        {
            if (input == null || input.Committed == 0)
            {
                output = AbstractBuffer<byte>.Empty;
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
                    //Compress(sequenceBuffer, sequence);
                }
                return true;
            }
            return true;
        }

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

        private void Compress(long inputLength, MemoryBlock<byte> compress, SequenceBuffer<byte> output)
        {
            output.Write(LZ4CompressionMark);
            _binarySerialization.Serialize(CompressionType.Block, output);
            _binarySerialization.Serialize(compress.Committed, output);
            _binarySerialization.Serialize(inputLength, output);
            output.Append(compress);
            compress.TryRelease();
        }

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
        public override bool TryDecompress(AbstractBuffer<byte> input, out AbstractBuffer<byte> output)
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