

namespace ExtenderApp.Repositoryies
{
    /// <summary>
    /// 数据库上下文接口。
    /// 提供获取指定类型仓储（Repository）实例的方法，
    /// 用于统一管理和访问不同实体的数据库操作。
    /// </summary>
    public interface IDbContext
    {
        /// <summary>
        /// 获取指定类型的仓储实例。
        /// </summary>
        /// <typeparam name="TRepository">仓储类型，必须实现 <see cref="IDbRepository"/> 接口。</typeparam>
        /// <returns>指定类型的仓储实例。</returns>
        TRepository GetRepository<TRepository>() where TRepository : IDbRepository;
    }
}
