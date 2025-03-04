using System.Net;
using System.Net.Sockets;
using ExtenderApp.Abstract;
using ExtenderApp.Common.ObjectPools;
using ExtenderApp.Data.File;

namespace ExtenderApp.Common.NetWorks
{
    /// <summary>
    /// LinkPoolPolicy 类，继承自 PooledObjectPolicy 类，用于管理实现了 ILinker 接口的对象池。
    /// </summary>
    /// <typeparam name="T">泛型参数，表示实现了 ILinker 接口的类型。</typeparam>
    public class LinkPoolPolicy<T> : PooledObjectPolicy<T> where T : ILinker
    {
        /// <summary>
        /// 二进制解析器。
        /// </summary>
        private readonly IBinaryParser _binaryParser;

        /// <summary>
        /// 字节序列池。
        /// </summary>
        private readonly SequencePool<byte> _sequencePool;

        /// <summary>
        /// 创建对象的委托。
        /// </summary>
        private readonly Func<IBinaryParser, SequencePool<byte>, T> _createFunc;

        public LinkPoolPolicy(IBinaryParser binaryParser, SequencePool<byte> sequencePool, Func<IBinaryParser, SequencePool<byte>, T> createFunc)
        {
            _binaryParser = binaryParser;
            _sequencePool = sequencePool;
            _createFunc = createFunc;
        }

        /// <summary>
        /// 重写 Create 方法，用于创建新的对象。
        /// </summary>
        /// <returns>返回新创建的对象。</returns>
        public override T Create()
        {
            return _createFunc.Invoke(_binaryParser, _sequencePool);
        }

        /// <summary>
        /// 重写 Release 方法，用于释放对象。
        /// </summary>
        /// <param name="obj">要释放的对象。</param>
        /// <returns>如果对象成功重置则返回 true，否则返回 false。</returns>
        public override bool Release(T obj)
        {
            return obj.TryReset();
        }
    }
    public abstract class ListenerLinker<T>
    {
        private readonly Socket _listenerSocket;
        private readonly AsyncCallback _acceptCallback;

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

        public T Accept()
        {
            ThrowNotBound();

            var clientSocket = _listenerSocket.Accept();
            clientSocket.NoDelay = true;
            return CreateOperate(clientSocket);
        }

        public void BeginAccept(Action<T> callback)
        {
            ThrowNotBound();

            _listenerSocket.BeginAccept(_acceptCallback, callback);
        }

        private void AcceptCallback(IAsyncResult ar)
        {
            // 获取监听Socket
            Action<T> callback = (Action<T>)ar.AsyncState!;

            // 结束接受连接请求并获取新的Socket
            Socket handler = _listenerSocket.EndAccept(ar);
            handler.NoDelay = true;
            callback?.Invoke(CreateOperate(handler));

            // 继续监听下一个连接请求
            _listenerSocket.BeginAccept(_acceptCallback, callback);
        }

        private void ThrowNotBound()
        {
            if (!_listenerSocket.IsBound)
                throw new Exception("还未绑定本地接口");
        }

        protected abstract T CreateOperate(Socket clientSocket);
    }
}
