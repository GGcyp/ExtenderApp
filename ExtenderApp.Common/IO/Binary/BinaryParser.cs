using System.Buffers;
using ExtenderApp.Abstract;
using ExtenderApp.Common.DataBuffers;
using ExtenderApp.Common.Error;
using ExtenderApp.Common.IO.FileParsers;
using ExtenderApp.Common.IO.Splitter;
using ExtenderApp.Common.ObjectPools;
using ExtenderApp.Common.ObjectPools.Policy;
using ExtenderApp.Data;

namespace ExtenderApp.Common.IO.Binaries
{
    /// <summary>
    /// 二进制解析器类
    /// </summary>
    internal class BinaryParser : FileParser, IBinaryParser
    {
        /// <summary>
        /// 最大字节数组长度
        /// </summary>
        private const int MaxByteArrayLength = 65536;

        /// <summary>
        /// 建议的连续内存大小。
        /// </summary>
        /// <remarks>
        /// 值为1MB（1024 * 1024字节）。
        /// </remarks>
        private const int SuggestedContiguousMemorySize = 1024 * 1024;

        /// <summary>
        /// 二进制格式化器解析器
        /// </summary>
        private readonly IBinaryFormatterResolver _resolver;

        /// <summary>
        /// 字节序列池
        /// </summary>
        private readonly SequencePool<byte> _sequencePool;

        private readonly ObjectPool<WriteOperation> _writeOperationPool;
        private readonly ObjectPool<ReadOperation> _readOperationPool;
        private readonly string _binaryFileExtensions;

        public BinaryParser(IBinaryFormatterResolver binaryFormatterResolver, SequencePool<byte> sequencePool, FileOperateStore store) : base(store)
        {
            _resolver = binaryFormatterResolver;
            _sequencePool = sequencePool;
            _writeOperationPool = ObjectPool.Create(new SelfResetPooledObjectPolicy<WriteOperation>());
            _readOperationPool = ObjectPool.Create(new SelfResetPooledObjectPolicy<ReadOperation>());
            _binaryFileExtensions = FileExtensions.BinaryFileExtensions;
        }

        #region Get

        public IBinaryFormatter<T> GetFormatter<T>()
        {
            return _resolver.GetFormatterWithVerify<T>();
        }

        #endregion

        #region Read

        public override T? Read<T>(ExpectLocalFileInfo info) where T : default
        {
            if (info.IsEmpty)
            {
                ErrorUtil.ArgumentNull(nameof(info));
            }

            return Read<T>(info.CreateReadWriteOperate(_binaryFileExtensions));
        }

        public override T? Read<T>(FileOperateInfo info) where T : default
        {
            if (info.IsEmpty || !info.LocalFileInfo.Exists || !info.IsRead())
            {
                //var formatter = _resolver.GetFormatter<T>();
                //if (formatter == null)
                //    return default;

                //return formatter.Default;
                return default;
            }

            return PrivateRead<T>(GetOperate(info));
        }

        public override T? Read<T>(IConcurrentOperate fileOperate) where T : default
        {
            if (fileOperate is not FileConcurrentOperate operate
                || !fileOperate.CanOperate)
            {
                ErrorUtil.ArgumentNull(nameof(fileOperate));
                return default;
            }

            return PrivateRead<T>(operate);
        }

        public override T? Read<T>(ExpectLocalFileInfo info, long position, long length) where T : default
        {
            if (info.IsEmpty)
            {
                ErrorUtil.ArgumentNull(nameof(info));
            }

            return Read<T>(info.CreateReadWriteOperate(_binaryFileExtensions), position, length);
        }

        public override T? Read<T>(FileOperateInfo info, long position, long length) where T : default
        {
            if (info.IsEmpty || !info.LocalFileInfo.Exists || !info.IsRead())
            {
                //var formatter = _resolver.GetFormatter<T>();
                //if (formatter == null)
                //    return default;

                //return formatter.Default;
                return default;
            }

            return PrivateRead<T>(GetOperate(info), position, length);
        }

        public override T? Read<T>(IConcurrentOperate fileOperate, long position, long length) where T : default
        {
            if (fileOperate is not FileConcurrentOperate operate
                || !fileOperate.CanOperate)
            {
                ErrorUtil.ArgumentNull(nameof(fileOperate));
                return default;
            }

            return PrivateRead<T>(operate, position, length);
        }

        public byte[]? Read(ExpectLocalFileInfo info, long position, long length)
        {
            if (info.IsEmpty)
            {
                ErrorUtil.ArgumentNull(nameof(info));
            }

            return Read(info.CreateReadWriteOperate(_binaryFileExtensions), position, length);
        }

