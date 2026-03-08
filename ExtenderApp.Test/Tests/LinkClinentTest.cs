using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
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

            TcpListener listener = new(IPAddress.Loopback, 12345);
            listener.Start();
            var acceptSocketTask = listener.AcceptSocketAsync();

            // create a raw TcpClient to send test data to the accepted server socket.
            using var tcpClient = new TcpClient();
            await tcpClient.ConnectAsync(IPAddress.Loopback, 12345);
            var socket = await acceptSocketTask;

            try
            {
                // start server-side reader
                await Start(socket);

                // send several integers from the client side to the accepted socket
                var stream = tcpClient.GetStream();
                for (int i = 0; i < 10; i++)
                {
                    var bytes = BitConverter.GetBytes(100000 + i);
                    await stream.WriteAsync(bytes, 0, bytes.Length);
                    await stream.FlushAsync();
                    await Task.Delay(50);
                }
            }
            catch (Exception ex)
            {
                throw;
            }
            finally
            {
                try { socket.Shutdown(SocketShutdown.Both); } catch { }
                try { socket.Close(); } catch { }
                try { listener.Stop(); } catch { }
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
                    var received = await socket.ReceiveAsync(buffer, SocketFlags.None);
                    if (received <= 0)
                        break;

                    if (received >= 4)
                    {
                        int val = BitConverter.ToInt32(buffer, 0);
                        Debug.Print(val.ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                throw;
            }
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