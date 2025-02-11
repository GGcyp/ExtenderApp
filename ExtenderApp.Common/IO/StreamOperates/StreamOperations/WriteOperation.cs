

namespace ExtenderApp.Common.IO
{
    /// <summary>
    /// 写入操作类，继承自StreamOperation类
    /// </summary>
    internal class WriteOperation : StreamOperation
    {
        /// <summary>
        /// 待写入的字节数组
        /// </summary>
        private byte[] writeBytes;

        /// <summary>
        /// 写入操作的起始位置
        /// </summary>
        private int writePosition;

        /// <summary>
        /// 待写入的字节长度
        /// </summary>
        private int writeLength;

        /// <summary>
        /// 设置写入操作的起始位置、长度和字节数组
        /// </summary>
        /// <param name="position">写入操作的起始位置</param>
        /// <param name="length">待写入的字节长度</param>
        /// <param name="bytes">待写入的字节数组</param>
        public void Set(int position, int length, byte[] bytes)
        {
            writePosition = position;
            writeBytes = bytes;
            writeLength = length;
        }

        /// <summary>
        /// 设置写入操作的起始位置和字节数组
        /// </summary>
        /// <param name="position">写入操作的起始位置</param>
        /// <param name="bytes">待写入的字节数组</param>
        public void Set(int position, byte[] bytes)
        {
            writePosition = position;
            writeBytes = bytes;
            writeLength = bytes.Length;
        }

        /// <summary>
        /// 执行写入操作
        /// </summary>
        /// <param name="stream">待写入的流</param>
        public override void Execute(Stream stream)
        {
            stream.Write(writeBytes, writePosition, writeLength);
        }

        /// <summary>
        /// 尝试重置写入操作的状态
        /// </summary>
        /// <returns>如果成功重置状态，则返回true；否则返回false</returns>
        public override bool TryReset()
        {
            writeBytes = null;
            writePosition = 0;
            writeLength = 0;
            return true;
        }
    }
}
