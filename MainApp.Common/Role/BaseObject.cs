using MainApp.IRole;

namespace MainApp.Role
{
    /// <summary>
    /// 基础类
    /// </summary>
    public abstract class BaseObject
    {
        /// <summary>
        /// 创建时调用
        /// </summary>
        public abstract void Ctor();

        /// <summary>
        /// 开始使用前调用
        /// </summary>
        public abstract void Init();

        /// <summary>
        /// 轮询时调用
        /// </summary>
        /// /// <exception cref="NotImplementedException"></exception>
        public virtual void Tick()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 终止时调用
        /// </summary>
        /// <exception cref="NotImplementedException"></exception>
        public virtual void Close()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 销毁时调用
        /// </summary>
        public abstract void Dispose();
    }
}
