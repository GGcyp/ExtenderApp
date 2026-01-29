using ExtenderApp.Abstract;
using ExtenderApp.Common.Hash;
using ExtenderApp.Data;

namespace ExtenderApp.Common.Networks.LinkClients
{
    /// <summary>
    /// 连接服务端格式化器基类。
    /// </summary>
    /// <typeparam name="T">指定格式</typeparam>
    internal abstract class LinkClientFormatter<T> : ILinkClientFormatter<T>
    {
        /// <summary>
        /// 与业务数据类型 <typeparamref name="T"/> 关联的稳定哈希。
        /// </summary>
        /// <remarks>
        /// - 计算规则：<c>FNV-1a(nameof(TLinkClient))</c>；
        /// - 注意：若存在跨命名空间同名类型，建议统一迁移到使用 FullName 的策略（需通讯两端同时调整）；
        /// - 用途：在消息头中用于快速定位对应的格式化器或处理管道。
        /// </remarks>
        public int MessageType { get; private set; }

        public event Action<LinkClientReceivedValue<T>>? Received;

        public LinkClientFormatter()
        {
            MessageType = typeof(T).ComputeHash_FNV_1a();
        }

        public FrameContext Serialize(T value)
        {
            ByteBuffer buffer = ByteBuffer.CreateBuffer();
            Serialize(value, ref buffer);
            ByteBlock block = new(buffer);
            buffer.Dispose();
            return new(block);
        }

        public void DeserializeAndInvoke(SocketOperationValue operationValue, ref FrameContext frameContext)
        {
            ByteBuffer buffer = new(frameContext.UnreadMemory);
            T value = Deserialize(ref buffer);
            Received?.Invoke(new(value, operationValue));
        }

        /// <summary>
        /// 序列化指定的值到缓冲区。
        /// </summary>
        /// <param name="value">需要序列化的值。</param>
        /// <param name="buffer">指定缓冲区</param>
        protected abstract void Serialize(T value, ref ByteBuffer buffer);

        /// <summary>
        /// 反序列化缓冲区到指定的值。
        /// </summary>
        /// <param name="buffer">指定缓冲区</param>
        /// <returns><typeparamref name="T"/> 实例</returns>
        protected abstract T Deserialize(ref ByteBuffer buffer);
    }
}