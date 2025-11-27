using ExtenderApp.Abstract;
using ExtenderApp.Common;
using ExtenderApp.Data;

namespace ExtenderApp.Services
{
    /// <summary>
    /// LocalDataServiceExtensions 类提供了对 ILocalDataService 接口的扩展方法。
    /// </summary>
    public static class LocalDataServiceExtensions
    {
        /// <summary>
        /// 为指定的本地数据类型添加一个本地数据格式化器到格式化器存储中
        /// </summary>
        /// <typeparam name="TLoclaData">本地数据类型</typeparam>
        /// <typeparam name="TFormatter">数据格式化器类型，必须实现IVersionDataFormatter&lt;TLoclaData&gt;接口</typeparam>
        /// <param name="store">格式化器存储实例，通过扩展方法方式调用</param>
        public static void AddLocalDataFormatter<TLoclaData, TFormatter>(this IBinaryFormatterStore store)
            where TFormatter : IVersionDataFormatter<TLoclaData>
        {
            // 调用格式化器存储的AddVersionData方法，将本地数据类型和对应的格式化器类型添加到存储中
            store.AddVersionData<TLoclaData, TFormatter>();
        }

        /// <summary>
        /// 从本地数据服务中获取指定类型的数据。
        /// </summary>
        /// <typeparam name="T">要获取的数据的类型。</typeparam>
        /// <param name="service">ILocalDataService 接口的实现实例。</param>
        /// <param name="details">包含数据标题的 ModDetails 实例。</param>
        /// <param name="data">输出参数，用于接收获取到的数据。</param>
        /// <returns>如果成功获取数据，则返回 true；否则返回 false。</returns>
        /// <exception cref="ArgumentNullException">如果 details 参数为 null，则抛出 ArgumentNullException 异常。</exception>
        public static bool LoadData<T>(this ILocalDataService service, PluginDetails details, out LocalData<T>? data)
            where T : class, new()
        {
            if (details is null)
                throw new ArgumentNullException(nameof(details));

            return service.LoadData(details.Title, out data);
        }

        /// <summary>
        /// 将指定类型的数据保存到本地数据服务中。
        /// </summary>
        /// <typeparam name="T">要保存的数据的类型。</typeparam>
        /// <param name="service">ILocalDataService 接口的实现实例。</param>
        /// <param name="details">包含数据标题的 ModDetails 实例。</param>
        /// <param name="data">要保存的数据。</param>
        /// <returns>如果数据保存成功，则返回 true；否则返回 false。</returns>
        /// <exception cref="ArgumentNullException">如果 details 参数为 null，则抛出 ArgumentNullException 异常。</exception>
        public static bool SaveData<T>(this ILocalDataService service, PluginDetails details, T data)
            where T : class
        {
            if (details is null)
                throw new ArgumentNullException(nameof(details));

            return service.SaveData(details.Title, data, details.Version);
        }

        /// <summary>
        /// 为指定的本地数据服务设置数据，并指定版本。
        /// </summary>
        /// <typeparam name="T">数据的类型。</typeparam>
        /// <param name="service">本地数据服务接口。</param>
        /// <param name="dataName">数据的名称。</param>
        /// <param name="data">要设置的数据。</param>
        /// <param name="version">数据的版本。</param>
        /// <returns>如果设置成功，则返回 true；否则返回 false。</returns>
        public static bool SaveData<T>(this ILocalDataService service, string dataName, T data, Version? version)
            where T : class
        {
            return service.SaveData(dataName, new LocalData<T>(data, null, version));
        }

        /// <summary>
        /// 从本地数据服务中删除指定插件的数据。
        /// 通过 PluginDetails 的 Title 属性定位要删除的数据项。
        /// </summary>
        /// <param name="service">ILocalDataService 接口的实现实例。</param>
        /// <param name="details">包含插件标题的 PluginDetails 实例。</param>
        /// <returns>如果删除成功则返回 true，否则返回 false。</returns>
        /// <exception cref="ArgumentNullException">如果 details 参数为 null，则抛出异常。</exception>
        public static bool DeleteData(this ILocalDataService service, PluginDetails details)
        {
            if (details is null)
                throw new ArgumentNullException(nameof(details));
            return service.DeleteData(details.Title);
        }
    }
}
