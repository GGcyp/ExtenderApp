using ExtenderApp.Abstract.Networks;
using ExtenderApp.Buffer;
using ExtenderApp.Common.Networks.LinkChannels.Handlers;
using ExtenderApp.Contracts;
using HttpHeader = ExtenderApp.Abstract.Networks.HttpHeader;
using HttpMethod = ExtenderApp.Abstract.Networks.HttpMethod;
using HttpRequestMessage = ExtenderApp.Abstract.Networks.HttpRequestMessage;

namespace ExtenderApp.Common.Networks
{
    /// <summary>
    /// HttpRequestMessageHandler 消息处理器，负责将接收到的字节流解析为 HttpRequestMessage 对象，并将 HttpRequestMessage 对象序列化为字节流发送出去。
    /// </summary>
    public class HttpRequestMessageHandler : MessageHandler<HttpRequestMessage>
    {
        private MemoryBlock<byte>? bufferBlock;
        private HttpRequestMessage? message;

        public HttpRequestMessageHandler(MessageDirection direction) : base(direction)
        {
        }

        protected override ValueTask<Result<HttpRequestMessage>> DeserializationMessageAsync(MemoryBlock<byte> block)
        {
            if (bufferBlock == null)
                bufferBlock = MemoryBlock<byte>.GetBuffer((int)block.Committed);

            bufferBlock.Write(block.CommittedSpan);

            var span = bufferBlock.CommittedSpan;
            if (message == null)
            {
                if (!HttpHeaderHelps.TryGetHasHttpHeader(span, out int headerLength))
                    return NeedMoreDataResult;

                if (HttpHeaderHelps.TryParseRequestStartLine(span, out string method, out string path, out Version version, out int lineLength))
                {
                    HttpHeaderHelps.TryGetHttpHeaderNotStartLine(span.Slice(lineLength, headerLength - lineLength), out HttpHeader header);
                    message = new()
                    {
                        Method = new HttpMethod(method),
                        RequestUri = new Uri(path, UriKind.RelativeOrAbsolute),
                        Version = version,
                        Headers = header,
                    };
                    var newBlock = bufferBlock.Slice(headerLength);
                    bufferBlock.TryRelease();
                    bufferBlock = newBlock;
                    span = bufferBlock.CommittedSpan;
                }
                else
                {
                    return NeedMoreDataResult;
                }
            }

            // check for Content-Length
            if (message.Headers.TryGetOptionValue(HttpHeaderOptions.ContentLengthIdentifier, out long contentLen))
            {
                if (contentLen < 0)
                    return Result.Failure<HttpRequestMessage>("Invalid Content-Length");

                if (bufferBlock.Committed < contentLen)
                    return NeedMoreDataResult;

                // extract body
                var bodyBlock = bufferBlock.Slice(0, (int)contentLen);
                message.Body = bodyBlock;

                // advance remainder
                var remainder = bufferBlock.Slice((int)contentLen);
                bufferBlock.TryRelease();
                bufferBlock = remainder;
                return Result.Success(message);
            }

            // check Transfer-Encoding: chunked is not supported
            if (message.Headers.TryGetOptionValue(HttpHeaderOptions.TransferEncodingIdentifier, out ValueOrList<string>? te))
            {
                if (te != null && te.Any(s => string.Equals(s, "chunked", StringComparison.OrdinalIgnoreCase)))
                    return Result.Failure<HttpRequestMessage>("Chunked transfer encoding not supported");
            }

            // no body
            return Result.Success(message);
        }

        protected override ValueTask<Result<AbstractBuffer<byte>>> SerializationMessageAsync(HttpRequestMessage value)
        {
            SequenceBuffer<byte> buffer = SequenceBuffer<byte>.GetBuffer();
            value.WriteToBuffer(buffer);
            return Result.Success((AbstractBuffer<byte>)buffer);
        }
    }
}