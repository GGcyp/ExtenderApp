using ExtenderApp.Abstract;
using ExtenderApp.Data;

namespace ExtenderApp.Common.IO.Splitter
{
    /// <summary>
    /// 分割流操作数据类
    /// </summary>
    internal class SplitterStreamOperateData : FileStreamConcurrentOperateData
    {
        /// <summary>
        /// 分割器信息文件扩展名，默认为二进制文件扩展名
        /// </summary>
        public const string SplitterInfoExtensions = FileExtensions.BinaryFileExtensions;

        /// <summary>
        /// 分割器信息文件信息
        /// </summary>
        public ExpectLocalFileInfo SplitterInfoFileInfo { get; private set; }

        /// <summary>
        /// 分割器信息
        /// </summary>
        public SplitterInfo SplitterInfo { get; set; }

        internal void OpenFile(ExpectLocalFileInfo fileInfo)
        {
            SplitterInfoFileInfo = fileInfo;
        }

        public override bool TryReset()
        {
            SplitterInfoFileInfo=ExpectLocalFileInfo.Empty;
            SplitterInfo = null;
            return base.TryReset();
        }
    }
}
