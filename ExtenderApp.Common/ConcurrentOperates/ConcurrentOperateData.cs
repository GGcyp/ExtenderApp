using ExtenderApp.Abstract;

namespace ExtenderApp.Common.ConcurrentOperates
{
    /// <summary>
    /// 可并发操作的数据类，继承自DisposableObject类，实现IResettable接口
    /// </summary>
    public class ConcurrentOperateData : DisposableObject, IResettable
    {
        /// <summary>
        /// 尝试重置Token
        /// </summary>
        /// <returns>返回true表示重置成功，false表示重置失败</returns>
        public virtual bool TryReset()
        {
            return true;
        }

        /// <summary>
        /// 释放或重置由 <see cref="DisposableObject"/> 占用的资源
        /// </summary>
        /// <param name="disposing">指示是否应释放托管资源</param>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            TryReset();
        }
    }
}
