﻿using System.Buffers;
using System.IO;
using System.IO.MemoryMappedFiles;
using ExtenderApp.Abstract;
using ExtenderApp.Common.Error;

namespace ExtenderApp.Common.IO.Splitter
{
    /// <summary>
    /// 读取操作类，继承自 StreamOperation 类。
    /// </summary>
    internal class ReadOperation : FileOperation
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
        /// 读取到的字节数组。
        /// </summary>
        public byte[] ReadBytes { get; private set; }

        /// <summary>
        /// 初始化ReadOperation实例。
        /// </summary>
        /// <param name="releaseAction">释放操作时的回调动作。</param>
        public ReadOperation()
        {
            this.calback = null;
            this.readPosition = 0;
            this.readLength = 0;
            this.ReadBytes = Array.Empty<byte>();
        }

        /// <summary>
        /// 执行读取操作。
        /// </summary>
        /// <param name="item">内存映射视图访问器。</param>
        public override void Execute(MemoryMappedViewAccessor item)
        {
            if (readPosition < 0 || readLength < 0 || readPosition > item.Capacity || readPosition + readLength > item.Capacity)
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

            for (long i = readPosition; i < readLength; i++)
            {
                readBytes[i] = item.ReadByte(i);
            }
            ReadBytes = new ArraySegment<byte>(readBytes, 0, readLength).ToArray();
            calback?.Invoke(ReadBytes);
            pool.Return(readBytes);
        }

        /// <summary>
        /// 设置读取操作完成后的回调函数。
        /// </summary>
        /// <param name="action">回调函数。</param>
        public void Set(Action<byte[]> action)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            readPosition = 0;
            readLength = -1;
            calback = action;
        }

        /// <summary>
        /// 设置读取操作的起始位置和长度，以及回调函数。
        /// </summary>
        /// <param name="position">读取的起始位置。</param>
        /// <param name="length">读取的长度。</param>
        /// <param name="action">回调函数。</param>
        public void Set(long position, int length, Action<byte[]> action)
        {
            if (position < 0 || length < 0)
                throw new ArgumentOutOfRangeException(nameof(position));

            if (action == null)
                throw new ArgumentNullException(nameof(action));

            readPosition = position;
            readLength = length;
            calback = action;
        }

        /// <summary>
        /// 设置读取操作的起始位置、长度和字节数组。
        /// </summary>
        /// <param name="position">读取的起始位置。</param>
        /// <param name="length">读取的长度。</param>
        /// <param name="bytes">要读取的字节数组。</param>
        public void Set(long position, int length, byte[] bytes)
        {
            if (position < 0 || length < 0)
                throw new ArgumentOutOfRangeException(nameof(position));

            if (bytes == null)
                throw new ArgumentNullException(nameof(bytes));

            readPosition = position;
            readLength = bytes.Length;
            ReadBytes = bytes;
        }

        /// <summary>
        /// 设置读取位置和长度
        /// </summary>
        /// <param name="position">读取起始位置</param>
        /// <param name="length">读取长度</param>
        /// <exception cref="ArgumentOutOfRangeException">如果 position 或 length 小于 0，则抛出此异常</exception>
        public void Set(long position, int length)
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
            calback = null;
            readPosition = 0;
            readLength = 0;
            ReadBytes = Array.Empty<byte>();
            return true;
        }
    }
}
