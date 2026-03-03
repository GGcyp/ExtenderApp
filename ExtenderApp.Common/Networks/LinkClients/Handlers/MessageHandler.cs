using ExtenderApp.Abstract;
using ExtenderApp.Buffer;
using ExtenderApp.Contracts;

namespace ExtenderApp.Common.Networks.LinkClients.Handlers
{
    public abstract class MessageHandler<T> : LinkClientHandler
    {
        private readonly bool _isClassType;

        public event Action<ILinkClient, T>? Callback;

        public MessageHandler() : base()
        {
            _isClassType = typeof(T).IsClass;
        }

        public override async ValueTask<Result<int>> InboundHandleAsync(ILinkClientHandlerContext context, ValueCache cache, CancellationToken token = default)
        {
            if (token.IsCancellationRequested)
                return Result.Failure(0);

            int resultInt = 0;
            Result<int> contextResult = default;
            try
            {
                if (cache.TryTakeValue<AbstractBuffer<byte>>(out var buffer))
                {
                    var result = await DeserializationMessageAsync(buffer);
                    if (result)
                    {
                        T value = result!;
                        Callback?.Invoke(context.LinkClient, value);
                        resultInt = (int)buffer.Committed;
                        buffer.Release();
                    }
                }
            }
            catch (Exception ex)
            {
                context.ExceptionCaught(ex);
            }
            finally
            {
                contextResult = await base.InboundHandleAsync(context, cache, token);
            }
            return Result.Success(resultInt + contextResult, contextResult.Message);
        }

        public override async ValueTask<Result> OutboundHandleAsync(ILinkClientHandlerContext context, ValueCache cache, CancellationToken token = default)
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
                    {
                        cache.AddValue(buffer);
                    }
                    else
                    {
                        buffer.Release();
                        context.ExceptionCaught(result.Exception);
                    }
                }
                return Result.Success();
            }
            catch (Exception ex)
            {
                context.ExceptionCaught(ex);
            }
            return await base.OutboundHandleAsync(context, cache, token);
        }

        protected abstract ValueTask<Result> SerializationMessageAsync(T value, AbstractBuffer<byte> buffer);

        protected abstract ValueTask<Result<T>> DeserializationMessageAsync(AbstractBuffer<byte> reader);
    }
}