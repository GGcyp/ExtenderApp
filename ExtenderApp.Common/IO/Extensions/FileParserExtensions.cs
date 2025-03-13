using AppHost.Extensions.DependencyInjection;
using ExtenderApp.Abstract;
using ExtenderApp.Common.IO.Splitter;
using ExtenderApp.Common.IO;
using ExtenderApp.Common.IO.Binaries;
using ExtenderApp.Data;

namespace ExtenderApp.Common
{
    /// <summary>
    /// 文件解析器扩展类
    /// </summary>
    public static class FileParserExtensions
    {
        /// <summary>
        /// 为服务集合添加文件相关服务
        /// </summary>
        /// <param name="services">服务集合</param>
        /// <returns>返回添加文件相关服务后的服务集合</returns>
        public static IServiceCollection AddFile(this IServiceCollection services)
        {
            //Parser
            services.AddParser();

            //Binary
            services.AddBinary();

            //Splitter
            services.ConfigurationFileSplitter();

            //StreamOperate
            services.AddFileStore();

            return services;
        }

        /// <summary>
        /// 为服务集合添加解析器服务
        /// </summary>
        /// <param name="services">服务集合</param>
        /// <returns>返回添加解析器服务后的服务集合</returns>
        private static IServiceCollection AddParser(this IServiceCollection services)
        {
            services.AddSingleton<IJsonParser, JsonParser>();
            services.AddSingleton<IBinaryParser, BinaryParser>();
            services.AddSingleton<ISplitterParser, SplitterParser>();
            return services;
        }

        /// <summary>
        /// 为服务集合添加文件存储服务
        /// </summary>
        /// <param name="services">服务集合</param>
        /// <returns>返回添加文件存储服务后的服务集合</returns>
        private static IServiceCollection AddFileStore(this IServiceCollection services)
        {
            services.AddSingleton<FileStore>();
            services.AddSingleton<IFileParserStore, FileParserStore>();
            return services;
        }

        #region Write

        /// <summary>
        /// 使用文件解析器将值写入文件
        /// </summary>
        /// <param name="parser">文件解析器</param>
        /// <param name="info">文件信息</param>
        /// <param name="value">要写入的值</param>
        /// <param name="fileOperate">文件操作接口</param>
        /// <param name="options">选项</param>
        /// <typeparam name="T">值的类型</typeparam>
        public static void Write<T>(this IFileParser parser, LocalFileInfo info, T value, IConcurrentOperate fileOperate)
        {
            parser.Write(new FileOperateInfo(info, FileMode.OpenOrCreate, FileAccess.ReadWrite), value, fileOperate);
        }

        #endregion

        #region Read

        /// <summary>
        /// 使用文件解析器从文件中读取值
        /// </summary>
        /// <param name="parser">文件解析器</param>
        /// <param name="info">文件信息</param>
        /// <param name="fileOperate">文件操作接口</param>
        /// <param name="options">选项</param>
        /// <returns>读取到的值</returns>
        /// <typeparam name="T">值的类型</typeparam>
        public static T? Read<T>(this IFileParser parser, LocalFileInfo info, IConcurrentOperate fileOperate)
        {
            return parser.Read<T>(new FileOperateInfo(info, FileMode.Open, FileAccess.Read), fileOperate);
        }

        #endregion
    }
}
