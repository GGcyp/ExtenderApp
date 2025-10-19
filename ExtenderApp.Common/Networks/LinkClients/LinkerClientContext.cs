using ExtenderApp.Common.Pipelines;
using ExtenderApp.Data;

namespace ExtenderApp.Common.Networks
{
    public class LinkerClientContext : PipelineContext
    {
        public ByteBlock ReceiveBlock { get; set; }

        public ByteBlock SendBlock { get; set; }

        public ByteBlock DataBlock { get; set; }

        public LinkHeader LinkHeader { get; set; }

        protected override void ProtectedReset()
        {
            ReceiveBlock.Reset();
            SendBlock.Reset();
            DataBlock.Reset();
            LinkHeader = default;
        }

        protected override void Dispose(bool disposing)
        {
            if (!disposing)
                return;

            ReceiveBlock.Dispose();
            SendBlock.Dispose();
            DataBlock.Dispose();
        }
    }
}
