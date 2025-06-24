using System.Net.Sockets;
using ExtenderApp.Abstract;
using ExtenderApp.Common.DataBuffers;

namespace ExtenderApp.Common.Networks
{
    /// <summary>
    /// LinkerFactory 类是一个内部类，实现了 ILinkerFactory 接口。
    /// 用于创建不同类型的 Linker 对象。
    /// </summary>
    internal class LinkerFactory : ILinkerFactory
    {
        private readonly DataBuffer _tcpBuffer;

        public LinkerFactory()
        {
            _tcpBuffer = CreateDataBuffer<ITcpLinker>(s => new TcpLinker(s));
        }

        private DataBuffer CreateDataBuffer<T>(Func<Socket, T> func)
        {
            var result = DataBuffer.GetDataBuffer();
            result.SetProcessFunc(func);
            return result;
        }

        public T CreateLinker<T>() where T : ILinker
        {
            return CreateLinker<T>(null);
        }

        public T CreateLinker<T>(Socket? socket) where T : ILinker
        {
            return typeof(T) switch
            {
                var t when t == typeof(ITcpLinker) => _tcpBuffer.Process<Socket, T>(socket)!,
                //var t when t == typeof(UdpLinker) => new UdpLinker(_fileParserStore) as T,
                _ => throw new System.NotImplementedException()
            };
        }
    }
}
