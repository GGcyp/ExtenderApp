using ExtenderApp.Abstract;
using ExtenderApp.Common.Error;

namespace ExtenderApp.Common.ObjectPools
{
    public class FactoryPooledObjectPolicy<T> : PooledObjectPolicy<T>
    {
        private readonly Func<T> _factory;

        public FactoryPooledObjectPolicy(Func<T> factory)
        {
            factory.ArgumentNull(nameof(factory));

            _factory = factory;
        }

        public override T Create()
        {
            return _factory.Invoke();
        }

        public override bool Release(T obj)
        {
            if (obj is IResettable resettable)
            {
                return resettable.TryReset();
            }

            return true;
        }
    }
}
