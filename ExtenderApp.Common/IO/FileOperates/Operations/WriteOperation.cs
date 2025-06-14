
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
        private byte[]? writeBytes;

        /// <summary>
        /// 回调委托，用于处理字节数组
        /// </summary>
        private Action<byte[]>? callback;

        /// <summary>
        /// 写入的长度
        /// </summary>
        private long writeLength;

        /// <summary>
        /// 写位置变量，用于记录当前写入的位置。
        /// </summary>
        private long writePosition;

        /// <summary>
        /// 表示字节位置的私有变量
        /// </summary>
        private long bytesPosition;

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
        /// <param name="lenght">写入长度</param>
        public void Set(byte[] bytes, long lenght, Action<byte[]>? callback)
        {
            Set(bytes, 0, lenght, callback);
        }

        /// <summary>
        /// 设置数据到指定位置
        /// </summary>
        /// <param name="bytes">要写入的字节数组</param>
        /// <param name="position">写入开始的位置</param>
        /// <param name="lenght">要写入的字节长度</param>
        /// <param name="callback">操作完成后的回调函数，参数为写入的字节数组</param>
        public void Set(byte[] bytes, long position, long lenght, Action<byte[]>? callback)
        {
            Set(bytes, position, lenght, 0, callback);
        }

        public void Set(byte[] bytes, long position, long lenght, long bytesPosition, Action<byte[]>? callback)
        {
            writeBytes = bytes;
            this.callback = callback;
            writeLength = lenght;
            writePosition = position;
            this.bytesPosition = bytesPosition;
        }

        /// <summary>
        /// 尝试重置写入操作的状态
        /// </summary>
        /// <returns>如果成功重置状态，则返回true；否则返回false</returns>
        public override bool TryReset()
        {
            writeBytes = null;
            callback = null;
            writeLength = 0;
            writePosition = 0;
            return true;
        }

        public override void Execute(FileOperateData item)
        {
            if (item.FStream.Length < writePosition + writeLength)
            {
                item.ExpandCapacity(writePosition + writeLength);
            }

            long writeIndex = writePosition;
            long bytesIndex = bytesPosition;
            var accessor = item.Accessor;
            for (long i = 0; i < writeLength; i++)
            {
                accessor.Write(writeIndex, writeBytes[bytesIndex]);
                writeIndex++;
                bytesIndex++;
            }
            callback?.Invoke(writeBytes);
        }
    }
}
