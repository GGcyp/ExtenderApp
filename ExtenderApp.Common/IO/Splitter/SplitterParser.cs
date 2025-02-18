using System.Buffers;
using ExtenderApp.Abstract;
using ExtenderApp.Common.Error;
using ExtenderApp.Data;
using ExtenderApp.Common.ObjectPools;
using ExtenderApp.Common.ConcurrentOperates;
using ExtenderApp.Common.DataBuffers;
using ExtenderApp.Common.IO.FileParsers;
using System.IO.MemoryMappedFiles;

namespace ExtenderApp.Common.IO.Splitter
{
    /// <summary>
    /// 分割器解析器类
    /// </summary>
    internal class SplitterParser : FileParser<SplitterStreamOperatePolicy, SplitterStreamOperateData>, ISplitterParser
    {
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
            _readOperationPool = ObjectPool.Create(new ConcurrentOperationPoolPolicy<SplitterReadOperation>(a => new SplitterReadOperation(a)));
            _writeOperationPool = ObjectPool.Create(new ConcurrentOperationPoolPolicy<SplitterWriteOperation>(a => new SplitterWriteOperation(a)));
            Policy = new SplitterStreamOperatePolicy(parser);
        }

        #region Read

        public override T? Read<T>(ExpectLocalFileInfo info, IConcurrentOperate fileOperate = null, object? options = null) where T : default
        {
            if (info.IsEmpty)
            {
                ErrorUtil.ArgumentNull(nameof(info));
            }

            return Read<T>(info.CreateWriteOperate(FileExtensions.SplitterFileExtensions), fileOperate, options);
        }

        public override T? Read<T>(FileOperateInfo info, IConcurrentOperate fileOperate = null, object? options = null) where T : default
        {
            info.ThrowFileNotFound();

            var splitterOperate = GetFileSplitterOperate(info, fileOperate);
            var operation = _readOperationPool.Get();

            if (options is SplitterInfo splitterInfo)
            {
                splitterOperate.Data.SplitterInfo = splitterInfo;
            }
            else
            {
                splitterInfo = splitterOperate.Data.SplitterInfo;
            }

            //operation.Set(splitterInfo.GetLastChunkIndex(), splitterInfo);
            operation.Set(1, splitterInfo);
            splitterOperate.ExecuteOperation(operation);
            T result = _binaryParser.Deserialize<T>(operation.ReadBytes);
            ArrayPool<byte>.Shared.Return(operation.ReadBytes);
            operation.Release();
            return result;
        }

        public override void ReadAsync<T>(ExpectLocalFileInfo info, Action<T>? callback, IConcurrentOperate fileOperate = null, object? options = null)
        {
            if (info.IsEmpty)
            {
                ErrorUtil.ArgumentNull(nameof(info));
            }

            ReadAsync(info.CreateWriteOperate(FileExtensions.SplitterFileExtensions), callback, fileOperate, options); ;
        }

        public override void ReadAsync<T>(FileOperateInfo info, Action<T>? callback, IConcurrentOperate fileOperate = null, object? options = null)
        {
            info.ThrowFileNotFound();

            var splitterOperate = GetFileSplitterOperate(info, fileOperate);
            var operation = _readOperationPool.Get();

            var buffer = DataBuffer<IBinaryParser, Delegate>.GetDataBuffer();
            buffer.Item1 = _binaryParser;
            buffer.Item2 = callback;
            var action = buffer.Process<byte[]>((d, b) =>
            {
                var binary = d.Item1;
                var callback = d.Item2 as Action<T>;

                var temp = binary.Deserialize<T>(b);
                callback?.Invoke(temp);
                d.Release();
            });

            if (options is SplitterInfo splitterInfo)
            {
                splitterOperate.Data.SplitterInfo = splitterInfo;
            }
            else
            {
                splitterInfo = splitterOperate.Data.SplitterInfo;
            }

            operation.Set(splitterInfo.GetLastChunkIndex(), action, splitterInfo);
            splitterOperate.QueueOperation(operation);
        }

