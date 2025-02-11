using System.Buffers;
using ExtenderApp.Common.Error;

namespace ExtenderApp.Common.IO.Splitter
{
    /// <summary>
    /// 读取操作类，继承自 StreamOperation 类。
    /// </summary>
    internal class ReadOperation : StreamOperation
    {
        /// <summary>
        /// 回调方法，用于处理读取到的字节数组
        /// </summary>
        private Action<byte[]>? calback;

        /// <summary>
        /// 读取开始位置
        /// </summary>
        private int readPosition;

        /// <summary>
        /// 读取长度
        /// </summary>
        private int readLength;

        public override void Execute(Stream stream)
        {
            if (readPosition > stream.Length || readPosition + readLength > stream.Length)
            {
                ErrorUtil.ArgumentOutOfRange(nameof(ReadOperation));
            }

            var pool = ArrayPool<byte>.Shared;
            var readBytes = pool.Rent(readLength);
            stream.Read(readBytes, readPosition, readLength);
            calback?.Invoke(readBytes);
            pool.Return(readBytes);
        }

        /// <summary>
        /// 设置读取参数
        /// </summary>
        /// <param name="position">读取开始位置</param>
        /// <param name="length">读取长度</param>
        /// <param name="action">回调方法，用于处理读取到的字节数组</param>
        public void Set(int position, int length, Action<byte[]> action)
        {
            readPosition = position;
            readLength = length;
            calback = action;
        }

        public override bool TryReset()
        {
            calback = null;
            readPosition = 0;
            readLength = 0;
            return true;
        }
    }
}
