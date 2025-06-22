using ExtenderApp.Abstract;
using ExtenderApp.Common.ObjectPools;

namespace ExtenderApp.Common.DataBuffers
{
    /// <summary>
    /// 数据缓冲区类，继承自<see cref="DisposableObject"/>类，实现了<see cref="IResettable"/>接口。
    /// </summary>
    public class DataBuffer : DisposableObject, IResettable
    {
        /// <summary>
        /// 一个静态的<see cref="ObjectPool{DataBuffer}"/>对象，用于管理<see cref="DataBuffer"/>对象的池。
        /// </summary>
        private readonly static ObjectPool<DataBuffer> _pool = ObjectPool.CreateDefaultPool<DataBuffer>();

        /// <summary>
        /// 从对象池中获取一个<see cref="DataBuffer"/>实例。
        /// </summary>
        /// <returns>返回从对象池中获取的<see cref="DataBuffer"/>实例。</returns>
        public static DataBuffer GetDataBuffer() => _pool.Get();

        /// <summary>
        /// 将一个<see cref="DataBuffer"/>实例释放回对象池中。
        /// </summary>
        /// <param name="item">要释放回对象池的<see cref="DataBuffer"/>实例。</param>
        public static void ReleaseDataBuffer(DataBuffer item) => _pool.Release(item);

        /// <summary>
        /// 一个委托对象，用于存储处理操作。
        /// </summary>
        protected Delegate processAction;

        /// <summary>
        /// 尝试重置<see cref="DataBuffer"/>实例。
        /// </summary>
        /// <returns>如果重置成功，则返回true；否则返回false。</returns>
        public virtual bool TryReset()
        {
            processAction = null;
            return true;
        }

        /// <summary>
        /// 释放<see cref="DataBuffer"/>实例。
        /// </summary>
        public virtual void Release()
        {
            ReleaseDataBuffer(this);
        }

        /// <summary>
        /// 设置处理操作。
        /// </summary>
        /// <typeparam name="TResult">处理操作的结果类型。</typeparam>
        /// <param name="action">处理操作。</param>
        public void SetProcessAction<TResult>(Action<TResult> action)
        {
            processAction = action;
        }

        /// <summary>
        /// 处理数据。
        /// </summary>
        /// <typeparam name="TResult">处理结果的类型。</typeparam>
        /// <param name="varule">要处理的数据。</param>
        public virtual void Process<TResult>(TResult varule)
        {
            var callback = processAction as Action<TResult>;
            callback?.Invoke(varule);
        }
    }

    /// <summary>
    /// 泛型数据缓冲区类，继承自DataBuffer类。
    /// </summary>
    /// <typeparam name="T">缓冲区存储的数据类型。</typeparam>
    public class DataBuffer<T> : DataBuffer
    {
        /// <summary>
        /// 数据缓冲区对象池。
        /// </summary>
        private readonly static ObjectPool<DataBuffer<T>> _pool = ObjectPool.CreateDefaultPool<DataBuffer<T>>();

        /// <summary>
        /// 从对象池中获取一个DataBuffer<T>实例。
        /// </summary>
        /// <returns>返回从对象池中获取的DataBuffer<T>实例。</returns>
        public static DataBuffer<T> GetDataBuffer() => _pool.Get();

        /// <summary>
        /// 将DataBuffer<T>实例释放回对象池。
        /// </summary>
        /// <param name="item">要释放的DataBuffer<T>实例。</param>
        public static void ReleaseDataBuffer(DataBuffer<T> item) => _pool.Release(item);

        /// <summary>
        /// 获取或设置缓冲区存储的数据项。
        /// </summary>
        public T Item1 { get; set; }

        /// <summary>
        /// 获取处理动作。
        /// </summary>
        /// <typeparam name="TResult">返回结果类型。</typeparam>
        /// <param name="action">处理动作，包含两个参数，DataBuffer<T>类型的数据和TResult类型的返回结果。</param>
        /// <returns>返回处理动作。</returns>
        public Action<TResult> GetProcessAction<TResult>(Action<DataBuffer<T>, TResult> action)
        {
            processAction = action;
            return Process;
        }

        /// <summary>
        /// 设置处理动作。
        /// </summary>
        /// <typeparam name="TResult">返回结果类型。</typeparam>
        /// <param name="action">处理动作，包含两个参数，DataBuffer<T>类型的数据和TResult类型的返回结果。</param>
        public void SetProcessAction<TResult>(Action<DataBuffer<T>, TResult> action)
        {
            processAction = action;
        }

        public override void Process<TResult>(TResult varule)
        {
            var callback = processAction as Action<DataBuffer<T>, TResult>;
            callback?.Invoke(this, varule);
        }

        /// <summary>
        /// 释放当前DataBuffer<T>实例，将其放回对象池中。
        /// </summary>
        public override void Release()
        {
            ReleaseDataBuffer(this);
        }

        /// <summary>
        /// 尝试重置DataBuffer<T>实例。
        /// </summary>
        /// <returns>如果成功重置则返回true，否则返回false。</returns>
        public override bool TryReset()
        {
            Item1 = default;
            return true;
        }
    }