        #endregion

        #region Write

        public override void Write<T>(ExpectLocalFileInfo info, T value, IConcurrentOperate fileOperate = null, object? options = null)
        {
            if (info.IsEmpty)
            {
                ErrorUtil.ArgumentNull(nameof(info));
            }

            Write(info.CreateWriteOperate(FileExtensions.SplitterFileExtensions), value, fileOperate, options);
        }

        public override void Write<T>(FileOperateInfo info, T value, IConcurrentOperate fileOperate = null, object? options = null)
        {
            info.ThrowFileNotFound();

            var splitterOperate = GetFileSplitterOperate(info, fileOperate);
            var operation = _writeOperationPool.Get();

            if (options is SplitterInfo splitterInfo)
            {
                splitterOperate.Data.SplitterInfo = splitterInfo;
            }
            else
            {
                splitterInfo = splitterOperate.Data.SplitterInfo;
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

        public override void WriteAsync<T>(ExpectLocalFileInfo info, T value, Action? callback = null, IConcurrentOperate fileOperate = null, object? options = null)
        {
            WriteAsync(info.CreateWriteOperate(FileExtensions.SplitterFileExtensions), value, callback, fileOperate, options);
        }

        public override void WriteAsync<T>(FileOperateInfo info, T value, Action? callback = null, IConcurrentOperate fileOperate = null, object? options = null)
        {
            info.ThrowFileNotFound();

            var splitterOperate = GetFileSplitterOperate(info, fileOperate);
            var operation = _writeOperationPool.Get();

            if (options is SplitterInfo splitterInfo)
            {
                splitterOperate.Data.SplitterInfo = splitterInfo;
            }
            else
            {
                splitterInfo = splitterOperate.Data.SplitterInfo;
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

        public void WriteAsync<T>(ExpectLocalFileInfo info, T value, Action<byte[]>? callback = null, object? options = null, IConcurrentOperate fileOperate = null)
        {
            WriteAsync(info.CreateWriteOperate(FileExtensions.SplitterFileExtensions), value, callback, options, fileOperate);
        }

        public void WriteAsync<T>(FileOperateInfo info, T value, Action<byte[]>? callback = null, object? options = null, IConcurrentOperate fileOperate = null)
        {
            info.ThrowFileNotFound();

            var splitterOperate = GetFileSplitterOperate(info, fileOperate);
            var operation = _writeOperationPool.Get();

            if (options is SplitterInfo splitterInfo)
            {
                splitterOperate.Data.SplitterInfo = splitterInfo;
            }
            else
            {
                splitterInfo = splitterOperate.Data.SplitterInfo;
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

        public void Creat(ExpectLocalFileInfo fileInfo, SplitterInfo info)
        {
            var infoFile = fileInfo.CreateWriteOperate(infoExtensions);
            _binaryParser.Write(infoFile, info);
        }

        /// <summary>
        /// 根据本地文件信息获取文件分割操作实例
        /// </summary>
        /// <param name="fileInfo">本地文件信息</param>
        /// <returns>文件分割操作实例</returns>
        private IConcurrentOperate<MemoryMappedViewAccessor, SplitterStreamOperateData> GetFileSplitterOperate(ExpectLocalFileInfo fileInfo, object? fileOperate)
        {
            return GetFileSplitterOperate(fileInfo.CreateWriteOperate(FileExtensions.SplitterFileExtensions), fileOperate);
        }

        /// <summary>
        /// 根据文件操作信息获取文件分割操作实例
        /// </summary>
        /// <param name="operateInfo">文件操作信息</param>
        /// <returns>文件分割操作实例</returns>
        private IConcurrentOperate<MemoryMappedViewAccessor, SplitterStreamOperateData> GetFileSplitterOperate(FileOperateInfo operateInfo, object? fileOperate)
        {
            var operate = GetOperate(operateInfo, fileOperate);
            operate.Data.OpenFile(operateInfo.LocalFileInfo.CreateExpectLocalFileInfo());
            return operate;
        }
    }
}
