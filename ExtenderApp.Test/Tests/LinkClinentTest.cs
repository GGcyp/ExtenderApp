using System.Diagnostics;
using System.Net;
using ExtenderApp.Abstract;
using ExtenderApp.Buffer;
using ExtenderApp.Common.Networks.LinkClients;
using ExtenderApp.Common.Networks.LinkClients.Handlers;
using ExtenderApp.Contracts;
using Microsoft.Extensions.DependencyInjection;

namespace ExtenderApp.Test.Tests
{
    internal static class LinkClinentTest
    {
        public static async Task TestLinkClientHandlerAsync(IServiceProvider serviceProvider)
        {
            LinkClient<ITcpLinker> client = new(serviceProvider.GetRequiredService<ITcpLinker>());
            var handler = new TempHandler();
            client.AddLast("TempHandler", handler);
            TempMessageHandler messageHandler = new();
            client.AddLast("TempMessageHandler", messageHandler);

            await client.SendAsync(100000);
        }

        private sealed class TempHandler : LinkClientHandler
        {
            public override void Active(ILinkClientHandlerContext context)
            {
                Debug.Print("Handler {0} is active.", context.Name);
                base.Active(context);
            }

            public override ValueTask<Result> BindAsync(ILinkClientHandlerContext context, EndPoint localAddress, CancellationToken token = default)
            {
                Debug.Print("Handler {0} is binding to local address: {1}", context.Name, localAddress);
                return base.BindAsync(context, localAddress, token);
            }

            public override ValueTask<Result> CloseAsync(ILinkClientHandlerContext context, CancellationToken token = default)
            {
                Debug.Print("Handler {0} is closing the connection.", context.Name);
                return base.CloseAsync(context, token);
            }

            public override ValueTask<Result<int>> InboundHandleAsync(ILinkClientHandlerContext context, ValueCache cache, CancellationToken token = default)
            {
                Debug.Print("Handler {0} is handling inbound data.", context.Name);
                return base.InboundHandleAsync(context, cache, token);
            }

            public override ValueTask<Result> OutboundHandleAsync(ILinkClientHandlerContext context, ValueCache cache, CancellationToken token = default)
            {
                return base.OutboundHandleAsync(context, cache, token);
            }
        }

        private sealed class TempMessageHandler : MessageHandler<int>
        {
            protected override ValueTask<Result<int>> DeserializationMessageAsync(AbstractBuffer<byte> reader)
            {
                int value = reader.Read<int>();
                Debug.Print("Deserialized value: {0}", value);
                return Result.Success(value);

            }

            protected override ValueTask<Result> SerializationMessageAsync(int value, AbstractBuffer<byte> buffer)
            {
                buffer.Write(value);
                Debug.Print("Serialized value: {0}", value);
                return Result.Success();
            }
        }
    }
}