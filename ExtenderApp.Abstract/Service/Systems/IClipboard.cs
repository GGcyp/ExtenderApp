using System.Collections.Specialized;
using ExtenderApp.Contracts;

namespace ExtenderApp.Abstract
{
    /// <summary>
    /// 提供对系统剪贴板的抽象访问接口，支持多种数据格式的操作。
    /// </summary>
    public interface IClipboard
    {

        /// <summary>
        /// 清空剪贴板内容。
        /// </summary>
        void Clear();


        /// <summary>
        /// 检查剪贴板是否包含音频数据。
        /// </summary>
        /// <returns>如果包含音频数据返回 true，否则返回 false。</returns>
        bool ContainsAudio();

        /// <summary>
        /// 检查剪贴板是否包含指定格式的数据。
        /// </summary>
        /// <param name="format">数据格式标识符（如 "FileNameW" 表示文件列表）。</param>
        /// <returns>如果包含指定格式数据返回 true，否则返回 false。</returns>
        bool ContainsData(string format);

        /// <summary>
        /// 检查剪贴板是否包含文件拖放列表。
        /// </summary>
        /// <returns>如果包含文件列表返回 true，否则返回 false。</returns>
        bool ContainsFileDropList();

        /// <summary>
        /// 检查剪贴板是否包含文本数据（任意格式）。
        /// </summary>
        /// <returns>如果包含文本返回 true，否则返回 false。</returns>
        bool ContainsText();

        /// <summary>
        /// 检查剪贴板是否包含指定格式的文本数据。
        /// </summary>
        /// <param name="format">文本格式枚举值。</param>
        /// <returns>如果包含指定格式文本返回 true，否则返回 false。</returns>
        bool ContainsText(TextDataFormat format);




        /// <summary>
        /// 获取剪贴板中的纯文本内容。
        /// </summary>
        /// <returns>剪贴板中的文本字符串，如果无文本则返回空字符串。</returns>
        string GetText();

        /// <summary>
        /// 获取剪贴板中指定格式的文本内容。
        /// </summary>
        /// <param name="format">需要的文本格式。</param>
        /// <returns>格式化后的文本内容，如果格式不支持则返回 null。</returns>
        string GetText(TextDataFormat format);




        /// <summary>
        /// 设置剪贴板音频数据（字节数组形式）。
        /// </summary>
        /// <param name="audioBytes">音频数据的字节数组。</param>
        void SetAudio(byte[] audioBytes);

        /// <summary>
        /// 设置剪贴板音频数据（流形式）。
        /// </summary>
        /// <param name="audioStream">包含音频数据的流对象（调用方负责关闭流）。</param>
        void SetAudio(Stream audioStream);

        /// <summary>
        /// 设置剪贴板自定义数据对象。
        /// </summary>
        /// <param name="data">要放入剪贴板的数据对象。</param>
        void SetDataObject(object data);

        /// <summary>
        /// 设置剪贴板自定义数据对象并指定延迟渲染选项。
        /// </summary>
        /// <param name="data">要放入剪贴板的数据对象。</param>
        /// <param name="copy">true 表示延迟渲染（需要时才序列化），false 表示立即渲染。</param>
        void SetDataObject(object data, bool copy);

        /// <summary>
        /// 设置剪贴板文件拖放列表。
        /// </summary>
        /// <param name="fileDropList">包含文件路径的字符串集合。</param>
        void SetFileDropList(StringCollection fileDropList);

        /// <summary>
        /// 设置剪贴板纯文本内容。
        /// </summary>
        /// <param name="text">要设置的文本字符串。</param>
        void SetText(string text);

        /// <summary>
        /// 设置剪贴板指定格式的文本内容。
        /// </summary>
        /// <param name="text">要设置的文本字符串。</param>
        /// <param name="format">文本格式枚举值。</param>
        void SetText(string text, TextDataFormat format);
    }
}
