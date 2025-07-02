using System.Buffers;
using System.Security.Cryptography;
using ExtenderApp.Abstract;
using ExtenderApp.Common.Error;
using ExtenderApp.Data;

namespace ExtenderApp.Common.IO.Splitter
{
    /// <summary>
    /// 分割器解析器类
    /// </summary>
    internal class SplitterParser : ISplitterParser
    {
        /// <summary>
        /// 默认的最大块大小（以字节为单位）。
        /// </summary>
        private const int DefaultMaxChunkSize = 1024 * 1024;

        private readonly IFileOperateProvider _provider;
        private readonly IHashProvider _hashProvider;

        /// <summary>
        /// 信息扩展
        /// </summary>
        private readonly string infoExtensions;

        public SplitterParser(IFileOperateProvider provider,IHashProvider hashProvider)
        {
            infoExtensions = FileExtensions.SplitterFileExtensions;
            _hashProvider = hashProvider;
            _provider = provider;
        }

        #region Create

        public void CreateInfoFile(ExpectLocalFileInfo fileInfo, SplitterInfo sInfo)
        {
            if (fileInfo.IsEmpty)
            {
                ErrorUtil.ArgumentNull(nameof(fileInfo), "文件信息不能为空");
            }
            var infoFile = fileInfo.CreateReadWriteOperate(infoExtensions);
            //_provider.GetOperate(fileInfo);
        }

        public void CreateInfoFile(LocalFileInfo fileInfo, SplitterInfo sInfo)
        {
            if (fileInfo.IsEmpty)
            {
                ErrorUtil.ArgumentNull(nameof(fileInfo), "文件信息不能为空");
            }
            var infoFile = fileInfo.ChangeFileExtension(infoExtensions);
            //_binaryParser.Write(infoFile, sInfo);
        }

        public void CreateInfoFile(IFileOperate operate, SplitterInfo sInfo)
        {
            if (operate is not FileConcurrentOperate fileOperate)
            {
                ErrorUtil.ArgumentNull(nameof(operate));
                return;
            }

            var infoFile = fileOperate.Data.OperateInfo.LocalFileInfo.ChangeFileExtension(infoExtensions);
            //_binaryParser.Write(infoFile, sInfo);
        }

        public SplitterInfo CreateInfoForFile(LocalFileInfo targtFileInfo, bool createLoaderChunks = false)
        {
            return CreateInfoForFile(targtFileInfo, DefaultMaxChunkSize, createLoaderChunks);
        }

        public SplitterInfo CreateInfoForFile(LocalFileInfo targtFileInfo, int chunkMaxLength, bool createLoadedChunks = false)
        {
            targtFileInfo.FileNotFound();

            long length = targtFileInfo.FileInfo.Length;
            uint chunkCount = (uint)(length / chunkMaxLength);
            if (length % chunkMaxLength != 0)
                chunkCount++;

            if (chunkCount == 1 && length < chunkMaxLength)
            {
                chunkMaxLength = (int)length;
            }

            HashValue md5 = _hashProvider.ComputeHash<MD5>(targtFileInfo);
            return new SplitterInfo((int)length, chunkCount, 0, chunkMaxLength, targtFileInfo.Extension, md5, createLoadedChunks ? new PieceData(new byte[chunkCount]) : PieceData.Empty); ;
        }

        #endregion

        #region  Read

        public SplitterInfo? ReadInfo(ExpectLocalFileInfo fileInfo)
        {
            if (fileInfo.IsEmpty)
            {
                ErrorUtil.ArgumentNull(nameof(fileInfo), "文件信息不能为空");
            }
            return ReadInfo(fileInfo.CreatLocalFileInfo(infoExtensions));
        }

        public SplitterInfo? ReadInfo(LocalFileInfo fileInfo)
        {
            if (fileInfo.Extension != infoExtensions)
            {
                fileInfo = fileInfo.ChangeFileExtension(infoExtensions);
            }

            fileInfo.ThrowFileNotFound();

            //return _binaryParser.Read<SplitterInfo>(fileInfo);
            return null;
        }

