using System.Net;
using ExtenderApp.Abstract.Networks;
using ExtenderApp.Buffer;
using ExtenderApp.Common.Networks.LinkChannels.Handlers;
using ExtenderApp.Contracts;
using HttpHeader = ExtenderApp.Abstract.Networks.HttpHeader;
using HttpResponseMessage = ExtenderApp.Abstract.Networks.HttpResponseMessage;

namespace ExtenderApp.Common.Networks.Http
{
    /// <summary>
    /// HttpResponseMessage 消息处理器，负责将接收到的字节流解析为 HttpResponseMessage 对象，并将 HttpResponseMessage 对象序列化为字节流发送出去。
    /// </summary>
    public class HttpResponseMessageHandler : MessageHandler<HttpResponseMessage>
    {
        private MemoryBlock<byte>? bufferBlock;
        private HttpResponseMessage? message;

        public HttpResponseMessageHandler(MessageDirection direction) : base(direction)
        {
        }

        protected override ValueTask<Result<HttpResponseMessage>> DeserializationMessageAsync(MemoryBlock<byte> block)
        {
            if (bufferBlock == null)
                bufferBlock = MemoryBlock<byte>.GetBuffer((int)block.Committed);

            bufferBlock.Write(block.CommittedSpan);
            var span = bufferBlock.CommittedSpan;

            if (message == null)
            {
                if (!HttpHeaderHelps.TryGetHasHttpHeader(span, out int headerLength))
                    return NeedMoreDataResult;

                if (HttpHeaderHelps.TryParseResponseStartLine(span, out Version version, out int statusCode, out string reasonPhrase, out int lineLength))
                {
                    HttpHeaderHelps.TryGetHttpHeaderNotStartLine(span.Slice(lineLength, headerLength - lineLength), out HttpHeader header);
                    message = new()
                    {
                        StatusCode = (HttpStatusCode)statusCode,
                        Version = version,
                        ReasonPhrase = reasonPhrase,
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
                    return Result.Failure<HttpResponseMessage>("Invalid Content-Length");

                if (bufferBlock.Committed < contentLen)
                    return NeedMoreDataResult;

                var bodyBlock = bufferBlock.Slice(0, (int)contentLen);
                message.Body = bodyBlock;

                var remainder = bufferBlock.Slice((int)contentLen);
                bufferBlock.TryRelease();
                bufferBlock = remainder;
                return Result.Success(message);
            }

            // chunked not supported
            if (message.Headers.TryGetOptionValue(HttpHeaderOptions.TransferEncodingIdentifier, out ValueOrList<string>? te))
            {
                if (te != null && te.Any(s => string.Equals(s, "chunked", StringComparison.OrdinalIgnoreCase)))
                    return Result.Failure<HttpResponseMessage>("Chunked transfer encoding not supported");
            }

            return Result.Success(message);
        }

        protected override ValueTask<Result<AbstractBuffer<byte>>> SerializationMessageAsync(HttpResponseMessage value)
        {
            SequenceBuffer<byte> buffer = SequenceBuffer<byte>.GetBuffer();
            value.WriteToBuffer(buffer);
            return Result.Success((AbstractBuffer<byte>)buffer);
        }
    }
}