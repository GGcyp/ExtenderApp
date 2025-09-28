using System.Linq.Expressions;

namespace ExtenderApp.Repositoryies
{
    /// <summary>
    /// 数据库操作通用接口
    /// </summary>
    public interface IDbRepository : IDisposable
    {
    }

    /// <summary>
    /// 数据库操作通用接口
    /// </summary>
    /// <typeparam name="TEntity">实体类型</typeparam>
    /// <typeparam name="TKey">主键类型</typeparam>
    public interface IDbRepository<TEntity, TKey> : IDbRepository
        where TEntity : class
    {
        /// <summary>
        /// 根据主键获取实体。
        /// </summary>
        /// <param name="id">主键值。</param>
        /// <returns>对应的实体对象。</returns>
        TEntity GetById(TKey id);

        /// <summary>
        /// 异步根据主键获取实体。
        /// </summary>
        /// <param name="id">主键值。</param>
        /// <returns>对应的实体对象。</returns>
        Task<TEntity> GetByIdAsync(TKey id);

        /// <summary>
        /// 获取所有实体。
        /// </summary>
        /// <returns>实体集合的查询对象。</returns>
        IQueryable<TEntity> GetAll();

        /// <summary>
        /// 根据条件查询实体。
        /// </summary>
        /// <param name="predicate">查询条件表达式。</param>
        /// <returns>符合条件的实体集合的查询对象。</returns>
        IQueryable<TEntity> Find(Expression<Func<TEntity, bool>> predicate);

        /// <summary>
        /// 添加单个实体。
        /// </summary>
        /// <param name="entity">要添加的实体对象。</param>
        void Add(TEntity entity);

        /// <summary>
        /// 异步添加单个实体。
        /// </summary>
        /// <param name="entity">要添加的实体对象。</param>
        Task AddAsync(TEntity entity);

        /// <summary>
        /// 批量添加实体。
        /// </summary>
        /// <param name="entities">要添加的实体对象数组。</param>
        void AddRange(params TEntity[] entities);

        /// <summary>
        /// 异步批量添加实体。
        /// </summary>
        /// <param name="entities">要添加的实体对象数组。</param>
        Task AddRangeAsync(params TEntity[] entities);

        /// <summary>
        /// 更新实体。
        /// </summary>
        /// <param name="entity">要更新的实体对象。</param>
        void Update(TEntity entity);

        /// <summary>
        /// 删除实体。
        /// </summary>
        /// <param name="entity">要删除的实体对象。</param>
        void Delete(TEntity entity);

        /// <summary>
        /// 根据主键删除实体。
        /// </summary>
        /// <param name="id">主键值。</param>
        void DeleteById(TKey id);

        /// <summary>
        /// 批量删除实体。
        /// </summary>
        /// <param name="entities">要删除的实体对象数组。</param>
        void DeleteRange(params TEntity[] entities);

        /// <summary>
        /// 保存更改。
        /// </summary>
        /// <returns>受影响的行数。</returns>
        int SaveChanges();

        /// <summary>
        /// 异步保存更改。
        /// </summary>
        /// <returns>受影响的行数。</returns>
        Task<int> SaveChangesAsync();
    }
}