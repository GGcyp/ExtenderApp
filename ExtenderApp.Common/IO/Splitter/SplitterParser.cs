using System.Buffers;
using System.Security.Cryptography;
using ExtenderApp.Abstract;
using ExtenderApp.Common.Error;
using ExtenderApp.Common.Hash;
using ExtenderApp.Data;

namespace ExtenderApp.Common.IO.Splitter
{
    /// <summary>
    /// 分割器解析器类
    /// </summary>
    internal class SplitterParser
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
            if (operate is not FileOperate fileOperate)
            {
                ErrorUtil.ArgumentNull(nameof(operate));
                return;
            }

            //var infoFile = fileOperate.Data.OperateInfo.LocalFileInfo.ChangeFileExtension(infoExtensions);
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

            //HashValue md5 = _hashProvider.ComputeHash<MD5>(targtFileInfo);
            //return new SplitterInfo((int)length, chunkCount, 0, chunkMaxLength, targtFileInfo.Extension, md5, createLoadedChunks ? new PieceData(new byte[chunkCount]) : PieceData.Empty); ;
            return default;
        }

        #endregion
    }
}