        public SplitterDto ReadDot(IFileOperate fileOperate, uint chunkIndex, SplitterInfo sinfo)
        {
            fileOperate.ArgumentNull(nameof(fileOperate));
            if (chunkIndex > sinfo.ChunkCount)
            {
                ErrorUtil.ArgumentOutOfRange(nameof(chunkIndex), "块索引超出范围");
            }

            int length = sinfo.MaxChunkSize;
            var bytes = ArrayPool<byte>.Shared.Rent(length);
            var position = sinfo.MaxChunkSize * chunkIndex;
            //_binaryParser.Read(fileOperate, position, length, bytes);

            return new SplitterDto((uint)sinfo.MaxChunkSize, bytes, length);
        }

        public SplitterDto ReadDto(LocalFileInfo fileInfo, uint chunkIndex, SplitterInfo sinfo)
        {
            sinfo.ArgumentNull(nameof(sinfo));
            if (chunkIndex > sinfo.ChunkCount)
            {
                ErrorUtil.ArgumentOutOfRange(nameof(chunkIndex), "块索引超出范围");
            }

            int length = sinfo.MaxChunkSize;
            var bytes = ArrayPool<byte>.Shared.Rent(length);
            var position = sinfo.MaxChunkSize * chunkIndex;
            //_binaryParser.Read(fileInfo, position, length, bytes);

            return new SplitterDto((uint)sinfo.MaxChunkSize, bytes, length);
        }

        public byte[]? Read(LocalFileInfo fileInfo, uint chunkIndex, SplitterInfo sinfo)
        {
            sinfo.ArgumentNull(nameof(sinfo));
            if (chunkIndex > sinfo.ChunkCount)
            {
                ErrorUtil.ArgumentOutOfRange(nameof(chunkIndex), "块索引超出范围");
            }

            long position = sinfo.MaxChunkSize * (sinfo.ChunkCount - 1);
            long length = sinfo.ChunkCount > chunkIndex ? sinfo.MaxChunkSize : (int)(sinfo.Length - position);
            //return _binaryParser.Read(fileInfo, position, length);
            return null;
        }

        public byte[]? Read(IFileOperate operate, uint chunkIndex, SplitterInfo sinfo)
        {
            sinfo.ArgumentNull(nameof(sinfo));
            if (chunkIndex > sinfo.ChunkCount)
            {
                ErrorUtil.ArgumentOutOfRange(nameof(chunkIndex), "块索引超出范围");
            }

            long position = sinfo.MaxChunkSize * (sinfo.ChunkCount - 1);
            long length = sinfo.ChunkCount > chunkIndex ? sinfo.MaxChunkSize : (int)(sinfo.Length - position);
            //return _binaryParser.Read(operate, position, length);
            return null;
        }

        public void Read(LocalFileInfo fileInfo, uint chunkIndex, SplitterInfo sinfo, byte[] bytes)
        {
            sinfo.ArgumentNull(nameof(sinfo));
            if (chunkIndex > sinfo.ChunkCount)
            {
                ErrorUtil.ArgumentOutOfRange(nameof(chunkIndex), "块索引超出范围");
            }

            long position = sinfo.MaxChunkSize * (sinfo.ChunkCount - 1);
            long length = sinfo.ChunkCount > chunkIndex ? sinfo.MaxChunkSize : (int)(sinfo.Length - position);
            //_binaryParser.Read(fileInfo, position, length, bytes);
        }

        public void Read(IFileOperate operate, uint chunkIndex, SplitterInfo sinfo, byte[] bytes)
        {
            sinfo.ArgumentNull(nameof(sinfo));
            if (chunkIndex > sinfo.ChunkCount)
            {
                ErrorUtil.ArgumentOutOfRange(nameof(chunkIndex), "块索引超出范围");
            }

            long position = sinfo.MaxChunkSize * (sinfo.ChunkCount - 1);
            long length = sinfo.ChunkCount > chunkIndex ? sinfo.MaxChunkSize : (int)(sinfo.Length - position);
            //_binaryParser.Read(operate, position, length, bytes);
        }

        public T Read<T>(LocalFileInfo fileInfo, uint chunkIndex, SplitterInfo sinfo)
        {
            sinfo.ArgumentNull(nameof(sinfo));
            if (chunkIndex > sinfo.ChunkCount)
            {
                ErrorUtil.ArgumentOutOfRange(nameof(chunkIndex), "块索引超出范围");
            }

            long position = sinfo.MaxChunkSize * (sinfo.ChunkCount - 1);
            long length = sinfo.ChunkCount > chunkIndex ? sinfo.MaxChunkSize : (int)(sinfo.Length - position);
            //return _binaryParser.Read<T>(fileInfo, position, length);
            return default;
        }

