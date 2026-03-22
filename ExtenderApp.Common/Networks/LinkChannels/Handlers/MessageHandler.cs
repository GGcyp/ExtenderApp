using ExtenderApp.Abstract;
using ExtenderApp.Buffer;
using ExtenderApp.Contracts;

namespace ExtenderApp.Common.Networks.LinkChannels.Handlers
{
    public abstract class MessageHandler<T> : LinkChannelHandler
    {
        protected static readonly Result<T> NeedMoreDataResult = Result.Failure<T>(default!, "Need more data");

        public event Action<ILinkChannel, T>? Callback;

        public MessageDirection Direction { get; set; }

        public MessageHandler() : this(MessageDirection.Both)
        {
        }

        public MessageHandler(MessageDirection direction) : base()
        {
            Direction = direction;
        }

        public override async ValueTask<Result> InboundHandleAsync(ILinkChannelHandlerContext context, ValueCache cache, CancellationToken token = default)
        {
            if (token.IsCancellationRequested)
                return Result.Failure();
            if ((Direction & MessageDirection.Inbound) == 0)
                return await context.InboundHandleAsync(cache, token).ConfigureAwait(false);

            try
            {
                if (cache.TryGetValue(out MemoryBlock<byte> buffer))
                {
                    var result = await DeserializationMessageAsync(buffer);
                    if (result)
                    {
                        T value = result!;
                        Callback?.Invoke(context.LinkChannel, value);
                        cache.TryTakeValue(out buffer);
                        buffer.TryRelease();
                    }
                    else if (result == NeedMoreDataResult)
                    {
                        cache.TryTakeValue(out buffer);
                        buffer.TryRelease();
                    }
                    else
                        return result.HasException ? context.ExceptionCaught(result.ResultException!) : result;
                }

                return await context.InboundHandleAsync(cache, token).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return ExceptionCaught(context, ex);
            }
        }

        public override async ValueTask<Result> OutboundHandleAsync(ILinkChannelHandlerContext context, ValueCache cache, CancellationToken token = default)
        {
            if (token.IsCancellationRequested)
                return Result.Failure();
            if ((Direction & MessageDirection.Outbound) == 0)
                return await context.OutboundHandleAsync(cache, token).ConfigureAwait(false);

            try
            {
                while (cache.TryTakeValue(out T value))
                {
                    var result = await SerializationMessageAsync(value);

                    if (result)
                        cache.AddValue(result.Value);
                    else
                        return result.HasException ? context.ExceptionCaught(result.ResultException!) : result;
                }
                return await context.OutboundHandleAsync(cache, token).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return ExceptionCaught(context, ex);
            }
        }

        protected abstract ValueTask<Result<AbstractBuffer<byte>>> SerializationMessageAsync(T value);

        protected abstract ValueTask<Result<T>> DeserializationMessageAsync(MemoryBlock<byte> block);
    }
}