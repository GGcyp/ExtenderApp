namespace ExtenderApp.Abstract
{
    /// <summary>
    /// Model层,Model接口
    /// </summary>
    public interface IModel
    {
        ///// <summary>
        ///// 模型转换启动器
        ///// </summary>
        //IModelConverterExecutor Converter { get; }

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
}