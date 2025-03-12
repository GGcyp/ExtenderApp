using System.Buffers;
using ExtenderApp.Abstract;
using ExtenderApp.Common.Error;
using ExtenderApp.Data;
using ExtenderApp.Common.ObjectPools;
using ExtenderApp.Common.DataBuffers;
using ExtenderApp.Common.IO.FileParsers;
using System.IO.MemoryMappedFiles;
using ExtenderApp.Common.ObjectPools.Policy;

namespace ExtenderApp.Common.IO.Splitter
{
    /// <summary>
    /// 分割器解析器类
    /// </summary>
    internal class SplitterParser : FileParser<SplitterStreamOperatePolicy, SplitterStreamOperateData>, ISplitterParser
    {
        private const int DefaultMaxChunkSize = 1024 * 1024;

        /// <summary>
        /// 二进制解析器接口
        /// </summary>
        private readonly IBinaryParser _binaryParser;

        /// <summary>
        /// 用于管理SplitterWriteOperation对象的对象池。
        /// </summary>
        private readonly ObjectPool<SplitterWriteOperation> _writeOperationPool;

        /// <summary>
        /// 用于管理SplitterReadOperation对象的对象池。
        /// </summary>
        private readonly ObjectPool<SplitterReadOperation> _readOperationPool;

        /// <summary>
        /// 信息扩展
        /// </summary>
        private readonly string infoExtensions;

        protected override SplitterStreamOperatePolicy Policy { get; }

        public SplitterParser(IBinaryParser parser, FileStore store) : base(store)
        {
            _binaryParser = parser;
            infoExtensions = FileExtensions.BinaryFileExtensions;
            _readOperationPool = ObjectPool.Create(new SelfResetPooledObjectPolicy<SplitterReadOperation>());
            _writeOperationPool = ObjectPool.Create(new SelfResetPooledObjectPolicy<SplitterWriteOperation>());
            Policy = new SplitterStreamOperatePolicy(parser);
        }

        #region Read

        public override T? Read<T>(ExpectLocalFileInfo info, IConcurrentOperate fileOperate = null) where T : default
        {
            if (info.IsEmpty)
            {
                ErrorUtil.ArgumentNull(nameof(info));
            }

            return Read<T>(info.CreateWriteOperate(FileExtensions.SplitterFileExtensions), fileOperate);
        }

        public override T? Read<T>(FileOperateInfo info, IConcurrentOperate fileOperate = null) where T : default
        {
            info.ThrowFileNotFound();

            var splitterOperate = GetFileSplitterOperate(info, fileOperate);
            var operation = _readOperationPool.Get();

            SplitterInfo? splitterInfo = splitterOperate.Data.SplitterInfo;
            if (splitterInfo == null)
            {
                splitterInfo = _binaryParser.Read<SplitterInfo>(info.LocalFileInfo.ChangeFileExtension(infoExtensions).CreateWriteOperate());
                if (splitterInfo == null)
                    throw new FileNotFoundException(info.LocalFileInfo.FilePath);

                splitterOperate.Data.SplitterInfo = splitterInfo;
            }

            var array = ArrayPool<byte>.Shared.Rent(splitterInfo.MaxChunkSize);
            operation.Set(splitterInfo.GetLastChunkIndex(), splitterInfo, array);
            splitterOperate.ExecuteOperation(operation);
            T result = _binaryParser.Deserialize<T>(operation.ReadBytes);
            ArrayPool<byte>.Shared.Return(operation.ReadBytes);
            operation.Release();
            return result;
        }

        public override void ReadAsync<T>(ExpectLocalFileInfo info, Action<T>? callback, IConcurrentOperate fileOperate = null)
        {
            if (info.IsEmpty)
            {
                ErrorUtil.ArgumentNull(nameof(info));
            }

            ReadAsync(info.CreateWriteOperate(FileExtensions.SplitterFileExtensions), callback, fileOperate); ;
        }

        public override void ReadAsync<T>(FileOperateInfo info, Action<T>? callback, IConcurrentOperate fileOperate = null)
        {
            info.ThrowFileNotFound();

            var splitterOperate = GetFileSplitterOperate(info, fileOperate);
            var operation = _readOperationPool.Get();

            var buffer = DataBuffer<IBinaryParser, Delegate>.GetDataBuffer();
            buffer.Item1 = _binaryParser;
            buffer.Item2 = callback;
            var action = buffer.SetProcessAction<byte[]>((d, b) =>
            {
                var binary = d.Item1;
                var callback = d.Item2 as Action<T>;

                var temp = binary.Deserialize<T>(b);
                callback?.Invoke(temp);
                d.Release();
            });


            SplitterInfo? splitterInfo = splitterOperate.Data.SplitterInfo;
            if (splitterInfo == null)
            {
                splitterInfo = _binaryParser.Read<SplitterInfo>(info.LocalFileInfo.ChangeFileExtension(infoExtensions).CreateWriteOperate());
                if (splitterInfo == null)
                    throw new FileNotFoundException(info.LocalFileInfo.FilePath);

                splitterOperate.Data.SplitterInfo = splitterInfo;
            }

            operation.Set(splitterInfo.GetLastChunkIndex(), action, splitterInfo);
            splitterOperate.QueueOperation(operation);
        }

