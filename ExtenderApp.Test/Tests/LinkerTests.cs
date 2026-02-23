using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using ExtenderApp.Abstract;
using ExtenderApp.Buffer;
using ExtenderApp.Common;
using ExtenderApp.Contracts;

namespace ExtenderApp.Test.Tests
{
    /// <summary>
    /// <see cref="ILinker"/> 的基础测试用例集合（基于本地回环 TCP）。
    /// </summary>
    internal static class LinkerTests
    {
        /// <summary>
        /// 运行所有链接器测试用例。
        /// </summary>
        /// <param name="linkerFactory">用于创建 TCP 链接器的工厂。</param>
        /// <param name="binarySerialization">二进制序列化器。</param>
        public static void RunAll(ILinkerFactory<ITcpLinker> linkerFactory, IBinarySerialization binarySerialization)
        {
            ArgumentNullException.ThrowIfNull(linkerFactory, nameof(linkerFactory));
            ArgumentNullException.ThrowIfNull(binarySerialization, nameof(binarySerialization));

            Debug.Print("-- Linker 测试开始 --");
            TestTcpSerializedRoundTrip(linkerFactory, binarySerialization);
            TestTcpSerializedRoundTripAsync(linkerFactory, binarySerialization).GetAwaiter().GetResult();
            TestTcpSendRate(linkerFactory, binarySerialization);
            Debug.Print("-- Linker 测试结束 --");
        }

        private static void TestTcpSerializedRoundTrip(ILinkerFactory<ITcpLinker> linkerFactory, IBinarySerialization binarySerialization)
        {
            var payload = $"linker-sync:{DateTimeOffset.UtcNow:O}";

            using var scope = new TcpTestScope(linkerFactory);
            binarySerialization.Serialize(payload, out AbstractBuffer<byte> buffer);
            try
            {
                var sendResult = scope.Client.Send(buffer);
                if (!sendResult)
                {
                    Debug.Print($"[失败] TestTcpSerializedRoundTrip Send: {sendResult.Exception}");
                    return;
                }

                var receiveBuffer = new byte[buffer.Committed];
                ReceiveExactly(scope.Server, receiveBuffer);

                var actual = binarySerialization.Deserialize<string>(receiveBuffer.AsSpan());
                var same = string.Equals(payload, actual, StringComparison.Ordinal);
                Debug.Print(same ? "[通过] TestTcpSerializedRoundTrip" : "[失败] TestTcpSerializedRoundTrip 数据不一致");
            }
            catch (Exception ex)
            {
                Debug.Print($"[异常] TestTcpSerializedRoundTrip: {ex}");
            }
            finally
            {
                if (!buffer.TryRelease())
                {
                    try { buffer.UnfreezeWrite(); } catch { }
                    try { buffer.Unfreeze(); } catch { }
                    buffer.TryRelease();
                }
            }
        }

        private static async ValueTask TestTcpSerializedRoundTripAsync(ILinkerFactory<ITcpLinker> linkerFactory, IBinarySerialization binarySerialization)
        {
            var payload = $"linker-async:{DateTimeOffset.UtcNow:O}";

            using var scope = new TcpTestScope(linkerFactory);
            binarySerialization.Serialize(payload, out AbstractBuffer<byte> buffer);
            try
            {
                var sendResult = await scope.Client.SendAsync(buffer, token: CancellationToken.None).ConfigureAwait(false);
                if (!sendResult)
                {
                    Debug.Print($"[失败] TestTcpSerializedRoundTripAsync Send: {sendResult.Exception}");
                    return;
                }

                var receiveBuffer = new byte[buffer.Committed];
                await ReceiveExactlyAsync(scope.Server, receiveBuffer).ConfigureAwait(false);

                var actual = binarySerialization.Deserialize<string>(receiveBuffer.AsSpan());
                var same = string.Equals(payload, actual, StringComparison.Ordinal);
                Debug.Print(same ? "[通过] TestTcpSerializedRoundTripAsync" : "[失败] TestTcpSerializedRoundTripAsync 数据不一致");
            }
            catch (Exception ex)
            {
                Debug.Print($"[异常] TestTcpSerializedRoundTripAsync: {ex}");
            }
            finally
            {
                if (!buffer.TryRelease())
                {
                    try { buffer.UnfreezeWrite(); } catch { }
                    try { buffer.Unfreeze(); } catch { }
                    buffer.TryRelease();
                }
            }
        }

