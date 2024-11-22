using ExtenderApp.Common;
using ExtenderApp.Common.File;


namespace ExtenderApp.Abstract
{
    /// <summary>
    /// 模型文件的存储与加载
    /// </summary>
    public static class IModelExtensions
    {
        /// <summary>
        /// 根据文件名、文件扩展类型和文件路径类型读取模型数据的扩展方法
        /// </summary>
        /// <param name="model">模型对象</param>
        /// <param name="fileName">文件名</param>
        /// <param name="extensionType">文件扩展类型</param>
        /// <param name="filePathType">文件路径类型</param>
        /// <returns>读取后的模型对象</returns>
        public static IModel ModelRead(this IModel model, string fileName, FileExtensionType extensionType, FileArchitectureInfo info)
        {
            return model.ModelConverter(new FileInfoData(fileName, extensionType, info, FileAccess.Read));
        }

        /// <summary>
        /// 根据文件路径读取模型数据的扩展方法
        /// </summary>
        /// <param name="model">模型对象</param>
        /// <param name="path">文件路径</param>
        /// <returns>读取后的模型对象</returns>
        public static IModel ModelRead(this IModel model, string path)
        {
            return model.ModelConverter(new FileInfoData(path, FileAccess.Read, FileMode.Open));
        }

        /// <summary>
        /// 根据文件名、文件扩展类型和文件路径类型写入模型数据的扩展方法
        /// </summary>
        /// <param name="model">模型对象</param>
        /// <param name="fileName">文件名</param>
        /// <param name="extensionType">文件扩展类型</param>
        /// <param name="filePathType">文件路径类型</param>
        /// <returns>写入后的模型对象</returns>
        public static IModel ModelWrite(this IModel model, string fileName, FileExtensionType extensionType, FileArchitectureInfo info)
        {
            return model.ModelConverter(new FileInfoData(fileName, extensionType, info, FileAccess.Write));
        }

        /// <summary>
        /// 根据文件路径写入模型数据的扩展方法
        /// </summary>
        /// <param name="model">模型对象</param>
        /// <param name="path">文件路径</param>
        /// <returns>写入后的模型对象</returns>
        public static IModel ModelWrite(this IModel model, string path)
        {
            return model.ModelConverter(new FileInfoData(path, FileAccess.Write));
        }

        /// <summary>
        /// 根据文件信息数据转换模型数据的扩展方法
        /// </summary>
        /// <param name="model">模型对象</param>
        /// <param name="fileInfo">文件信息数据</param>
        /// <returns>转换后的模型对象</returns>
        public static IModel ModelConverter(this IModel model, FileInfoData fileInfo)
        {
            var converter = model.Converter;
            converter.Execute(model, fileInfo);
            return model;
        }
    }
}