        public byte[] Read(ExpectLocalFileInfo info, uint chunkIndex, SplitterInfo splitterInfo, IConcurrentOperate? fileOperate = null, byte[]? bytes = null)
        {
            if (info.IsEmpty)
            {
                ErrorUtil.ArgumentNull(nameof(info));
            }

            return Read(info.CreateWriteOperate(FileExtensions.SplitterFileExtensions), chunkIndex, splitterInfo, fileOperate, bytes);
        }

        public byte[] Read(FileOperateInfo info, uint chunkIndex, SplitterInfo splitterInfo, IConcurrentOperate? fileOperate = null, byte[]? bytes = null)
        {
            if (info.IsEmpty)
            {
                ErrorUtil.ArgumentNull(nameof(info));
            }

            splitterInfo.ArgumentNull(nameof(splitterInfo));

            bytes ??= ArrayPool<byte>.Shared.Rent(splitterInfo.MaxChunkSize);
            var operate = GetOperate(info, fileOperate);
            var operation = _readOperationPool.Get();
            operation.Set(chunkIndex, splitterInfo, bytes);
            operate.ExecuteOperation(operation);
            var result = operation.ReadBytes;
            operation.Release();
            return result;
        }

        #endregion

        #region Write

        public override void Write<T>(ExpectLocalFileInfo info, T value, IConcurrentOperate fileOperate = null)
        {
            if (info.IsEmpty)
            {
                ErrorUtil.ArgumentNull(nameof(info));
            }

            Write(info.CreateWriteOperate(FileExtensions.SplitterFileExtensions), value, fileOperate);
        }

        public override void Write<T>(FileOperateInfo info, T value, IConcurrentOperate fileOperate = null)
        {
            info.ThrowFileNotFound();

            var splitterOperate = GetFileSplitterOperate(info, fileOperate);
            var operation = _writeOperationPool.Get();

            SplitterInfo? splitterInfo = splitterOperate.Data.SplitterInfo;
            if (splitterInfo == null)
            {
                splitterInfo = _binaryParser.Read<SplitterInfo>(info.LocalFileInfo.ChangeFileExtension(infoExtensions).CreateWriteOperate());
                if (splitterInfo == null)
                    throw new FileNotFoundException(info.LocalFileInfo.FilePath);

                splitterOperate.Data.SplitterInfo = splitterInfo;
            }

            var bytes = _binaryParser.Serialize(value);

            if (bytes.Length > splitterInfo.MaxChunkSize)
            {
                ErrorUtil.ArgumentOutOfRange(nameof(bytes), "数据长度超出最大块大小");
            }

            operation.Set(bytes, splitterInfo.GetPosition(splitterInfo.Progress), bytes.Length, splitterInfo);
            splitterOperate.ExecuteOperation(operation);
            operation.Release();
        }

        public override void WriteAsync<T>(ExpectLocalFileInfo info, T value, Action? callback = null, IConcurrentOperate fileOperate = null)
        {
            WriteAsync(info.CreateWriteOperate(FileExtensions.SplitterFileExtensions), value, callback, fileOperate);
        }

        public override void WriteAsync<T>(FileOperateInfo info, T value, Action? callback = null, IConcurrentOperate fileOperate = null)
        {
            info.ThrowFileNotFound();

            var splitterOperate = GetFileSplitterOperate(info, fileOperate);
            var operation = _writeOperationPool.Get();

            SplitterInfo? splitterInfo = splitterOperate.Data.SplitterInfo;
            if (splitterInfo == null)
            {
                splitterInfo = _binaryParser.Read<SplitterInfo>(info.LocalFileInfo.ChangeFileExtension(infoExtensions).CreateWriteOperate());
                if (splitterInfo == null)
                    throw new FileNotFoundException(info.LocalFileInfo.FilePath);

                splitterOperate.Data.SplitterInfo = splitterInfo;
            }

            var bytes = _binaryParser.Serialize(value);

            if (bytes.Length > splitterInfo.MaxChunkSize)
            {
                ErrorUtil.ArgumentOutOfRange(nameof(bytes), "数据长度超出最大块大小");
            }

            operation.Set(bytes, splitterInfo.GetPosition(splitterInfo.Progress), bytes.Length, splitterInfo, callback);
            splitterOperate.QueueOperation(operation);
        }

