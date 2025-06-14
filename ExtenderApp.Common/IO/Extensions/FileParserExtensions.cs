using AppHost.Extensions.DependencyInjection;
using ExtenderApp.Abstract;
using ExtenderApp.Common.IO.Splitter;
using ExtenderApp.Common.IO;
using ExtenderApp.Common.IO.Binaries;
using ExtenderApp.Data;
using ExtenderApp.Common.Error;

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
        public static IServiceCollection AddIO(this IServiceCollection services)
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

        #region Read

        /// <summary>
        /// 使用指定的文件解析器从本地文件中读取数据并转换为指定类型
        /// </summary>
        /// <typeparam name="T">目标数据类型</typeparam>
        /// <param name="parser">文件解析器实例</param>
        /// <param name="info">本地文件信息</param>
        /// <returns>解析后的目标数据类型实例，如果读取失败或文件为空则返回null</returns>
        public static T? Read<T>(this IFileParser parser, LocalFileInfo info)
        {
            if (info.IsEmpty)
            {
                ErrorUtil.ArgumentNull(nameof(info));
            }

            return parser.Read<T>(info.CreateWriteOperate());
        }

        /// <summary>
        /// 使用指定的文件解析器从本地文件的指定位置和长度读取数据并转换为指定类型
        /// </summary>
        /// <typeparam name="T">目标数据类型</typeparam>
        /// <param name="parser">文件解析器实例</param>
        /// <param name="info">本地文件信息</param>
        /// <param name="position">读取起始位置</param>
        /// <param name="length">读取长度</param>
        /// <returns>解析后的目标数据类型实例，如果读取失败或文件为空则返回null</returns>
        public static T? Read<T>(this IFileParser parser, LocalFileInfo info, long position, long length)
        {
            if (info.IsEmpty)
            {
                ErrorUtil.ArgumentNull(nameof(info));
            }

            return parser.Read<T>(info.CreateWriteOperate());
        }

        #endregion

        #region ReadAsync

        /// <summary>
        /// 异步读取文件内容
        /// </summary>
        /// <typeparam name="T">读取内容的数据类型</typeparam>
        /// <param name="parser">文件解析器</param>
        /// <param name="info">文件信息</param>
        /// <param name="callback">读取完成后的回调函数，返回类型为T的泛型对象</param>
        /// <exception cref="ArgumentNullException">当info为空时抛出</exception>
        public static void ReadAsync<T>(this IFileParser parser, LocalFileInfo info, Action<T?> callback)
        {
            if (info.IsEmpty)
            {
                ErrorUtil.ArgumentNull(nameof(info));
            }

            parser.ReadAsync(info.CreateWriteOperate(), callback);
        }

        /// <summary>
        /// 异步读取文件部分内容
        /// </summary>
        /// <typeparam name="T">读取内容的数据类型</typeparam>
        /// <param name="parser">文件解析器</param>
        /// <param name="info">文件信息</param>
        /// <param name="position">读取的起始位置</param>
        /// <param name="length">读取的长度</param>
        /// <param name="callback">读取完成后的回调函数，返回类型为T的泛型对象</param>
        /// <exception cref="ArgumentNullException">当info为空时抛出</exception>
        public static void ReadAsync<T>(this IFileParser parser, LocalFileInfo info, long position, long length, Action<T?> callback)
        {
            if (info.IsEmpty)
            {
                ErrorUtil.ArgumentNull(nameof(info));
            }

            parser.ReadAsync(info.CreateWriteOperate(), position, length, callback);
        }

        #endregion

        #region Write

        /// <summary>
        /// 将指定值写入到指定文件。
        /// </summary>
        /// <typeparam name="T">要写入的值的类型。</typeparam>
        /// <param name="parser">文件解析器实例。</param>
        /// <param name="info">文件信息对象。</param>
        /// <param name="value">要写入的值。</param>
        /// <exception cref="ArgumentNullException">如果 <paramref name="info"/> 为空，则抛出此异常。</exception>
        public static void Write<T>(this IFileParser parser, LocalFileInfo info, T value)
        {
            if (info.IsEmpty)
            {
                ErrorUtil.ArgumentNull(nameof(info));
            }
            parser.Write(info.CreateWriteOperate(), value);
        }

        /// <summary>
        /// 将指定值写入到指定文件的指定位置。
        /// </summary>
        /// <typeparam name="T">要写入的值的类型。</typeparam>
        /// <param name="parser">文件解析器实例。</param>
        /// <param name="info">文件信息对象。</param>
        /// <param name="value">要写入的值。</param>
        /// <param name="position">要写入的起始位置。</param>
        /// <exception cref="ArgumentNullException">如果 <paramref name="info"/> 为空，则抛出此异常。</exception>
        public static void Write<T>(this IFileParser parser, LocalFileInfo info, T value, long position)
        {
            if (info.IsEmpty)
            {
                ErrorUtil.ArgumentNull(nameof(info));
            }
            parser.Write(info.CreateWriteOperate(), value, position);
        }

        #endregion
    }
}
