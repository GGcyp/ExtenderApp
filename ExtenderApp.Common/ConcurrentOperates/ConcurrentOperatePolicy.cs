using ExtenderApp.Abstract;
using ExtenderApp.Common.ObjectPools;

namespace ExtenderApp.Common.ConcurrentOperates
{
    public abstract class ConcurrentOperatePolicy<TOperate, TData> : DisposableObject, IConcurrentOperatePolicy<TOperate, TData>
        where TOperate : class
        where TData : ConcurrentOperateData, new()
    {
        private readonly ObjectPool<TData> _pool = ObjectPool.CreateDefaultPool<TData>();

        public CancellationToken Token { get; protected set; }

        public virtual void AfterExecute(TOperate operate, TData data)
        {

        }

        public virtual void BeforeExecute(TOperate operate, TData data)
        {

        }

        public abstract TOperate Create(TData data);

        public void ReleaseData(TData data)
        {
            _pool.Release(data);
        }

        public TData GetData()
        {
            return _pool.Get();
        }
    }
}
