using System.Net;
using ExtenderApp.Abstract;
using ExtenderApp.Data;

namespace ExtenderApp.Common.Networks
{
    public class LinkClient : DisposableObject, IClient
    {
        private readonly ILinker _linker;
        public Exception? Erorr { get; protected set; }
        public IClientFormatterManager? FormatterManager { get; private set; }
        public IClientPluginManager? PluginManager { get; private set; }

        #region ILinker 直通属性

        public virtual bool Connected => _linker.Connected;

        public virtual EndPoint? LocalEndPoint => _linker.LocalEndPoint;

        public virtual EndPoint? RemoteEndPoint => _linker.RemoteEndPoint;

        public virtual CapacityLimiter CapacityLimiter => _linker.CapacityLimiter;

        public virtual ValueCounter SendCounter => _linker.SendCounter;

        public virtual ValueCounter ReceiveCounter => _linker.ReceiveCounter;

        #endregion ILinker 直通属性

        public LinkClient(ILinker linker)
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
            return _linker.SendAsync(buffer);
        }

        public void SetClientPluginManager(IClientPluginManager pluginManager)
        {
            ArgumentNullException.ThrowIfNull(pluginManager, nameof(pluginManager));

            PluginManager = pluginManager;
        }

        public void SetClientFormatterManager(IClientFormatterManager formatterManager)
        {
            ArgumentNullException.ThrowIfNull(formatterManager, nameof(formatterManager));

            FormatterManager = formatterManager;
        }

        #region Linker 直通方法

        public virtual void Connect(EndPoint remoteEndPoint)
        {
            _linker.Connect(remoteEndPoint);
        }

        public virtual ValueTask ConnectAsync(EndPoint remoteEndPoint, CancellationToken token = default)
        {
            return _linker.ConnectAsync(remoteEndPoint, token);
        }

        public virtual void Disconnect()
        {
            _linker.Disconnect();
        }

        public virtual ValueTask DisconnectAsync(CancellationToken token = default)
        {
            return _linker.DisconnectAsync(token);
        }

        public virtual SocketOperationResult Receive(Memory<byte> memory)
        {
            return _linker.Receive(memory);
        }

        public virtual ValueTask<SocketOperationResult> ReceiveAsync(Memory<byte> memory, CancellationToken token = default)
        {
            return _linker.ReceiveAsync(memory, token);
        }

        public virtual SocketOperationResult Send(Memory<byte> memory)
        {
            return _linker.Send(memory);
        }

        public virtual ValueTask<SocketOperationResult> SendAsync(Memory<byte> memory, CancellationToken token = default)
        {
            return _linker.SendAsync(memory, token);
        }

        #endregion Linker 直通方法
    }
}