        public T Read<T>(IFileOperate operate, uint chunkIndex, SplitterInfo sinfo)
        {
            sinfo.ArgumentNull(nameof(sinfo));
            if (chunkIndex > sinfo.ChunkCount)
            {
                ErrorUtil.ArgumentOutOfRange(nameof(chunkIndex), "块索引超出范围");
            }

            long position = sinfo.MaxChunkSize * (sinfo.ChunkCount - 1);
            long length = sinfo.ChunkCount > chunkIndex ? sinfo.MaxChunkSize : (int)(sinfo.Length - position);
            //return _binaryParser.Read<T>(operate, position, length);
            return default;
        }

        #endregion

        #region ReadAsync

        public void ReadAsync(LocalFileInfo fileInfo, uint chunkIndex, SplitterInfo sinfo, Action<byte[]?> callback)
        {
            sinfo.ArgumentNull(nameof(sinfo));
            if (chunkIndex > sinfo.ChunkCount)
            {
                ErrorUtil.ArgumentOutOfRange(nameof(chunkIndex), "块索引超出范围");
            }

            long position = sinfo.MaxChunkSize * (sinfo.ChunkCount - 1);
            long length = sinfo.ChunkCount > chunkIndex ? sinfo.MaxChunkSize : (int)(sinfo.Length - position);
            //_binaryParser.ReadAsync(fileInfo, position, length, callback);
        }

        public void ReadAsync(IFileOperate operate, uint chunkIndex, SplitterInfo sinfo, Action<byte[]?> callback)
        {
            sinfo.ArgumentNull(nameof(sinfo));
            if (chunkIndex > sinfo.ChunkCount)
            {
                ErrorUtil.ArgumentOutOfRange(nameof(chunkIndex), "块索引超出范围");
            }

            long position = sinfo.MaxChunkSize * (sinfo.ChunkCount - 1);
            long length = sinfo.ChunkCount > chunkIndex ? sinfo.MaxChunkSize : (int)(sinfo.Length - position);
            //_binaryParser.ReadAsync(operate, position, length, callback);
        }

        public void ReadAsync(LocalFileInfo fileInfo, uint chunkIndex, SplitterInfo sinfo, byte[] bytes, Action<byte[]> callback)
        {
            sinfo.ArgumentNull(nameof(sinfo));
            if (chunkIndex > sinfo.ChunkCount)
            {
                ErrorUtil.ArgumentOutOfRange(nameof(chunkIndex), "块索引超出范围");
            }

            long position = sinfo.MaxChunkSize * (sinfo.ChunkCount - 1);
            long length = sinfo.ChunkCount > chunkIndex ? sinfo.MaxChunkSize : (int)(sinfo.Length - position);
            //_binaryParser.ReadAsync(fileInfo, position, length, bytes, callback);
        }

        public void ReadAsync(IFileOperate operate, uint chunkIndex, SplitterInfo sinfo, byte[] bytes, Action<byte[]> callback)
        {
            sinfo.ArgumentNull(nameof(sinfo));
            if (chunkIndex > sinfo.ChunkCount)
            {
                ErrorUtil.ArgumentOutOfRange(nameof(chunkIndex), "块索引超出范围");
            }

            long position = sinfo.MaxChunkSize * (sinfo.ChunkCount - 1);
            int length = sinfo.ChunkCount > chunkIndex ? sinfo.MaxChunkSize : (int)(sinfo.Length - position);
            operate.ReadAsync(position, length, bytes, callback);
        }

        public void ReadAsync<T>(LocalFileInfo fileInfo, uint chunkIndex, SplitterInfo sinfo, Action<T?> callback)
        {
            sinfo.ArgumentNull(nameof(sinfo));
            if (chunkIndex > sinfo.ChunkCount)
            {
                ErrorUtil.ArgumentOutOfRange(nameof(chunkIndex), "块索引超出范围");
            }

            long position = sinfo.MaxChunkSize * (sinfo.ChunkCount - 1);
            long length = sinfo.ChunkCount > chunkIndex ? sinfo.MaxChunkSize : (int)(sinfo.Length - position);
            //_binaryParser.ReadAsync(fileInfo, position, length, callback);

        }

