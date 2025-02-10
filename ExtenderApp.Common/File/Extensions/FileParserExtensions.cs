using AppHost.Extensions.DependencyInjection;
using ExtenderApp.Abstract;
using ExtenderApp.Common.File.Splitter;
using ExtenderApp.Common.Files;
using ExtenderApp.Common.Files.Binary;
using ExtenderApp.Common.Files.Splitter;
using ExtenderApp.Data;

namespace ExtenderApp.Common
{
    /// <summary>
    /// 文件解析器扩展类
    /// </summary>
    public static class FileParserExtensions
    {
        /// <summary>
        /// 为服务集合添加文件解析功能
        /// </summary>
        /// <param name="services">服务集合</param>
        /// <returns>返回修改后的服务集合</returns>
        public static IServiceCollection AddFile(this IServiceCollection services)
        {
            //Parser
            services.AddParser();

            //Binary
            services.AddBinary();

            //Splitter
            services.ConfigurationFileSplitter();

            return services;
        }

        /// <summary>
        /// 为服务集合添加JSON解析器
        /// </summary>
        /// <param name="services">服务集合</param>
        /// <returns>返回修改后的服务集合</returns>
        private static IServiceCollection AddParser(this IServiceCollection services)
        {
            services.AddSingleton<IJsonParser, JsonParser>();
            services.AddSingleton<IBinaryParser, BinaryParser>();
            services.AddSingleton<ISplitterParser, SplitterParser>();
            return services;
        }

        #region Serialize

        /// <summary>
        /// 将对象序列化为文件
        /// </summary>
        /// <typeparam name="T">对象的类型</typeparam>
        /// <param name="parser">文件解析器接口</param>
        /// <param name="info">本地文件信息</param>
        /// <param name="value">要序列化的对象</param>
        /// <returns>序列化是否成功</returns>
        public static bool Serialize<T>(this IFileParser parser, LocalFileInfo info, T value)
        {
            return parser.Serialize(info, value, null);
        }

        /// <summary>
        /// 将对象序列化为文件，并允许指定选项
        /// </summary>
        /// <typeparam name="T">对象的类型</typeparam>
        /// <param name="parser">文件解析器接口</param>
        /// <param name="info">本地文件信息</param>
        /// <param name="value">要序列化的对象</param>
        /// <param name="options">序列化选项</param>
        /// <returns>序列化是否成功</returns>
        public static bool Serialize<T>(this IFileParser parser, LocalFileInfo info, T value, object options)
        {
            return parser.Serialize(new FileOperate(info, FileMode.OpenOrCreate, FileAccess.ReadWrite), value, options);
        }

        #endregion

        #region Deserialize

        /// <summary>
        /// 将指定文件解析为指定类型的对象。
        /// </summary>
        /// <typeparam name="T">要解析成的类型。</typeparam>
        /// <param name="parser">文件解析器实例。</param>
        /// <param name="info">本地文件信息。</param>
        /// <returns>解析后的对象，如果解析失败则返回null。</returns>
        public static T? Deserialize<T>(this IFileParser parser, LocalFileInfo info)
        {
            return parser.Deserialize<T>(info, null);
        }

        /// <summary>
        /// 将指定文件解析为指定类型的对象。
        /// </summary>
        /// <typeparam name="T">要解析成的类型。</typeparam>
        /// <param name="parser">文件解析器实例。</param>
        /// <param name="info">本地文件信息。</param>
        /// <param name="options">解析选项。</param>
        /// <returns>解析后的对象，如果解析失败则返回null。</returns>
        public static T? Deserialize<T>(this IFileParser parser, LocalFileInfo info, object options)
        {
            return parser.Deserialize<T>(new FileOperate(info, FileMode.Open, FileAccess.Read), options);
        }

        /// <summary>
        /// 将指定文件操作解析为指定类型的对象。
        /// </summary>
        /// <typeparam name="T">要解析成的类型。</typeparam>
        /// <param name="parser">文件解析器实例。</param>
        /// <param name="operate">文件操作实例。</param>
        /// <param name="value">解析后的对象，如果解析失败则为null。</param>
        /// <returns>如果解析失败，则返回true；否则返回false。</returns>
        public static bool Deserialize<T>(this IFileParser parser, FileOperate operate, out T? value, object options = null)
        {
            value = parser.Deserialize<T>(operate, options);
            return value == null;
        }

        #endregion
    }
}
