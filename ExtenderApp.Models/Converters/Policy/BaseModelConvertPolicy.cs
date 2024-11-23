using ExtenderApp.Abstract;
using ExtenderApp.Data;

namespace ExtenderApp.Models.Converters
{
    /// <summary>
    /// 模型转换数据基类
    /// </summary>
    /// <typeparam name="TData">模型与文件相关的特定类型数据</typeparam>
    public abstract class BaseModelConvertPolicy<TReadData, TWriteData> : IModelConvertPolicy where TReadData : class, new() where TWriteData : class, new()
    {
        public abstract FileExtensionType ExtensionType { get; }

        public object CreateReadOrWriteData(FileAccess fileAccess)
        {
            return fileAccess switch
            {
                FileAccess.Read => CreateReadData(),
                FileAccess.Write => CreateWriteData(),
                FileAccess.ReadWrite => (CreateReadData(), CreateWriteData())
            };
        }

        /// <summary>
        /// 创建一个用于读取数据的实例。
        /// </summary>
        /// <returns>返回一个<typeparamref name="TReadData"/>类型的实例。</returns>
        protected abstract TReadData CreateReadData();

        /// <summary>
        /// 创建一个用于写入数据的实例。
        /// </summary>
        /// <returns>返回一个<typeparamref name="TReadData"/>类型的实例。</returns>
        protected abstract TWriteData CreateWriteData();

        public void Convert(IModel model, FileInfoData infoData, object readOrWriteData, object fileData)
        {
            switch (infoData.FileAccess)
            {
                case FileAccess.Read:
                    TReadData readData = readOrWriteData as TReadData;
                    if (readData == null && readOrWriteData != null)
                    {
                        throw new ArgumentNullException(nameof(TReadData));
                    }
                    ConvertToModel(model, infoData, readData, fileData);
                    break;
                case FileAccess.Write:
                    TWriteData writeData = readOrWriteData as TWriteData;
                    if (writeData == null && readOrWriteData != null)
                    {
                        throw new ArgumentNullException(nameof(TWriteData));
                    }
                    ConvertToFile(model, infoData, writeData, fileData);
                    break;
            }
        }

        /// <summary>
        /// 将模型转换为文件。
        /// </summary>
        /// <param name="model">待转换的模型对象。</param>
        /// <param name="infoData">文件信息数据。</param>
        /// <param name="writeData">写入文件的数据。</param>
        /// <param name="fileData">文件数据数组。</param>
        /// <remarks>这是一个抽象方法，需要在派生类中实现。</remarks>
        protected abstract void ConvertToFile(IModel model, FileInfoData infoData, TWriteData writeData, object fileData);

        /// <summary>
        /// 从文件中转换为模型。
        /// </summary>
        /// <param name="model">待转换的模型对象。</param>
        /// <param name="infoData">文件信息数据。</param>
        /// <param name="readData">从文件中读取的数据。</param>
        /// <param name="fileData">文件数据数组。</param>
        /// <remarks>这是一个抽象方法，需要在派生类中实现。</remarks>
        protected abstract void ConvertToModel(IModel model, FileInfoData infoData, TReadData readData, object fileData);
    }
}
