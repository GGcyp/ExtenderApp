using System.Net;
using ExtenderApp.Abstract;
using ExtenderApp.Buffer;
using ExtenderApp.Contracts;

namespace ExtenderApp.Common.Networks.LinkChannels
{
    /// <summary>
    /// 将底层 ILinker 封装为管道中的一个处理器（Transport handler）。
    /// - 出站：接收来自上层的已序列化缓冲并通过 ILinker.SendAsync 发送到网络；
    /// - 入站：作为接收驱动器启动一个后台循环，从 ILinker.ReceiveAsync 读取原始字节并将其推入管道的入站处理；
    /// - 连接/断开则委托给 ILinker 实现。
    ///
    /// 设计要点：把 ILinker 放入管线可以把传输侧的接收/发送逻辑与上层处理器统一，便于在同一管线内插入 SslHandler 等转换层。
    /// </summary>
    internal sealed class LinkerTransportHandler : LinkChannelHandler
    {
        private readonly LinkChannel _linkChannel;

        private ILinker Linker => _linkChannel.Linker;

        public LinkerTransportHandler(LinkChannel linkChannel)
        {
            _linkChannel = linkChannel;
        }

        public override async ValueTask<Result> ConnectAsync(ILinkChannelHandlerContext context, EndPoint remoteAddress, EndPoint localAddress, CancellationToken token = default)
        {
            try
            {
                Result result = await Linker.ConnectAsync(remoteAddress, localAddress, token).ConfigureAwait(false);
                if (!result)
                    return context.ExceptionCaught(result);

                result = await context.ActiveAsync(token).ConfigureAwait(false);
                if (!result)
                    return context.ExceptionCaught(result);

                return result;
            }
            catch (Exception ex)
            {
                return context.ExceptionCaught(ex);
            }
        }

        public override async ValueTask<Result> DisconnectAsync(ILinkChannelHandlerContext context, CancellationToken token = default)
        {
            try
            {
                Result result = await Linker.DisconnectAsync(token).ConfigureAwait(false);
                if (!result)
                    return context.ExceptionCaught(result);

                result = await context.InactiveAsync(token).ConfigureAwait(false);
                if (!result)
                    return context.ExceptionCaught(result);

                return result;
            }
            catch (Exception ex)
            {
                return context.ExceptionCaught(ex);
            }
        }

        public override async ValueTask<Result> OutboundHandleAsync(ILinkChannelHandlerContext context, ValueCache cache, CancellationToken token = default)
        {
            int sendBufferSize = Linker.SendBufferSize;
            var sequence = SequenceBuffer<byte>.GetBuffer();
            try
            {
                // 将 cache 中的所有缓冲追加到 sequence 中（直到达到 send window）
                while (cache.TryGetValue(out AbstractBuffer<byte> operationValue))
                {
                    if (sequence.Committed + operationValue.Committed > sendBufferSize)
                    {
                        break;
                    }

                    cache.TryTakeValue(out operationValue);
                    sequence.Append(operationValue);
                    if (operationValue is SequenceBuffer<byte> sequenceBuffer)
                        sequenceBuffer.TryRelease();

                    continue;
                }

                if (sequence.Committed > 0)
                {
                    var result = await Linker!.SendAsync(sequence, token).ConfigureAwait(false);
                    cache.AddValue(result);
                    sequence.Clear();
                }

                return Result.Success();
            }
            catch (Exception ex)
            {
                return context.ExceptionCaught(ex);
            }
            finally
            {
                sequence.TryRelease();
            }
        }

        public override async ValueTask<Result> InboundHandleAsync(ILinkChannelHandlerContext context, ValueCache cache, CancellationToken token = default)
        {
            if (!cache.TryGetValue(out MemoryBlock<byte> buffer))
            {
                return Result.Failure("没有可用的缓冲区进行接收。");
            }

            try
            {
                var linkOperationValue = await Linker.ReceiveAsync(buffer, token).ConfigureAwait(false);
                cache.AddValue(linkOperationValue);

                return await context.InboundHandleAsync(cache, token).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return context.ExceptionCaught(ex);
            }
        }
    }
}