        public byte[]? Read(LocalFileInfo info, long position, long length)
        {
            if (info.IsEmpty)
            {
                ErrorUtil.ArgumentNull(nameof(info));
            }
            return Read(info.CreateReadWriteOperate(), position, length);
        }

        public byte[]? Read(FileOperateInfo info, long position, long length)
        {
            if (info.IsEmpty || !info.IsRead())
            {
                return null;
            }

            var tempArray = PrivateRead(GetOperate(info), position, length);
            var reslut = tempArray.AsSpan().Slice(0, (int)length).ToArray();
            ArrayPool<byte>.Shared.Return(tempArray);
            return reslut;
        }

        public byte[]? Read(IConcurrentOperate? fileOperate, long position, long length)
        {
            if (fileOperate is not FileConcurrentOperate operate
                || !fileOperate.CanOperate)
            {
                ErrorUtil.ArgumentNull(nameof(fileOperate));
                return default;
            }

            var tempArray = PrivateRead(operate, position, length);
            var reslut = tempArray.AsSpan().Slice(0, (int)length).ToArray();
            ArrayPool<byte>.Shared.Return(tempArray);
            return reslut;
        }

        public bool Read(ExpectLocalFileInfo info, long position, long length, byte[] bytes)
        {
            if (info.IsEmpty)
            {
                ErrorUtil.ArgumentNull(nameof(info));
                return false;
            }

            return Read(info.CreateReadWriteOperate(_binaryFileExtensions), position, length, bytes);
        }

        public bool Read(LocalFileInfo info, long position, long length, byte[] bytes)
        {
            if (info.IsEmpty)
            {
                ErrorUtil.ArgumentNull(nameof(info));
                return false;
            }
            return Read(info.CreateReadWriteOperate(), position, length, bytes);
        }

        public bool Read(FileOperateInfo info, long position, long length, byte[] bytes)
        {
            if (bytes is null)
            {
                ErrorUtil.ArgumentNull(nameof(bytes));
                return false;
            }
            else if (bytes.LongLength < length)
            {
                ErrorUtil.ArgumentOutOfRange(nameof(bytes), $"需要覆盖的数组长度小于需要长度");
                return false;
            }

            if (info.IsEmpty || !info.IsRead())
            {
                ErrorUtil.ArgumentNull(nameof(info));
                return false;
            }

            PrivateRead(GetOperate(info), position, length);

            return true;
        }

        public bool Read(IConcurrentOperate? fileOperate, long position, long length, byte[] bytes)
        {
            if (bytes is null)
            {
                ErrorUtil.ArgumentNull(nameof(bytes));
                return false;
            }
            else if (bytes.LongLength < length)
            {
                ErrorUtil.ArgumentOutOfRange(nameof(bytes), $"需要覆盖的数组长度小于需要长度");
                return false;
            }

            if (fileOperate is not FileConcurrentOperate operate
                || !fileOperate.CanOperate)
            {
                ErrorUtil.ArgumentNull(nameof(fileOperate));
                return false;
            }

            PrivateRead(operate, position, length);
            return true;
        }

        /// <summary>
        /// 私有方法，用于从指定的并发操作接口中读取指定类型的数据。
        /// </summary>
        /// <typeparam name="T">需要读取的数据类型。</typeparam>
        /// <param name="operate">并发操作接口。</param>
        /// <param name="position">开始读取的位置。</param>
        /// <param name="length">需要读取的长度，默认为-1，表示读取到文件末尾。</param>
        /// <returns>读取到的数据。</returns>
        private T? PrivateRead<T>(FileConcurrentOperate operate, long position = 0, long length = 0)
        {
            var operation = _readOperationPool.Get();

            if (position + length > operate.Data.CurrentCapacity)
                ErrorUtil.ArgumentOutOfRange(nameof(length), $"读取长度超出文件长度，文件长度为{operate.Data.CurrentCapacity}，请求读取位置为{position}，请求读取长度为{length}。");

            length = length == -1 ? operate.Data.CurrentCapacity : length;
            operation.Set(position, length);
            operate.Execute(operation);

            var result = Deserialize<T>(operation.ReslutBytes);
            operation.Release();
            return result;
        }

        /// <summary>
        /// 私有方法，用于从指定的并发操作接口中读取字节数组数据。
        /// </summary>
        /// <param name="operate">并发操作接口。</param>
        /// <param name="bytes">存储读取数据的字节数组。</param>
        /// <param name="position">开始读取的位置。</param>
        /// <param name="length">需要读取的长度，默认为-1，表示读取到文件末尾。</param>
        /// <returns>读取到的字节数组。</returns>
        private byte[] PrivateRead(FileConcurrentOperate operate, long position = 0, long length = -1)
        {
            var operation = _readOperationPool.Get();

            if (position + length > operate.Data.CurrentCapacity)
                ErrorUtil.ArgumentOutOfRange(nameof(length), $"读取长度超出文件长度，文件长度为{operate.Data.CurrentCapacity}，请求读取位置为{position}，请求读取长度为{length}。");

            length = length == -1 ? operate.Data.CurrentCapacity : length;
            operation.Set(position, length);
            operate.Execute(operation);

            var result = operation.ReslutBytes.ToArray();
            operation.Release();
            return result;
        }

