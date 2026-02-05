using ExtenderApp.Data;

namespace ExtenderApp.Abstract
{
    /// <summary>
    /// 强类型传输客户端接口。
    /// <para>在具备协议管理能力的基础上，提供了发送和接收任意泛型对象或文件的能力。</para>
    /// </summary>
    public interface ITransferLinkClient : ILinkClient<ITransferLinkClient>
    {
        /// <summary>
        /// 发送一个泛型对象。
        /// </summary>
        /// <typeparam name="T">待发送的数据类型。</typeparam>
        /// <param name="data">待发送的数据实例。</param>
        /// <returns>表示异步操作的任务。</returns>
        ValueTask<Result<SocketOperationValue>> SendAsync<T>(T data, CancellationToken token = default);

        Result<SocketOperationValue> Send<T>(T data);
    }

    public interface ITransferLinkClient<TLinker> : ITransferLinkClient
        where TLinker : ILinker
    {
    }
}