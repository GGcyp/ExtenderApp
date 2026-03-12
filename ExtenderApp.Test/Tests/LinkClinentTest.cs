using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using ExtenderApp.Abstract;
using ExtenderApp.Buffer;
using ExtenderApp.Common.Networks.LinkChannels;
using ExtenderApp.Common.Networks.LinkChannels.Handlers;
using ExtenderApp.Contracts;
using Microsoft.Extensions.DependencyInjection;

namespace ExtenderApp.Test.Tests
{
    internal static class LinkClinentTest
    {
        public static async Task TestLinkClientHandlerAsync(IServiceProvider serviceProvider)
        {
            LinkChannel channel = new(serviceProvider.GetRequiredService<ITcpLinker>());
            var handler = new TempHandler();
            channel.AddLast("TempHandler", handler);
            TempMessageHandler messageHandler = new();
            channel.AddLast("TempMessageHandler", messageHandler);

            var ip = new IPEndPoint(IPAddress.Loopback, 12345);
            TcpListener listener = new(ip);
            listener.Start();
            var acceptSocketTask = listener.AcceptSocketAsync().ConfigureAwait(false);

            var channelConnectTask = channel.ConnectAsync(ip).ConfigureAwait(false);
            var socket = await acceptSocketTask;
            await channelConnectTask;

            var block = MemoryBlock<byte>.GetBuffer(1024);
            BitConverter.GetBytes(123456).CopyTo(block.GetSpan(1024));
            block.Advance(1024);
            block.Freeze();
            channel.StartReceive();
            try
            {
                // start server-side reader
                await Start(socket);

                for (int i = 0; i < 10; i++)
                {
                    var temp = await channel.SendAsync(block).ConfigureAwait(false);
                    Debug.Print(temp.ToString());
                    await Task.Delay(50).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                throw;
            }
            finally
            {
                await channel.DisconnectAsync();
                block.TryRelease();
            }
        }

        private static ValueTask Start(Socket socket)
        {
            Task.Run(() => { StartLocalClient(socket); });
            return ValueTask.CompletedTask;
        }

        private static async ValueTask StartLocalClient(Socket socket)
        {
            try
            {
                byte[] buffer = new byte[1024];
                while (true)
                {
                    // 使用 ReceiveAsync 返回实际接收字节数，避免解析未初始化的数据
                    var received = await socket.ReceiveAsync(buffer).ConfigureAwait(false);
                    if (received <= 0)
                        break;

                    if (received >= 4)
                    {
                        int val = BitConverter.ToInt32(buffer, 0);
                        Debug.Print(val.ToString());
                        var temp = await socket.SendAsync(buffer, SocketFlags.None).ConfigureAwait(false);
                    }
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        private sealed class TempHandler : LinkChannelHandler
        {
            public override ValueTask<Result> ActiveAsync(ILinkChannelHandlerContext context, CancellationToken token = default)
            {
                Debug.Print("Handler {0} is active.", context.Name);
                return context.ActiveAsync(token);
            }

            public override ValueTask<Result> BindAsync(ILinkChannelHandlerContext context, EndPoint localAddress, CancellationToken token = default)
            {
                Debug.Print("Handler {0} is binding to local address: {1}", context.Name, localAddress);
                return context.BindAsync(localAddress, token);
            }

            public override ValueTask<Result> CloseAsync(ILinkChannelHandlerContext context, CancellationToken token = default)
            {
                Debug.Print("Handler {0} is closing the connection.", context.Name);
                return context.CloseAsync(token);
            }

            public override ValueTask<Result> InboundHandleAsync(ILinkChannelHandlerContext context, ValueCache cache, CancellationToken token = default)
            {
                Debug.Print("Handler {0} is handling inbound data.", context.Name);
                return context.InboundHandleAsync(cache, token);
            }

            public override ValueTask<Result> OutboundHandleAsync(ILinkChannelHandlerContext context, ValueCache cache, CancellationToken token = default)
            {
                Debug.Print("Handler {0} is handling outbound data.", context.Name);
                return context.OutboundHandleAsync(cache, token);
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