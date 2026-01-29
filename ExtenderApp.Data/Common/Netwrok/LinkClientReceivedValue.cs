namespace ExtenderApp.Data
{
    /// <summary>
    /// 表示从链路接收的数据项及该次套接字操作的附加信息。
    /// 这是一个不可变值类型（struct），用于在接收路径中将业务对象与其接收上下文一并传递。
    /// </summary>
    /// <typeparam name="T">业务对象的类型。</typeparam>
    public struct LinkClientReceivedValue<T>
    {
        /// <summary>
        /// 接收到的业务对象值。
        /// </summary>
        public T Value { get; init; }

        /// <summary>
        /// 与本次接收操作相关的元数据（例如接收字节数、远端地址或操作结果等）。
        /// </summary>
        public SocketOperationValue OperationValue { get; init; }

        /// <summary>
        /// 使用业务对象与接收元数据构造一个新的 <see cref="LinkClientReceivedValue{T}"/> 实例。
        /// </summary>
        /// <param name="value">接收到的业务对象。</param>
        /// <param name="operationValue">本次接收操作的元数据。</param>
        public LinkClientReceivedValue(T value, SocketOperationValue operationValue)
        {
            Value = value;
            OperationValue = operationValue;
        }

        /// <summary>
        /// 隐式转换为业务对象 <typeparamref name="T"/>，返回 <see cref="Value"/>。
        /// 便于调用方在需要直接使用业务对象时进行简写。
        /// </summary>
        /// <param name="receivedValue">要转换的封装实例。</param>
        public static implicit operator T(LinkClientReceivedValue<T> receivedValue)
            => receivedValue.Value;

        /// <summary>
        /// 隐式转换为 <see cref="SocketOperationValue"/>，返回 <see cref="OperationValue"/>。
        /// 便于在只需要操作元数据的场景直接获取该信息。
        /// </summary>
        /// <param name="receivedValue">要转换的封装实例。</param>
        public static implicit operator SocketOperationValue(LinkClientReceivedValue<T> receivedValue)
            => receivedValue.OperationValue;
    }
}