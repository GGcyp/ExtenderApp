using System.Buffers;
using ExtenderApp.Abstract;
using ExtenderApp.Data;

namespace ExtenderApp.Common.Compressions.LZ4
{
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
        public override bool TryCompress(ReadOnlySpan<byte> input, out ByteBlock block)
        {
            block = new();
            if (input.Length < CompressionMinLength)
            {
                return false;
            }

            Compress(input, out block);
            return true;
        }

        /// <inheritdoc/>
        public override bool TryCompress(ReadOnlyMemory<byte> input, out ByteBlock block)
        {
            block = new();
            if (input.Length < CompressionMinLength)
            {
                return false;
            }

            Compress(input.Span, out block);
            return true;
        }

        /// <inheritdoc/>
        public override bool TryCompress(ReadOnlySequence<byte> sequence, out ByteBuffer buffer, CompressionType compression = CompressionType.BlockArray)
        {
            buffer = new();
            if (sequence.Length < CompressionMinLength || compression == CompressionType.None)
            {
                return false;
            }

            if (sequence.IsSingleSegment)
            {
                Compress(sequence.First.Span, out buffer);
                return true;
            }

            if (compression == CompressionType.Block)
            {
                ByteBlock compressBlock = new((int)sequence.Length);
                foreach (var item in sequence)
                {
                    compressBlock.Write(item);
                }

                Compress(compressBlock.CommittedSpan, out buffer);
                compressBlock.Dispose();
                return true;
            }

            buffer = new();
            buffer.Write(LZ4CompressionMark);
            _binarySerialization.Serialize(CompressionType.BlockArray, ref buffer);
            foreach (var item in sequence)
            {
                CompressArray(item.Span, ref buffer);
            }
            _binarySerialization.Serialize(LZ4CompressionArrayEndSign, ref buffer);
            return true;
        }

        private void Compress(ReadOnlySpan<byte> span, out ByteBlock block)
        {
            var maxCompressedLength = LZ4Codec.MaximumOutputLength(span.Length);
            ByteBlock compressBlock = new(maxCompressedLength);
            var compressSpan = compressBlock.GetSpan(maxCompressedLength).Slice(0, maxCompressedLength);
            var compressLength = LZ4CodecEncode(span, compressSpan);
            compressBlock.Advance(compressLength);

            block = new(compressLength);
            block.Write(LZ4CompressionMark);
            _binarySerialization.Serialize(CompressionType.Block, ref block);
            _binarySerialization.Serialize(compressLength, ref block);
            _binarySerialization.Serialize(span.Length, ref block);
            block.Write(compressBlock);
            compressBlock.Dispose();
        }

        private void Compress(ReadOnlySpan<byte> span, out ByteBuffer buffer)
        {
            var maxCompressedLength = LZ4Codec.MaximumOutputLength(span.Length);
            ByteBlock compressBlock = new(maxCompressedLength);
            var compressSpan = compressBlock.GetSpan(maxCompressedLength).Slice(0, maxCompressedLength);
            var compressLength = LZ4CodecEncode(span, compressSpan);
            compressBlock.Advance(compressLength);

            buffer = new();
            buffer.Write(LZ4CompressionMark);
            _binarySerialization.Serialize(CompressionType.Block, ref buffer);
            _binarySerialization.Serialize(compressLength, ref buffer);
            _binarySerialization.Serialize(span.Length, ref buffer);
            buffer.Write(compressBlock);
            compressBlock.Dispose();
        }

        private void CompressArray(ReadOnlySpan<byte> span, ref ByteBuffer buffer)
        {
            var maxCompressedLength = LZ4Codec.MaximumOutputLength(span.Length);
            ByteBlock compressBlock = new(maxCompressedLength);
            var compressSpan = compressBlock.GetSpan(maxCompressedLength).Slice(0, maxCompressedLength);
            var compressLength = LZ4CodecEncode(span, compressSpan);
            compressBlock.Advance(compressLength);

            _binarySerialization.Serialize(compressLength, ref buffer);
            _binarySerialization.Serialize(span.Length, ref buffer);
            buffer.Write(compressBlock);
            compressBlock.Dispose();
        }

        #endregion Compress

        #region Decompress

        /// <inheritdoc/>
        public override bool TryDecompress(ReadOnlySpan<byte> input, out ByteBlock block)
        {
            block = new();
            if (_binarySerialization.Deserialize<byte>(input) != LZ4CompressionMark)
            {
                return false;
            }
            ByteBlock inputBlock = new(input);
            bool result = TryDecompress(inputBlock, out block);
            inputBlock.Dispose();
            return result;
        }

