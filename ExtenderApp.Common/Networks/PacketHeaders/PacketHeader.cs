using ExtenderApp.Abstract;
using ExtenderApp.Data;

namespace ExtenderApp.Common.Networks
{
    public class PacketHeader : IMiddleware<ILinkerClientPiplineContext>
    {
        private static byte[] DefaultHeaderData = { 1, 1, 1, 1 };

        private readonly Memory<byte> _headerMemory;
        private readonly IBinaryFormatter<int> _intFormatter;

        private int HeaderLength => _headerMemory.Length;

        public PacketHeader(IBinaryFormatter<int> intFormatter) : this(DefaultHeaderData, intFormatter)
        {
        }

        public PacketHeader(byte[] headers, IBinaryFormatter<int> intFormatter)
        {
            _headerMemory = headers;
            _intFormatter = intFormatter;
        }

        public void ApplyHeader(ref ByteBuffer buffer, out ByteBlock sendBlock)
        {
            sendBlock = new ByteBlock((int)buffer.Length + HeaderLength);
            if (buffer.IsEmpty || buffer.Remaining <= 0)
            {
                sendBlock.Write(buffer);
                return;
            }


            sendBlock.Write(_headerMemory);

        }

        public Task InvokeAsync(ILinkerClientPiplineContext context, Func<Task> next)
        {
            throw new NotImplementedException();
        }
    }
}
