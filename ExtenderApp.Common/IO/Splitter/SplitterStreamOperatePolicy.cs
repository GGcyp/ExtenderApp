using System.IO.MemoryMappedFiles;
using ExtenderApp.Abstract;
using ExtenderApp.Data;


namespace ExtenderApp.Common.IO.Splitter
{
    internal class SplitterStreamOperatePolicy : FileStreamConcurrentOperatePolicy<SplitterStreamOperateData>
    {
        private readonly IBinaryParser _binaryParser;

        public SplitterStreamOperatePolicy(IBinaryParser binaryParser)
        {
            _binaryParser = binaryParser;
        }

        //public override FileStream Create(SplitterStreamOperateData data)
        //{
        //    data.SplitterInfo = _binaryParser.Read<SplitterInfo>(data.SplitterInfoFileInfo);
        //    return base.Create(data);
        //}

        public override MemoryMappedViewAccessor Create(SplitterStreamOperateData data)
        {
            data.SplitterInfo = _binaryParser.Read<SplitterInfo>(data.SplitterInfoFileInfo);

            if (data.SplitterInfo == null)
            {
                throw new FileNotFoundException(data.OperateInfo.LocalFileInfo.FilePath);
            }
            return MemoryMappedFile.CreateFromFile(data.OperateInfo.OpenFile(), data.OperateInfo.LocalFileInfo.FileName, data.SplitterInfo.Length, MemoryMappedFileAccess.ReadWrite, HandleInheritability.Inheritable, true).CreateViewAccessor();
        }

        ///// <summary>
        ///// 执行后的操作
        ///// </summary>
        ///// <param name="item">文件流</param>
        //public override void AfterExecute(FileStream operate, SplitterStreamOperateData data)
        //{
        //    if (data.SplitterInfo.IsComplete)
        //    {
        //        var info = data.SplitterInfoFileInfo;
        //        var operateInfo = data.OperateInfo;
        //        data.ReleaseFileOperateInfo(operateInfo);

        //        var fileinfoOperate = info.CreateFileOperate(SplitterStreamOperateData.SplitterInfoExtensions);
        //        data.ReleaseFileOperateInfo(fileinfoOperate);

        //        operateInfo.Move(operateInfo.LocalFileInfo.ChangeFileExtension(data.SplitterInfo.TargetExtensions));
        //        fileinfoOperate.Delete();
        //    }
        //    else
        //    {
        //        _binaryParser.Write(data.SplitterInfoFileInfo, data.SplitterInfo);
        //    }
        //}
    }
}
