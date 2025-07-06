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
        private readonly DataBuffer _tcpSDataBuffer;
        private readonly DataBuffer _udpSDataBuffer;
        private readonly DataBuffer _tcpADataBuffer;
        private readonly DataBuffer _udpADataBuffer;

        public LinkerFactory()
        {
            _tcpSDataBuffer = CreateDataBuffer<Socket, TcpLinker>(s => new TcpLinker(s));
            _udpSDataBuffer = CreateDataBuffer<Socket, UdpLinker>(s => new UdpLinker(s));
            _tcpADataBuffer = CreateDataBuffer<AddressFamily, TcpLinker>(a => new TcpLinker(a));
            _udpADataBuffer = CreateDataBuffer<AddressFamily, UdpLinker>(a => new UdpLinker(a));
        }

        private DataBuffer CreateDataBuffer<T1, T2>(Func<T1, T2> func)
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
                var t when t == typeof(ITcpLinker) => _tcpSDataBuffer.Process<Socket, T>(socket)!,
                var t when t == typeof(IUdpLinker) => _udpSDataBuffer.Process<Socket, T>(socket)!,
                _ => throw new System.NotImplementedException()
            };
        }

        public T CreateLinker<T>(AddressFamily addressFamily) where T : ILinker
        {
            return typeof(T) switch
            {
                var t when t == typeof(ITcpLinker) => _tcpADataBuffer.Process<AddressFamily, T>(addressFamily)!,
                var t when t == typeof(IUdpLinker) => _udpADataBuffer.Process<AddressFamily, T>(addressFamily)!,
                _ => throw new System.NotImplementedException()
            };
        }
    }
}
