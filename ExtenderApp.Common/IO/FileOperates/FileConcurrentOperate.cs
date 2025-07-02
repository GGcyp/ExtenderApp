using System.Buffers;
using ExtenderApp.Abstract;
using ExtenderApp.Common.ConcurrentOperates;
using ExtenderApp.Common.Error;
using ExtenderApp.Common.IO.Splitter;
using ExtenderApp.Common.ObjectPools;
using ExtenderApp.Common.ObjectPools.Policy;
using ExtenderApp.Data;

namespace ExtenderApp.Common.IO
{
    /// <summary>
    /// 文件并发操作类，继承自ConcurrentOperate泛型类
    /// </summary>
    /// <typeparam name="FileOperatePolicy">文件操作策略类型</typeparam>
    /// <typeparam name="FileOperateData">文件操作数据类型</typeparam>
    public class FileConcurrentOperate : ConcurrentOperate<FileOperateData>, IFileOperate
    {
        private readonly ObjectPool<WriteOperation> _writeOperationPool;
        private readonly ObjectPool<ReadOperation> _readOperationPool;

        public FileConcurrentOperate()
        {
            _writeOperationPool = ObjectPool.Create(new SelfResetPooledObjectPolicy<WriteOperation>());
            _readOperationPool = ObjectPool.Create(new SelfResetPooledObjectPolicy<ReadOperation>());
        }

        public LocalFileInfo Info => Data.OperateInfo.LocalFileInfo;

        #region Read

        public byte[] Read()
        {
            return PrivateRead(0, (int)Data.CurrentCapacity, null, 0);
        }

        public byte[] Read(long filePosition, int length)
        {
            return PrivateRead(filePosition, length, null, 0);
        }

        public byte[] ReadForArrayPool(long filePosition, int length)
        {
            var bytes = ArrayPool<byte>.Shared.Rent(length);
            PrivateRead(filePosition, length, bytes, 0);
            return bytes;
        }

        public void Read(long filePosition, int length, byte[] bytes, int bytesStart = 0)
        {
            bytes = bytes ?? throw new ArgumentNullException(nameof(bytes));
            if (bytesStart < 0 || bytesStart >= bytes.Length)
                throw new ArgumentOutOfRangeException(nameof(bytesStart));

            PrivateRead(filePosition, length, bytes, bytesStart);
        }

        private byte[] PrivateRead(long position, int length, byte[]? bytes, int bytesStart)
        {
            var operation = _readOperationPool.Get();

            if (position + length > Data.CurrentCapacity)
                ErrorUtil.ArgumentOutOfRange(nameof(length), $"读取长度超出文件长度，文件长度为{Data.CurrentCapacity}，请求读取位置为{position}，请求读取长度为{length}。");

            length = length == -1 ? (int)Data.CurrentCapacity : length;
            operation.Set(position, length, bytes, bytesStart, null);
            Execute(operation);

            var result = operation.ReslutBytes;
            operation.Release();
            return result;
        }

        #endregion

        #region ReadAsync

        public void ReadAsync(long filePosition, int length, Action<byte[]> callback)
        {
            PrivateReadAsync(callback, null, filePosition, length, 0);
        }

        public byte[] ReadForArrayPoolAsync(long filePosition, int length, Action<byte[]> callback)
        {
            var bytes = ArrayPool<byte>.Shared.Rent(length);
            PrivateReadAsync(callback, bytes, filePosition, length, 0);
            return bytes;
        }

        public void ReadAsync(long filePosition, int length, byte[] bytes, Action<byte[]> callback, int bytesStart = 0)
        {
            bytes = bytes ?? throw new ArgumentNullException(nameof(bytes));
            if (bytesStart < 0 || bytesStart >= bytes.Length)
                throw new ArgumentOutOfRangeException(nameof(bytesStart));

            PrivateReadAsync(callback, bytes, filePosition, length, bytesStart);
        }

        private void PrivateReadAsync(Action<byte[]> callback, byte[] bytes, long position = 0, int length = -1, int bytesStart = 0)
        {
            if (position + length > Data.CurrentCapacity)
                ErrorUtil.ArgumentOutOfRange(nameof(length), $"读取长度超出文件长度，文件长度为{Data.CurrentCapacity}，请求读取位置为{position}，请求读取长度为{length}。");

            var operation = _readOperationPool.Get();

            length = length == -1 ? (int)Data.CurrentCapacity : length;
            operation.Set(position, length, bytes, bytesStart, callback);

            ExecuteAsync(operation);
        }

