using ExtenderApp.Abstract;
using ExtenderApp.Common.ObjectPool;

namespace ExtenderApp.ViewModels
{
    public struct DtoPool<TDto> where TDto : class,IDto,new()
    {
        private ObjectPool<TDto> pool;

        public DtoPool()
        {
            pool = ObjectPool.Create<TDto>();
        }

        private void Expansion()
        {
            if (pool is not null) return;
            pool = ObjectPool.Create<TDto>();
        }

        /// <summary>
        /// 获取指定类型的DTO对象。
        /// </summary>
        /// <param name="entity">可选参数，用于更新DTO对象的实体对象。</param>
        /// <returns>返回指定类型的DTO对象。</returns>
        public TDto Get(object? entity = null)
        {
            Expansion();

            TDto dto = pool.Get();
            dto.UpdateEntity(entity);
            return dto;
        }

        /// <summary>
        /// 释放指定类型的DTO对象到对象池。
        /// 会自动更新为null
        /// </summary>
        /// <param name="dto">需要释放的DTO对象。</param>
        public void Release(TDto dto)
        {
            Expansion();

            if (dto == null) return;
            dto.UpdateEntity(null);
            pool.Release(dto);
        }
    }
}
