using ExtenderApp.Abstract;

namespace ExtenderApp.Common.ObjectPools
{
    public class ObjectPoolDictionary<T> : Dictionary<T, IObjectPool> where T : notnull
    {

    }
}
