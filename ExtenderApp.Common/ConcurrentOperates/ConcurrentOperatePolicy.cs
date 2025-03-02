using ExtenderApp.Abstract;
using ExtenderApp.Common.ObjectPools;

namespace ExtenderApp.Common.ConcurrentOperates
{
    /// <summary>
    /// 一个抽象的并发操作策略类，实现了<see cref="IConcurrentOperatePolicy{TOperate, TData}"/>接口。
    /// 该类是一个泛型类，其中<typeparamref name="TOperate"/>代表操作的类型，<typeparamref name="TData"/>代表操作数据的类型。
    /// </summary>
    /// <typeparam name="TOperate">操作的类型，必须是一个类。</typeparam>
    /// <typeparam name="TData">操作数据的类型，必须继承自<see cref="ConcurrentOperateData"/>并且有一个无参构造函数。</typeparam>
    public abstract class ConcurrentOperatePolicy<TOperate, TData> : DisposableObject, IConcurrentOperatePolicy<TOperate, TData>
        where TOperate : class
        where TData : ConcurrentOperateData
    {
        //private static readonly ObjectPool<TData> _pool = ObjectPool.CreateDefaultPool<TData>();

        public virtual void AfterExecute(TOperate operate, TData data)
        {

        }

        public virtual void BeforeExecute(TOperate operate, TData data)
        {

        }

        public abstract TOperate Create(TData data);

        //public virtual void ReleaseData(TData data)
        //{
        //    _pool.Release(data);
        //}

        //public virtual TData GetData()
        //{
        //    return _pool.Get();
        //}

        public abstract void ReleaseData(TData data);

        public abstract TData GetData();
    }
}
