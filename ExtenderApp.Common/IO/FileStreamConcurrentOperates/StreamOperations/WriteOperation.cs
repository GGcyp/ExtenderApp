using ExtenderApp.Abstract;

namespace ExtenderApp.Common.IO
{
    /// <summary>
    /// 写入操作类，继承自StreamOperation类
    /// </summary>
    internal class WriteOperation : FileStreamOperation
    {
        /// <summary>
        /// 待写入的字节数组
        /// </summary>
        private byte[] writeBytes;

        /// <summary>
        /// 回调委托，用于处理字节数组
        /// </summary>
        private Action<byte[]>? callback;

        public WriteOperation(Action<IConcurrentOperation> releaseAction) : base(releaseAction)
        {
        }

        /// <summary>
        /// 设置字节数组和回调函数
        /// </summary>
        /// <param name="bytes">要设置的字节数组</param>
        /// <param name="callback">回调函数，可选参数</param>
        public void Set(byte[] bytes, Action<byte[]>? callback)
        {
            writeBytes = bytes;
            this.callback = callback;
        }

        /// <summary>
        /// 执行写入操作
        /// </summary>
        /// <param name="stream">待写入的流</param>
        public override void Execute(FileStream stream)
        {
            stream.Write(writeBytes, 0, writeBytes.Length);
            callback?.Invoke(writeBytes);
        }

        /// <summary>
        /// 尝试重置写入操作的状态
        /// </summary>
        /// <returns>如果成功重置状态，则返回true；否则返回false</returns>
        public override bool TryReset()
        {
            writeBytes = null;
            callback = null;
            return true;
        }
    }
}
