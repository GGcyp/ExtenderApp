using System.Net;
using System.Net.Sockets;
using ExtenderApp.Abstract;

namespace ExtenderApp.Common.Networks
{
    public abstract class ListenerLinker
    {
        private readonly Socket _listenerSocket;
        private readonly AsyncCallback _acceptCallback;
        private int _isAccepting;

        public EndPoint ListenerPoint => _listenerSocket.LocalEndPoint;

        public ListenerLinker(AddressFamily addressFamily, SocketType socketType, ProtocolType protocolType)
        {
            _listenerSocket = new Socket(addressFamily, socketType, protocolType);
            _acceptCallback = new AsyncCallback(AcceptCallback);
        }

        #region Bind

        public void Bind(IPAddress address, int port)
        {
            Bind(new IPEndPoint(address, port));
        }

        public void Bind(IPEndPoint iPEndPoint)
        {
            if (_listenerSocket.IsBound)
                throw new Exception("无法重新绑定本地接口");

            _listenerSocket.Bind(iPEndPoint);
        }

        #endregion

        public void Listen(int backlog = 10)
        {
            ThrowNotBound();

            _listenerSocket.Listen(backlog);
        }

        public ILinker Accept()
        {
            ThrowNotBound();

            if (Interlocked.CompareExchange(ref _isAccepting, 1, 0) != 0)
            {
                throw new Exception("已经在接受连接请求");
            }

            var clientSocket = _listenerSocket.Accept();
            return CreateLinker(clientSocket);
        }

        public void BeginAccept(Action<ILinker> callback)
        {
            ThrowNotBound();

            if (Interlocked.CompareExchange(ref _isAccepting, 1, 0) != 0)
            {
                throw new Exception("已经在接受连接请求");
            }
            _listenerSocket.BeginAccept(_acceptCallback, callback);
        }

        private void AcceptCallback(IAsyncResult ar)
        {
            // 获取监听Socket
            Action<ILinker> callback = (Action<ILinker>)ar.AsyncState!;

            // 结束接受连接请求并获取新的Socket
            Socket handler = _listenerSocket.EndAccept(ar);
            callback?.Invoke(CreateLinker(handler));

            // 继续监听下一个连接请求
            _listenerSocket.BeginAccept(_acceptCallback, callback);
        }

        private void ThrowNotBound()
        {
            if (!_listenerSocket.IsBound)
                throw new Exception("还未绑定本地接口");
        }

        protected abstract ILinker CreateLinker(Socket clientSocket);
    }
}
