using ExtenderApp.Abstract;
using ExtenderApp.Common.Pipelines;
using ExtenderApp.Data;

namespace ExtenderApp.Common.Networks
{
    public class LinkHeaderMiddleware : MiddlewareBase<LinkerClientContext>
    {
        private static byte[] DefaultHeaderData = { 1, 1, 1, 1 };

        private readonly byte[] _headerArray;
        private readonly IBinaryFormatter<LinkHeader> _linkHeaderFormatter;
        private readonly IByteBufferFactory _byteBufferFactory;

        private Memory<byte> headerMemory => _headerArray.AsMemory();
        private int HeaderLength => headerMemory.Length;

        public LinkHeaderMiddleware(IBinaryFormatter<LinkHeader> linkHeaderFormatter, IByteBufferFactory byteBufferFactory) : this(DefaultHeaderData, linkHeaderFormatter, byteBufferFactory)
        {
        }

        public LinkHeaderMiddleware(byte[] headers, IBinaryFormatter<LinkHeader> linkHeaderFormatter, IByteBufferFactory byteBufferFactory)
        {
            _headerArray = headers;
            _linkHeaderFormatter = linkHeaderFormatter;
            _byteBufferFactory = byteBufferFactory;
        }


        public override async Task InvokeAsync(LinkerClientContext context, Func<Task> next)
        {
            var receiveBlock = context.ReceiveBlock;
            var receiveMemory = receiveBlock.UnreadMemory;
            if (!receiveMemory.IsEmpty && receiveMemory.Length > HeaderLength
                || ValidateHeader(receiveMemory))
            {
                receiveBlock.ReadAdvance(HeaderLength);

                var linkHeader = ReadLinkHeader(ref receiveBlock);

                context.LinkHeader = linkHeader;
                context.ReceiveBlock = receiveBlock;
            }

            await next();

            var dataBlock = context.DataBlock;
            var sendBlock = context.SendBlock;
            if (dataBlock.IsEmpty || dataBlock.Remaining <= 0)
            {
                sendBlock.Write(headerMemory);
                WriteLinkHeader(ref sendBlock, context.LinkHeader);
                context.SendBlock = sendBlock;
                return;
            }


            var sendMemory = sendBlock.UnreadMemory;
        }

        private bool ValidateHeader(ReadOnlyMemory<byte> memory)
        {
            return memory.Span.StartsWith(headerMemory.Span);
        }

        private LinkHeader ReadLinkHeader(ref ByteBlock block)
        {
            ByteBuffer buffer = block;
            long oldConsumed = buffer.Consumed;
            var result = _linkHeaderFormatter.Deserialize(ref buffer);
            int length = (int)(buffer.Consumed - oldConsumed);
            block.ReadAdvance(length);
            return result;
        }

        private void WriteLinkHeader(ref ByteBlock block, LinkHeader linkHeader)
        {
            var buffer = _byteBufferFactory.Create();
            _linkHeaderFormatter.Serialize(ref buffer, linkHeader);
            block.Write(buffer);
        }
    }
}
