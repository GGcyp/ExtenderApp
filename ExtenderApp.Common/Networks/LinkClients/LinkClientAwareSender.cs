using System.Buffers;
using System.Net;
using ExtenderApp.Abstract;
using ExtenderApp.Data;

namespace ExtenderApp.Common.Networks.LinkClients
{
    /// <summary>
    /// 支持插件与格式化器的链接客户端发送器基类。
    /// </summary>
    /// <typeparam name="TLinker">指定类型连接器</typeparam>
    public abstract class LinkClientAwareSender<TLinkClient, TLinker> : LinkClient<TLinker>, ILinkClientAwareSender<TLinkClient>
        where TLinkClient : ILinkClientAwareSender<TLinkClient>
        where TLinker : ILinker
    {
        /// <summary>
        /// 保护对接收相关状态的并发访问锁：用于在
        /// StartReceive/StopReceive 中同步检查与切换 <see
        /// cref="_receiveCts"/> 与 <see
        /// cref="_receiveTask"/>。 注意：StopReceive 中可能在锁外等待任务完成或在持有锁的情况下启动取消，但应尽量保持锁粒度短以避免阻塞接收路径。
        /// </summary>
        private readonly object _receiveLock = new();

        /// <summary>
        /// 当前客户端自身的引用，便于在基类中传递给插件管理器等使用。
        /// </summary>
        public readonly TLinkClient _thisClient;

        /// <summary>
        /// 用于控制接收循环的取消令牌源。 在 StartReceive 创建，在
        /// StopReceive 取消并释放；为 null 表示接收循环未在运行或已被释放。
        /// </summary>
        private CancellationTokenSource? _receiveCts;

        /// <summary>
        /// 表示当前正在运行的接收循环任务（由 StartReceive 启动）。
        /// 可能为 null（未启动或已完成/已释放）。对其检查/赋值需在 <see
        /// cref="_receiveLock"/> 保护下进行以避免竞态。
        /// </summary>
        private Task? _receiveTask;

        public ILinkClientFramer? Framer { get; protected set; }
        public ILinkClientFormatterManager? FormatterManager { get; protected set; }
        public ILinkClientPluginManager<TLinkClient>? PluginManager { get; protected set; }

        public LinkClientAwareSender(TLinker linker) : base(linker)
        {
            _thisClient = (TLinkClient)(ILinkClientAwareSender<TLinkClient>)this;
            if (Connected)
            {
                StartReceive();
            }
        }

        public void SetClientPluginManager(ILinkClientPluginManager<TLinkClient> pluginManager)
        {
            ThrowIfDisposed();
            ArgumentNullException.ThrowIfNull(pluginManager, nameof(pluginManager));

            PluginManager = pluginManager;
        }

        public void SetClientFormatterManager(ILinkClientFormatterManager formatterManager)
        {
            ThrowIfDisposed();
            ArgumentNullException.ThrowIfNull(formatterManager, nameof(formatterManager));

            FormatterManager = formatterManager;
        }

        public void SetClientFramer(ILinkClientFramer framer)
        {
            ThrowIfDisposed();
            ArgumentNullException.ThrowIfNull(framer, nameof(framer));
            Framer = framer;
        }

        #region Send And Receive

        public virtual ValueTask<Result<SocketOperationValue>> SendAsync<T>(T data, CancellationToken token = default)
        {
            ThrowIfDisposed();

            var sendBuffer = ValueToByteBuffer(data);

            return ProtectedSendAsync(sendBuffer.UnreadSequence, sendBuffer.Rental, token);
        }

        /// <summary>
        /// 将指定类型转换为可发送的<see cref="ByteBuffer"/>。
        /// </summary>
        /// <typeparam name="T">指定类型<see cref="{T}"/></typeparam>
        /// <param name="data">指定类型数据</param>
        /// <returns>返回装完数据的<see cref="ByteBuffer"/></returns>
        /// <exception cref="InvalidOperationException">当<see cref="FormatterManager"/>为空或找不到指定<see cref="IClientFormatter{T}"/>时候弹出</exception>
        protected ByteBuffer ValueToByteBuffer<T>(T data)
        {
            if (FormatterManager is null)
                throw new InvalidOperationException("转换器管理为空，不能使用泛型方法");

            var formatter = FormatterManager.GetFormatter<T>();
            if (formatter is null)
                throw new InvalidOperationException($"未找到类型 {typeof(T).FullName} 的格式化器，无法发送数据");

            var buffer = formatter.Serialize(data);
            LinkClientPluginSendMessage pluginSendData = new(buffer, formatter.MessageType);
            PluginManager?.OnSend(_thisClient, ref pluginSendData);

            ByteBuffer sendBuffer = default;
            if (Framer != null)
            {
                Framer.Encode(pluginSendData.MessageType, (int)pluginSendData.ResultOutMessageBuffer.Length, out var framedMessage);

                sendBuffer = ByteBuffer.CreateBuffer();
                sendBuffer.Write(framedMessage);
                sendBuffer.Write(pluginSendData.ResultOutMessageBuffer);

                framedMessage.Dispose();
                pluginSendData.Dispose();
            }
            else
            {
                if (pluginSendData.OutMessageBuffer.Remaining > 0)
                {
                    sendBuffer = pluginSendData.OutMessageBuffer;
                    pluginSendData.OriginalMessageBuffer.Dispose();
                }
                else
                {
                    sendBuffer = pluginSendData.OriginalMessageBuffer;
                    pluginSendData.OutMessageBuffer.Dispose();
                }
            }

            return sendBuffer;
        }

        /// <summary>
        /// 受保护的发送方法，负责调用底层链接器发送数据并释放租借的序列资源。
        /// </summary>
        /// <param name="memories">要发送的序列</param>
        /// <param name="rental">要发送序列的租约</param>
        /// <param name="token">可取消令牌</param>
        /// <returns></returns>
        protected virtual async ValueTask<Result<SocketOperationValue>> ProtectedSendAsync(ReadOnlySequence<byte> memories, SequencePool<byte>.SequenceRental rental, CancellationToken token)
        {
            var result = await Linker.SendAsync(memories, token);
            rental.Dispose();
            return result;
        }

        private async Task LoopReceive(CancellationToken token)
        {
            byte[] bytes = ArrayPool<byte>.Shared.Rent((int)Utility.KilobytesToBytes(4));
            try
            {
                while (!token.IsCancellationRequested)
                {
                    Result<SocketOperationValue> result;
                    try
                    {
                        // ReceiveAsync 接受
                        // ResultMessage<byte> 并支持传入取消令牌
                        result = await Linker.ReceiveAsync(bytes.AsMemory(), token);
                    }
                    catch (OperationCanceledException)
                    {
                        // 被取消：正常退出接收循环
                        break;
                    }
                    catch (Exception ex)
                    {
                        // 非取消异常：通知插件并退出循环（认为链路已异常断开）
                        PluginManager?.OnDisconnected(_thisClient, ex);
                        break;
                    }

                    var value = result.Value;
                    // 若对端优雅关闭（TCP）通常返回 0，或者实现以 0 表示已断开，退出接收循环以进入暂停状态
                    if (value.BytesTransferred <= 0)
                    {
                        break;
                    }

                    // 调用插件处理收到的数据（即使
                    // BytesTransferred 为 0，也通知一次）
                    try
                    {
                        await PrivatePluginReceiveMessage(result, bytes.AsMemory(0, value.BytesTransferred));
                    }
                    catch
                    {
                        // 插件异常不应导致接收循环崩溃，记录/吞掉（具体日志可由实现添加）
                    }
                }
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(bytes);
            }
        }

        private ValueTask PrivatePluginReceiveMessage(Result<SocketOperationValue> result, ReadOnlyMemory<byte> resultMessage)
        {
            ByteBuffer buffer = new(resultMessage);
            LinkClientPluginReceiveMessage message = default;
            if (Framer != null)
            {
                Framer.Decode(ref buffer, out var framedList);
                // 未能解析出任何帧，直接返回
                if (framedList.IsEmpty)
                    return ValueTask.CompletedTask;

                message = new(result, framedList);
            }
            else
            {
                message = new(result, default);
            }

            PluginManager?.OnReceive(_thisClient, ref message);
            if (FormatterManager != null && !message.OutMessageFrames.IsEmpty)
            {
                for (int i = 0; i < message.OutMessageFrames.Count; i++)
                {
                    var frame = message.OutMessageFrames[i];
                    var formatter = FormatterManager.GetFormatter(frame.MessageType);
                    if (formatter is null)
                    {
                        // 未找到对应格式化器，跳过
                        continue;
                    }
                    ByteBuffer dataBuffer = new(frame.Payload);
                    formatter.DeserializeAndInvoke(ref dataBuffer);
                }
            }
            message.Dispose();
            return ValueTask.CompletedTask;
        }

        protected void StartReceive()
        {
            lock (_receiveLock)
            {
                if (_receiveTask != null && !_receiveTask.IsCompleted)
                {
                    // 已在运行
                    return;
                }

                _receiveCts = new CancellationTokenSource();
                _receiveTask = Task.Run(() => LoopReceive(_receiveCts.Token));
            }
        }

        protected void StopReceive()
        {
            lock (_receiveLock)
            {
                if (_receiveCts == null)
                    return;

                try
                {
                    _receiveCts.Cancel();
                }
                catch { }

                try
                {
                    _receiveTask?.Wait(); // 可阻塞等待接收任务退出
                }
                catch (AggregateException) { }
                finally
                {
                    _receiveTask = null;
                    _receiveCts.Dispose();
                    _receiveCts = null;
                }
            }
        }

        #endregion Send And Receive

        #region ILinker 直通方法

        public virtual new void Connect(EndPoint remoteEndPoint)
        {
            ThrowIfDisposed();
            PluginManager?.OnConnecting(_thisClient, remoteEndPoint);
            try
            {
                Linker.Connect(remoteEndPoint);
                // 连接成功后启动接收循环
                StartReceive();
                PluginManager?.OnConnected(_thisClient, remoteEndPoint, null);
            }
            catch (Exception ex)
            {
                PluginManager?.OnConnected(_thisClient, remoteEndPoint, ex);
                throw;
            }
        }

        public virtual async new ValueTask ConnectAsync(EndPoint remoteEndPoint, CancellationToken token = default)
        {
            ThrowIfDisposed();
            PluginManager?.OnConnecting(_thisClient, remoteEndPoint);

            try
            {
                await Linker.ConnectAsync(remoteEndPoint, token);
                // 连接成功后启动接收循环
                StartReceive();
                PluginManager?.OnConnected(_thisClient, remoteEndPoint, null);
            }
            catch (Exception ex)
            {
                PluginManager?.OnConnected(_thisClient, remoteEndPoint, ex);
                throw;
            }
        }

        public virtual new void Disconnect()
        {
            ThrowIfDisposed();
            PluginManager?.OnDisconnecting(_thisClient);
            // 先暂停接收，再关闭底层（避免在关闭过程中继续处理数据）
            StopReceive();
            try
            {
                Linker.Disconnect();
                PluginManager?.OnDisconnected(_thisClient, null);
            }
            catch (Exception ex)
            {
                PluginManager?.OnDisconnected(_thisClient, ex);
                throw;
            }
        }

        public virtual async new ValueTask DisconnectAsync(CancellationToken token = default)
        {
            ThrowIfDisposed();
            PluginManager?.OnDisconnecting(_thisClient);
            // 先暂停接收，再异步断开底层
            StopReceive();
            try
            {
                await Linker.DisconnectAsync(token);
                PluginManager?.OnDisconnected(_thisClient, null);
            }
            catch (Exception ex)
            {
                PluginManager?.OnDisconnected(_thisClient, ex);
                throw;
            }
        }

        public virtual new Result<SocketOperationValue> Send(Memory<byte> memory)
        {
            return Linker.Send(memory);
        }

        public virtual new ValueTask<Result<SocketOperationValue>> SendAsync(Memory<byte> memory, CancellationToken token = default)
        {
            return Linker.SendAsync(memory, token);
        }

        #endregion ILinker 直通方法
    }
}