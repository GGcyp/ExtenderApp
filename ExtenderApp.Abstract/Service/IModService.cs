using ExtenderApp.Data;

namespace ExtenderApp.Abstract
{
    /// <summary>
    /// 模组服务接口
    /// </summary>
    public interface IModService
    {
        /// <summary>
        /// 获取指定MOD的详细信息。
        /// </summary>
        /// <param name="modStartDLLName">MOD启动DLL的名称。</param>
        /// <returns>返回包含MOD详细信息的ModDetails对象，如果未找到则返回null。</returns>
        ModDetails? GetModDetails(string modStartDLLName);

        /// <summary>
        /// 加载模组信息
        /// </summary>
        /// <param name="modFolderPath">模组文件夹路径</param>
        /// <remarks>
        /// 此方法用于加载指定路径下的模组信息。
        /// </remarks>
        void LoadModInfo(string modFolderPath = null);

        /// <summary>
        /// 加载模组
        /// </summary>
        /// <param name="details">模组详细信息</param>
        void LoadMod(ModDetails details);

        /// <summary>
        /// 卸载模组
        /// </summary>
        /// <param name="details">模组详细信息</param>
        void UnloadMod(ModDetails details);
    }
}
