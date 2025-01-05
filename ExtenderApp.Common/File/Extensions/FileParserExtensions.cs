using AppHost.Extensions.DependencyInjection;
using ExtenderApp.Abstract;
using ExtenderApp.Common.File.Binary;
using ExtenderApp.Data;

namespace ExtenderApp.Common.File
{
    /// <summary>
    /// 文件解析器扩展类
    /// </summary>
    internal static class FileParserExtensions
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
            return services;
        }


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
    }
}
