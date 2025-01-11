using ExtenderApp.Data;

namespace ExtenderApp.Abstract
{
    /// <summary>
    /// 本地数据服务接口
    /// </summary>
    public interface ILocalDataService
    {
        /// <summary>
        /// 根据名称获取数据。
        /// </summary>
        /// <typeparam name="T">数据的类型。</typeparam>
        /// <param name="dataName">数据的名称。</param>
        /// <param name="data">输出的数据，如果数据不存在则为null。</param>
        /// <returns>如果成功获取到数据则返回true，否则返回false。</returns>
        bool GetData<T>(string? dataName, out LocalData<T>? data);

        /// <summary>
        /// 设置指定名称的数据。
        /// </summary>
        /// <typeparam name="T">数据的类型。</typeparam>
        /// <param name="dataName">数据的名称。</param>
        /// <param name="data">要设置的数据。</param>
        /// <param name="version">数据的版本，如果为null，则使用最新版本。</param>
        /// <returns>如果设置成功，则返回true；否则返回false。</returns>
        bool SetData<T>(string? dataName, LocalData<T>? data);
    }
}
