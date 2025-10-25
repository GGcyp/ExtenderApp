using System.Buffers;
using System.Net;
using ExtenderApp.Abstract;
using ExtenderApp.Data;

namespace ExtenderApp.Common.Networks
{
    public abstract class LinkClient<TLinker> : DisposableObject, ILinkClient<TLinker>
        where TLinker : ILinker
    {
        private readonly TLinker _linker;
        public ILinkClientFormatterManager? FormatterManager { get; private set; }
        public ILinkClientPluginManager<TLinker>? PluginManager { get; private set; }

        #region ILinker 直通属性

        public virtual bool Connected => _linker.Connected;

        public virtual EndPoint? LocalEndPoint => _linker.LocalEndPoint;

        public virtual EndPoint? RemoteEndPoint => _linker.RemoteEndPoint;

        public virtual CapacityLimiter CapacityLimiter => _linker.CapacityLimiter;

        public virtual ValueCounter SendCounter => _linker.SendCounter;

        public virtual ValueCounter ReceiveCounter => _linker.ReceiveCounter;

        #endregion ILinker 直通属性

        private CancellationTokenSource? _receiveCts;
        private Task? _receiveTask;
        private readonly object _receiveLock = new();

        public LinkClient(TLinker linker)
        {
            _linker = linker;
        }

        public ValueTask<SocketOperationResult> SendAsync<T>(T data)
        {
            if (FormatterManager is null)
                throw new InvalidOperationException("转换器管理为空，不能使用泛型方法");

            var formatter = FormatterManager.GetFormatter<T>();
            if (formatter is null)
                throw new InvalidOperationException($"未找到类型 {typeof(T).FullName} 的格式化器，无法发送数据");

            var buffer = formatter.Serialize(data);
            LinkClientPluginSendMessage pluginSendData = new(buffer, formatter.DataType);
            PluginManager?.OnSend(this, ref pluginSendData);

            ByteBlock sendBlock = pluginSendData.ToBlock();
            buffer.Dispose();
            return PrivateSendAsync(sendBlock);
        }

        private async ValueTask<SocketOperationResult> PrivateSendAsync(ByteBlock sendBlock)
        {
            SocketOperationResult result = await _linker.SendAsync(sendBlock);
            sendBlock.Dispose();
            return result;
        }

        public void SetClientPluginManager(ILinkClientPluginManager<TLinker> pluginManager)
        {
            ArgumentNullException.ThrowIfNull(pluginManager, nameof(pluginManager));

            PluginManager = pluginManager;
        }

        public void SetClientFormatterManager(ILinkClientFormatterManager formatterManager)
        {
            ArgumentNullException.ThrowIfNull(formatterManager, nameof(formatterManager));

            FormatterManager = formatterManager;
        }

        private async Task LoopReceive(CancellationToken token)
        {
            byte[] bytes = ArrayPool<byte>.Shared.Rent((int)Utility.KilobytesToBytes(4));
            try
            {
                while (!token.IsCancellationRequested)
                {
                    SocketOperationResult result;
                    try
                    {
                        // ReceiveAsync 接受
                        // ResultMessage<byte> 并支持传入取消令牌
                        result = await _linker.ReceiveAsync(bytes.AsMemory(), token);
                    }
                    catch (OperationCanceledException)
                    {
                        // 被取消：正常退出接收循环
                        break;
                    }
                    catch (Exception ex)
                    {
                        // 非取消异常：通知插件并退出循环（认为链路已异常断开）
                        PluginManager?.OnDisconnected(this, ex);
                        break;
                    }

                    // 若对端优雅关闭（TCP）通常返回 0，或者实现以 0 表示已断开，退出接收循环以进入暂停状态
                    if (result.BytesTransferred <= 0)
                    {
                        break;
                    }

                    // 调用插件处理收到的数据（即使
                    // BytesTransferred 为 0，也通知一次）
                    try
                    {
                        await PrivatePluginReceiveMessage(result, bytes.AsMemory(0, result.BytesTransferred));
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

        private ValueTask PrivatePluginReceiveMessage(SocketOperationResult result, ReadOnlyMemory<byte> resultMessage)
        {
            LinkClientPluginReceiveMessage message = new(result, resultMessage);
            PluginManager?.OnReceive(this, ref message);
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
                    formatter.DeserializeAndInvoke((ByteBuffer)frame.Payload);
                }
            }
            return ValueTask.CompletedTask;
        }

        private void StartReceive()
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

        private void StopReceive()
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

        #region ILinker 直通方法

        protected virtual void Connect(EndPoint remoteEndPoint)
        {
            PluginManager?.OnConnecting(this, remoteEndPoint);
            try
            {
                _linker.Connect(remoteEndPoint);
                // 连接成功后启动接收循环
                StartReceive();
                PluginManager?.OnConnected(this, remoteEndPoint, null);
            }
            catch (Exception ex)
            {
                PluginManager?.OnConnected(this, remoteEndPoint, ex);
                throw;
            }
        }

        protected virtual async ValueTask ConnectAsync(EndPoint remoteEndPoint, CancellationToken token = default)
        {
            PluginManager?.OnConnecting(this, remoteEndPoint);

            try
            {
                await _linker.ConnectAsync(remoteEndPoint, token);
                // 连接成功后启动接收循环
                StartReceive();
                PluginManager?.OnConnected(this, remoteEndPoint, null);
            }
            catch (Exception ex)
            {
                PluginManager?.OnConnected(this, remoteEndPoint, ex);
                throw;
            }
        }

        protected virtual void Disconnect()
        {
            PluginManager?.OnDisconnecting(this);
            // 先暂停接收，再关闭底层（避免在关闭过程中继续处理数据）
            StopReceive();
            try
            {
                _linker.Disconnect();
                PluginManager?.OnDisconnected(this, null);
            }
            catch (Exception ex)
            {
                PluginManager?.OnDisconnected(this, ex);
                throw;
            }
        }

        protected virtual async ValueTask DisconnectAsync(CancellationToken token = default)
        {
            PluginManager?.OnDisconnecting(this);
            // 先暂停接收，再异步断开底层
            StopReceive();
            try
            {
                await _linker.DisconnectAsync(token);
                PluginManager?.OnDisconnected(this, null);
            }
            catch (Exception ex)
            {
                PluginManager?.OnDisconnected(this, ex);
                throw;
            }
        }

        protected virtual SocketOperationResult Send(Memory<byte> memory)
        {
            return _linker.Send(memory);
        }

        protected virtual ValueTask<SocketOperationResult> SendAsync(Memory<byte> memory, CancellationToken token = default)
        {
            return _linker.SendAsync(memory, token);
        }

        #endregion Linker 直通方法
    }
}