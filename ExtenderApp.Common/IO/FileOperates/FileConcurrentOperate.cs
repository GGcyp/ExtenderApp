using ExtenderApp.Common.ConcurrentOperates;
using ExtenderApp.Common.ObjectPools;

namespace ExtenderApp.Common.IO
{
    /// <summary>
    /// 文件并发操作类，继承自ConcurrentOperate泛型类
    /// </summary>
    /// <typeparam name="FileOperatePolicy">文件操作策略类型</typeparam>
    /// <typeparam name="FileOperateData">文件操作数据类型</typeparam>
    public class FileConcurrentOperate : ConcurrentOperate<FileOperateData>
    {
        /// <summary>
        /// 延迟初始化的对象池，用于存储FileConcurrentOperate对象
        /// </summary>
        private static Lazy<ObjectPool<FileConcurrentOperate>> _poolLazy =
            new(() => ObjectPool.CreateDefaultPool<FileConcurrentOperate>());

        /// <summary>
        /// 获取对象池实例
        /// </summary>
        private static ObjectPool<FileConcurrentOperate> _pool => _poolLazy.Value;

        /// <summary>
        /// 从对象池中获取一个FileConcurrentOperate实例
        /// </summary>
        /// <returns>返回获取到的FileConcurrentOperate实例</returns>
        public static FileConcurrentOperate Get() => _pool.Get();

        /// <summary>
        /// 将一个FileConcurrentOperate实例释放回对象池
        /// </summary>
        /// <param name="obj">要释放的FileConcurrentOperate实例</param>
        public void Release() => _pool.Release(this);
    }
}
