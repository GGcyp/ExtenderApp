using ExtenderApp.Abstract;
using ExtenderApp.Buffer;
using ExtenderApp.Contracts;

namespace ExtenderApp.Common.Networks.LinkChannels.Handlers
{
    public abstract class MessageHandler<T> : LinkChannelHandler
    {
        private readonly bool _isClassType;

        public event Action<ILinkChannel, T>? Callback;

        public MessageHandler() : base()
        {
            _isClassType = typeof(T).IsClass;
        }

        public override async ValueTask<Result> InboundHandleAsync(ILinkChannelHandlerContext context, ValueCache cache, CancellationToken token = default)
        {
            if (token.IsCancellationRequested)
                return Result.Failure();

            try
            {
                if (cache.TryTakeValue<AbstractBuffer<byte>>(out var buffer))
                {
                    var result = await DeserializationMessageAsync(buffer);
                    if (result)
                    {
                        T value = result!;
                        Callback?.Invoke(context.LinkChannel, value);
                        buffer.TryRelease();
                    }
                }

                return await context.InboundHandleAsync(cache, token).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return context.ExceptionCaught(ex);
            }
        }

        public override async ValueTask<Result> OutboundHandleAsync(ILinkChannelHandlerContext context, ValueCache cache, CancellationToken token = default)
        {
            if (token.IsCancellationRequested)
                return Result.Failure();
            try
            {
                while (cache.TryTakeValue(out T value))
                {
                    AbstractBuffer<byte> buffer = _isClassType ? AbstractBuffer.GetSequence<byte>() : AbstractBuffer.GetBlock<byte>();
                    var result = await SerializationMessageAsync(value, buffer);

                    if (result)
                        cache.AddValue(buffer);
                    else
                        return context.ExceptionCaught(result);
                }
                return await context.OutboundHandleAsync(cache, token).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return context.ExceptionCaught(ex);
            }
        }

        protected abstract ValueTask<Result> SerializationMessageAsync(T value, AbstractBuffer<byte> buffer);

        protected abstract ValueTask<Result<T>> DeserializationMessageAsync(AbstractBuffer<byte> reader);
    }
}