    /// <summary>
    /// 泛型数据缓冲区类，支持两个泛型参数。
    /// </summary>
    /// <typeparam name="T1">第一个泛型参数类型。</typeparam>
    /// <typeparam name="T2">第二个泛型参数类型。</typeparam>
    public class DataBuffer<T1, T2> : DataBuffer
    {
        /// <summary>
        /// 数据缓冲区对象池。
        /// </summary>
        private static ObjectPool<DataBuffer<T1, T2>> pool = ObjectPool.CreateDefaultPool<DataBuffer<T1, T2>>();

        /// <summary>
        /// 从对象池中获取一个新的 <see cref="DataBuffer{T1, T2}"/> 实例。
        /// </summary>
        /// <returns>返回一个新的 <see cref="DataBuffer{T1, T2}"/> 实例。</returns>
        public static DataBuffer<T1, T2> GetDataBuffer() => pool.Get();

        /// <summary>
        /// 将 <see cref="DataBuffer{T1, T2}"/> 实例释放回对象池。
        /// </summary>
        /// <param name="item">要释放的 <see cref="DataBuffer{T1, T2}"/> 实例。</param>
        public static void ReleaseDataBuffer(DataBuffer<T1, T2> item) => pool.Release(item);

        /// <summary>
        /// 第一个泛型参数的数据项。
        /// </summary>
        public T1 Item1 { get; set; }

        /// <summary>
        /// 第二个泛型参数的数据项。
        /// </summary>
        public T2 Item2 { get; set; }

        /// <summary>
        /// 获取处理动作
        /// </summary>
        /// <typeparam name="TResult">处理结果类型</typeparam>
        /// <param name="action">处理动作</param>
        /// <returns>处理动作</returns>
        public Action<TResult> GetProcessAction<TResult>(Action<DataBuffer<T1, T2>, TResult> action)
        {
            processAction = action;
            return Process;
        }

        /// <summary>
        /// 设置处理动作
        /// </summary>
        /// <typeparam name="TResult">处理结果类型</typeparam>
        /// <param name="action">处理动作</param>
        public void SetProcessAction<TResult>(Action<DataBuffer<T1, T2>, TResult> action)
        {
            processAction = action;
        }

        public override void Process<TResult>(TResult varule)
        {
            var callback = processAction as Action<DataBuffer<T1, T2>, TResult>;
            callback?.Invoke(this, varule);
        }

        /// <summary>
        /// 释放当前实例到对象池。
        /// </summary>
        public override void Release()
        {
            ReleaseDataBuffer(this);
        }

        /// <summary>
        /// 尝试重置当前实例。
        /// </summary>
        /// <returns>如果重置成功，则返回 true；否则返回 false。</returns>
        public override bool TryReset()
        {
            Item1 = default;
            Item2 = default;
            return true;
        }
    }

    public class DataBuffer<T1, T2, T3> : DataBuffer
    {
        /// <summary>
        /// 数据缓冲区对象池。
        /// </summary>
        private static ObjectPool<DataBuffer<T1, T2, T3>> pool = ObjectPool.CreateDefaultPool<DataBuffer<T1, T2, T3>>();

        /// <summary>
        /// 从对象池中获取一个新的 <see cref="DataBuffer{T1, T2}"/> 实例。
        /// </summary>
        /// <returns>返回一个新的 <see cref="DataBuffer{T1, T2}"/> 实例。</returns>
        public static DataBuffer<T1, T2, T3> GetDataBuffer() => pool.Get();

        /// <summary>
        /// 将 <see cref="DataBuffer{T1, T2}"/> 实例释放回对象池。
        /// </summary>
        /// <param name="item">要释放的 <see cref="DataBuffer{T1, T2}"/> 实例。</param>
        public static void ReleaseDataBuffer(DataBuffer<T1, T2, T3> item) => pool.Release(item);

        /// <summary>
        /// 第一个泛型参数的数据项。
        /// </summary>
        public T1 Item1 { get; set; }

        /// <summary>
        /// 第二个泛型参数的数据项。
        /// </summary>
        public T2 Item2 { get; set; }

        /// <summary>
        /// 第三个泛型参数的数据项。
        /// </summary>
        public T3 Item3 { get; set; }

        /// <summary>
        /// 获取处理结果的动作。
        /// </summary>
        /// <typeparam name="TResult">处理结果的数据类型。</typeparam>
        /// <param name="action">处理数据缓冲区和处理结果的动作。</param>
        /// <returns>处理结果的动作。</returns>
        public Action<TResult> GetProcessAction<TResult>(Action<DataBuffer<T1, T2, T3>, TResult> action)
        {
            processAction = action;
            return Process;
        }

        /// <summary>
        /// 设置处理结果的动作。
        /// </summary>
        /// <typeparam name="TResult">处理结果的数据类型。</typeparam>
        /// <param name="action">处理数据缓冲区和处理结果的动作。</param>
        public void SetProcessAction<TResult>(Action<DataBuffer<T1, T2, T3>, TResult> action)
        {
            processAction = action;
        }

        public override void Process<TResult>(TResult varule)
        {
            var callback = processAction as Action<DataBuffer<T1, T2, T3>, TResult>;
            callback?.Invoke(this, varule);
        }

        /// <summary>
        /// 释放当前实例到对象池。
        /// </summary>
        public override void Release()
        {
            ReleaseDataBuffer(this);
        }

        /// <summary>
        /// 尝试重置当前实例。
        /// </summary>
        /// <returns>如果重置成功，则返回 true；否则返回 false。</returns>
        public override bool TryReset()
        {
            Item1 = default;
            Item2 = default;
            return true;
        }
    }
}
