namespace ExtenderApp.Abstract
{
    public interface IFactoryPolicy<T>
    {
        void Apply(T instance);
    }
}
