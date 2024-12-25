

using ExtenderApp.Data;

namespace ExtenderApp.Abstract
{
    /// <summary>
    /// 网络提供程序接口
    /// </summary>
    public interface INetWorkProider
    {
        /// <summary>
        /// 根据连接类型获取网络客户端
        /// </summary>
        /// <param name="connectionsType">连接类型</param>
        /// <returns>返回对应的网络客户端</returns>
        INetworkClient GetNetworkClient(ConnectionsType connectionsType);
    }
}
