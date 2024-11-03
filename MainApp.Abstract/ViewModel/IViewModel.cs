using MainApp.Common;
using MainApp.Common.File;

namespace MainApp.Abstract
{
    public interface IViewModel
    {
        ///// <summary>
        ///// 读取文件内容
        ///// </summary>
        ///// <param name="path">文件路径</param>
        ///// <param name="callBack">读取成功回调函数</param>
        ///// <param name="errorCallBack">读取失败回调函数</param>
        //void Read(string path, Action? callBack = null, Action<string>? errorCallBack = null);

        ///// <summary>
        ///// 读取指定文件名和扩展名类型的文件内容
        ///// </summary>
        ///// <param name="fileName">文件名</param>
        ///// <param name="extensionType">文件扩展名类型</param>
        ///// <param name="filePathType">文件路径类型</param>
        ///// <param name="callBack">读取成功回调函数</param>
        ///// <param name="errorCallBack">读取失败回调函数</param>
        //void Read(string fileName, FileExtensionType extensionType, FileArchitectureInfo info, Action? callBack = null, Action<string>? errorCallBack = null);

        ///// <summary>
        ///// 写入文件内容
        ///// </summary>
        ///// <param name="path">文件路径</param>
        ///// <param name="callBack">写入成功回调函数</param>
        ///// <param name="errorCallBack">写入失败回调函数</param>
        //void Write(string path, Action? callBack = null, Action<string>? errorCallBack = null);

        ///// <summary>
        ///// 写入指定文件名、扩展名类型和路径类型的文件内容
        ///// </summary>
        ///// <param name="fileName">文件名</param>
        ///// <param name="extensionType">文件扩展名类型</param>
        ///// <param name="filePathType">文件路径类型</param>
        ///// <param name="callBack">写入成功回调函数</param>
        ///// <param name="errorCallBack">写入失败回调函数</param>
        //void Write(string fileName, FileExtensionType extensionType, FileArchitectureInfo info, Action? callBack = null, Action<string>? errorCallBack = null);
    }
}