        #endregion

        #region Write

        public void Write(byte[] bytes)
        {
            PrivateWrite(bytes);
        }

        public void Write(byte[] bytes, long filePosition)
        {
            PrivateWrite(bytes, filePosition);
        }

        public void Write(byte[] bytes, long filePosition, int bytesPosition, int bytesLength)
        {
            PrivateWrite(bytes, filePosition, bytesPosition, bytesLength);
        }

        public void Write(ExtenderBinaryWriter writer, long filePosition)
        {
            if (writer.Rental.Value is null)
                throw new ArgumentNullException(nameof(writer));

            var operation = _writeOperationPool.Get();
            operation.Set(writer, filePosition, null);
            Execute(operation);
            operation.Release();
        }

        /// <summary>
        /// 将字节数组写入到文件中
        /// </summary>
        /// <param name="bytes">要写入的字节数组</param>
        /// <param name="filePosition">文件写入位置</param>
        /// <param name="bytesPosition">字节数组起始位置</param>
        /// <param name="length">要写入的字节长度</param>
        private void PrivateWrite(byte[] bytes, long filePosition = 0, int bytesPosition = 0, int length = -1)
        {
            if (bytes is null)
            {
                ErrorUtil.ArgumentNull(nameof(bytes));
                return;
            }

            if (length < 0)
            {
                length = (int)(bytes.LongLength - bytesPosition);
            }
            else if (bytes.LongLength < bytesPosition + length)
            {
                ErrorUtil.ArgumentOutOfRange(nameof(bytes), $"需要覆盖的数组长度小于需要长度");
            }

            var operation = _writeOperationPool.Get();
            operation.Set(bytes, filePosition, length, bytesPosition, null);
            Execute(operation);

            operation.Release();
        }

        #endregion

        #region WriteAsync

        public void WriteAsync(byte[] bytes, Action<byte[]>? callback = null)
        {
            PrivateWriteAsync(bytes, callback: callback);
        }

        public void WriteAsync(byte[] bytes, long filePosition, Action<byte[]>? callback = null)
        {
            PrivateWriteAsync(bytes, filePosition, callback: callback);
        }

        public void WriteAsync(byte[] bytes, long filePosition, int bytesPosition, int bytesLength, Action<byte[]>? callback = null)
        {
            PrivateWriteAsync(bytes, filePosition, bytesPosition, bytesLength, callback);
        }

        public void WriteAsync(ExtenderBinaryWriter writer, long filePosition, Action callback)
        {
            if (writer.Rental.Value is null)
                throw new ArgumentNullException(nameof(writer));

            var operation = _writeOperationPool.Get();
            operation.Set(writer, filePosition, callback);
            ExecuteAsync(operation);
            operation.Release();
        }

        /// <summary>
        /// 异步写入数据到文件中。
        /// </summary>
        /// <param name="bytes">要写入的字节数组。</param>
        /// <param name="filePosition">要写入的文件位置。默认为0。</param>
        /// <param name="bytesPosition">字节数组中的起始位置。默认为0。</param>
        /// <param name="length">要写入的字节数。默认为-1，表示写入剩余所有字节。</param>
        /// <param name="callback">写入完成后的回调函数。参数为写入的字节数组。</param>
        private void PrivateWriteAsync(byte[] bytes, long filePosition = 0, int bytesPosition = 0, int length = -1, Action<byte[]>? callback = null)
        {
            if (bytes is null)
            {
                ErrorUtil.ArgumentNull(nameof(bytes));
                return;
            }

            if (length < 0)
            {
                length = (int)(bytes.LongLength - bytesPosition);
            }
            else if (bytes.LongLength < bytesPosition + length)
            {
                ErrorUtil.ArgumentOutOfRange(nameof(bytes), $"需要覆盖的数组长度小于需要长度");
            }

            var operation = _writeOperationPool.Get();
            operation.Set(bytes, filePosition, length, bytesPosition, callback);
            ExecuteAsync(operation);
        }

        #endregion

        public void ExpandCapacity(long newCapacity)
        {
            Data.ExpandCapacity(newCapacity);
        }
    }
}
