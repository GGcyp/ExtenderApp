using ExtenderApp.Data;

namespace ExtenderApp.Abstract
{
    /// <summary>
    /// 模型转换策略接口
    /// </summary>
    public interface IModelConvertPolicy
    {
        /// <summary>
        /// 获取文件扩展名类型。
        /// </summary>
        /// <value>
        /// 返回表示文件扩展名类型的<see cref="ExtensionType"/>枚举值。
        /// </value>
        FileExtensionType ExtensionType { get; }

        /// <summary>
        /// 创建一个用于读取或写入数据的对象。
        /// </summary>
        /// <param name="fileAccess">文件访问权限，指定是读取、写入还是两者都进行。</param>
        /// <returns>返回一个用于读取或写入数据的对象。</returns>
        object CreateReadOrWriteData(FileAccess fileAccess);

        /// <summary>
        /// 将模型数据转换为指定格式的文件内容或从文件内容读取到模型中。
        /// </summary>
        /// <param name="model">要转换或填充数据的模型对象。</param>
        /// <param name="infoData">文件信息数据对象，包含文件路径、文件名等。</param>
        /// <param name="readOrWriteData">读取或写入的数据对象，根据操作类型决定。</param>
        /// <param name="objects">可选参数，包含其他与操作相关的对象数组。</param>
        void Convert(IModel model, FileInfoData infoData, object readOrWriteData, object objects);
    }
}