        public void Write(ExpectLocalFileInfo info, byte[] bytes, uint chunkIndex, SplitterInfo? splitterInfo = null, IConcurrentOperate fileOperate = null)
        {
            var operate = GetFileSplitterOperate(info, fileOperate);
            var operation = _writeOperationPool.Get();

            if (splitterInfo == null)
            {
                splitterInfo = operate.Data.SplitterInfo;
            }
            else
            {
                operate.Data.SplitterInfo = splitterInfo;
            }

            if (chunkIndex < 0)
            {
                chunkIndex = splitterInfo.Progress;
            }

            if (chunkIndex >= splitterInfo.ChunkCount)
            {
                ErrorUtil.ArgumentOutOfRange(nameof(chunkIndex), string.Format("块索引超出范围:{0}", chunkIndex.ToString()));
            }

            if (bytes.Length > splitterInfo.MaxChunkSize)
            {
                ErrorUtil.ArgumentOutOfRange(nameof(bytes), "数据长度超出最大块大小");
            }

            operation.Set(bytes, splitterInfo.GetPosition(chunkIndex), bytes.Length, splitterInfo);
            operate.ExecuteOperation(operation);
        }

        public void Write<T>(ExpectLocalFileInfo info, T value, uint chunkIndex, SplitterInfo? splitterInfo = null, IConcurrentOperate fileOperate = null)
        {
            var bytes = _binaryParser.Serialize(value);
            Write(info, bytes, chunkIndex, splitterInfo);
            ArrayPool<byte>.Shared.Return(bytes);
        }

        public void WriteAsync<T>(ExpectLocalFileInfo info, T value, Action<byte[]>? callback = null, IConcurrentOperate fileOperate = null)
        {
            WriteAsync(info.CreateWriteOperate(FileExtensions.SplitterFileExtensions), value, callback, fileOperate);
        }

        public void WriteAsync<T>(FileOperateInfo info, T value, Action<byte[]>? callback = null, IConcurrentOperate fileOperate = null)
        {
            info.ThrowFileNotFound();

            var splitterOperate = GetFileSplitterOperate(info, fileOperate);
            var operation = _writeOperationPool.Get();

            SplitterInfo? splitterInfo = splitterOperate.Data.SplitterInfo;
            if (splitterInfo == null)
            {
                splitterInfo = _binaryParser.Read<SplitterInfo>(info.LocalFileInfo.ChangeFileExtension(infoExtensions).CreateWriteOperate());
                if (splitterInfo == null)
                    throw new FileNotFoundException(info.LocalFileInfo.FilePath);

                splitterOperate.Data.SplitterInfo = splitterInfo;
            }

            var bytes = _binaryParser.Serialize(value);

            if (bytes.Length > splitterInfo.MaxChunkSize)
            {
                ErrorUtil.ArgumentOutOfRange(nameof(bytes), "数据长度超出最大块大小");
            }

            operation.Set(bytes, splitterInfo.GetPosition(splitterInfo.Progress), bytes.Length, splitterInfo, callback);
            splitterOperate.QueueOperation(operation);
        }

        public void WriteAsync(ExpectLocalFileInfo info, byte[] bytes, uint chunkIndex, SplitterInfo? splitterInfo = null, IConcurrentOperate fileOperate = null)
        {
            var operate = GetFileSplitterOperate(info, fileOperate);
            var operation = _writeOperationPool.Get();

            if (splitterInfo == null)
            {
                splitterInfo = operate.Data.SplitterInfo;
            }
            else
            {
                operate.Data.SplitterInfo = splitterInfo;
            }

            if (chunkIndex < 0)
            {
                chunkIndex = splitterInfo.Progress;
            }

            if (chunkIndex >= splitterInfo.ChunkCount)
            {
                ErrorUtil.ArgumentOutOfRange(nameof(chunkIndex), string.Format("块索引超出范围:{0}", chunkIndex.ToString()));
            }

            if (bytes.Length > splitterInfo.MaxChunkSize)
            {
                ErrorUtil.ArgumentOutOfRange(nameof(bytes), "数据长度超出最大块大小");
            }

            operation.Set(bytes, splitterInfo.GetPosition(chunkIndex), bytes.Length, splitterInfo);
            operate.QueueOperation(operation);
        }

        public void WriteAsync<T>(ExpectLocalFileInfo info, T value, uint chunkIndex, SplitterInfo? splitterInfo = null, IConcurrentOperate fileOperate = null)
        {
            var bytes = _binaryParser.Serialize(value);
            WriteAsync(info, bytes, chunkIndex, splitterInfo, fileOperate);
        }

        #endregion

        #region Create

