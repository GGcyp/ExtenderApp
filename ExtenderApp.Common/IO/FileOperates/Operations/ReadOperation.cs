using System.Buffers;
using ExtenderApp.Common.DataBuffers;
using ExtenderApp.Common.Error;

namespace ExtenderApp.Common.IO.Splitter
{
    /// <summary>
    /// 读取操作类，继承自 StreamOperation 类。
    /// </summary>
    internal class ReadOperation : FileOperation
    {
        /// <summary>
        /// 一个可选的字节数组回调委托，用于处理字节数组数据。
        /// </summary>
        private Action<byte[]>? bytesCallback;

        /// <summary>
        /// 一个布尔值回调委托，用于处理布尔值数据。
        /// </summary>
        private Action<bool>? boolCallback;

        /// <summary>
        /// 读取的起始位置。
        /// </summary>
        private long readPosition;

        /// <summary>
        /// 读取的长度。
        /// </summary>
        private long readLength;

        /// <summary>
        /// 用于读取的字节数组。
        /// </summary>
        private byte[]? readBytes;

        /// <summary>
        /// 获取操作结果的字节数组。
        /// </summary>
        public byte[]? ReslutBytes { get; private set; }

        /// <summary>
        /// 读取操作完成后的回调函数。
        /// </summary>
        public DataBuffer? DataBuffer;

        /// <summary>
        /// 初始化ReadOperation实例。
        /// </summary>
        /// <param name="releaseAction">释放操作时的回调动作。</param>
        public ReadOperation()
        {
            this.DataBuffer = null;
            this.readPosition = 0;
            this.readLength = 0;
            this.bytesCallback = null;
            this.boolCallback = null;
            this.readBytes = null;
            this.ReslutBytes = null;
        }

        /// <summary>
        /// 执行读取操作。
        /// </summary>
        /// <param name="item">内存映射视图访问器。</param>
        public override void Execute(FileOperateData item)
        {
            if (readPosition < 0 || readLength < 0 || readPosition > item.CurrentCapacity || readPosition + readLength > item.CurrentCapacity)
            {
                ErrorUtil.ArgumentOutOfRange(nameof(ReadOperation));
            }

            var pool = ArrayPool<byte>.Shared;

            byte[] bytes = readBytes;
            readLength = readLength <= 0 ? (int)item.CurrentCapacity : readLength;
            if (bytes == null)
            {
                bytes = pool.Rent((int)readLength);
                ReslutBytes = bytes;
            }

            var accessor = item.Accessor;
            for (long i = readPosition; i < readLength; i++)
            {
                bytes[i] = accessor.ReadByte(i);
            }
            DataBuffer?.Process(bytes);
            bytesCallback?.Invoke(bytes);
            boolCallback?.Invoke(true);
        }

        public void Set(DataBuffer dataBuffer)
        {
            if (dataBuffer == null)
                throw new ArgumentNullException(nameof(dataBuffer));

            readPosition = 0;
            readLength = -1;
            this.DataBuffer = dataBuffer;
        }

        /// <summary>
        /// 设置数据读取的起始位置和长度，并指定数据缓冲区。
        /// </summary>
        /// <param name="position">数据读取的起始位置。</param>
        /// <param name="length">需要读取的数据长度。</param>
        /// <param name="dataBuffer">数据缓冲区。</param>
        /// <exception cref="ArgumentOutOfRangeException">如果起始位置或长度小于0，则抛出此异常。</exception>
        /// <exception cref="ArgumentNullException">如果数据缓冲区为null，则抛出此异常。</exception>
        public void Set(long position, long length, DataBuffer dataBuffer)
        {
            if (position < 0 || length < 0)
                throw new ArgumentOutOfRangeException(nameof(position));

            if (dataBuffer == null)
                throw new ArgumentNullException(nameof(dataBuffer));

            readPosition = position;
            readLength = length;
            this.DataBuffer = dataBuffer;
        }

        /// <summary>
        /// 设置数据读取的起始位置和长度，并指定回调函数。
        /// </summary>
        /// <param name="position">数据读取的起始位置。</param>
        /// <param name="length">需要读取的数据长度。</param>
        /// <param name="callback">读取完成后的回调函数。</param>
        /// <exception cref="ArgumentOutOfRangeException">如果起始位置或长度小于0，则抛出此异常。</exception>
        /// <exception cref="ArgumentNullException">如果回调函数为null，则抛出此异常。</exception>
        public void Set(long position, long length, Action<byte[]> callback)
        {
            if (position < 0 || length < 0)
                throw new ArgumentOutOfRangeException(nameof(position));

            if (callback == null)
                throw new ArgumentNullException(nameof(callback));

            readPosition = position;
            readLength = length;
            bytesCallback = callback;
        }

        /// <summary>
        /// 设置读取位置、长度和回调方法。
        /// </summary>
        /// <param name="position">读取开始的位置。</param>
        /// <param name="length">读取的长度。</param>
        /// <param name="callback">读取完成后的回调方法，参数为读取是否成功。</param>
        /// <exception cref="ArgumentOutOfRangeException">如果读取位置或长度小于0，则抛出此异常。</exception>
        /// <exception cref="ArgumentNullException">如果回调方法为空，则抛出此异常。</exception>
        public void Set(long position, long length, byte[] bytes, Action<bool> callback)
        {
            if (position < 0 || length < 0)
                throw new ArgumentOutOfRangeException(nameof(position));

            if (callback == null)
                throw new ArgumentNullException(nameof(callback));

            if (bytes == null)
                throw new ArgumentNullException(nameof(bytes));

            readPosition = position;
            readLength = length;
            boolCallback = callback;
            readBytes = bytes;
        }

        /// <summary>
        /// 设置读取操作的起始位置、长度和字节数组。
        /// </summary>
        /// <param name="position">读取的起始位置。</param>
        /// <param name="length">读取的长度。</param>
        /// <param name="bytes">要读取的字节数组。</param>
        public void Set(long position, long length, byte[] bytes)
        {
            if (position < 0 || length < 0)
                throw new ArgumentOutOfRangeException(nameof(position));

            if (bytes == null)
                throw new ArgumentNullException(nameof(bytes));

            readPosition = position;
            readLength = bytes.Length;
            readBytes = bytes;
        }

        /// <summary>
        /// 设置读取位置和长度
        /// </summary>
        /// <param name="position">读取起始位置</param>
        /// <param name="length">读取长度</param>
        /// <exception cref="ArgumentOutOfRangeException">如果 position 或 length 小于 0，则抛出此异常</exception>
        public void Set(long position, long length)
        {
            if (position < 0 || length < 0)
                throw new ArgumentOutOfRangeException(nameof(position));

            readPosition = position;
            readLength = length;
        }

        /// <summary>
        /// 尝试重置读取操作的状态。
        /// </summary>
        /// <returns>如果重置成功，则返回 true；否则返回 false。</returns>
        public override bool TryReset()
        {
            DataBuffer?.Release();
            DataBuffer = null;
            readPosition = 0;
            readLength = 0;
            ArrayPool<byte>.Shared.Return(ReslutBytes);
            readBytes = null;
            ReslutBytes = null;
            return true;
        }
    }
}
