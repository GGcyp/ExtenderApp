using System;
using ExtenderApp.Abstract;
using ExtenderApp.Data;

namespace ExtenderApp.Common.IO.Splitter
{
    /// <summary>
    /// 表示一个分割写入操作，继承自StreamOperation类。
    /// </summary>
    internal class SplitterWriteOperation : FileStreamOperation
    {
        /// <summary>
        /// 要写入的字节数组。
        /// </summary>
        private byte[] writeBytes;

        /// <summary>
        /// 写入的起始位置。
        /// </summary>
        private long writePosition;

        /// <summary>
        /// 要写入的字节长度。
        /// </summary>
        private int writeLength;

        /// <summary>
        /// 写入的块索引
        /// </summary>
        private uint writeChunkIndex;

        /// <summary>
        /// 分割信息。
        /// </summary>
        private SplitterInfo splitterInfo;

        /// <summary>
        /// 操作完成后的回调方法。
        /// </summary>
        private Action<byte[]>? callbackHasByteArray;

        /// <summary>
        /// 回调委托
        /// </summary>
        private Action? callback;

        public SplitterWriteOperation(Action<IConcurrentOperation> releaseAction) : base(releaseAction)
        {
            writeBytes = Array.Empty<byte>();
            writePosition = 0;
            writeLength = 0;
            splitterInfo = SplitterInfo.Empty;
            callbackHasByteArray = null;
        }

        /// <summary>
        /// 设置字节数组和分隔符信息。
        /// </summary>
        /// <param name="bytes">要设置的字节数组。</param>
        /// <param name="splitterInfo">分隔符信息。</param>
        public void Set(byte[] bytes, SplitterInfo splitterInfo)
        {
            Set(bytes, bytes.Length, splitterInfo, null);
        }

        /// <summary>
        /// 设置要写入的字节数组、分割信息和回调方法。
        /// </summary>
        /// <param name="bytes">要写入的字节数组。</param>
        /// <param name="splitterInfo">分割信息。</param>
        /// <param name="action">操作完成后的回调方法。</param>
        public void Set(byte[] bytes, SplitterInfo splitterInfo, Action<byte[]>? action)
        {
            Set(bytes, bytes.Length, splitterInfo, action);
        }

        /// <summary>
        /// 设置字节数组的长度和分隔符信息。
        /// </summary>
        /// <param name="bytes">需要设置的字节数组。</param>
        /// <param name="length">字节数组的长度。</param>
        /// <param name="splitterInfo">分隔符信息。</param>
        public void Set(byte[] bytes, int length, SplitterInfo splitterInfo)
        {
            Set(bytes,length, splitterInfo, null);
        }

        /// <summary>
        /// 设置要写入的字节数组、写入长度、分割信息和回调方法。
        /// </summary>
        /// <param name="bytes">要写入的字节数组。</param>
        /// <param name="length">要写入的字节长度。</param>
        /// <param name="splitterInfo">分割信息。</param>
        /// <param name="action">操作完成后的回调方法。</param>
        public void Set(byte[] bytes, int length, SplitterInfo splitterInfo, Action<byte[]>? action)
        {
            Set(bytes, splitterInfo.GetLastChunkIndexPosition(), length, splitterInfo, action);
        }

        /// <summary>
        /// 将字节数组写入指定位置，并设置数据长度和分割信息。
        /// </summary>
        /// <param name="bytes">要写入的字节数组。</param>
        /// <param name="position">要写入的起始位置。</param>
        /// <param name="length">要写入的字节长度。</param>
        /// <param name="info">分割信息。</param>
        public void Set(byte[] bytes, long position, int length, SplitterInfo info)
        {
            writeBytes = bytes;
            writePosition = position;
            writeLength = length;
            splitterInfo = info;
            writeChunkIndex = splitterInfo.GetChunkIndex(writePosition);
        }

        /// <summary>
        /// 设置要写入的字节数组、写入位置、写入长度、分割信息和回调方法。
        /// </summary>
        /// <param name="bytes">要写入的字节数组。</param>
        /// <param name="position">写入的起始位置。</param>
        /// <param name="length">要写入的字节长度。</param>
        /// <param name="info">分割信息。</param>
        /// <param name="action">操作完成后的回调方法。</param>
        public void Set(byte[] bytes, long position, int length, SplitterInfo info, Action<byte[]>? action)
        {
            writeBytes = bytes;
            writePosition = position;
            writeLength = length;
            splitterInfo = info;
            callbackHasByteArray = action;
            writeChunkIndex = splitterInfo.GetChunkIndex(writePosition);
        }

        /// <summary>
        /// 设置要写入的字节数组、位置、长度、分割信息以及回调函数。
        /// </summary>
        /// <param name="bytes">要写入的字节数组。</param>
        /// <param name="position">要写入的起始位置。</param>
        /// <param name="length">要写入的字节长度。</param>
        /// <param name="info">分割信息。</param>
        /// <param name="action">回调函数，用于处理写入完成后的操作。</param>
        public void Set(byte[] bytes, long position, int length, SplitterInfo info, Action? action)
        {
            writeBytes = bytes;
            writePosition = position;
            writeLength = length;
            splitterInfo = info;
            callback = action;
            writeChunkIndex = splitterInfo.GetChunkIndex(writePosition);
        }

        /// <summary>
        /// 在指定的流中执行写入操作。
        /// </summary>
        /// <param name="stream">要写入的流。</param>
        public override void Execute(FileStream stream)
        {
            stream.Seek(writePosition, SeekOrigin.Begin);
            stream.Write(writeBytes, 0, writeLength);
            //splitterInfo.AddChunk(writePosition / splitterInfo.MaxChunkSize);
            splitterInfo.AddChunk(writeChunkIndex);
            callbackHasByteArray?.Invoke(writeBytes);
            callback?.Invoke();
        }

        /// <summary>
        /// 尝试重置 SplitterWriteOperation 对象的状态。
        /// </summary>
        /// <returns>如果成功重置状态，则返回 true；否则返回 false。</returns>
        public override bool TryReset()
        {
            writeBytes = Array.Empty<byte>();
            writePosition = 0;
            writeLength = 0;
            splitterInfo = SplitterInfo.Empty;
            callbackHasByteArray = null;
            return true;
        }
    }
}