        /// <inheritdoc/>
        public override bool TryDecompress(ReadOnlyMemory<byte> input, out ByteBlock block)
        {
            block = new();
            if (_binarySerialization.Deserialize<byte>(input) != LZ4CompressionMark)
            {
                return false;
            }
            ByteBlock inputBlock = new(input);
            bool result = TryDecompress(inputBlock, out block);
            inputBlock.Dispose();
            return result;
        }

        /// <inheritdoc/>
        public override bool TryDecompress(ReadOnlySequence<byte> input, out ByteBuffer buffer)
        {
            buffer = new();
            if (_binarySerialization.Deserialize<byte>(input) != LZ4CompressionMark)
            {
                return false;
            }
            return TryDecompress(new(input), out buffer);
        }

        private bool TryDecompress(ByteBlock input, out ByteBlock output)
        {
            output = new();
            if (_binarySerialization.Deserialize<byte>(ref input) != LZ4CompressionMark)
            {
                return false;
            }

            CompressionType compressionType = _binarySerialization.Deserialize<CompressionType>(ref input);

            switch (compressionType)
            {
                case CompressionType.Block:
                    return TryDecompressBlock(input, out output);

                case CompressionType.BlockArray:
                    return TryDecompressArray(input, out output);
            }

            return false;
        }

        private bool TryDecompressBlock(ByteBlock input, out ByteBlock output)
        {
            output = new();
            int compressedLength = _binarySerialization.Deserialize<int>(ref input);
            if (input.Remaining < compressedLength)
            {
                output.Dispose();
                return false;
            }

            int length = _binarySerialization.Deserialize<int>(ref input);
            output = new(length);
            int decopressLength = LZ4CodecDecode(input, output.GetSpan(length).Slice(0, length));
            if (decopressLength != length)
            {
                output.Dispose();
                return false;
            }
            output.Advance(decopressLength);
            return true;
        }

        private bool TryDecompressArray(ByteBlock input, out ByteBlock output)
        {
            output = new();
            while (input.TryPeek(out byte next))
            {
                if (next == LZ4CompressionArrayEndSign)
                {
                    break;
                }

                int compressedLength = _binarySerialization.Deserialize<int>(ref input);
                if (input.Remaining < compressedLength)
                {
                    output.Dispose();
                    return false;
                }

                int length = _binarySerialization.Deserialize<int>(ref input);
                int decopressLength = LZ4CodecDecode(input, output.GetSpan(length).Slice(0, length));
                if (decopressLength != length)
                {
                    output.Dispose();
                    return false;
                }
                output.Advance(decopressLength);
            }
            return true;
        }

        private bool TryDecompress(ByteBuffer input, out ByteBuffer output)
        {
            output = new();
            if (_binarySerialization.Deserialize<byte>(ref input) != LZ4CompressionMark)
            {
                return false;
            }

            CompressionType compressionType = _binarySerialization.Deserialize<CompressionType>(ref input);

            switch (compressionType)
            {
                case CompressionType.Block:
                    return TryDecompressBlock(input, out output);

                case CompressionType.BlockArray:
                    return TryDecompressArray(input, out output);
            }

            return false;
        }

        private bool TryDecompressBlock(ByteBuffer input, out ByteBuffer output)
        {
            output = new();
            int compressedLength = _binarySerialization.Deserialize<int>(ref input);
            if (input.Remaining < compressedLength)
            {
                output.Dispose();
                return false;
            }

            int length = _binarySerialization.Deserialize<int>(ref input);
            output = new();
            ByteBlock inputBlock = new(input);
            int decopressLength = LZ4CodecDecode(inputBlock, output.GetSpan(length).Slice(0, length));
            inputBlock.Dispose();

            if (decopressLength != length)
            {
                output.Dispose();
                return false;
            }
            output.Advance(decopressLength);
            return true;
        }

        private bool TryDecompressArray(ByteBuffer input, out ByteBuffer output)
        {
            output = new();

            while (input.TryPeek(out byte next))
            {
                if (next == LZ4CompressionArrayEndSign)
                {
                    break;
                }

                int compressedLength = _binarySerialization.Deserialize<int>(ref input);
                if (input.Remaining < compressedLength)
                {
                    output.Dispose();
                    return false;
                }

                int length = _binarySerialization.Deserialize<int>(ref input);
                ByteBlock inputBlock = new(input);
                int decopressLength = LZ4CodecDecode(inputBlock, output.GetSpan(length).Slice(0, length));
                inputBlock.Dispose();

                if (decopressLength != length)
                {
                    output.Dispose();
                    return false;
                }
                output.Advance(decopressLength);
            }
            return true;
        }

        #endregion Decompress
    }
}