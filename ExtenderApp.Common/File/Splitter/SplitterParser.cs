using System.Collections.Concurrent;
using ExtenderApp.Abstract;
using ExtenderApp.Common.File.Splitter;
using ExtenderApp.Common.ObjectPools;
using ExtenderApp.Data;

namespace ExtenderApp.Common.Files.Splitter
{
    /// <summary>
    /// 分割器解析器类
    /// </summary>
    internal class SplitterParser : ISplitterParser
    {
        /// <summary>
        /// 文件分割操作对象池策略类
        /// </summary>
        internal class FileSplitterOperatePooledObjectPolicy : PooledObjectPolicy<SplitterOperate>
        {
            /// <summary>
            /// 二进制解析器接口
            /// </summary>
            private readonly IBinaryParser _parser;

            /// <summary>
            /// 释放操作
            /// </summary>
            private readonly Action<SplitterOperate> _release;

            /// <summary>
            /// 初始化文件分割操作对象池策略类
            /// </summary>
            /// <param name="parser">二进制解析器接口</param>
            /// <param name="action">释放操作</param>
            public FileSplitterOperatePooledObjectPolicy(IBinaryParser parser, Action<SplitterOperate> action)
            {
                _parser = parser;
                _release = action;
            }

            /// <summary>
            /// 创建分割操作对象
            /// </summary>
            /// <returns>分割操作对象</returns>
            public override SplitterOperate Create()
            {
                return new SplitterOperate(_parser, ReleaseAction);
            }

            /// <summary>
            /// 释放分割操作对象
            /// </summary>
            /// <param name="obj">分割操作对象</param>
            /// <returns>是否成功释放</returns>
            public override bool Release(SplitterOperate obj)
            {
                _release.Invoke(obj);
                return obj.TryReset();
            }
        }

        /// <summary>
        /// 二进制解析器接口
        /// </summary>
        private readonly IBinaryParser _binaryParser;

        /// <summary>
        /// 分割操作对象池
        /// </summary>
        private readonly ObjectPool<SplitterOperate> _pool;

        /// <summary>
        /// 本地文件信息字典
        /// </summary>
        private readonly ConcurrentDictionary<ExpectLocalFileInfo, SplitterOperate> _dict;

        /// <summary>
        /// 分割操作工厂
        /// </summary>
        private readonly SplitterOperationFactory _factory;

        /// <summary>
        /// 信息扩展
        /// </summary>
        private readonly string infoExtensions;

        public SplitterParser(IBinaryParser parser)
        {
            _binaryParser = parser;
            _pool = ObjectPool.Create(new FileSplitterOperatePooledObjectPolicy(parser, Release));
            _dict = new();
            _factory = new();
            infoExtensions = FileExtensions.BinaryFileExtensions;
        }

        private void Release(SplitterOperate operate)
        {
            if (operate.SplitterInfo.IsComplete)
            {
                _dict.Remove(operate.ExpectFileInfo, out operate);
            }
        }

        #region 无法这么使用

        public T? Deserialize<T>(ExpectLocalFileInfo info, object? options = null)
        {
            throw new NotImplementedException();
        }

        public T? Deserialize<T>(FileOperate operate, object? options = null)
        {
            throw new NotImplementedException();
        }

        public ValueTask<T?> DeserializeAsync<T>(ExpectLocalFileInfo info, object? options = null)
        {
            throw new NotImplementedException();
        }

        public ValueTask<T?> DeserializeAsync<T>(FileOperate operate, object? options = null)
        {
            throw new NotImplementedException();
        }

        public bool Serialize<T>(ExpectLocalFileInfo info, T value, object? options = null)
        {
            throw new NotImplementedException();
        }

        public bool Serialize<T>(FileOperate operate, T value, object? options = null)
        {
            throw new NotImplementedException();
        }

        public ValueTask<bool> SerializeAsync<T>(ExpectLocalFileInfo info, T value, object? options = null)
        {
            throw new NotImplementedException();
        }

        public ValueTask<bool> SerializeAsync<T>(FileOperate operate, T value, object? options = null)
        {
            throw new NotImplementedException();
        }

        #endregion

        public void Write(ExpectLocalFileInfo info, byte[] bytes, uint chunkIndex)
        {
            var operate = GetFileSplitterOperate(info);
            var operation = _factory.GetWriteOperation(operate);
            operation.Set(bytes, chunkIndex);
            operate.Set(operation);
        }

        public void Write<T>(ExpectLocalFileInfo info, T value, uint chunkIndex)
        {
            var operate = GetFileSplitterOperate(info);

            var bytes = _binaryParser.Serialize(value);

            if (bytes.Length > operate.SplitterInfo.MaxChunkSize)
            {
                throw new ArgumentOutOfRangeException(nameof(value), "数据长度超出最大块大小");
            }

            var operation = _factory.GetWriteOperation(operate);
            operation.Set(bytes, chunkIndex);
            operate.Set(operation);
        }

        public void Creat(ExpectLocalFileInfo fileInfo, FileSplitterInfo info)
        {
            var infoFile = fileInfo.CreateWriteOperate(infoExtensions);
            if (!infoFile.LocalFileInfo.Exists)
            {
                _binaryParser.Serialize(fileInfo, info);
                infoFile.LocalFileInfo.UpdateFileInfo();
            }


            var splitterFile = fileInfo.CreateWriteOperate(FileExtensions.SplitterFileExtensions);
            var operate = _pool.Get();
            _dict.TryAdd(fileInfo, operate);
            operate.OpenFile(splitterFile, infoFile, fileInfo);
        }

        private SplitterOperate GetFileSplitterOperate(ExpectLocalFileInfo fileInfo)
        {
            if (_dict.TryGetValue(fileInfo, out var operate))
            {
                return operate;
            }

            var infoFile = fileInfo.CreateFileOperate(infoExtensions, FileMode.OpenOrCreate, FileAccess.ReadWrite);
            if (!infoFile.LocalFileInfo.Exists)
            {
                throw new FileNotFoundException(string.Format("未发现文件，路径：{0}，名字：{1}", fileInfo.FileName, fileInfo.FileName));
            }

            var file = fileInfo.CreateFileOperate(FileExtensions.SplitterFileExtensions, FileMode.OpenOrCreate, FileAccess.ReadWrite);

            operate = _pool.Get();
            _dict.TryAdd(fileInfo, operate);
            operate.OpenFile(file, infoFile, fileInfo);

            return operate;
        }
    }
}