        #endregion

        #region ReadAsync

        public override void ReadAsync<T>(ExpectLocalFileInfo info, Action<T?> callback) where T : default
        {
            if (info.IsEmpty)
            {
                ErrorUtil.ArgumentNull(nameof(info));
            }

            ReadAsync(info.CreateReadWriteOperate(_binaryFileExtensions), callback);
        }

        public override void ReadAsync<T>(FileOperateInfo info, Action<T?> callback) where T : default
        {
            if (info.IsEmpty || !info.IsRead())
            {
                ErrorUtil.ArgumentNull(nameof(info));
            }

            var operate = GetOperate(info);
            PrivateReadAsync(operate, callback);
        }

        public override void ReadAsync<T>(IConcurrentOperate fileOperate, Action<T?> callback) where T : default
        {
            if (fileOperate is not FileConcurrentOperate operate
                || !fileOperate.CanOperate)
            {
                ErrorUtil.ArgumentNull(nameof(fileOperate));
                return;
            }
            PrivateReadAsync(operate, callback);
        }

        public override void ReadAsync<T>(ExpectLocalFileInfo info, long position, long length, Action<T?> callback) where T : default
        {
            if (info.IsEmpty)
            {
                ErrorUtil.ArgumentNull(nameof(info));
            }
            ReadAsync(info.CreateReadWriteOperate(_binaryFileExtensions), position, length, callback);
        }

        public override void ReadAsync<T>(FileOperateInfo info, long position, long length, Action<T?> callback) where T : default
        {
            if (info.IsEmpty || !info.IsRead())
            {
                ErrorUtil.ArgumentNull(nameof(info));
            }

            PrivateReadAsync(GetOperate(info), callback, position, length);
        }

        public override void ReadAsync<T>(IConcurrentOperate fileOperate, long position, long length, Action<T?> callback) where T : default
        {
            if (fileOperate is not FileConcurrentOperate operate
                || !fileOperate.CanOperate)
            {
                ErrorUtil.ArgumentNull(nameof(fileOperate));
                return;
            }
            PrivateReadAsync(operate, callback, position, length);
        }

        public void ReadAsync(ExpectLocalFileInfo info, long position, long length, Action<byte[]?> callback)
        {
            if (info.IsEmpty)
            {
                ErrorUtil.ArgumentNull(nameof(info));
            }

            ReadAsync(info.CreateReadWriteOperate(_binaryFileExtensions), position, length, callback);
        }

        public void ReadAsync(LocalFileInfo info, long position, long length, Action<byte[]?> callback)
        {
            if (info.IsEmpty)
            {
                ErrorUtil.ArgumentNull(nameof(info));
            }

            ReadAsync(info.CreateReadWriteOperate(), position, length, callback);
        }

        public void ReadAsync(FileOperateInfo info, long position, long length, Action<byte[]?> callback)
        {
            if (info.IsEmpty || !info.IsRead())
            {
                ErrorUtil.ArgumentNull(nameof(info));
            }

            PrivateReadAsync(GetOperate(info), callback, position, length);
        }

        public void ReadAsync(IConcurrentOperate? fileOperate, long position, long length, Action<byte[]?> callback)
        {
            if (fileOperate is not FileConcurrentOperate operate
                || !fileOperate.CanOperate)
            {
                ErrorUtil.ArgumentNull(nameof(fileOperate));
                return;
            }

            PrivateReadAsync(operate, callback, null, position, length);
        }

        public void ReadAsync(ExpectLocalFileInfo info, long position, long length, byte[] bytes, Action<bool> callback)
        {
            if (info.IsEmpty)
            {
                ErrorUtil.ArgumentNull(nameof(info));
            }
            ReadAsync(info.CreateReadWriteOperate(_binaryFileExtensions), position, length, bytes, callback);
        }

        public void ReadAsync(LocalFileInfo info, long position, long length, byte[] bytes, Action<bool> callback)
        {
            if (info.IsEmpty)
            {
                ErrorUtil.ArgumentNull(nameof(info));
            }
            ReadAsync(info.CreateReadWriteOperate(), position, length, bytes, callback);
        }

