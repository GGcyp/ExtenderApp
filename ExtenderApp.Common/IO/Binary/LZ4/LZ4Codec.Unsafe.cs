namespace ExtenderApp.Common.IO.Binaries.LZ4
{
    /// <summary>
    /// LZ4 压缩编解码器类
    /// </summary>
    internal partial class LZ4Codec
    {
        /// <summary>
        /// 使用LZ4算法对输入数据进行编码。
        /// </summary>
        /// <param name="input">输入的字节数据。</param>
        /// <param name="output">输出的字节缓冲区。</param>
        /// <returns>返回编码后的数据长度。</returns>
        /// <exception cref="Exception">如果输出缓冲区为空，则抛出异常。</exception>
        public static unsafe int Encode(ReadOnlySpan<byte> input, Span<byte> output)
        {
            if (output.Length == 0)
            {
                throw new Exception("Output is empty.");
            }

            fixed (byte* inputPtr = input)
            fixed (byte* outputPtr = output)
            {
                if (input.Length < LZ4_64KLIMIT)
                {
                    var uHashTable = GetUShortHashTablePool();
                    fixed (ushort* hash1 = &uHashTable[0])
                    {
                        if (IntPtr.Size == 4)
                        {
                            return LZ4_compress64kCtx_32(hash1, inputPtr, outputPtr, input.Length, output.Length);
                        }
                        else
                        {
                            return LZ4_compress64kCtx_64(hash1, inputPtr, outputPtr, input.Length, output.Length);
                        }
                    }
                }
                else
                {
                    var bHashTable = GetUIntHashTablePool();
                    fixed (uint* hash2 = &bHashTable[0])
                    {
                        if (IntPtr.Size == 4)
                        {
                            return LZ4_compressCtx_32(hash2, inputPtr, outputPtr, input.Length, output.Length);
                        }
                        else
                        {
                            return LZ4_compressCtx_64(hash2, inputPtr, outputPtr, input.Length, output.Length);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 解码LZ4压缩数据。
        /// </summary>
        /// <param name="input">包含压缩数据的字节数组。</param>
        /// <param name="output">用于存储解码后的数据的字节数组。</param>
        /// <returns>解码后的数据长度。</returns>
        /// <exception cref="Exception">如果输出数组为空，将抛出此异常。</exception>
        /// <exception cref="Exception">如果LZ4块损坏或提供了无效的长度，将抛出此异常。</exception>
        public static unsafe int Decode(ReadOnlySpan<byte> input, Span<byte> output)
        {
            if (output.Length == 0)
            {
                throw new Exception("Output is empty.");
            }

            fixed (byte* inputPtr = input)
            fixed (byte* outputPtr = output)
            {
                int length;
                if (IntPtr.Size == 4)
                {
                    length = LZ4_uncompress_32(inputPtr, outputPtr, output.Length);
                }
                else
                {
                    length = LZ4_uncompress_64(inputPtr, outputPtr, output.Length);
                }

                if (length != input.Length)
                {
                    throw new Exception("LZ4 block is corrupted, or invalid length has been given.");
                }

                return output.Length;
            }
        }
    }
}
