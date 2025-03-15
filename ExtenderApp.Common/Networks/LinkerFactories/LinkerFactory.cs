using System.Net.Sockets;
using ExtenderApp.Abstract;
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

        /// <summary>
        /// 初始化 LinkerFactory 类的新实例。
        /// </summary>
        /// <param name="fileParserStore">文件解析器存储对象。</param>
        /// <param name="sequencePool">序列池对象。</param>
        public LinkerFactory(IFileParserStore fileParserStore, SequencePool<byte> sequencePool)
        {
            _fileParserStore = fileParserStore;
            _sequencePool = sequencePool;
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
                ProtocolType.Tcp => new TcpLinker(_fileParserStore.BinaryParser, _sequencePool),
                //ProtocolType.Udp => new UdpLinker(_fileParserStore),
                _ => throw new System.NotImplementedException()
            };
        }
    }
}
