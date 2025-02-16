

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
        /// 获取取消令牌，用于控制并发操作的取消。
        /// </summary>
        CancellationToken Token { get; }

        /// <summary>
        /// 创建一个操作对象。
        /// </summary>
        /// <param name="data">用于创建操作对象的数据。</param>
        /// <returns>创建的操作对象。</returns>
        TOperate Create(TData data);

        /// <summary>
        /// 在并发操作执行前执行的钩子方法。
        /// </summary>
        /// <param name="operate">并发操作实例。</param>
        /// <param name="data">并发操作需要处理的数据。</param>
        void BeforeExecute(TOperate operate, TData data);

        /// <summary>
        /// 在并发操作执行后执行的钩子方法。
        /// </summary>
        /// <param name="operate">并发操作实例。</param>
        /// <param name="data">并发操作处理后的数据。</param>
        void AfterExecute(TOperate operate, TData data);

        /// <summary>
        /// 释放数据
        /// </summary>
        /// <param name="data">要释放的数据</param>
        void ReleaseData(TData data);

        /// <summary>
        /// 获取数据。
        /// </summary>
        /// <returns>返回类型为TData的数据。</returns>
        TData GetData();
    }
}