        public void ReadAsync(FileOperateInfo info, long position, long length, byte[] bytes, Action<bool> callback)
        {
            if (info.IsEmpty)
            {
                ErrorUtil.ArgumentNull(nameof(info));
            }

            PrivateReadAsync(GetOperate(info), callback, bytes, position, length);
        }

        public void ReadAsync(IConcurrentOperate? fileOperate, long position, long length, byte[] bytes, Action<bool> callback)
        {
            if (fileOperate is not FileConcurrentOperate operate
                || !fileOperate.CanOperate)
            {
                ErrorUtil.ArgumentNull(nameof(fileOperate));
                return;
            }

            PrivateReadAsync(operate, callback, bytes, position, length);
        }

        /// <summary>
        /// 异步读取数据，将数据反序列化为泛型类型T，并调用回调函数处理结果。
        /// </summary>
        /// <typeparam name="T">要反序列化的类型</typeparam>
        /// <param name="operate">并发操作接口</param>
        /// <param name="callback">处理结果的回调函数</param>
        /// <param name="position">开始读取的位置</param>
        /// <param name="length">要读取的长度</param>
        private void PrivateReadAsync<T>(FileConcurrentOperate operate, Action<T?> callback, long position = 0, long length = -1)
        {
            if (position + length > operate.Data.CurrentCapacity)
                ErrorUtil.ArgumentOutOfRange(nameof(length), $"读取长度超出文件长度，文件长度为{operate.Data.CurrentCapacity}，请求读取位置为{position}，请求读取长度为{length}。");

            var operation = _readOperationPool.Get();
            DataBuffer dataBuffer = DataBuffer.GetDataBuffer();
            dataBuffer.SetProcessAction<byte[]>(b =>
            {
                var reslut = Deserialize<T>(b);
                callback?.Invoke(reslut);
            });

            length = length == -1 ? operate.Data.CurrentCapacity : length;
            operation.Set(position, length, dataBuffer);

            operate.ExecuteAsync(operation);
        }

        /// <summary>
        /// 异步读取数据到字节数组，并调用回调函数处理结果。
        /// </summary>
        /// <param name="operate">并发操作接口</param>
        /// <param name="callback">处理结果的回调函数</param>
        /// <param name="bytes">存储读取数据的字节数组</param>
        /// <param name="position">开始读取的位置</param>
        /// <param name="length">要读取的长度</param>
        private void PrivateReadAsync(FileConcurrentOperate operate, Action<byte[]?> callback, byte[]? bytes, long position = 0, long length = -1)
        {
            if (position + length > operate.Data.CurrentCapacity)
                ErrorUtil.ArgumentOutOfRange(nameof(length), $"读取长度超出文件长度，文件长度为{operate.Data.CurrentCapacity}，请求读取位置为{position}，请求读取长度为{length}。");

            var operation = _readOperationPool.Get();

            length = length == -1 ? operate.Data.CurrentCapacity : length;
            operation.Set(position, length, callback);

            operate.ExecuteAsync(operation);
        }

        /// <summary>
        /// 异步读取数据到字节数组，并调用回调函数表示读取完成。
        /// </summary>
        /// <param name="operate">并发操作接口</param>
        /// <param name="callback">表示读取完成的回调函数</param>
        /// <param name="bytes">存储读取数据的字节数组</param>
        /// <param name="position">开始读取的位置</param>
        /// <param name="length">要读取的长度</param>
        private void PrivateReadAsync(FileConcurrentOperate operate, Action<bool> callback, byte[] bytes, long position = 0, long length = -1)
        {
            if (position + length > operate.Data.CurrentCapacity)
                ErrorUtil.ArgumentOutOfRange(nameof(length), $"读取长度超出文件长度，文件长度为{operate.Data.CurrentCapacity}，请求读取位置为{position}，请求读取长度为{length}。");

            var operation = _readOperationPool.Get();

            length = length == -1 ? operate.Data.CurrentCapacity : length;
            operation.Set(position, length, bytes);

            operate.ExecuteAsync(operation);
        }

        #endregion

        #region Write

        public override void Write<T>(ExpectLocalFileInfo info, T value)
        {
            if (info.IsEmpty)
            {
                ErrorUtil.ArgumentNull(nameof(info));
            }
            Write(info.CreateReadWriteOperate(_binaryFileExtensions), value);
        }

        public override void Write<T>(FileOperateInfo info, T value)
        {
            if (info.IsEmpty || !info.IsWrite())
            {
                ErrorUtil.ArgumentNull(nameof(info));
            }
            PrivateWrite(GetOperate(info), value);
        }

        public override void Write<T>(IConcurrentOperate fileOperate, T value)
        {
            if (fileOperate is not FileConcurrentOperate operate
                || !fileOperate.CanOperate)
            {
                ErrorUtil.ArgumentNull(nameof(fileOperate));
                return;
            }
            PrivateWrite(operate, value);
        }

