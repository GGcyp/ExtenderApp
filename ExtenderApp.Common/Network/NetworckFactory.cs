using ExtenderApp.Data;
using ExtenderApp.Common.ObjectPools;

namespace ExtenderApp.Common.Network
{


    internal static class NetworckFactory
    {
        private class NetworckPooledObjectPolicy<T> : IPooledObjectPolicy<NetworkClient> where T : NetworkClient, new()
        {
            public NetworkClient Create()
            {
                return new T();
            }

            public bool Release(NetworkClient obj)
            {
                return obj != null;
            }
        }

        public static ObjectPool<NetworkClient> CreatePool(ConnectionsType connectionsType)
        {
            return connectionsType switch
            {
                ConnectionsType.Tcp => ObjectPool.Create(CreatePooledObjectPolicy<NetworkClient_TCP>()),
                ConnectionsType.Http => ObjectPool.Create(CreatePooledObjectPolicy<NetworkClient_Http>()),
            };
        }

        private static IPooledObjectPolicy<NetworkClient> CreatePooledObjectPolicy<T>() where T : NetworkClient, new()
        {
            return new NetworckPooledObjectPolicy<T>();
        }
    }
}
