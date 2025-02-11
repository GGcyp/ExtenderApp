using ExtenderApp.Abstract;
using ExtenderApp.Common.IO.StreamOperates;
using ExtenderApp.Data;

namespace ExtenderApp.Common.IO.Splitter
{
    /// <summary>
    /// 分割器解析器类
    /// </summary>
    internal class SplitterParser : ISplitterParser
    {
        /// <summary>
        /// 二进制解析器接口
        /// </summary>
        private readonly IBinaryParser _binaryParser;

        private readonly StreamOperatePool _pool;

        /// <summary>
        /// 信息扩展
        /// </summary>
        private readonly string infoExtensions;

        public SplitterParser(IBinaryParser parser, StreamOperatePool pool)
        {
            _binaryParser = parser;
            infoExtensions = FileExtensions.BinaryFileExtensions;
            _pool = pool;
        }

        #region 无法这么使用

        public T? Deserialize<T>(ExpectLocalFileInfo info, object? options = null)
        {
            throw new NotImplementedException();
        }

        public T? Deserialize<T>(FileOperateInfo operate, object? options = null)
        {
            throw new NotImplementedException();
        }

        public ValueTask<T?> DeserializeAsync<T>(ExpectLocalFileInfo info, object? options = null)
        {
            throw new NotImplementedException();
        }

        public ValueTask<T?> DeserializeAsync<T>(FileOperateInfo operate, object? options = null)
        {
            throw new NotImplementedException();
        }

        public bool Serialize<T>(ExpectLocalFileInfo info, T value, object? options = null)
        {
            throw new NotImplementedException();
        }

        public bool Serialize<T>(FileOperateInfo operate, T value, object? options = null)
        {
            throw new NotImplementedException();
        }

        public ValueTask<bool> SerializeAsync<T>(ExpectLocalFileInfo info, T value, object? options = null)
        {
            throw new NotImplementedException();
        }

        public ValueTask<bool> SerializeAsync<T>(FileOperateInfo operate, T value, object? options = null)
        {
            throw new NotImplementedException();
        }

        #endregion

        public void Write(ExpectLocalFileInfo info, byte[] bytes, uint chunkIndex)
        {
            var operate = GetFileSplitterOperate(info);
            //var operation = _factory.GetWriteOperation(operate);
            //operation.Set(bytes, chunkIndex);
            //operate.Set(operation);
        }

        public void Write<T>(ExpectLocalFileInfo info, T value, uint chunkIndex)
        {
            var operate = GetFileSplitterOperate(info);

            var bytes = _binaryParser.Serialize(value);

            //if (bytes.Length > operate.SplitterInfo.MaxChunkSize)
            //{
            //    throw new ArgumentOutOfRangeException(nameof(value), "数据长度超出最大块大小");
            //}

            //var operation = _factory.GetWriteOperation(operate);
            //operation.Set(bytes, chunkIndex);
            //operate.Set(operation);
        }

        public void Creat(ExpectLocalFileInfo fileInfo, SplitterInfo info)
        {
            var infoFile = fileInfo.CreatLocalFileInfo(infoExtensions);
            if (!infoFile.Exists)
            {
                _binaryParser.Serialize(fileInfo, info);
                infoFile.UpdateFileInfo();
            }
        }

        private SplitterStreamOperate GetFileSplitterOperate(ExpectLocalFileInfo fileInfo)
        {
            var splitterFile = fileInfo.CreateWriteOperate(FileExtensions.SplitterFileExtensions);
            var operate = _pool.GetOperate<SplitterStreamOperate>(splitterFile);
            operate.OpenFile(_binaryParser, fileInfo);
            return operate;
        }
    }
}