        public void Create(ExpectLocalFileInfo fileInfo, SplitterInfo info)
        {
            info.ArgumentNull(nameof(info));

            var infoFile = fileInfo.CreateWriteOperate(infoExtensions);
            _binaryParser.Write(infoFile, info);
        }

        public SplitterInfo Create(LocalFileInfo fileInfo, bool createLoaderChunks = true)
        {
            var info = Create(fileInfo, DefaultMaxChunkSize, createLoaderChunks);
            return info;
        }

        public SplitterInfo Create(LocalFileInfo info, int maxLength, bool createLoaderChunks = true)
        {
            //ErrorUtil.FileNotFound(info);
            info.FileNotFound();

            //LocalFileInfo splitterInfoFileInfo = info.ChangeFileExtension(infoExtensions);
            //SplitterInfo splitterInfo;
            //if (splitterInfoFileInfo.Exists)
            //{
            //    splitterInfo = _binaryParser.Read<SplitterInfo>(info.CreateExpectLocalFileInfo());
            //}
            //else
            //{
            //    long length = info.FileInfo.Length;
            //    uint chunkCount = (uint)(length / maxLength);
            //    splitterInfo = new SplitterInfo(length, chunkCount, 0, maxLength, info.Extension);
            //    _binaryParser.Write(info.CreateExpectLocalFileInfo(), splitterInfo);
            //}

            long length = info.FileInfo.Length;
            uint chunkCount = (uint)(length / maxLength);
            SplitterInfo splitterInfo = new SplitterInfo(length, chunkCount, 0, maxLength, info.Extension, createLoaderChunks ? new byte[chunkCount] : null);
            return splitterInfo;
        }

        #endregion

        #region  Get

        public SplitterInfo? GetSplitterInfo(ExpectLocalFileInfo fileInfo)
        {
            var infoFile = fileInfo.CreateWriteOperate(infoExtensions);
            infoFile.ThrowFileNotFound();
            return _binaryParser.Read<SplitterInfo>(infoFile);
        }

        public SplitterInfo? GetSplitterInfo(LocalFileInfo fileInfo)
        {
            if (fileInfo.Extension != infoExtensions)
            {
                fileInfo = fileInfo.ChangeFileExtension(infoExtensions);
            }

            fileInfo.ThrowFileNotFound();

            var infoFile = fileInfo.CreateWriteOperate();
            return _binaryParser.Read<SplitterInfo>(infoFile);
        }

        public SplitterDto GetSplitterDto(LocalFileInfo fileInfo, uint chunkIndex, SplitterInfo? info = null, IConcurrentOperate? fileOperate = null)
        {
            if (info == null)
            {
                info = GetSplitterInfo(fileInfo);

                info.ArgumentNull(nameof(info));
            }

            if (chunkIndex >= info.ChunkCount)
            {
                ErrorUtil.ArgumentOutOfRange(nameof(chunkIndex), "块索引超出范围");
            }

            int length = info.MaxChunkSize;
            var valueBytes = Read(fileInfo.CreateWriteOperate(), chunkIndex, info, fileOperate);

            return new SplitterDto(chunkIndex, valueBytes, length);
        }

        #endregion

        #region Delet

        public override void Delete(ExpectLocalFileInfo info)
        {
            var splitterFileInfo = info.CreatLocalFileInfo(FileExtensions.SplitterFileExtensions);
            _store.Delete(splitterFileInfo);
            splitterFileInfo.Delete();

            var splitterInfoFileInfo = info.CreatLocalFileInfo(infoExtensions);
            _store.Delete(splitterInfoFileInfo);
            splitterInfoFileInfo.Delete();
        }

        #endregion

        /// <summary>
        /// 根据本地文件信息获取文件分割操作实例
        /// </summary>
        /// <param name="fileInfo">本地文件信息</param>
        /// <returns>文件分割操作实例</returns>
        private IConcurrentOperate<MemoryMappedViewAccessor, SplitterStreamOperateData> GetFileSplitterOperate(ExpectLocalFileInfo fileInfo, IConcurrentOperate? fileOperate)
        {
            return GetFileSplitterOperate(fileInfo.CreateWriteOperate(FileExtensions.SplitterFileExtensions), fileOperate);
        }

        /// <summary>
        /// 根据文件操作信息获取文件分割操作实例
        /// </summary>
        /// <param name="operateInfo">文件操作信息</param>
        /// <returns>文件分割操作实例</returns>
        private IConcurrentOperate<MemoryMappedViewAccessor, SplitterStreamOperateData> GetFileSplitterOperate(FileOperateInfo operateInfo, IConcurrentOperate? fileOperate)
        {
            var operate = GetOperate(operateInfo, fileOperate);
            operate.Data.OpenFile(operateInfo.LocalFileInfo.CreateExpectLocalFileInfo());
            return operate;
        }
    }
}
