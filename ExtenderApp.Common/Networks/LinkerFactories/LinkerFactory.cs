using System.Net.Sockets;
using ExtenderApp.Abstract;
using ExtenderApp.Common.ObjectPools;
using ExtenderApp.Data;

namespace ExtenderApp.Common.Networks
{
    /// <summary>
    /// LinkerFactory 类是一个内部类，实现了 ILinkerFactory 接口。
    /// 用于创建不同类型的 Linker 对象。
    /// </summary>
    internal class LinkerFactory : ILinkerFactory
    {
        /// <summary>
        /// 文件解析器存储对象。
        /// </summary>
        private readonly IFileParserStore _fileParserStore;

        /// <summary>
        /// 序列池对象，用于存储字节序列。
        /// </summary>
        private readonly SequencePool<byte> _sequencePool;

        private readonly ObjectPool<ITcpLinker> _tcpLinkerPool;

        /// <summary>
        /// 初始化 LinkerFactory 类的新实例。
        /// </summary>
        /// <param name="fileParserStore">文件解析器存储对象。</param>
        /// <param name="sequencePool">序列池对象。</param>
        public LinkerFactory(IFileParserStore fileParserStore, SequencePool<byte> sequencePool)
        {
            _fileParserStore = fileParserStore;
            _sequencePool = sequencePool;

            _tcpLinkerPool = ObjectPool.Create(new FactoryPooledObjectPolicy<ITcpLinker>(() => new TcpLinker(_fileParserStore.BinaryParser, _sequencePool, _tcpLinkerPool.Release)));
        }

        /// <summary>
        /// 根据协议类型创建 Linker 对象。
        /// </summary>
        /// <param name="type">协议类型。</param>
        /// <returns>返回创建的 Linker 对象。</returns>
        /// <exception cref="System.NotImplementedException">如果协议类型未实现，则抛出此异常。</exception>
        public ILinker CreateLinker(ProtocolType type)
        {
            return type switch
            {
                ProtocolType.Tcp => _tcpLinkerPool.Get(),
                //ProtocolType.Udp => new UdpLinker(_fileParserStore),
                _ => throw new System.NotImplementedException()
            };
        }

        public T GetLinker<T>() where T : ILinker
        {
            var resultType = typeof(T);
            if (resultType == typeof(ITcpLinker))
            {
                return (T)_tcpLinkerPool.Get();
            }

            throw new Exception("未找到可用的连接器");
        }

        public void ReleaseLinker(ILinker linker)
        {
            //_tcpLinkerPool.Release(tcpLinker);
            switch (linker)
            {
                case ITcpLinker tcpLinker:
                    _tcpLinkerPool.Release(tcpLinker);
                    return;
            }

            throw new KeyNotFoundException("未找到可回收的内存池");
        }
    }
}
