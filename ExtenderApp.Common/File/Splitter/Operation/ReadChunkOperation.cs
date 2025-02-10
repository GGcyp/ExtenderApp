using System.Buffers;


namespace ExtenderApp.Common.File.Splitter
{
    /// <summary>
    /// 分割器操作类，用于读取数据块
    /// </summary>
    internal class ReadChunkOperation : SplitterOperation
    {
        /// <summary>
        /// 回调函数，用于处理读取到的数据块
        /// </summary>
        private Action<byte[]>? calback;

        /// <summary>
        /// 当前读取位置
        /// </summary>
        private int readPosition;

        /// <summary>
        /// 当前读取长度
        /// </summary>
        private int readLength;

        /// <summary>
        /// 设置读取参数
        /// </summary>
        /// <param name="chunkIndex">数据块索引</param>
        /// <param name="action">处理读取到的数据块的回调函数</param>
        /// <exception cref="ArgumentOutOfRangeException">如果数据块索引超出范围或读取长度超出总长度，则抛出异常</exception>
        public void Set(uint chunkIndex, Action<byte[]> action)
        {
            readPosition = (int)chunkIndex;
            readLength = readPosition * SplitterInfo.MaxChunkSize;
            if (chunkIndex >= SplitterInfo.ChunkCount || readLength > SplitterInfo.Length)
            {
                throw new ArgumentOutOfRangeException();
            }

            calback = action;
        }

        protected override void ExecuteProtected(FileStream stream)
        {
            var pool = ArrayPool<byte>.Shared;
            var readBytes = pool.Rent(readLength);
            stream.Read(readBytes, readPosition, readLength);
            calback?.Invoke(readBytes);
            pool.Return(readBytes);
        }

        protected override bool Reset()
        {
            calback = null;
            readPosition = 0;
            readLength = 0;
            return true;
        }
    }
}