        public void ReadAsync<T>(IFileOperate operate, uint chunkIndex, SplitterInfo sinfo, Action<T?> callback)
        {
            sinfo.ArgumentNull(nameof(sinfo));
            if (chunkIndex > sinfo.ChunkCount)
            {
                ErrorUtil.ArgumentOutOfRange(nameof(chunkIndex), "块索引超出范围");
            }

            long position = sinfo.MaxChunkSize * (sinfo.ChunkCount - 1);
            long length = sinfo.ChunkCount > chunkIndex ? sinfo.MaxChunkSize : (int)(sinfo.Length - position);
            //_binaryParser.ReadAsync(operate, position, length, callback);
        }

        #endregion

        #region Write

        public void Write(LocalFileInfo targetFileInfo, SplitterInfo sinfo, byte[] bytes, uint chunkIndex, int bytesLength = 0)
        {
            sinfo.ArgumentNull(nameof(sinfo));
            if (chunkIndex > sinfo.ChunkCount)
            {
                ErrorUtil.ArgumentOutOfRange(nameof(chunkIndex), "块索引超出范围");
            }
            if (bytesLength < 0 || bytesLength > sinfo.MaxChunkSize || bytesLength > bytes.LongLength)
            {
                throw new ArgumentOutOfRangeException(nameof(bytesLength), "字节长度超出范围");
            }
            bytesLength = bytesLength == 0 ? bytes.Length : bytesLength;
            var position = sinfo.MaxChunkSize * chunkIndex;
            //_binaryParser.Write(targetFileInfo, bytes, position, 0, bytesLength);
        }

        public void Write(IFileOperate operate, SplitterInfo sinfo, byte[] bytes, uint chunkIndex, int bytesLength = 0)
        {
            sinfo.ArgumentNull(nameof(sinfo));
            if (chunkIndex > sinfo.ChunkCount)
            {
                ErrorUtil.ArgumentOutOfRange(nameof(chunkIndex), "块索引超出范围");
            }
            if (bytesLength < 0 || bytesLength > sinfo.MaxChunkSize || bytesLength > bytes.LongLength)
            {
                throw new ArgumentOutOfRangeException(nameof(bytesLength), "字节长度超出范围");
            }
            bytesLength = bytesLength == 0 ? bytes.Length : bytesLength;
            var position = sinfo.MaxChunkSize * chunkIndex;
            //_binaryParser.Write(operate, bytes, position, 0, bytesLength);
        }

        public void Write<T>(LocalFileInfo targetFileInfo, SplitterInfo sinfo, T value, uint chunkIndex)
        {
            sinfo.ArgumentNull(nameof(sinfo));
            if (chunkIndex > sinfo.ChunkCount)
            {
                ErrorUtil.ArgumentOutOfRange(nameof(chunkIndex), "块索引超出范围");
            }

            //var length = _binaryParser.GetLength(value);
            //if (length > sinfo.MaxChunkSize)
            //{
            //    throw new ArgumentOutOfRangeException(nameof(value), "值的长度超出最大块大小");
            //}

            //var position = sinfo.MaxChunkSize * chunkIndex;
            //_binaryParser.Write(targetFileInfo, value, position);
        }

        public void Write<T>(IFileOperate operate, SplitterInfo sinfo, T value, uint chunkIndex)
        {
            sinfo.ArgumentNull(nameof(sinfo));
            if (chunkIndex > sinfo.ChunkCount)
            {
                ErrorUtil.ArgumentOutOfRange(nameof(chunkIndex), "块索引超出范围");
            }

            //var length = _binaryParser.GetLength(value);
            //if (length > sinfo.MaxChunkSize)
            //{
            //    throw new ArgumentOutOfRangeException(nameof(value), "值的长度超出最大块大小");
            //}

            //var position = sinfo.MaxChunkSize * chunkIndex;
            //_binaryParser.Write(operate, value, position);
        }

        public void Write<T>(LocalFileInfo targetFileInfo, SplitterInfo sinfo, SplitterDto dto)
        {
            Write(targetFileInfo, sinfo, dto.Bytes, dto.ChunkIndex, dto.Length);
        }

