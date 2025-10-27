using ExtenderApp.Data;

namespace ExtenderApp.Abstract
{
    /// <summary>
    /// 链路客户端消息打包/解包器接口。
    /// </summary>
    public interface ILinkClientFramer : IDisposable
    {
        /// <summary>
        /// 当前使用的协议魔数（协议标识字节序列），用于帧同步或协议识别。
        /// </summary>
        /// <remarks>
        /// - 返回的数组为只读视图或副本取决于实现；外部请勿修改返回数组以避免未定义行为。  
        /// - 可为 null 或长度为 0（表示不使用魔数），具体语义由实现定义。
        /// </remarks>
        ReadOnlyMemory<byte> Magic { get; }

        /// <summary>
        /// 设置/替换协议魔数（Magic bytes）。
        /// </summary>
        /// <param name="magic">要设置的魔数字节序列。传入 null 或空数组表示禁用魔数检查（由实现定义行为）。</param>
        /// <exception cref="ArgumentException">当 magic 长度不被某些实现接受时可抛出。</exception>
        void SetMagic(ReadOnlySpan<byte> magic);

        /// <summary>
        /// 将单条消息封装为帧（或多个缓冲片段）以便发送。
        /// </summary>
        /// <param name="messageType">应用层定义的消息类型标识。</param>
        /// <param name="length">消息负载长度（字节）。实现可据此预分配输出缓冲。</param>
        /// <param name="framedMessage">输出的已封装消息缓冲，通常包含头部（如魔数、类型、长度）与负载字节。</param>
        void Encode(int messageType, int length, out ByteBuffer framedMessage);

        /// <summary>
        /// 从接收缓冲中解析出一个或多个消息帧。
        /// </summary>
        /// <param name="originalMessage">
        /// 输入/输出的字节缓冲：解析过程可能会移动/消费其读取指针或对其进行截断/重置，具体行为由实现约定。
        /// 使用 ref 传递以便实现可以在必要时替换或修改缓冲实例。
        /// </param>
        /// <param name="framedList">输出的帧集合；每个帧包含消息类型与对应的负载。调用方负责释放集合及其内部帧的资源。</param>
        void Decode(ref ByteBuffer originalMessage, out PooledFrameList framedList);
    }
}
