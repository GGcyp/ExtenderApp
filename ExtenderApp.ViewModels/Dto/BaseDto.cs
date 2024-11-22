using ExtenderApp.Abstract;

namespace ExtenderApp.ViewModels
{
    /// <summary>
    /// 泛型基类BaseDot，继承自BaseDot，用于指定类型的实体更新。
    /// </summary>
    /// <typeparam name="TEntity">实体类型，必须是一个类。</typeparam>
    public class BaseDto<TEntity> : IDto<TEntity> where TEntity : class
    {
        /// <summary>
        /// 获取或设置当前实体对象。
        /// </summary>
        public virtual TEntity? Entity { get; private set; }

        public virtual void UpdateEntity(object? obj)
        {
            switch (obj)
            {
                case null:
                    UpdateEntity(null);
                    break;

                case TEntity entity:
                    UpdateEntity(entity);
                    break;

                    default:
                    throw new InvalidCastException(nameof(obj));
            }
        }

        public void UpdateEntity(TEntity? entity)
        {
            //if (entity == null)
            //    throw new ArgumentNullException(nameof(entity));

            if (entity == Entity) return;
            Entity = entity;
            if (Entity is not null) OnEntityChanged();
        }

        /// <summary>
        /// 更新实体对象。
        /// </summary>
        protected virtual void OnEntityChanged()
        {
            // 空方法，用于子类重写。
        }
    }
}
