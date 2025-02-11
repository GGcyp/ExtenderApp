using ExtenderApp.Abstract;
using ExtenderApp.Data;

namespace ExtenderApp.Service
{
    /// <summary>
    /// LocalDataServiceExtensions 类提供了对 ILocalDataService 接口的扩展方法。
    /// </summary>
    public static class LocalDataServiceExtensions
    {
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
        {
            if (details is null)
                throw new ArgumentNullException(nameof(details));

            return service.GetData(details.Title, out data);
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
        public static bool SaveData<T>(this ILocalDataService service, PluginDetails details, T? data)
        {
            if (details is null)
                throw new ArgumentNullException(nameof(details));

            return service.SetData(details.Title, data);
        }

        /// <summary>
        /// 为指定的本地数据服务设置数据。
        /// </summary>
        /// <typeparam name="T">数据的类型。</typeparam>
        /// <param name="service">本地数据服务接口。</param>
        /// <param name="dataName">数据的名称。</param>
        /// <param name="data">要设置的数据。</param>
        /// <returns>如果设置成功，则返回 true；否则返回 false。</returns>
        public static bool SetData<T>(this ILocalDataService service, string dataName, T? data)
        {
            return service.SetData(dataName, new LocalData<T>(data, null));
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
        public static bool SetData<T>(this ILocalDataService service, string dataName, T? data, Version? version)
        {
            return service.SetData(dataName, new LocalData<T>(data, version));
        }
    }
}
