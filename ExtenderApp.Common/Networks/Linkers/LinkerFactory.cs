using System.Net.Sockets;
using ExtenderApp.Abstract;

namespace ExtenderApp.Common.Networks
{
    /// <summary>
    /// <see cref="ILinker"/> 工厂抽象类
    /// </summary>
    /// <typeparam name="T">链接接口继承自<see cref="ILinker"/></typeparam>
    public abstract class LinkerFactory<T> : ILinkerFactory<T>
        where T : ILinker
    {
        public abstract SocketType LinkerSocketType { get; }
        public abstract ProtocolType LinkerProtocolType { get; }

        public T CreateLinker()
        {
            return CreateLinker(AddressFamily.InterNetwork);
        }

        public T CreateLinker(Socket socket)
        {
            if (socket == null)
            {
                throw new ArgumentNullException(nameof(socket));
            }

            if (socket.SocketType != LinkerSocketType || socket.ProtocolType != LinkerProtocolType)
            {
                throw new ArgumentException(string.Format("传入的套字节类型和协议类型需要一致:{0}", typeof(T).Name), nameof(socket));
            }

            return CreateLinkerInternal(socket);
        }

        public T CreateLinker(AddressFamily addressFamily)
        {
            return CreateLinker(new Socket(addressFamily, LinkerSocketType, LinkerProtocolType));
        }

        protected abstract T CreateLinkerInternal(Socket socket);
    }
}
