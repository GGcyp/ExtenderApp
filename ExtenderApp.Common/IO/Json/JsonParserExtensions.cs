using ExtenderApp.Abstract;
using ExtenderApp.Data;

namespace ExtenderApp.Common
{
    /// <summary>
    /// Json解析器扩展类
    /// </summary>
    public static class JsonParserExtensions
    {
        /// <summary>
        /// 序列化对象到文件
        /// </summary>
        /// <typeparam name="T">需要序列化的对象类型</typeparam>
        /// <param name="parser">IJsonParser接口的实现对象</param>
        /// <param name="info">本地文件信息对象</param>
        /// <param name="value">需要序列化的对象</param>
        /// <returns>如果序列化成功则返回true，否则返回false</returns>
        public static bool Serialize<T>(this IJsonParser parser, LocalFileInfo info, T value)
        {
            return parser.Serialize(info, value, null);
        }

        /// <summary>
        /// 将对象序列化为 JSON 字符串并保存到文件中。
        /// </summary>
        /// <typeparam name="T">要序列化的对象的类型。</typeparam>
        /// <param name="parser">JSON 解析器实例。</param>
        /// <param name="info">本地文件信息对象，包含文件的路径和名称。</param>
        /// <param name="value">要序列化的对象。</param>
        /// <param name="options">序列化选项对象。</param>
        /// <returns>如果序列化成功并成功保存到文件，则返回 true；否则返回 false。</returns>
        public static bool Serialize<T>(this IJsonParser parser, LocalFileInfo info, T value, object options)
        {
            return parser.Serialize(new FileOperateInfo(info), value, options);
        }
    }
}
