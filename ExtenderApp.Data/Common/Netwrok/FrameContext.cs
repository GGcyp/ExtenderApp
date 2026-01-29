namespace ExtenderApp.Data
{
    /// <summary>
    /// 表示单个已解析消息帧的容器。
    /// 包含帧负载的缓冲区（<see cref="ByteBlock"/>），并负责在生命周期结束时释放该缓冲区资源。
    /// </summary>
    /// <remarks>
    /// - 该类型为值类型（struct），便于在高频调用场景中传递。
    /// - 对 <see cref="LastPayload"/> 的所有权归属于此 <see cref="FrameContext"/> 实例；调用 <see cref="Dispose"/> 将释放持有的 <see cref="ByteBlock"/>。
    /// - 请避免跨线程共享同一实例的可变操作；若需并发使用，请在外部同步或复制新的实例/缓冲。
    /// </remarks>
    public struct FrameContext : IDisposable
    {
        /// <summary>
        /// 帧负载缓冲，承载已写入的字节数据。
        /// </summary>
        /// <remarks>
        /// - 此属性对外为只读（private set），但返回的 <see cref="ByteBlock"/> 实例本身可能是可变的。
        /// - FrameContext 对该缓冲拥有释放责任；当不再需要时，调用 <see cref="Dispose"/> 以归还或释放底层资源。
        /// </remarks>
        public ByteBlock LastPayload;

        /// <summary>
        /// 访问帧负载的字节数据。
        /// </summary>
        public ReadOnlySpan<byte> UnreadSpan => LastPayload.UnreadSpan;

        public ReadOnlyMemory<byte> UnreadMemory => LastPayload.UnreadMemory;

        private Exception? exception;

        public bool HasException => exception != null;

        /// <summary>
        /// 使用已有的 <see cref="ByteBlock"/> 构造一个 <see cref="FrameContext"/>。
        /// 构造后本实例对 <paramref name="payload"/> 负责释放。
        /// </summary>
        /// <param name="payload">已准备好的字节缓冲，所有权将转移到创建的 <see cref="FrameContext"/>。</param>
        public FrameContext(ByteBlock payload)
        {
            LastPayload = payload;
        }

        /// <summary>
        /// 使用只读字节序列构造一个新的 <see cref="FrameContext"/>，内部将分配并复制数据到新的 <see cref="ByteBlock"/>。
        /// </summary>
        /// <param name="payload">要复制的负载字节。</param>
        public FrameContext(ReadOnlySpan<byte> payload) : this(new(payload))
        {
        }

        public FrameContext(Exception exception)
        {
            this.exception = exception;
        }

        /// <summary>
        /// 用提供的 <see cref="ByteBlock"/> 替换当前负载并释放原缓冲。
        /// </summary>
        /// <param name="payload">新的 <see cref="ByteBlock"/>，所有权将转移到此实例。</param>
        /// <remarks>
        /// - 调用此方法后，传入的 <paramref name="payload"/> 不应在外部继续使用，直到它被重新分配或复制。
        /// - 此方法会释放原有的 <see cref="LastPayload"/>。
        /// </remarks>
        public void WriteNextPayload(ByteBlock payload)
        {
            LastPayload.Dispose();
            LastPayload = payload;
        }

        public void SetException(Exception ex)
        {
            exception = ex;
        }

        public void ThrowIfException()
        {
            if (exception != null)
            {
                throw exception;
            }
        }

        /// <summary>
        /// 释放当前持有的负载缓冲资源。调用后 <see cref="LastPayload"/> 内的资源被归还/释放。
        /// </summary>
        /// <remarks>
        /// - 推荐在处理完帧后显式调用或在使用 <see langword="using"/> / try-finally 模式中确保释放。
        /// - 结构体为值类型，拷贝后对副本调用 <see cref="Dispose"/> 不会影响原实例的字段值（但会释放相同的底层资源），使用时需注意所有权语义。
        /// </remarks>
        public void Dispose()
        {
            LastPayload.Dispose();
        }

        /// <summary>
        /// 将一个 <see cref="ByteBlock"/> 隐式转换为 <see cref="FrameContext"/>，并将缓冲所有权转移至结果实例。
        /// </summary>
        /// <param name="payload">要包装的缓冲。</param>
        public static implicit operator FrameContext(ByteBlock payload) => new FrameContext(payload);

        /// <summary>
        /// 将 <see cref="FrameContext"/> 隐式转换为 <see cref="ByteBlock"/>，取得当前持有的缓冲实例（调用方不得重复释放该实例）。
        /// </summary>
        /// <param name="frame">要转换的帧上下文。</param>
        public static implicit operator ByteBlock(FrameContext frame) => frame.LastPayload;
    }
}