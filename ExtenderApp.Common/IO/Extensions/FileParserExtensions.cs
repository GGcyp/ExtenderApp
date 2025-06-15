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
            services.AddSingleton<FileOperateStore>();
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
            info.ThrowFileNotFound();

            return parser.Read<T>(info.CreateReadWriteOperate());
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
            info.ThrowFileNotFound();

            return parser.Read<T>(info.CreateReadWriteOperate());
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
            info.ThrowFileNotFound();

            parser.ReadAsync(info.CreateReadWriteOperate(), callback);
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
            info.ThrowFileNotFound();

            parser.ReadAsync(info.CreateReadWriteOperate(), position, length, callback);
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
            info.ThrowFileNotFound();

            parser.Write(info.CreateReadWriteOperate(), value);
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
            info.ThrowFileNotFound();

            parser.Write(info.CreateReadWriteOperate(), value, position);
        }

        #endregion

        #region WriteAsync

        /// <summary>
        /// 以异步方式将值写入文件。
        /// </summary>
        /// <typeparam name="T">要写入的值的数据类型。</typeparam>
        /// <param name="parser">文件解析器实例。</param>
        /// <param name="info">包含文件信息的LocalFileInfo实例。</param>
        /// <param name="value">要写入的值。</param>
        /// <param name="callback">操作完成后的回调函数。</param>
        public static void WriteAsync<T>(this IFileParser parser, LocalFileInfo info, T value, Action callback = null)
        {
            info.ThrowFileNotFound();

            parser.WriteAsync(info.CreateReadWriteOperate(), value, callback);
        }


        /// <summary>
        /// 以异步方式将值写入文件的指定位置。
        /// </summary>
        /// <typeparam name="T">要写入的值的数据类型。</typeparam>
        /// <param name="parser">文件解析器实例。</param>
        /// <param name="info">包含文件信息的LocalFileInfo实例。</param>
        /// <param name="value">要写入的值。</param>
        /// <param name="position">要写入的起始位置。</param>
        /// <param name="callback">操作完成后的回调函数。</param>
        public static void WriteAsync<T>(this IFileParser parser, LocalFileInfo info, T value, long position, Action callback = null)
        {
            info.ThrowFileNotFound();

            parser.WriteAsync(info.CreateReadWriteOperate(), value, position, callback);
        }

        #endregion

        #region Get

        /// <summary>
        /// 根据给定的文件解析器和文件信息，获取文件的并发操作对象。
        /// </summary>
        /// <param name="parser">文件解析器。</param>
        /// <param name="info">本地文件信息。</param>
        /// <returns>文件的并发操作对象。</returns>
        public static IConcurrentOperate GetOperate(this IFileParser parser, LocalFileInfo info)
        {
            info.ThrowFileNotFound();
            return parser.GetOperate(info.CreateReadWriteOperate());
        }

        #endregion
    }
}
