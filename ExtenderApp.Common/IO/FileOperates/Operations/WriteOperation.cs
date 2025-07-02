using ExtenderApp.Data;
using System.Buffers;

namespace ExtenderApp.Common.IO
{
    /// <summary>
    /// 写入操作类，继承自StreamOperation类
    /// </summary>
    internal class WriteOperation : FileOperation
    {
        /// <summary>
        /// 待写入的字节数组
        /// </summary>
        private byte[] writeBytes;

        /// <summary>
        /// 回调委托，用于处理字节数组
        /// </summary>
        private Delegate? callback;

        /// <summary>
        /// 写位置变量，用于记录当前写入的位置。
        /// </summary>
        private long writePosition;

        /// <summary>
        /// 只读字节序列。
        /// </summary>
        private ReadOnlySequence<byte> readOnlySequence;

        /// <summary>
        /// 字节序列池的租赁。
        /// </summary>
        private SequencePool<byte>.Rental rental;

        /// <summary>
        /// 设置字节数组，并可选地提供一个回调函数。
        /// </summary>
        /// <param name="bytes">要设置的字节数组。</param>
        /// <param name="callback">操作完成后的回调函数，参数为设置成功的字节数组。可以为null。</param>
        public void Set(byte[] bytes, Action<byte[]>? callback)
        {
            Set(bytes, bytes.Length, callback);
        }

        /// <summary>
        /// 设置字节数组和回调函数
        /// </summary>
        /// <param name="bytes">要设置的字节数组</param>
        /// <param name="callback">回调函数，可选参数</param>
        /// <param name="length">写入长度</param>
        public void Set(byte[] bytes, int length, Action<byte[]>? callback)
        {
            Set(bytes, 0, length, callback);
        }

        /// <summary>
        /// 设置数据到指定位置
        /// </summary>
        /// <param name="bytes">要写入的字节数组</param>
        /// <param name="position">写入开始的位置</param>
        /// <param name="length">要写入的字节长度</param>
        /// <param name="callback">操作完成后的回调函数，参数为写入的字节数组</param>
        public void Set(byte[] bytes, long position, int length, Action<byte[]>? callback)
        {
            Set(bytes, position, length, 0, callback);
        }

        public void Set(byte[] bytes, long position, int length, int bytesPosition, Action<byte[]>? callback)
        {
            writeBytes = bytes;
            this.callback = callback;
            writePosition = position;

            readOnlySequence = new ReadOnlySequence<byte>(bytes, bytesPosition, length);
        }

        public void Set(ExtenderBinaryWriter writer, long position, Action? callback = null)
        {
            rental = writer.Rental;
            readOnlySequence = rental.Value;
            writePosition = position;
            this.callback = callback;
        }

        //public void 

        /// <summary>
        /// 尝试重置写入操作的状态
        /// </summary>
        /// <returns>如果成功重置状态，则返回true；否则返回false</returns>
        public override bool TryReset()
        {
            writeBytes = null;
            callback = null;
            writePosition = 0;
            return true;
        }

        public override void Execute(FileOperateData item)
        {
            if (item.FStream.Length < writePosition + readOnlySequence.Length)
            {
                item.ExpandCapacity(writePosition + readOnlySequence.Length);
            }

            long writeIndex = writePosition;
            int bytesIndex = 0;
            var accessor = item.Accessor;

            foreach (ReadOnlyMemory<byte> meory in readOnlySequence)
            {
                accessor.Write(writeIndex, meory.Span[bytesIndex]);
                writeIndex++;
                bytesIndex++;
            }

            if (callback is Action<byte[]> arrayCallback)
            {
                arrayCallback?.Invoke(writeBytes);
            }
            else if (callback is Action actionCallbakc)
            {
                actionCallbakc?.Invoke();
            }
            rental.Dispose();
        }
    }
}
