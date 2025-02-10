using System.Buffers;

namespace ExtenderApp.Common.File.Splitter
{
    /// <summary>
    /// 读操作类，继承自SplitterOperation类
    /// </summary>
    internal class ReadOperation : SplitterOperation
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

        /// <summary>
        /// 设置读取参数
        /// </summary>
        /// <param name="position">读取开始位置</param>
        /// <param name="length">读取长度</param>
        /// <param name="action">回调方法，用于处理读取到的字节数组</param>
        /// <exception cref="System.ArgumentOutOfRangeException">如果读取位置超出SplitterInfo的长度范围，则抛出此异常</exception>
        public void Set(int position, int length, Action<byte[]> action)
        {
            if (position > SplitterInfo.Length || position + length > SplitterInfo.Length)
            {
                throw new System.ArgumentOutOfRangeException();
            }

            readPosition = position;
            readLength = length;
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
