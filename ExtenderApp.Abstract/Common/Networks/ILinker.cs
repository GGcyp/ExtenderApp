using System.Net;
using ExtenderApp.Data;

namespace ExtenderApp.Abstract
{
    /// <summary>
    /// 定义一个用于操作链接的接口
    /// </summary>
    public interface ILinker : IDisposable
    {
        bool NoDelay { get; set; }
        bool Connected { get; }
        EndPoint? LocalEndPoint { get; }
        EndPoint? RemoteEndPoint { get; }
        CapacityLimiter CapacityLimiter { get; }
        ValueCounter SendCounter { get; }
        ValueCounter ReceiveCounter { get; }

        int Send(ReadOnlySpan<byte> span);
        ValueTask<int> SendAsync(ReadOnlyMemory<byte> memory, CancellationToken token = default);
    }
}
