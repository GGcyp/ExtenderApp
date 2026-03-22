using System.Net.Security;
using ExtenderApp.Abstract;
using ExtenderApp.Buffer;
using ExtenderApp.Buffer.Streams;
using ExtenderApp.Contracts;

namespace ExtenderApp.Common.Networks.LinkChannels
{
    public class SslHandler : LinkChannelHandler
    {
        private AbstractBufferStreamDecorator _bufferStream;
        private readonly SslStream _sslStream;
        private readonly StreamDecorator _decorator;
        private object? options;

        public bool IsClient => ClientAuthenticationOptions != null;
        public bool IsServer => ServerAuthenticationOptions != null;

        public SslClientAuthenticationOptions? ClientAuthenticationOptions
        {
            get => options as SslClientAuthenticationOptions;
            set => options = value;
        }

        public SslServerAuthenticationOptions? ServerAuthenticationOptions
        {
            get => options as SslServerAuthenticationOptions;
            set => options = value;
        }

        public SslHandler(SslClientAuthenticationOptions sslClientAuthenticationOptions) : this(static s => new(s, true))
        {
            options = sslClientAuthenticationOptions;
        }

        public SslHandler(Func<Stream, SslStream> sslFactory, SslClientAuthenticationOptions sslClientAuthenticationOptions) : this(sslFactory)
        {
            options = sslClientAuthenticationOptions;
        }

        public SslHandler(SslServerAuthenticationOptions sslServerAuthenticationOptions) : this(static s => new(s, true))
        {
            options = sslServerAuthenticationOptions;
        }

        public SslHandler(Func<Stream, SslStream> sslFactory, SslServerAuthenticationOptions sslServerAuthenticationOptions) : this(sslFactory)
        {
            options = sslServerAuthenticationOptions;
        }

        public SslHandler(Func<Stream, SslStream> sslFactory)
        {
            _bufferStream = new();
            _decorator = new();
            _decorator.SetInnerStream(_bufferStream);
            _sslStream = sslFactory(_decorator);
        }

        public override async ValueTask<Result> ActiveAsync(ILinkChannelHandlerContext context, CancellationToken token = default)
        {
            try
            {
                if (context.LinkChannel.Linker is not ITcpLinker tcpLinker)
                    throw new InvalidOperationException("当前连接不是TCP连接，无法使用SSL");

                var tcpLinkerStream = tcpLinker.GetStream();
                _decorator.SetInnerStream(tcpLinkerStream);
                await AuthenticateAsync(token).ConfigureAwait(false);
                _decorator.SetInnerStream(_bufferStream);
                return await context.ActiveAsync(token).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return context.ExceptionCaught(ex);
            }
        }

        public override ValueTask<Result> OutboundHandleAsync(ILinkChannelHandlerContext context, ValueCache cache, CancellationToken token = default)
        {
            try
            {
                while (cache.TryTakeValue(out AbstractBuffer<byte> buffer))
                {
                    _bufferStream.SetReadBuffer(buffer);

                    MemoryBlock<byte> block = MemoryBlock<byte>.GetBuffer((int)_sslStream.Length);
                    int count = _sslStream.Read(block.GetAvailableSpan());
                    block.Advance(count);
                    cache.AddValue(block);
                }
                return context.OutboundHandleAsync(cache, token);
            }
            catch (Exception ex)
            {
                return context.ExceptionCaught(new InvalidOperationException("SslHandler 处理出站数据时发生错误", ex));
            }
        }

        public override ValueTask<Result> InboundHandleAsync(ILinkChannelHandlerContext context, ValueCache cache, CancellationToken token = default)
        {
            try
            {
                while (!token.IsCancellationRequested && cache.TryTakeValue(out AbstractBuffer<byte> buffer))
                {
                    _bufferStream.SetLength(buffer.Committed);
                    if (buffer is MemoryBlock<byte> memoryBlock)
                    {
                        _sslStream.Write(memoryBlock.CommittedSpan);
                    }
                    else if (buffer is SequenceBuffer<byte> sequenceBuffer)
                    {
                        foreach (var memory in sequenceBuffer.CommittedSequence)
                        {
                            _sslStream.Write(memory.Span);
                        }
                    }
                    buffer.TryRelease();

                    var writeBuffer = _bufferStream.GetWriteBuffer();
                    if (writeBuffer != null)
                        cache.AddValue(buffer);
                }
                return context.InboundHandleAsync(cache, token);
            }
            catch (Exception ex)
            {
                return context.ExceptionCaught(new InvalidOperationException("SslHandler 处理入站数据时发生错误", ex));
            }
        }

        public override ValueTask<Result> CloseAsync(ILinkChannelHandlerContext context, CancellationToken token = default)
        {
            _sslStream!.Close();
            _bufferStream.Clear();
            return context.CloseAsync(token);
        }

        private async ValueTask AuthenticateAsync(CancellationToken token)
        {
            if (options is SslClientAuthenticationOptions clientOptions)
                await _sslStream.AuthenticateAsClientAsync(clientOptions, token).ConfigureAwait(false);
            else if (options is SslServerAuthenticationOptions serverOptions)
                await _sslStream.AuthenticateAsServerAsync(serverOptions, token).ConfigureAwait(false);
        }

        protected override void DisposeManagedResources()
        {
            base.DisposeManagedResources();
            _sslStream?.Dispose();
            _bufferStream?.Dispose();
            _decorator?.Dispose();
        }
    }
}