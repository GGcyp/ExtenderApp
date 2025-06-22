

namespace ExtenderApp.Data
{
    /// <summary>
    /// PieceData类，用于管理分块数据。
    /// </summary>
    public struct PieceData
    {
        /// <summary>
        /// 获取一个空的PieceData实例。
        /// </summary>
        /// <returns>空的PieceData实例。</returns>
        public static PieceData Empty => new PieceData(null, 0, 0);

        /// <summary>
        /// 分块数据。
        /// </summary>
        private readonly byte[] pieces;

        /// <summary>
        /// 分块数据的长度。
        /// </summary>
        public int Length { get; }

        /// <summary>
        /// 真实数据的数量。
        /// </summary>
        private int trueCount;

        /// <summary>
        /// 获取真实数据的数量。
        /// </summary>
        public int TrueCount => trueCount;

        /// <summary>
        /// 获取是否为空。
        /// </summary>
        public bool IsEmpty => pieces is null;

        /// <summary>
        /// 通过索引访问数据项。
        /// </summary>
        /// <param name="index">索引。</param>
        /// <returns>如果索引处的数据项不为0，则返回true；否则返回false。</returns>
        /// <exception cref="ArgumentOutOfRangeException">索引超出范围时抛出。</exception>
        public bool this[int index]
        {
            get
            {
                if (index < 0 || index >= Length)
                {
                    throw new ArgumentOutOfRangeException(nameof(index), "Index is out of range.");
                }
                return pieces.AsSpan()[index] > 0;
            }
        }

        /// <summary>
        /// 使用字节数组初始化PieceData实例。
        /// </summary>
        /// <param name="pieces">字节数组。</param>
        public PieceData(byte[] pieces) : this(pieces, pieces?.Length ?? 0)
        {

        }

        /// <summary>
        /// 使用字节数组和长度初始化PieceData实例。
        /// </summary>
        /// <param name="pieces">字节数组。</param>
        /// <param name="length">长度。</param>
        public PieceData(byte[] pieces, int length) : this(pieces, length, 0)
        {

        }

        public PieceData(byte[] pieces, int length, int trueCount)
        {
            this.pieces = pieces;
            Length = length;
            this.trueCount = trueCount;
        }

        /// <summary>
        /// 判断两个PieceData实例是否相等。
        /// </summary>
        /// <param name="other">另一个PieceData实例。</param>
        /// <returns>如果两个实例相等，则返回true；否则返回false。</returns>
        public bool Equals(PieceData other)
        {
            return pieces.SequenceEqual(other.pieces) && Length == other.Length && trueCount == other.trueCount;
        }

        /// <summary>
        /// 加载指定索引处的分块。
        /// </summary>
        /// <param name="index">分块索引。</param>
        /// <exception cref="ArgumentOutOfRangeException">索引超出范围时抛出。</exception>
        public void LoadChunk(uint index)
        {
            if (index >= Length || index < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(index), "块索引超出范围");
            }

            lock (pieces)
            {
                pieces[index] = 1;
                //progress++;
            }
            Interlocked.Increment(ref trueCount);
        }

        /// <summary>
        /// 卸载指定索引处的分块。
        /// </summary>
        /// <param name="index">分块索引。</param>
        /// <exception cref="ArgumentOutOfRangeException">索引超出范围时抛出。</exception>
        public void ULoadChunk(uint index)
        {
            if (index >= Length || index < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(index), "块索引超出范围");
            }

            lock (pieces)
            {
                pieces[index] = 0;
            }

            Interlocked.Decrement(ref trueCount);
        }

        /// <summary>
        /// 获取最后一个未加载的分块索引。
        /// </summary>
        /// <returns>最后一个未加载的分块索引；如果没有未加载的分块，则返回-1。</returns>
        public int GetLastNotLoadChunkIndex()
        {
            if (pieces == null) return -1;

            for (int i = Length - 1; i > 0; i--)
            {
                if (pieces[i] == 0)
                {
                    return i;
                }
            }
            return -1;
        }

        public byte[]? CopeToArray()
        {
            return pieces?.ToArray() ?? null;
        }
    }
}