        public override void Write<T>(ExpectLocalFileInfo info, T value, long position)
        {
            if (info.IsEmpty)
            {
                ErrorUtil.ArgumentNull(nameof(info));
            }
            Write(info.CreateReadWriteOperate(_binaryFileExtensions), value, position);
        }

        public override void Write<T>(FileOperateInfo info, T value, long position)
        {
            if (!info.IsEmpty || !info.IsWrite())
            {
                ErrorUtil.ArgumentNull(nameof(info));
            }
            PrivateWrite(GetOperate(info), value, position);
        }

        public override void Write<T>(IConcurrentOperate fileOperate, T value, long position)
        {
            if (fileOperate is not FileConcurrentOperate operate
                || !fileOperate.CanOperate)
            {
                ErrorUtil.ArgumentNull(nameof(fileOperate));
                return;
            }
            PrivateWrite(operate, value, position);
        }

        public void Write(ExpectLocalFileInfo info, byte[] bytes, long filePosition)
        {
            if (info.IsEmpty)
            {
                ErrorUtil.ArgumentNull(nameof(info));
            }
            Write(info.CreateReadWriteOperate(_binaryFileExtensions), bytes, filePosition);
        }

        public void Write(LocalFileInfo info, byte[] bytes, long filePosition)
        {
            if (info.IsEmpty)
            {
                ErrorUtil.ArgumentNull(nameof(info));
            }
            Write(info.CreateReadWriteOperate(), bytes, filePosition);
        }

        public void Write(FileOperateInfo info, byte[] bytes, long filePosition)
        {
            if (info.IsEmpty || !info.IsWrite())
            {
                ErrorUtil.ArgumentNull(nameof(info));
            }
            PrivateWrite(GetOperate(info), bytes, filePosition);
        }

        public void Write(IConcurrentOperate? fileOperate, byte[] bytes, long filePosition)
        {
            if (fileOperate is not FileConcurrentOperate operate
                || !fileOperate.CanOperate)
            {
                ErrorUtil.ArgumentNull(nameof(fileOperate));
                return;
            }
            PrivateWrite(operate, bytes, filePosition);
        }

        public void Write(ExpectLocalFileInfo info, byte[] bytes, long filePosition, int bytesPosition, int bytesLength)
        {
            if (info.IsEmpty)
            {
                ErrorUtil.ArgumentNull(nameof(info));
            }
            Write(info.CreateReadWriteOperate(_binaryFileExtensions), bytes, filePosition, bytesPosition, bytesLength);
        }

        public void Write(LocalFileInfo info, byte[] bytes, long filePosition, int bytesPosition, int bytesLength)
        {
            if (info.IsEmpty)
            {
                ErrorUtil.ArgumentNull(nameof(info));
            }
            Write(info.CreateReadWriteOperate(), bytes, filePosition, bytesPosition, bytesLength);
        }

        public void Write(FileOperateInfo info, byte[] bytes, long filePosition, int bytesPosition, int bytesLength)
        {
            if (info.IsEmpty || !info.IsWrite())
            {
                ErrorUtil.ArgumentNull(nameof(info));
            }

            PrivateWrite(GetOperate(info), bytes, filePosition, bytesPosition, bytesLength);
        }

        public void Write(IConcurrentOperate? fileOperate, byte[] bytes, long filePosition, int bytesPosition, int bytesLength)
        {
            if (fileOperate is not FileConcurrentOperate operate
                || !fileOperate.CanOperate)
            {
                ErrorUtil.ArgumentNull(nameof(fileOperate));
                return;
            }
            PrivateWrite(operate, bytes, filePosition, bytesPosition, bytesLength);
        }

        private void PrivateWrite<T>(FileConcurrentOperate operate, T value, long filePosition = 0)
        {
            var bytes = SerializeForArrayPool(value, out long length);
            PrivateWrite(operate, bytes, filePosition, 0, length);
            ArrayPool<byte>.Shared.Return(bytes);
        }

        private void PrivateWrite(FileConcurrentOperate operate, byte[] bytes, long filePosition = 0, long bytesPosition = 0, long length = -1)
        {
            if (bytes is null)
            {
                ErrorUtil.ArgumentNull(nameof(bytes));
                return;
            }

            if (length < 0)
            {
                length = bytes.LongLength - bytesPosition;
            }
            else if (bytes.LongLength < bytesPosition + length)
            {
                ErrorUtil.ArgumentOutOfRange(nameof(bytes), $"需要覆盖的数组长度小于需要长度");
            }

            var operation = _writeOperationPool.Get();
            operation.Set(bytes, filePosition, length, bytesPosition, null);
            operate.Execute(operation);

            operation.Release();
        }

