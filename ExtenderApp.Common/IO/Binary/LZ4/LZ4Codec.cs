namespace ExtenderApp.Common.IO.Binary.LZ4
{
    internal static partial class LZ4Codec
    {
        #region Configuration

        /// <summary>
        ///N->2^N Bytes。
        ///这个公式描述了一个特定的内存使用量计算方法。
        ///这里的N是一个整数，而内存使用量则是2的N次方字节。
        ///这意味着，随着N的增加，内存使用量会以指数方式增长。
        ///10 -> 1KB：当N=10时，内存使用量为2^10 = 1024 Bytes，即1KB（千字节）。
        ///12 -> 4KB：当N=12时，内存使用量为2^12 = 4096 Bytes，即4KB。
        ///16 -> 64KB：当N=16时，内存使用量为2^16 = 65536 Bytes，即64KB。
        ///20 -> 1MB：当N=20时，内存使用量为2^20 = 1048576 Bytes，即1MB（兆字节）。
        /// </summary>
        private const int MEMORY_USAGE = 12; // modified use 12.

        /// <summary>
        ///如果减小这个参数的值，算法会更倾向于快速跳过被认为是“不可压缩”的数据段。
        ///这意味着算法对这些数据段进行压缩尝试的耐心减少了，可能会更早地决定这些数据段不适合压缩。
        ///这样做会显著地降低压缩比率（即压缩后的数据相对于原始数据的大小比例），因为算法可能错过了实际上可以通过某种方式压缩的数据段。
        ///但是，对于确实不可压缩的数据，这种策略会提高处理速度，因为算法不会在这些数据上浪费时间去尝试压缩。
        ///如果增加这个参数的值，算法在声明一个数据段为“不可压缩”之前会更加深入地搜索或尝试压缩。
        ///这意味着算法会花费更多时间来评估数据段是否可以被有效压缩。
        ///增加这个值可能会稍微提高压缩比率，因为算法有更多的机会发现可以被压缩的数据模式。
        ///然而，对于确实不可压缩的数据，这种策略会导致算法运行得更慢，因为它在这些数据上花费了更多的处理时间。
        ///最后，注释建议使用这个参数的默认值6。
        ///这意味着开发者认为，在大多数情况下，这个默认值能在压缩效率和处理速度之间提供一个良好的平衡。
        /// </summary>
        private const int NOTCOMPRESSIBLE_DETECTIONLEVEL = 6;

        #endregion

        #region Consts

        private const int MINMATCH = 4;

#pragma warning disable 162, 429

        private const int SKIPSTRENGTH = NOTCOMPRESSIBLE_DETECTIONLEVEL > 2 ? NOTCOMPRESSIBLE_DETECTIONLEVEL : 2;
#pragma warning restore 162, 429

        private const int COPYLENGTH = 8;
        private const int LASTLITERALS = 5;
        private const int MFLIMIT = COPYLENGTH + MINMATCH;
        private const int MINLENGTH = MFLIMIT + 1;
        private const int MAXD_LOG = 16;
        private const int MAXD = 1 << MAXD_LOG;
        private const int MAXD_MASK = MAXD - 1;
        private const int MAX_DISTANCE = (1 << MAXD_LOG) - 1;
        private const int ML_BITS = 4;
        private const int ML_MASK = (1 << ML_BITS) - 1;
        private const int RUN_BITS = 8 - ML_BITS;
        private const int RUN_MASK = (1 << RUN_BITS) - 1;
        private const int STEPSIZE_64 = 8;
        private const int STEPSIZE_32 = 4;

        private const int LZ4_64KLIMIT = (1 << 16) + (MFLIMIT - 1);

        private const int HASH_LOG = MEMORY_USAGE - 2;
        private const int HASH_TABLESIZE = 1 << HASH_LOG;
        private const int HASH_ADJUST = (MINMATCH * 8) - HASH_LOG;

        private const int HASH64K_LOG = HASH_LOG + 1;
        private const int HASH64K_TABLESIZE = 1 << HASH64K_LOG;
        private const int HASH64K_ADJUST = (MINMATCH * 8) - HASH64K_LOG;

        private const int HASHHC_LOG = MAXD_LOG - 1;
        private const int HASHHC_TABLESIZE = 1 << HASHHC_LOG;
        private const int HASHHC_ADJUST = (MINMATCH * 8) - HASHHC_LOG;
        ////private const int HASHHC_MASK = HASHHC_TABLESIZE - 1;

        private const int MAX_NB_ATTEMPTS = 256;
        private const int OPTIMAL_ML = ML_MASK - 1 + MINMATCH;

        private const int BLOCK_COPY_LIMIT = 16;

        private static readonly int[] DECODER_TABLE_32 = { 0, 3, 2, 3, 0, 0, 0, 0 };
        private static readonly int[] DECODER_TABLE_64 = { 0, 0, 0, -1, 0, 1, 2, 3 };

        private static readonly int[] DEBRUIJN_TABLE_32 =
        {
            0, 0, 3, 0, 3, 1, 3, 0, 3, 2, 2, 1, 3, 2, 0, 1,
            3, 3, 1, 2, 2, 2, 2, 0, 3, 1, 2, 0, 1, 0, 1, 1,
        };

        private static readonly int[] DEBRUIJN_TABLE_64 =
        {
            0, 0, 0, 0, 0, 1, 1, 2, 0, 3, 1, 3, 1, 4, 2, 7,
            0, 2, 3, 6, 1, 5, 3, 5, 1, 3, 4, 4, 2, 5, 6, 7,
            7, 0, 1, 2, 3, 3, 4, 6, 2, 6, 5, 5, 3, 4, 5, 6,
            7, 1, 2, 4, 6, 4, 4, 5, 7, 2, 6, 5, 7, 6, 7, 7,
        };

        #endregion


        /// <summary>
        /// 计算最大输出长度
        /// </summary>
        /// <param name="inputLength">输入长度</param>
        /// <returns>最大输出长度</returns>
        public static int MaximumOutputLength(int inputLength)
        {
            return inputLength + (inputLength / 255) + 16;
        }

        internal static void CheckArguments(byte[] input, int inputOffset, int inputLength, byte[] output, int outputOffset, int outputLength)
        {
            if (inputLength == 0)
            {
                outputLength = 0;
                return;
            }

            if (input == null)
            {
                throw new ArgumentNullException("input");
            }

            if ((uint)inputOffset > (uint)input.Length)
            {
                throw new ArgumentOutOfRangeException("inputOffset");
            }

            if ((uint)inputLength > (uint)input.Length - (uint)inputOffset)
            {
                throw new ArgumentOutOfRangeException("inputLength");
            }

            if (output == null)
            {
                throw new ArgumentNullException("output");
            }

            if ((uint)outputOffset > (uint)output.Length)
            {
                throw new ArgumentOutOfRangeException("outputOffset");
            }

            if ((uint)outputLength > (uint)output.Length - (uint)outputOffset)
            {
                throw new ArgumentOutOfRangeException("outputLength");
            }
        }
    }
}
