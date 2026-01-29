using ExtenderApp.Data;

namespace ExtenderApp.Abstract
{
    /// <summary>
    /// 链路客户端消息打包 / 解包 器接口。
    /// 提供将应用层消息封帧为传输帧，以及从接收字节流中解析完整帧的能力。
    /// </summary>
    public interface ILinkClientFramer : IDisposable
    {
        /// <summary>
        /// 当前使用的协议魔数字节序列（magic bytes）。
        /// </summary>
        ReadOnlyMemory<byte> Magic { get; }

        /// <summary>
        /// 将从链路读取到的原始字节输入到解帧器以尝试解析帧。
        /// </summary>
        /// <param name="span">包含新读取字节的只读跨度。</param>
        void Decode(ReadOnlySpan<byte> span);

        /// <summary>
        /// 将应用层消息封装为传输帧（添加头、长度、校验等，具体由实现决定）。
        /// </summary>
        /// <param name="messageSpan">要封装的消息负载字节范围。</param>
        /// <param name="framedMessage">输出的已封装帧上下文，包含用于发送的字节序列等信息。</param>
        void Encode(ReadOnlySpan<byte> messageSpan, out FrameContext framedMessage);

        /// <summary>
        /// 异步读取并返回下一个完整解析出的帧上下文。
        /// </summary>
        /// <param name="token">用于取消等待操作的 <see cref="CancellationToken"/>。</param>
        /// <returns>当可用时，返回包含解析后帧数据的 <see cref="FrameContext"/>。</returns>
        ValueTask<FrameContext> ReadFrameAsync(CancellationToken token = default);

        /// <summary>
        /// 设置或替换协议魔数（magic bytes）。
        /// </summary>
        /// <param name="magic">
        /// 要设置的魔数字节序列。传入长度为 0 或空视为禁用魔数检查（语义由实现定义）。
        /// 注意：参数以 <see cref="ReadOnlySpan{T}"/> 形式传入，调用者可传入栈分配或堆数据。
        /// </param>
        /// <exception cref="ArgumentException">当 magic 长度不被某些实现接受时可抛出。</exception>
        void SetMagic(ReadOnlySpan<byte> magic);
    }
}