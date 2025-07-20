using System.Net;
using System.Net.Sockets;
using ExtenderApp.Abstract;

namespace ExtenderApp.Common.Networks
{
    /// <summary>
    /// 一个抽象的监听器连接器类，实现了 <see cref="IListenerLinker{T}"/> 接口。
    /// </summary>
    /// <typeparam name="T">泛型参数，表示连接器类型，必须实现 <see cref="ILinker"/> 接口。</typeparam>
    public class ListenerLinker<T> : DisposableObject, IListenerLinker<T>
        where T : ILinker
    {
        /// <summary>
        /// 异步回调方法，用于处理连接请求。
        /// </summary>
        private readonly AsyncCallback _acceptCallback;

        /// <summary>
        /// 私有只读属性，表示链接器工厂。
        /// </summary>
        private readonly ILinkerFactory _linkerFactory;

        /// <summary>
        /// Socket类型
        /// </summary>
        private readonly SocketType _socketType;

        /// <summary>
        /// 协议类型
        /// </summary>
        private readonly ProtocolType _protocolType;

        /// <summary>
        /// 表示当前是否正在接受连接的标志。
        /// </summary>
        private int _isAccepting;

        /// <summary>
        /// 监听套接字。
        /// </summary>
        private Socket? listenerSocket;

        /// <summary>
        /// 获取监听点的端点信息。
        /// </summary>
        public EndPoint? ListenerPoint => listenerSocket?.LocalEndPoint;

        /// <summary>
        /// 初始化 <see cref="ListenerLinker{T}"/> 类的新实例。
        /// </summary>
        /// <param name="addressFamily">地址族。</param>
        /// <param name="socketType">套接字类型。</param>
        /// <param name="protocolType">协议类型。</param>
        public ListenerLinker(SocketType socketType, ProtocolType protocolType, ILinkerFactory linkerFactory)
        {
            _linkerFactory = linkerFactory;
            _socketType = socketType;
            _protocolType = protocolType;
            _acceptCallback = new AsyncCallback(AcceptCallback);
        }

        public void InitInterNetwork()
        {
            Init(AddressFamily.InterNetwork);
        }

        public void Init(AddressFamily addressFamily)
        {
            if (listenerSocket == null)
            {
                lock (this)
                {
                    listenerSocket = new Socket(addressFamily, _socketType, _protocolType);
                }
            }
            else if (listenerSocket.AddressFamily != addressFamily)
            {
                lock (this)
                {
                    listenerSocket.Close();
                    listenerSocket.Dispose();
                    listenerSocket = null;
                    listenerSocket = new Socket(addressFamily, _socketType, _protocolType);
                }
            }
        }

        #region Bind

        /// <summary>
        /// 绑定到指定的IP地址和端口。
        /// </summary>
        /// <param name="address">要绑定的IP地址</param>
        /// <param name="port">要绑定的端口号</param>
        public void Bind(IPAddress address, int port)
        {
            Bind(new IPEndPoint(address, port));
        }

        /// <summary>
        /// 绑定到指定的端点。
        /// </summary>
        /// <param name="iPEndPoint">要绑定的端点</param>
        /// <exception cref="Exception">如果套接字已经绑定，则抛出异常</exception>
        public void Bind(IPEndPoint iPEndPoint)
        {
            ThrowNotInitSocket();
            if (listenerSocket.IsBound)
                throw new Exception("套接字已经绑定");

            listenerSocket.Bind(iPEndPoint);
        }

        #endregion

        #region Accept

        /// <summary>
        /// 接受一个传入连接，并返回一个新的连接实例。
        /// </summary>
        /// <returns>返回一个新的连接实例</returns>
        /// <exception cref="Exception">如果已经在接受连接请求或套接字未绑定则抛出异常</exception>
        public T Accept()
        {
            ThrowNotInitSocket();
            ThrowNotBound();

            if (Interlocked.CompareExchange(ref _isAccepting, 1, 0) != 0)
            {
                throw new InvalidOperationException("已经在接受连接请求");
            }

            var clientSocket = listenerSocket.Accept();
            return _linkerFactory.CreateLinker<T>(clientSocket);
        }

        /// <summary>
        /// 开始异步接受一个传入连接，并在连接被接受时调用回调函数。
        /// </summary>
        /// <param name="callback">连接被接受时要调用的回调函数</param>
        /// <exception cref="Exception">如果已经在接受连接请求或套接字未绑定则抛出异常</exception>
        public void BeginAccept(Action<T> callback)
        {
            ThrowNotInitSocket();
            ThrowNotBound();

            if (Interlocked.CompareExchange(ref _isAccepting, 1, 0) != 0)
            {
                throw new Exception("已经开始接受连接请求");
            }
            listenerSocket.BeginAccept(_acceptCallback, callback);
        }

        /// <summary>
        /// 接受连接请求的回调函数。
        /// </summary>
        /// <param name="ar">异步操作的结果</param>
        private void AcceptCallback(IAsyncResult ar)
        {
            // 获取监听Socket
            Action<T> callback = (Action<T>)ar.AsyncState!;

            // 结束接受连接请求并获取新的Socket
            Socket handler = listenerSocket.EndAccept(ar);
            var linker = _linkerFactory.CreateLinker<T>(handler);
            callback?.Invoke(linker);

            // 继续监听下一个连接请求
            listenerSocket.BeginAccept(_acceptCallback, callback);
        }

        #endregion

        /// <summary>
        /// 开始监听传入连接。
        /// </summary>
        /// <param name="backlog">等待连接队列的最大长度，默认值为10</param>
        /// <exception cref="InvalidOperationException">如果套接字未绑定则抛出此异常</exception>
        public void Listen(int backlog = 10)
        {
            ThrowNotInitSocket();
            ThrowNotBound();

            listenerSocket.Listen(backlog);
        }

        /// <summary>
        /// 如果套接字未绑定，则抛出异常。
        /// </summary>
        /// <exception cref="Exception">如果套接字未绑定则抛出异常</exception>
        private void ThrowNotBound()
        {
            if (!listenerSocket.IsBound)
                throw new Exception("还未绑定本地接口");
        }

        /// <summary>
        /// 抛出未初始化套接字异常
        /// </summary>
        /// <exception cref="Exception">当listenerSocket为null时抛出异常，提示“还未初始化监听器”</exception>
        private void ThrowNotInitSocket()
        {
            if (listenerSocket == null)
                throw new Exception("还未初始化监听器");
        }

        protected override void Dispose(bool disposing)
        {
            if (!disposing)
                return;

            listenerSocket?.Close();
            listenerSocket?.Dispose();
        }
    }
}
