

namespace AppHost.Extensions.DependencyInjection
{
    public enum ServiceLifetime
    {
        /// <summary>
        /// 全局唯一
        /// </summary>
        Singleton,
        /// <summary>
        /// 在一个作用域内存在
        /// </summary>
        /// <remarks>
        /// 被创建后就一直存在，除非被抛弃
        /// </remarks>
        Scoped,
        /// <summary>
        /// 每次都新建
        /// </summary>
        Transient
    }
}
