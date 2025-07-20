using System.Net.Sockets;
using ExtenderApp.Abstract;
using ExtenderApp.Common.DataBuffers;
using ExtenderApp.Common.Networks.UDP;

namespace ExtenderApp.Common.Networks
{
    /// <summary>
    /// 监听器链接工厂类
    /// </summary>
    internal class ListenerLinkerFactory : IListenerLinkerFactory
    {
        private readonly ILinkerFactory _linkerFactory;
        private readonly DataBuffer _tcpDataBuffer;
        private readonly DataBuffer _udpDataBuffer;

        public ListenerLinkerFactory(ILinkerFactory linkerFactory)
        {
            _linkerFactory = linkerFactory;
            _tcpDataBuffer = DataBuffer.CreateDataBuffer(() => new ListenerLinker<ITcpLinker>(SocketType.Stream, ProtocolType.Tcp, _linkerFactory));
            _udpDataBuffer = DataBuffer.CreateDataBuffer(() => new ListenerLinker<IUdpLinker>(SocketType.Stream, ProtocolType.Tcp, _linkerFactory));
        }

        public IListenerLinker<T> CreateListenerLinker<T>()
            where T : ILinker
        {
            return (typeof(T)) switch
            {
                Type t when t == typeof(ITcpLinker) => _tcpDataBuffer.Process<IListenerLinker<T>>()!,
                Type t when t == typeof(IUdpLinker) => _tcpDataBuffer.Process<IListenerLinker<T>>()!,
                _ => throw new NotSupportedException($"不支持的链接器类型: {typeof(T).Name}"),
            };
        }
    }
}
