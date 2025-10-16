using System.Buffers;
using System.Net;
using System.Reflection;
using AppHost.Extensions.DependencyInjection;
using ExtenderApp.Abstract;
using ExtenderApp.Common.IO.Binary.Formatters;
using ExtenderApp.Common.IO.Binary.Formatters.Collection;
using ExtenderApp.Common.IO.Binary.Formatters;
using ExtenderApp.Common.IO.Binary.Formatters;
using ExtenderApp.Common.IO.Binary.Formatters.Class;
using ExtenderApp.Common.IO.Binary.Formatters;
using ExtenderApp.Data;


namespace ExtenderApp.Common.IO.Binary
{
    /// <summary>
    /// 提供二进制解析器扩展方法的静态内部类。
    /// </summary>
    internal static class BinaryParserExtensions
    {
        private static readonly MethodInfo _getFormatterMethodInfo = typeof(IBinaryFormatterResolver).GetMethod("GetFormatter");

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
            services.AddSingleton<ByteBufferConvert>();
            services.AddSingleton(new SequencePool<byte>());

            services.AddSingleton(typeof(IBinaryFormatter<>), (p, o) =>
            {
                Type[] types = o as Type[];
                Type type = types[0];

                IBinaryFormatterResolver resolver = p.GetRequiredService<IBinaryFormatterResolver>();

                MethodInfo method = _getFormatterMethodInfo.MakeGenericMethod(type);
                var result = method.Invoke(resolver, null);
                return result;
            });

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

            store.AddStructFormatter<Nil, NilFormatter>();
            store.AddStructFormatter<Guid, GuidFormatter>();
            store.AddStructFormatter<int, Int32Formatter>();
            store.AddStructFormatter<char, CharFormatter>();
            store.AddStructFormatter<long, Int64Formatter>();
            store.AddStructFormatter<short, Int16Formatter>();
            store.AddStructFormatter<sbyte, SByteFormatter>();
            store.AddStructFormatter<uint, UInt32Formatter>();
            store.AddStructFormatter<bool, BooleanFormatter>();
            store.AddStructFormatter<ulong, UInt64Formatter>();
            store.AddStructFormatter<float, SingleFormatter>();
            store.AddStructFormatter<ushort, UInt16Formatter>();
            store.AddStructFormatter<double, DoubleFormatter>();
            store.AddStructFormatter<DateTime, DateTimeFormatter>();
            store.AddStructFormatter<TimeSpan, TimeSpanFormatter>();

            store.AddClassFormatter<Uri, UriFormatter>();
            store.AddClassFormatter<Type, TypeFormatter>();
            store.AddClassFormatter<string, StringFormatter>();
            store.AddClassFormatter<Version, VersionFoematter>();
            store.AddClassFormatter<IPAddress, IPAddressFormatter>();
            store.AddClassFormatter<IPEndPoint, IPEndPoinFormatter>();
            store.AddClassFormatter<BitFieldData, BitFieldDataFormatter>();

            store.AddStructFormatter<LocalFileInfo, LocalFileInfoFormatter>();
            store.AddStructFormatter<ExtensionHeader, ExtensionHeaderFormatter>();

            store.AddByteArrayFormatter();

            return services.AddSingleton<IBinaryFormatterStore>(store);
        }

        /// <summary>
        /// 为指定的 <see cref="IBinaryFormatterStore"/> 实例添加字节数组格式化器。
        /// </summary>
        /// <param name="store">需要添加格式化器的 <see cref="IBinaryFormatterStore"/> 实例。</param>
        private static void AddByteArrayFormatter(this IBinaryFormatterStore store)
        {
            store.Add<byte, ByteFormatter>();
            store.AddNullableFormatter<byte>();
            store.Add<byte[], ByteArrayFormatter>();
            store.AddListFormatter<byte>();
            store.AddLinkedListFormatter<byte>();
            store.AddQueueFormatter<byte>();
            store.AddStackFormatter<byte>();
        }
    }
}
