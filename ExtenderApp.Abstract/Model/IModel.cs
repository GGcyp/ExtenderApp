
namespace ExtenderApp.Abstract
{
    /// <summary>
    /// Model层,Model接口
    /// </summary>
    public interface IModel
    {
        /// <summary>
        /// 模型转换启动器
        /// </summary>
        IModelConverterExecutor Converter { get; }

        /// <summary>
        /// 向数据源添加数据。
        /// </summary>
        /// <param name="data">要添加到数据源中的数据对象。</param>
        void AddDataSource(object? data);

        /// <summary>
        /// 从数据源中获取数据，并且此数据为模型中的所有数据。
        /// </summary>
        /// <returns>数据源中存储的数据对象，如果数据源为空或不存在则返回null。</returns>
        object? GetDataSource();
    }

    /// <summary>
    /// Model层,Model接口
    /// </summary>
    public interface IModel<TDto> : IModel where TDto : class,IDto,new()
    {
        /// <summary>
        /// 添加数据
        /// </summary>
        /// <param name="dto"></param>
        void Add(TDto dto);

        /// <summary>
        /// 删除所有数据
        /// </summary>
        void Clear();

        /// <summary>
        /// 删除指定数据
        /// </summary>
        /// <param name="dto"></param>
        void Remove(TDto dto);

        /// <summary>
        /// 传入一个key获取指定值
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        TDto? Get(object key);

        /// <summary>
        /// 遍历所有数据
        /// </summary>
        /// <param name="action">遍历委托</param>
        void ForEach(Action<TDto> action);
    }
}
