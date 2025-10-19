

using ExtenderApp.Data;

namespace ExtenderApp.Abstract
{
    /// <summary>
    /// LinkerClient管道上下文：用于处理链路数据的管道
    /// </summary>
    public interface ILinkerClientPiplineContext : IPipelineContext
    {
        /// <summary>
        /// 原始输入数据（例如TCP应用层收到的二进制流）
        /// </summary>
        public ref ByteBlock InputData { get; }

        /// <summary>
        /// 处理后的输出数据（例如要发送的响应）
        /// </summary>
        public ref ByteBlock OutputData { get; }

        /// <summary>
        /// 业务数据对象（解析后的结构化数据，如ProtoBuf对象）
        /// </summary>
        public object BusinessData { get; set; }
    }
}
