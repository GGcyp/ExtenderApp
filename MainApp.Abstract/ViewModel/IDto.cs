

namespace MainApp.Abstract
{
    /// <summary>
    /// DTO（数据传输对象）接口
    /// </summary>
    public interface IDto
    {
        /// <summary>
        /// 更新DTO中的实体
        /// </summary>
        /// <param name="entity">将要更新的实体</param>
        void UpdateEntity(object? entity);
    }

    /// <summary>
    /// DTO（数据传输对象）接口
    /// </summary>
    /// <typeparam name="TEntity">实体类型，必须为类类型</typeparam>
    public interface IDto<TEntity> : IDto where TEntity : class
    {
        /// <summary>
        /// 更新DTO中的实体
        /// </summary>
        /// <param name="entity">将要更新的实体</param>
        void UpdateEntity(TEntity? entity);
    }
}
