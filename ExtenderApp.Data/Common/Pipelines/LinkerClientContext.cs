namespace ExtenderApp.Data
{
    public class LinkerClientContext : PipelineContext
    {
        public LinkHeader LinkHeader;
        public ByteBlock MessageBlock;

        public PooledFrameList Frames;
        public object? ResultArray;
        public Dictionary<string, object?>? Items;
        public string? TraceId;
        public CancellationToken CancellationToken { get; set; }

        public void AddFrame(Frame frame)
        {
            Frames.Add(frame);
        }

        protected override void ProtectedReset()
        {
            // 释放单帧
            MessageBlock.Dispose();
            LinkHeader = default;

            // 释放多帧负载（逐帧 Dispose），清空并归还数组
            if (Frames.Count > 0)
            {
                var span = Frames.AsSpan();
                for (int i = 0; i < span.Length; i++)
                {
                    span[i].Payload.Dispose();
                }
            }
            Frames.Clear();
            Frames.Dispose();

            // 其它
            ResultArray = null;
            Items?.Clear();
            TraceId = null;
            CancellationToken = default;
        }

        protected override void Dispose(bool disposing)
        {
            if (!disposing)
                return;

            // 与 Reset 保持一致的释放逻辑（保证幂等）
            MessageBlock.Dispose();

            if (Frames.Count > 0)
            {
                var span = Frames.AsSpan();
                for (int i = 0; i < span.Length; i++)
                {
                    span[i].Payload.Dispose();
                }
            }
            Frames.Clear();
            Frames.Dispose();

            ResultArray = null;
            Items?.Clear();
        }
    }
}