        public void Write<T>(IFileOperate operate, SplitterInfo sinfo, SplitterDto dto)
        {
            Write(operate, sinfo, dto.Bytes, dto.ChunkIndex, dto.Length);
        }

        #endregion

        #region WriteAsync

        public void WriteAsync(LocalFileInfo targetFileInfo, SplitterInfo sinfo, byte[] bytes, uint chunkIndex, int bytesLength = 0, Action<byte[]>? callback = null)
        {
            sinfo.ArgumentNull(nameof(sinfo));
            if (chunkIndex > sinfo.ChunkCount)
            {
                ErrorUtil.ArgumentOutOfRange(nameof(chunkIndex), "块索引超出范围");
            }
            if (bytesLength < 0 || bytesLength > sinfo.MaxChunkSize || bytesLength > bytes.LongLength)
            {
                throw new ArgumentOutOfRangeException(nameof(bytesLength), "字节长度超出范围");
            }
            bytesLength = bytesLength == 0 ? bytes.Length : bytesLength;
            var position = sinfo.MaxChunkSize * chunkIndex;
            //_binaryParser.WriteAsync(targetFileInfo, bytes, position, 0, bytesLength, callback);
        }

        public void WriteAsync(IFileOperate operate, SplitterInfo sinfo, byte[] bytes, uint chunkIndex, int bytesLength = 0, Action<byte[]>? callback = null)
        {
            sinfo.ArgumentNull(nameof(sinfo));
            if (chunkIndex > sinfo.ChunkCount)
            {
                ErrorUtil.ArgumentOutOfRange(nameof(chunkIndex), "块索引超出范围");
            }
            if (bytesLength < 0 || bytesLength > sinfo.MaxChunkSize || bytesLength > bytes.LongLength)
            {
                throw new ArgumentOutOfRangeException(nameof(bytesLength), "字节长度超出范围");
            }
            bytesLength = bytesLength == 0 ? bytes.Length : bytesLength;
            var position = sinfo.MaxChunkSize * chunkIndex;
            //_binaryParser.WriteAsync(operate, bytes, position, 0, bytesLength, callback);
        }

        public void WriteAsync<T>(LocalFileInfo targetFileInfo, SplitterInfo sinfo, T value, uint chunkIndex, Action? callback = null)
        {
            sinfo.ArgumentNull(nameof(sinfo));
            if (chunkIndex > sinfo.ChunkCount)
            {
                ErrorUtil.ArgumentOutOfRange(nameof(chunkIndex), "块索引超出范围");
            }

            //var length = _binaryParser.GetLength(value);
            //if (length > sinfo.MaxChunkSize)
            //{
            //    throw new ArgumentOutOfRangeException(nameof(value), "值的长度超出最大块大小");
            //}

            //var position = sinfo.MaxChunkSize * chunkIndex;
            //_binaryParser.WriteAsync(targetFileInfo, value, position, callback);
        }

        public void WriteAsync<T>(IFileOperate operate, SplitterInfo sinfo, T value, uint chunkIndex, Action? callback = null)
        {
            sinfo.ArgumentNull(nameof(sinfo));
            if (chunkIndex > sinfo.ChunkCount)
            {
                ErrorUtil.ArgumentOutOfRange(nameof(chunkIndex), "块索引超出范围");
            }

            //var length = _binaryParser.GetLength(value);
            //if (length > sinfo.MaxChunkSize)
            //{
            //    throw new ArgumentOutOfRangeException(nameof(value), "值的长度超出最大块大小");
            //}

            //var position = sinfo.MaxChunkSize * chunkIndex;
            //_binaryParser.WriteAsync(operate, value, position, callback);
        }

        public void WriteAsync(LocalFileInfo targetFileInfo, SplitterInfo sinfo, SplitterDto dto, Action? callback = null)
        {
            WriteAsync(targetFileInfo, sinfo, dto.Bytes, dto.ChunkIndex, dto.Length, b => { callback?.Invoke(); });
        }

        public void WriteAsync(IFileOperate operate, SplitterInfo sinfo, SplitterDto dto, Action? callback = null)
        {
            WriteAsync(operate, sinfo, dto.Bytes, dto.ChunkIndex, dto.Length, callback == null ? null : b => { callback?.Invoke(); });
        }

        #endregion
    }
}
