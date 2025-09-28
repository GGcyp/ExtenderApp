
namespace ExtenderApp.DbEntities
{
    /// <summary>
    /// 数据库实体基类。
    /// 提供通用的主键属性，所有数据库实体应继承此类以统一主键定义。
    /// </summary>
    /// <typeparam name="TKey">主键类型。</typeparam>
    public class DbEntity<TKey>
    {
        /// <summary>
        /// 实体主键。
        /// </summary>
        public TKey Id { get; set; }
    }
}