        #endregion

        #region WriteAsync

        public override void WriteAsync<T>(ExpectLocalFileInfo info, T value, Action? callback = null)
        {
            if (info.IsEmpty)
            {
                ErrorUtil.ArgumentNull(nameof(info));
            }
            WriteAsync(info.CreateReadWriteOperate(_binaryFileExtensions), value, callback);
        }

        public override void WriteAsync<T>(FileOperateInfo info, T value, Action? callback = null)
        {
            if (info.IsEmpty || !info.IsWrite())
            {
                ErrorUtil.ArgumentNull(nameof(info));
            }

            PrivateWriteAsync(GetOperate(info), value, 0, callback);
        }

        public override void WriteAsync<T>(IConcurrentOperate fileOperate, T value, Action? callback = null)
        {
            if (fileOperate is not FileConcurrentOperate operate
                || !fileOperate.CanOperate)
            {
                ErrorUtil.ArgumentNull(nameof(fileOperate));
                return;
            }

            PrivateWriteAsync(operate, value, 0, callback);
        }

        public override void WriteAsync<T>(ExpectLocalFileInfo info, T value, long position, Action? callback = null)
        {
            if (info.IsEmpty)
            {
                ErrorUtil.ArgumentNull(nameof(info));
            }
            WriteAsync(info.CreateReadWriteOperate(_binaryFileExtensions), value, position, callback);
        }

        public override void WriteAsync<T>(FileOperateInfo info, T value, long position, Action? callback = null)
        {
            if (info.IsEmpty || !info.IsWrite())
            {
                ErrorUtil.ArgumentNull(nameof(info));
            }
            PrivateWriteAsync(GetOperate(info), value, position, callback);
        }

        public override void WriteAsync<T>(IConcurrentOperate fileOperate, T value, long position, Action? callback = null)
        {
            if (fileOperate is not FileConcurrentOperate operate
                || !fileOperate.CanOperate)
            {
                ErrorUtil.ArgumentNull(nameof(fileOperate));
                return;
            }
            PrivateWriteAsync(operate, value, position, callback);
        }

        public void WriteAsync(ExpectLocalFileInfo info, byte[] bytes, long filePosition, int bytesPosition, int bytesLength, Action<byte[]>? callback = null)
        {
            if (info.IsEmpty)
            {
                ErrorUtil.ArgumentNull(nameof(info));
            }
            WriteAsync(info.CreateReadWriteOperate(_binaryFileExtensions), bytes, filePosition, bytesPosition, bytesLength, callback);
        }

        public void WriteAsync(LocalFileInfo info, byte[] bytes, long filePosition, int bytesPosition, int bytesLength, Action<byte[]>? callback = null)
        {
            if (info.IsEmpty)
            {
                ErrorUtil.ArgumentNull(nameof(info));
            }
            WriteAsync(info.CreateReadWriteOperate(), bytes, filePosition, bytesPosition, bytesLength, callback);
        }

        public void WriteAsync(FileOperateInfo info, byte[] bytes, long filePosition, int bytesPosition, int bytesLength, Action<byte[]>? callback = null)
        {
            if (info.IsEmpty || !info.IsWrite())
            {
                ErrorUtil.ArgumentNull(nameof(info));
            }
            PrivateWriteAsync(GetOperate(info), bytes, filePosition, bytesPosition, bytesLength, callback);
        }

        public void WriteAsync(IConcurrentOperate? fileOperate, byte[] bytes, long filePosition, int bytesPosition, int bytesLength, Action<byte[]>? callback = null)
        {
            if (fileOperate is not FileConcurrentOperate operate
                || !fileOperate.CanOperate)
            {
                ErrorUtil.ArgumentNull(nameof(fileOperate));
                return;
            }
            PrivateWriteAsync(operate, bytes, filePosition, bytesPosition, bytesLength, callback);
        }


        public void WriteAsync(ExpectLocalFileInfo info, byte[] bytes, long filePosition, Action<byte[]>? callback = null)
        {
            if (info.IsEmpty)
            {
                ErrorUtil.ArgumentNull(nameof(info));
            }
            WriteAsync(info.CreateReadWriteOperate(_binaryFileExtensions), bytes, filePosition, callback);
        }

        public void WriteAsync(LocalFileInfo info, byte[] bytes, long filePosition, Action<byte[]>? callback = null)
        {
            if (info.IsEmpty)
            {
                ErrorUtil.ArgumentNull(nameof(info));
            }
            WriteAsync(info.CreateReadWriteOperate(), bytes, filePosition, callback);
        }

