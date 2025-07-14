using System.Net.Sockets;
using ExtenderApp.Abstract;
using ExtenderApp.Common.DataBuffers;
using ExtenderApp.Common.Networks.UDP;

namespace ExtenderApp.Common.Networks
{
    /// <summary>
    /// LinkerFactory 类是一个内部类，实现了 ILinkerFactory 接口。
    /// 用于创建不同类型的 Linker 对象。
    /// </summary>
    internal class LinkerFactory : ILinkerFactory
    {
        private readonly ResourceLimiter _resourceLimiter;
        private readonly DataBuffer _tcpSDataBuffer;
        private readonly DataBuffer _udpSDataBuffer;
        private readonly DataBuffer _tcpADataBuffer;
        private readonly DataBuffer _udpADataBuffer;

        public LinkerFactory(ResourceLimiter resourceLimiter)
        {
            _resourceLimiter = resourceLimiter;
            _tcpSDataBuffer = CreateDataBuffer<Socket, ResourceLimiter, TcpLinker>((s, r) => new TcpLinker(s, r));
            _udpSDataBuffer = CreateDataBuffer<Socket, ResourceLimiter, UdpLinker>((s, r) => new UdpLinker(s, r));
            _tcpADataBuffer = CreateDataBuffer<AddressFamily, ResourceLimiter, TcpLinker>((a, r) => new TcpLinker(a, r));
            _udpADataBuffer = CreateDataBuffer<AddressFamily, ResourceLimiter, UdpLinker>((a, r) => new UdpLinker(a, r));
        }

        private DataBuffer CreateDataBuffer<T1, T2, T3>(Func<T1, T2, T3> func)
        {
            var result = DataBuffer.GetDataBuffer();
            result.SetProcessFunc(func);
            return result;
        }

        public T CreateLinker<T>() where T : ILinker
        {
            return CreateLinker<T>(AddressFamily.InterNetwork);
        }

        public T CreateLinker<T>(Socket socket) where T : ILinker
        {
            return typeof(T) switch
            {
                var t when t == typeof(ITcpLinker) => _tcpSDataBuffer.Process<Socket, ResourceLimiter, T>(socket, _resourceLimiter)!,
                var t when t == typeof(IUdpLinker) => _udpSDataBuffer.Process<Socket, ResourceLimiter, T>(socket, _resourceLimiter)!,
                _ => throw new System.NotImplementedException()
            };
        }

        public T CreateLinker<T>(AddressFamily addressFamily) where T : ILinker
        {
            return typeof(T) switch
            {
                var t when t == typeof(ITcpLinker) => _tcpADataBuffer.Process<AddressFamily, ResourceLimiter, T>(addressFamily, _resourceLimiter)!,
                var t when t == typeof(IUdpLinker) => _udpADataBuffer.Process<AddressFamily, ResourceLimiter, T>(addressFamily, _resourceLimiter)!,
                _ => throw new System.NotImplementedException()
            };
        }
    }
}
