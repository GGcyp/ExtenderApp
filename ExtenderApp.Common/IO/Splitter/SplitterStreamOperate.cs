using ExtenderApp.Abstract;
using ExtenderApp.Data;
using ExtenderApp.Common.IO.StreamOperates;


namespace ExtenderApp.Common.IO.Splitter
{
    internal class SplitterStreamOperate : StreamOperate
    {
        private IBinaryParser binaryParser;
        private ExpectLocalFileInfo fileInfo;
        public SplitterInfo SplitterInfo { get; set; }

        internal void OpenFile(IBinaryParser binaryParser, ExpectLocalFileInfo fileInfo)
        {
            this.binaryParser = binaryParser;
            this.fileInfo = fileInfo;
            SplitterInfo = binaryParser.Deserialize<SplitterInfo>(fileInfo);
        }

        public override bool TryReset()
        {
            binaryParser = null;
            fileInfo = ExpectLocalFileInfo.Empty;
            return base.TryReset();
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }
    }
}
