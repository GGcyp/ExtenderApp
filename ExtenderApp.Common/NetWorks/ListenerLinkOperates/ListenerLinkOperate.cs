using System.Net;
using System.Net.Sockets;


namespace ExtenderApp.Common.NetWorks.TCP
{
    public abstract class ListenerLinkOperate<T>
    {
        private readonly Socket _socket;

        public EndPoint IPEndPoint => _socket.LocalEndPoint;
        private readonly AsyncCallback _acceptCallback;

        public ListenerLinkOperate(AddressFamily addressFamily, SocketType socketType, ProtocolType protocolType)
        {
            _socket = new Socket(addressFamily, socketType, protocolType);
            _acceptCallback = new AsyncCallback(AcceptCallback);
        }

        #region Bind

        public void Bind(IPAddress address, int port)
        {
            Bind(new IPEndPoint(address, port));
        }

        public void Bind(IPEndPoint iPEndPoint)
        {
            _socket.Bind(iPEndPoint);
        }

        #endregion

        public void Listen(int backlog = 10)
        {
            _socket.Listen(backlog);
        }

        public T Accept()
        {
            var clientSocket = _socket.Accept();
            return CreateOperate(clientSocket);
        }

        public void BeginAccept(Action<T> callback)
        {
            _socket.BeginAccept(_acceptCallback, callback);
        }

        private void AcceptCallback(IAsyncResult ar)
        {
            // 获取监听Socket
            Action<T> callback = (Action<T>)ar.AsyncState!;

            // 结束接受连接请求并获取新的Socket
            Socket handler = _socket.EndAccept(ar);
            callback?.Invoke(CreateOperate(handler));

            // 继续监听下一个连接请求
            _socket.BeginAccept(_acceptCallback, callback);
        }

        protected abstract T CreateOperate(Socket clientSocket);
    }
}
