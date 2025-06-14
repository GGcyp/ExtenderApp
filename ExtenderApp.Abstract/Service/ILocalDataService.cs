using ExtenderApp.Data;

namespace ExtenderApp.Abstract
{
    /// <summary>
    /// 本地数据服务接口
    /// </summary>
    public interface ILocalDataService
    {
        /// <summary>
        /// 加载指定名称的数据。
        /// </summary>
        /// <typeparam name="T">数据的类型。</typeparam>
        /// <param name="dataName">数据的名称。</param>
        /// <param name="data">加载的数据，如果数据不存在则为null。</param>
        /// <returns>如果加载成功返回true，否则返回false。</returns>
        bool LoadData<T>(string? dataName, out LocalData<T>? data) where T : class;

        /// <summary>
        /// 保存指定名称的数据。
        /// </summary>
        /// <typeparam name="T">数据的类型。</typeparam>
        /// <param name="dataName">数据的名称。</param>
        /// <param name="data">要保存的数据，可以为null。</param>
        /// <returns>如果保存成功返回true，否则返回false。</returns>
        bool SaveData<T>(string? dataName, LocalData<T>? data) where T : class;
    }
}
