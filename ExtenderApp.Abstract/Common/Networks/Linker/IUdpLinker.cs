

using System.Net;
using ExtenderApp.Data;

namespace ExtenderApp.Abstract
{
    /// <summary>
    /// 表示一个 UDP 链接器接口，继承自 <see cref="ILinker"/> 接口。
    /// </summary>
    public interface IUdpLinker : ILinker
    {
        SocketOperationResult SendTo(Memory<byte> memory, EndPoint endPoint);

        ValueTask<SocketOperationResult> SendToAsync(Memory<byte> memory, EndPoint endPoint, CancellationToken token = default);
    }
}
