using System.Buffers;
using System.IO;
using System.IO.MemoryMappedFiles;
using ExtenderApp.Abstract;
using ExtenderApp.Common.Error;
using ExtenderApp.Data;

namespace ExtenderApp.Common.IO.Splitter
{
    internal class SplitterReadOperation : FileOperation
    {
        /// <summary>
        /// 读取操作完成后的回调函数。
        /// </summary>
        private Action<byte[]>? calback;

        /// <summary>
        /// 读取的起始位置。
        /// </summary>
        private long readPosition;

        /// <summary>
        /// 读取的长度。
        /// </summary>
        private int readLength;

        /// <summary>
        /// 读取数据块索引
        /// </summary>
        private uint readChunkIndex;

        /// <summary>
        /// 读取到的字节数组。
        /// </summary>
        public byte[] ReadBytes { get; private set; }

        /// <summary>
        /// 分割信息。
        /// </summary>
        private SplitterInfo splitterInfo;

        public SplitterReadOperation(Action<IConcurrentOperation> releaseAction) : base(releaseAction)
        {
            calback = null;
            readPosition = 0;
            readLength = 0;
            readChunkIndex = 0;
            ReadBytes = Array.Empty<byte>();
            splitterInfo = SplitterInfo.Empty;
        }

        public override void Execute(MemoryMappedViewAccessor item)
        {
            if (readPosition < 0 || readLength < 0 || readPosition + readLength > item.Capacity || readPosition > item.Capacity)
            {
                ErrorUtil.ArgumentOutOfRange(nameof(ReadOperation));
            }

            var pool = ArrayPool<byte>.Shared;
            byte[] readBytes;
            if (ReadBytes == null)
            {
                readLength = readLength == -1 ? (int)item.Capacity : readLength;
                readBytes = pool.Rent(readLength);
            }
            else
            {
                readBytes = ReadBytes;
            }

            try
            {
                for (long i = readPosition; i < readLength; i++)
                {
                    readBytes[i] = item.ReadByte(i);
                }

                ReadBytes = new ArraySegment<byte>(readBytes, 0, readLength).ToArray();
                calback?.Invoke(ReadBytes);
                splitterInfo.RemoveChunk(readChunkIndex);
            }
            finally
            {
                pool.Return(readBytes);
            }
        }

        /// <summary>
        /// 设置处理字节数组的操作和分隔符信息。
        /// </summary>
        /// <param name="action">处理字节数组的操作。</param>
        /// <param name="splitterInfo">分隔符信息。</param>
        /// <exception cref="ArgumentNullException">如果 <paramref name="action"/> 为 null，则抛出此异常。</exception>
        public void Set(Action<byte[]> action, SplitterInfo splitterInfo)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            if (splitterInfo == null)
                throw new ArgumentNullException(nameof(splitterInfo));

            readPosition = splitterInfo.GetLastChunkIndexPosition();
            readLength = splitterInfo.MaxChunkSize;
            calback = action;
            this.splitterInfo = splitterInfo;
            readChunkIndex = splitterInfo.GetLastChunkIndex();
        }

        /// <summary>
        /// 设置从指定位置开始读取指定长度的字节数组的处理操作和分隔符信息。
        /// </summary>
        /// <param name="position">开始读取的位置。</param>
        /// <param name="length">要读取的字节数。</param>
        /// <param name="action">处理字节数组的操作。</param>
        /// <param name="splitterInfo">分隔符信息。</param>
        /// <exception cref="ArgumentOutOfRangeException">如果 <paramref name="position"/> 或 <paramref name="length"/> 小于 0，则抛出此异常。</exception>
        /// <exception cref="ArgumentNullException">如果 <paramref name="action"/> 为 null，则抛出此异常。</exception>
        public void Set(long position, int length, Action<byte[]> action, SplitterInfo splitterInfo)
        {
            if (position < 0 || length < 0)
                throw new ArgumentOutOfRangeException(nameof(position));

            if (action == null)
                throw new ArgumentNullException(nameof(action));

            if (splitterInfo == null)
                throw new ArgumentNullException(nameof(splitterInfo));

            readPosition = position;
            readLength = length;
            calback = action;
            this.splitterInfo = splitterInfo;
            readChunkIndex = splitterInfo.GetChunkIndex(position);
        }

        /// <summary>
        /// 设置指定分块的信息
        /// </summary>
        /// <param name="chunkIndex">分块的索引</param>
        /// <param name="splitterInfo">分块信息对象</param>
        /// <exception cref="ArgumentNullException">如果splitterInfo为null，则抛出此异常</exception>
        /// <exception cref="ArgumentOutOfRangeException">如果chunkIndex小于0或大于splitterInfo.ChunkCount，则抛出此异常</exception>
        public void Set(uint chunkIndex, SplitterInfo splitterInfo)
        {
            if (splitterInfo == null)
                throw new ArgumentNullException(nameof(splitterInfo));

            if (chunkIndex < 0 || chunkIndex > splitterInfo.ChunkCount)
                throw new ArgumentOutOfRangeException(nameof(chunkIndex));

            readPosition = splitterInfo.GetPosition(chunkIndex);
            readLength = splitterInfo.MaxChunkSize;
            this.splitterInfo = splitterInfo;
            readChunkIndex = chunkIndex;
        }

        /// <summary>
        /// 设置指定分块的信息
        /// </summary>
        /// <param name="chunkIndex">分块的索引</param>
        /// <param name="action">处理字节数组的操作。</param>
        /// <param name="splitterInfo">分块信息对象</param>
        /// <exception cref="ArgumentNullException">如果splitterInfo为null，则抛出此异常</exception>
        /// <exception cref="ArgumentOutOfRangeException">如果chunkIndex小于0或大于splitterInfo.ChunkCount，则抛出此异常</exception>
        public void Set(uint chunkIndex, Action<byte[]> callback, SplitterInfo splitterInfo)
        {
            if (splitterInfo == null)
                throw new ArgumentNullException(nameof(splitterInfo));

            if (chunkIndex < 0 || chunkIndex > splitterInfo.ChunkCount)
                throw new ArgumentOutOfRangeException(nameof(chunkIndex));

            readPosition = splitterInfo.GetPosition(chunkIndex);
            readLength = splitterInfo.MaxChunkSize;
            this.splitterInfo = splitterInfo;
            this.calback = callback;
            readChunkIndex = chunkIndex;
        }

        public override bool TryReset()
        {
            readPosition = 0;
            readLength = 0;
            calback = null;
            ReadBytes = null;
            splitterInfo = null;
            return true;
        }
    }
}
