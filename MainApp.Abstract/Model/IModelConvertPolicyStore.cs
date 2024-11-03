using MainApp.Common;

namespace MainApp.Abstract
{
    public interface IModelConvertPolicyStore : IList<IModelConvertPolicy>
    {
        /// <summary>
        /// 根据文件扩展类型查找模型转换策略。
        /// </summary>
        /// <param name="extensionType">文件扩展类型。</param>
        /// <returns>找到的模型转换策略，如果未找到则返回null。</returns>
        IModelConvertPolicy? Find(FileExtensionType extensionType);
    }

    /// <summary>
    /// 标志那个模型使用
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IModelConvertPolicyStore<T> : IModelConvertPolicyStore
    {
    }
}
