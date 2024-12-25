using ExtenderApp.Abstract;
using ExtenderApp.Common.ObjectPools;
using ExtenderApp.Data;

namespace ExtenderApp.Common.Network
{
    /// <summary>
    /// 网络提供者类
    /// </summary>
    internal class NetWorkProider : INetWorkProider
    {
        private ValueList<(ConnectionsType, ObjectPool<NetworkClient>)> _connections;

        public INetworkClient GetNetworkClient(ConnectionsType connectionsType)
        {
            for (int i = 0; i < _connections.Count; i++)
            {
                if (_connections[i].Item1 == connectionsType)
                {
                    return _connections[i].Item2.Get();
                }
            }

            var pool = NetworckFactory.CreatePool(connectionsType);

            if (pool is null)
                throw new InvalidOperationException(connectionsType.ToString());

            _connections.Add((connectionsType, pool));

            return pool.Get();
        }
    }
}
