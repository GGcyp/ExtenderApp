

namespace ExtenderApp.Abstract
{
    /// <summary>
    /// 定义了一个并发操作策略的接口，继承自IDisposable接口，用于在并发操作时提供创建、执行前后的钩子方法。
    /// </summary>
    /// <typeparam name="TOperate">并发操作的类型。</typeparam>
    /// <typeparam name="TData">并发操作需要处理的数据类型。</typeparam>
    public interface IConcurrentOperatePolicy<TOperate, TData> : IDisposable
    {
        /// <summary>
        /// 根据传入的数据创建一个操作对象。
        /// </summary>
        /// <param name="data">需要处理的数据。</param>
        /// <returns>创建的操作对象。</returns>
        TOperate Create(TData data);

        /// <summary>
        /// 在操作执行之前执行的操作。
        /// </summary>
        /// <param name="operation">即将执行的操作对象</param>
        /// <param name="data">操作所需的数据</param>
        void BeforeExecute(TOperate operation, TData data);

        /// <summary>
        /// 在操作执行之后执行的操作。
        /// </summary>
        /// <param name="operation">已执行的操作对象</param>
        /// <param name="data">操作所需的数据</param>
        void AfterExecute(TOperate operation, TData data);

        /// <summary>
        /// 获取数据。
        /// </summary>
        /// <returns>返回类型为TData的数据。</returns>
        TData GetData();

        /// <summary>
        /// 释放数据对象资源。
        /// </summary>
        /// <param name="data">要释放的数据对象</param>
        void ReleaseData(TData data);
    }
}