        private static void TestTcpSendRate(ILinkerFactory<ITcpLinker> linkerFactory, IBinarySerialization binarySerialization)
        {
            var payload = new byte[16 * 1024];
            Random.Shared.NextBytes(payload);

            binarySerialization.Serialize(payload, out AbstractBuffer<byte> buffer);
            try
            {
                using var scope = new TcpTestScope(linkerFactory);
                var iterations = 100;
                var totalBytes = buffer.Committed * iterations;
                var receiveBuffer = new byte[buffer.Committed];

                var stopwatch = Stopwatch.StartNew();
                for (int i = 0; i < iterations; i++)
                {
                    //Debug.Print($"[过程] TestTcpSendRate 发送第 {i + 1}/{iterations} 次");
                    var sendResult = scope.Client.Send(buffer);
                    if (!sendResult)
                    {
                        Debug.Print($"[失败] TestTcpSendRate Send: {sendResult.Exception}");
                        return;
                    }

                    ReceiveExactly(scope.Server, receiveBuffer);
                    //Debug.Print($"[过程] TestTcpSendRate 接收第 {i + 1}/{iterations} 次");
                }
                stopwatch.Stop();

                var rate = totalBytes / Math.Max(stopwatch.Elapsed.TotalSeconds, 0.001);
                Debug.Print($"[指标] TestTcpSendRate 总字节={totalBytes} 总大小{Utility.BytesToMegabytes(totalBytes)} 用时={stopwatch.Elapsed.TotalMilliseconds:F0}ms 速率={rate / (1024 * 1024):F2}MB/s");
            }
            catch (Exception ex)
            {
                Debug.Print($"[异常] TestTcpSendRate: {ex}");
            }
            finally
            {
                if (!buffer.TryRelease())
                {
                    try { buffer.UnfreezeWrite(); } catch { }
                    try { buffer.Unfreeze(); } catch { }
                    buffer.TryRelease();
                }
            }
        }

        private static void ReceiveExactly(ILinker linker, Span<byte> destination)
        {
            var remaining = destination;
            while (!remaining.IsEmpty)
            {
                var result = linker.Receive(remaining);
                if (!result)
                    throw result.Exception ?? new InvalidOperationException("接收失败。");

                var transferred = result.Value.BytesTransferred;
                if (transferred <= 0)
                    throw new InvalidOperationException("接收字节数为 0。");

                remaining = remaining.Slice(transferred);
            }
        }

        private static async ValueTask ReceiveExactlyAsync(ILinker linker, Memory<byte> destination)
        {
            var remaining = destination;
            while (!remaining.IsEmpty)
            {
                var result = await linker.ReceiveAsync(remaining, LinkFlags.None).ConfigureAwait(false);
                if (!result)
                    throw result.Exception ?? new InvalidOperationException("接收失败。");

                var transferred = result.Value.BytesTransferred;
                if (transferred <= 0)
                    throw new InvalidOperationException("接收字节数为 0。");

                remaining = remaining.Slice(transferred);
            }
        }

        private sealed class TcpTestScope : IDisposable
        {
            public TcpTestScope(ILinkerFactory<ITcpLinker> linkerFactory)
            {
                Listener = new TcpListener(IPAddress.Loopback, 0);
                Listener.Start();

                var endpoint = (IPEndPoint)Listener.LocalEndpoint;
                AcceptTask = Listener.AcceptSocketAsync();

                Client = linkerFactory.CreateLinker();
                Client.Connect(endpoint);

                var serverSocket = AcceptTask.GetAwaiter().GetResult();
                Server = linkerFactory.CreateLinker(serverSocket);
            }

            public ITcpLinker Client { get; }
            public ITcpLinker Server { get; }
            private TcpListener Listener { get; }
            private Task<Socket> AcceptTask { get; }

            public void Dispose()
            {
                try { Client.Disconnect(); } catch { }
                try { Server.Disconnect(); } catch { }
                Client.Dispose();
                Server.Dispose();
                Listener.Stop();
            }
        }
    }
}