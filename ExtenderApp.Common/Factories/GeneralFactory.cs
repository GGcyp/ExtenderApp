using ExtenderApp.Abstract;

namespace ExtenderApp.Common.Factories
{
    public abstract class GeneralFactory
    {

    }

    public abstract class GeneralFactory<T> : GeneralFactory
    {
        public abstract T Create();
    }

    public abstract class GeneralFactory<TValue, TType> : GeneralFactory
    {
        private readonly IFactoryPolicy<TType> _policy;
        public abstract TValue Create();

        protected GeneralFactory(IFactoryPolicy<TType> policy)
        {
            _policy = policy;
        }
    }
}
