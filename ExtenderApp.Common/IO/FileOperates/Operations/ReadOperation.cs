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
        /// 读取的起始位置。
        /// </summary>
        private long readPosition;

        /// <summary>
        /// 读取的长度。
        /// </summary>
        private int readLength;

        /// <summary>
        /// 表示字节位置的私有字段
        /// </summary>
        private int bytesPosition;

        /// <summary>
        /// 用于读取的字节数组。
        /// </summary>
        private byte[]? readBytes;

        /// <summary>
        /// 获取操作结果的字节数组。
        /// </summary>
        public byte[] ReslutBytes { get; private set; }

        /// <summary>
        /// 初始化ReadOperation实例。
        /// </summary>
        /// <param name="releaseAction">释放操作时的回调动作。</param>
        public ReadOperation()
        {
            this.readPosition = 0;
            this.readLength = 0;
            this.bytesCallback = null;
            this.readBytes = null;
            this.ReslutBytes = null;
        }

        /// <summary>
        /// 执行读取操作。
        /// </summary>
        /// <param name="item">内存映射视图访问器。</param>
        public override void Execute(FileOperateData item)
        {
            if (readLength < 0 || readPosition > item.CurrentCapacity || readPosition + readLength > item.CurrentCapacity)
            {
                ErrorUtil.ArgumentOutOfRange(nameof(ReadOperation));
            }

            readPosition = readPosition < 0 ? 0 : readPosition;

            byte[]? bytes = readBytes;
            readLength = readLength <= 0 ? (int)item.CurrentCapacity : readLength;
            if (bytes == null)
            {
                //bytes = pool.Rent((int)readLength);
                //ReslutBytes = bytes;
                bytes = new byte[readLength];
                ReslutBytes = bytes;
            }

            var accessor = item.Accessor;
            var span = bytes.AsSpan();
            for (long i = readPosition; i < readLength; i++)
            {
                span[(int)(i - readPosition + bytesPosition)] = accessor.ReadByte(i);
            }
            bytesCallback?.Invoke(bytes);
        }

        /// <summary>
        /// 设置数据读取的起始位置和长度，并指定数据缓冲区。
        /// </summary>
        /// <param name="position">数据读取的起始位置。</param>
        /// <param name="length">需要读取的数据长度。</param>
        /// <param name="dataBuffer">数据缓冲区。</param>
        /// <exception cref="ArgumentOutOfRangeException">如果起始位置或长度小于0，则抛出此异常。</exception>
        /// <exception cref="ArgumentNullException">如果数据缓冲区为null，则抛出此异常。</exception>
        public void Set(long position, int length)
        {
            readPosition = position;
            readLength = length;
        }

        /// <summary>
        /// 设置读取位置、长度和回调方法。
        /// </summary>
        /// <param name="position">读取开始的位置。</param>
        /// <param name="length">读取的长度。</param>
        /// <param name="callback">读取完成后的回调方法，参数为读取是否成功。</param>
        /// <exception cref="ArgumentOutOfRangeException">如果读取位置或长度小于0，则抛出此异常。</exception>
        /// <exception cref="ArgumentNullException">如果回调方法为空，则抛出此异常。</exception>
        public void Set(long position, int length, byte[]? bytes, int bytesPosition, Action<byte[]> callback)
        {
            readPosition = position;
            readLength = length;
            readBytes = bytes;
            this.bytesPosition = bytesPosition;
        }

        /// <summary>
        /// 尝试重置读取操作的状态。
        /// </summary>
        /// <returns>如果重置成功，则返回 true；否则返回 false。</returns>
        public override bool TryReset()
        {
            readPosition = 0;
            readLength = 0;
            readBytes = null;
            ReslutBytes = null;
            return true;
        }
    }
}