        public void WriteAsync(FileOperateInfo info, byte[] bytes, long filePosition, Action<byte[]>? callback = null)
        {
            if (info.IsEmpty || !info.IsWrite())
            {
                ErrorUtil.ArgumentNull(nameof(info));
            }
            PrivateWriteAsync(GetOperate(info), bytes, filePosition, 0, -1, callback);
        }

        public void WriteAsync(IConcurrentOperate? fileOperate, byte[] bytes, long filePosition, Action<byte[]>? callback = null)
        {
            if (fileOperate is not FileConcurrentOperate operate
                || !fileOperate.CanOperate)
            {
                ErrorUtil.ArgumentNull(nameof(fileOperate));
                return;
            }
            PrivateWriteAsync(operate, bytes, filePosition, 0, -1, callback);
        }

        private void PrivateWriteAsync<T>(FileConcurrentOperate operate, T value, long filePosition = 0, Action? callback = null)
        {
            Action<byte[]> action = (bytes) =>
            {
                callback?.Invoke();
                ArrayPool<byte>.Shared.Return(bytes);
            };

            var bytes = SerializeForArrayPool(value, out long length);
            PrivateWriteAsync(operate, bytes, filePosition, 0, length, action);
        }

        private void PrivateWriteAsync(FileConcurrentOperate operate, byte[] bytes, long filePosition = 0, long bytesPosition = 0, long length = -1, Action<byte[]>? callback = null)
        {
            if (bytes is null)
            {
                ErrorUtil.ArgumentNull(nameof(bytes));
                return;
            }

            if (length < 0)
            {
                length = bytes.LongLength - bytesPosition;
            }
            else if (bytes.LongLength < bytesPosition + length)
            {
                ErrorUtil.ArgumentOutOfRange(nameof(bytes), $"需要覆盖的数组长度小于需要长度");
            }

            var operation = _writeOperationPool.Get();
            operation.Set(bytes, filePosition, length, bytesPosition, callback);
            operate.ExecuteAsync(operation);
        }

        #endregion

        #region Serialize

        public void Serialize<T>(T value, byte[] bytes, out int length)
        {
            Serialize(value, bytes, out long longLength);
            length = (int)longLength;
        }

        public void Serialize<T>(T value, byte[] bytes, out long length)
        {
            bytes.ArgumentNull(nameof(bytes));

            length = GetLength(value);
            if (bytes.LongLength < length)
            {
                throw new ArgumentOutOfRangeException(nameof(length));
            }

            var writer = new ExtenderBinaryWriter(_sequencePool, bytes);
            Serialize(ref writer, value);
            writer.Flush();
        }

        public void Serialize<T>(T value, byte[] bytes)
        {
            bytes.ArgumentNull(nameof(bytes));

            var result = SerializeForArrayPool(value, out long length);
            if (bytes.LongLength < length)
            {
                throw new ArgumentOutOfRangeException(nameof(length));
            }

            result.CopyTo(bytes, 0);
            ArrayPool<byte>.Shared.Return(result);
        }

        public byte[] Serialize<T>(T value)
        {
            byte[] bytes = ArrayPool<byte>.Shared.Rent(MaxByteArrayLength);
            var writer = new ExtenderBinaryWriter(_sequencePool, bytes);
            Serialize(ref writer, value);

            var result = writer.FlushAndGetArray();
            ArrayPool<byte>.Shared.Return(bytes);
            return result;
        }

        public void Serialize<T>(ref ExtenderBinaryWriter writer, T value)
        {
            try
            {
                _resolver.GetFormatterWithVerify<T>().Serialize(ref writer, value);
            }
            catch (Exception ex)
            {
                //throw new MessagePackSerializationException($"Failed to serialize {typeof(T).FullName} value.", ex);
                throw;
            }
        }

        public void Serialize<T>(Stream stream, T value)
        {
            var rent = _sequencePool.Rent();
            var writer = new ExtenderBinaryWriter(rent.Value);
            Serialize(ref writer, value);
            writer.Commit();

            foreach (ReadOnlyMemory<byte> segment in rent.Value.AsReadOnlySequence)
            {
                var sharedBuffer = ArrayPool<byte>.Shared.Rent(segment.Length);
                segment.CopyTo(sharedBuffer);
                stream.Write(sharedBuffer, 0, sharedBuffer.Length);
            }
            rent.Dispose();
        }

        /// <summary>
        /// 将指定对象序列化为字节数组，并存储在 ArrayPool 中。
        /// </summary>
        /// <typeparam name="T">要序列化的对象的类型。</typeparam>
        /// <param name="value">要序列化的对象。</param>
        /// <returns>包含序列化数据的字节数组。</returns>
        public byte[] SerializeForArrayPool<T>(T value, out long length)
        {
            length = GetLength(value);
            byte[] bytes = ArrayPool<byte>.Shared.Rent((int)length);
            var writer = new ExtenderBinaryWriter(_sequencePool, bytes);
            Serialize(ref writer, value);

            writer.Flush();
            return bytes;
        }

