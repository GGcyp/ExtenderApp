
namespace ExtenderApp.Data
{
    /// <summary>
    /// 表示本地数据的抽象基类，包含版本信息和数据保存功能
    /// </summary>
    [Serializable]
    public abstract class LocalData
    {
        /// <summary>
        /// 获取或设置本地数据的版本信息
        /// </summary>
        public Version Version { get; set; }

        /// <summary>
        /// 初始化LocalData的新实例
        /// </summary>
        /// <param name="version">本地数据的版本信息</param>
        public LocalData(Version version)
        {
            Version = version;
        }

        /// <summary>
        /// 抽象方法，用于保存本地数据到指定位置
        /// </summary>
        /// <param name="info">包含预期本地文件信息的对象</param>
        public abstract void SaveData(ExpectLocalFileInfo info);
    }

    /// <summary>
    /// 泛型本地数据类，继承自LocalData，提供具体的数据存储和保存功能
    /// </summary>
    /// <typeparam name="T">本地数据的具体类型</typeparam>
    [Serializable]
    public class LocalData<T> : LocalData
    {
        /// <summary>
        /// 获取或设置保存数据的动作委托
        /// </summary>
        public Action<ExpectLocalFileInfo, Version, T> SaveAcion { get; set; }

        /// <summary>
        /// 获取本地数据（只读）
        /// </summary>
        public T Data { get; }

        /// <summary>
        /// 初始化LocalData<T>的新实例
        /// </summary>
        /// <param name="data">要存储的本地数据</param>
        /// <param name="saveAction">保存数据的动作委托</param>
        /// <param name="version">本地数据的版本信息</param>
        public LocalData(T data, Action<ExpectLocalFileInfo, Version, T> saveAction, Version version) : base(version)
        {
            Data = data;
            SaveAcion = saveAction;
        }

        /// <summary>
        /// 重写基类方法，执行保存数据的动作
        /// </summary>
        /// <param name="info">包含预期本地文件信息的对象</param>
        public override void SaveData(ExpectLocalFileInfo info)
        {
            SaveAcion.Invoke(info, Version, Data);
        }

        /// <summary>
        /// 将当前本地数据转换为VersionData<T>对象
        /// </summary>
        /// <returns>包含版本信息和数据的VersionData<T>对象</returns>
        public VersionData<T> ToVersionData()
        {
            return new VersionData<T>(Version, Data);
        }
    }
}
