using AppHost.Extensions.DependencyInjection;
using ExtenderApp.Abstract;
using ExtenderApp.Common.IO.Binaries.Formatter;
using ExtenderApp.Common.IO.Binaries.Formatter.Struct;
using ExtenderApp.Data;


namespace ExtenderApp.Common.IO.Binaries
{
    /// <summary>
    /// 提供二进制解析器扩展方法的静态内部类。
    /// </summary>
    internal static class BinaryParserExtensions
    {
        /// <summary>
        /// 为服务集合添加二进制格式化器。
        /// </summary>
        /// <param name="services">服务集合。</param>
        /// <returns>返回添加后的服务集合。</returns>
        public static IServiceCollection AddBinary(this IServiceCollection services)
        {
            services.AddSingleton<IBinaryFormatterResolver, BinaryFormatterResolver>();
            services.AddSingleton<BinaryConvert>();
            services.AddSingleton<BinaryOptions>();
            services.AddSingleton<ExtenderBinaryReaderConvert>();
            services.AddSingleton<ExtenderBinaryWriterConvert>();

            services.AddBinaryFormatter();

            return services;
        }

        /// <summary>
        /// 私有方法，用于向服务集合中添加二进制格式化器。
        /// </summary>
        /// <param name="services">服务集合。</param>
        private static IServiceCollection AddBinaryFormatter(this IServiceCollection services)
        {
            var store = new BinaryFormatterStore();

            store.AddStructFormatter<DateTime, DateTimeFormatter>();
            store.AddStructFormatter<TimeSpan, TimeSpanFormatter>();
            store.AddStructFormatter<Guid, GuidFormatter>();
            store.AddStructFormatter<short, Int16Formatter>();
            store.AddStructFormatter<int, Int32Formatter>();
            store.AddStructFormatter<long, Int64Formatter>();
            store.AddStructFormatter<ushort, UInt16Formatter>();
            store.AddStructFormatter<uint, UInt32Formatter>();
            store.AddStructFormatter<ulong, UInt64Formatter>();
            store.AddStructFormatter<bool, BooleanFormatter>();
            store.AddStructFormatter<byte, ByteFormatter>();
            store.AddStructFormatter<sbyte, SByteFormatter>();
            store.AddStructFormatter<double, DoubleFormatter>();
            store.AddStructFormatter<float, SingleFormatter>();
            store.AddStructFormatter<char, CharFormatter>();

            store.AddClassFormatter<string, StringFormatter>();
            store.AddClassFormatter<Version, VersionFoematter>();
            store.AddClassFormatter<Uri, UriFormatter>();

            return services.AddSingleton<IBinaryFormatterStore>(store);
        }
    }
}