        public byte[] SerializeForArrayPool<T>(T value, out int length)
        {
            var bytes = SerializeForArrayPool(value, out long longLength);
            length = (int)longLength;
            return bytes;
        }

        #endregion

        #region Deserialize

        public T? Deserialize<T>(byte[] bytes)
        {
            if (bytes == null)
                throw new ArgumentNullException(nameof(bytes));


            ExtenderBinaryReader reader = new ExtenderBinaryReader(bytes);
            return Deserialize<T>(ref reader);
        }

        /// <summary>
        /// 反序列化数据。
        /// </summary>
        /// <typeparam name="T">反序列化后的数据类型</typeparam>
        /// <param name="reader">用于读取数据的<see cref="ExtenderBinaryReader"/>对象</param>
        /// <returns>反序列化后的对象，如果剩余数据为0且没有默认格式化器，则返回默认值</returns>
        private T? Deserialize<T>(ref ExtenderBinaryReader reader)
        {
            if (reader.Remaining == 0)
            {
                var formatter = _resolver.GetFormatter<T>();
                if (formatter == null)
                    return default;

                return formatter.Default;
            }

            return _resolver.GetFormatterWithVerify<T>().Deserialize(ref reader);
        }

        public T? Deserialize<T>(Stream stream)
        {
            if (TryDeserializeFromMemoryStream<T>(stream, out var result))
            {
                return result;
            }

            var rent = _sequencePool.Rent();
            var sequence = rent.Value;
            int bytesRead = 0;
            do
            {
                Span<byte> span = sequence.GetSpan(stream.CanSeek ? (int)System.Math.Min(SuggestedContiguousMemorySize, stream.Length - stream.Position) : 0);
                bytesRead = stream.Read(span);
                sequence.Advance(bytesRead);
            }
            while (bytesRead > 0);

            result = DeserializeFromSequenceAndRewindStreamIfPossible<T>(stream, sequence);

            rent.Dispose();
            return result;
        }

        public async Task<T?> DeserializeAsync<T>(Stream stream)
        {
            if (TryDeserializeFromMemoryStream<T>(stream, out var result))
            {
                return result;
            }

            var rent = _sequencePool.Rent();
            var sequence = rent.Value;
            int bytesRead = 0;
            while (bytesRead > 0)
            {
                Memory<byte> memory = sequence.GetMemory(stream.CanSeek ? (int)System.Math.Min(SuggestedContiguousMemorySize, stream.Length - stream.Position) : 0);
                bytesRead = await stream.ReadAsync(memory);
                sequence.Advance(bytesRead);
            }

            result = DeserializeFromSequenceAndRewindStreamIfPossible<T>(stream, sequence);

            rent.Dispose();
            return result;
        }

        private T? DeserializeFromSequenceAndRewindStreamIfPossible<T>(Stream stream, ReadOnlySequence<byte> sequence)
        {
            if (stream is null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            ExtenderBinaryReader reader = new ExtenderBinaryReader(sequence);

            T? result = Deserialize<T>(ref reader);

            //读取完成后返回到未读取的第一个字节
            if (stream.CanSeek && !reader.End)
            {
                long bytesNotRead = reader.Sequence.Slice(reader.Position).Length;
                stream.Seek(-bytesNotRead, SeekOrigin.Current);
            }

            return result;
        }

        private bool TryDeserializeFromMemoryStream<T>(Stream stream, out T? result)
        {
            if (stream is MemoryStream ms && ms.TryGetBuffer(out ArraySegment<byte> streamBuffer))
            {
                var buffer = streamBuffer.AsMemory(checked((int)ms.Position));
                var reader = new ExtenderBinaryReader(buffer);

                result = Deserialize<T>(ref reader);

                var bytesRead = buffer.Slice(0, (int)reader.Consumed).Length;
                ms.Seek(bytesRead, SeekOrigin.Current);
                return true;
            }

            result = default;
            return false;
        }

        #endregion

        #region Delete

        public override void Delete(ExpectLocalFileInfo info)
        {
            var binaryFileInfo = info.CreatLocalFileInfo(FileExtensions.BinaryFileExtensions);
            Delete(binaryFileInfo);
        }

        #endregion

        #region Count

        public long GetLength<T>(T value)
        {
            return _resolver.GetFormatterWithVerify<T>().GetLength(value);
        }

        public long GetDefaulLength<T>()
        {
            return _resolver.GetFormatterWithVerify<T>().Length;
        }

        #endregion
    }
}
