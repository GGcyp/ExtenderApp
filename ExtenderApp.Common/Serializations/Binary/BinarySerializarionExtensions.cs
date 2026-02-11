using System.Net;
using ExtenderApp.Abstract;
using ExtenderApp.Common.Serializations.Binary.Formatters;
using ExtenderApp.Common.Serializations.Binary.Formatters.Class;
using ExtenderApp.Common.Serializations.Binary.Formatters.Collection;
using ExtenderApp.Contracts;
using Microsoft.Extensions.DependencyInjection;

namespace ExtenderApp.Common.Serializations.Binary
{
    /// <summary>
    /// 提供用于注册二进制序列化组件和常用序列化/反序列化扩展方法的静态辅助类。 此类负责将格式化器、转换器和 <see cref="IBinarySerialization"/> 服务注册到 DI 容器。
    /// </summary>
    internal static class BinarySerializarionExtensions
    {
        /// <summary>
        /// 向 <see cref="IServiceCollection"/> 注册二进制序列化所需的服务和格式化器。
        /// </summary>
        /// <param name="services">目标服务集合。</param>
        /// <returns>传入的 <see cref="IServiceCollection"/>，便于链式调用。</returns>
        public static IServiceCollection AddBinary(this IServiceCollection services)
        {
            services.AddSingleton(typeof(IBinaryFormatter<>), typeof(GenericFormatter<>));
            services.AddSingleton<IBinaryFormatterResolver, BinaryFormatterResolver>();
            services.AddSingleton<IBinarySerialization, BinarySerialization>();

            services.AddBinaryFormatter();

            return services;
        }

        /// <summary>
        /// 向服务集合中添加内置的二进制格式化器并将格式化器存储注册为单例。 （私有实现细节，仅在内部使用。）
        /// </summary>
        /// <param name="services">目标服务集合。</param>
        private static IServiceCollection AddBinaryFormatter(this IServiceCollection services)
        {
            var store = new BinaryFormatterStore();

            store.AddUnManagedFormatter<byte>();
            store.AddUnManagedFormatter<ushort>();
            store.AddUnManagedFormatter<uint>();
            store.AddUnManagedFormatter<ulong>();
            store.AddUnManagedFormatter<sbyte>();
            store.AddUnManagedFormatter<short>();
            store.AddUnManagedFormatter<int>();
            store.AddUnManagedFormatter<long>();

            store.AddUnManagedFormatter<double>();
            store.AddUnManagedFormatter<float>();

            store.AddStructFormatter<Guid, GuidFormatter>();
            store.AddStructFormatter<char, CharFormatter>();
            store.AddStructFormatter<bool, BooleanFormatter>();
            store.AddStructFormatter<DateTime, DateTimeFormatter>();
            store.AddStructFormatter<TimeSpan, TimeSpanFormatter>();

            store.AddClassFormatter<Uri, UriFormatter>();
            store.AddClassFormatter<Type, TypeFormatter>();
            store.AddClassFormatter<string, StringFormatter>();
            store.AddClassFormatter<Version, VersionFoematter>();
            store.AddClassFormatter<IPAddress, IPAddressFormatter>();
            store.AddClassFormatter<IPEndPoint, IPEndPoinFormatter>();
            store.AddStructFormatter<LocalFileInfo, LocalFileInfoFormatter>();

            store.Add<Result, ResultFormatter>();
            store.AddByteArrayFormatter();

            return services.AddSingleton<IBinaryFormatterStore>(store);
        }

        /// <summary>
        /// 为 <see cref="IBinaryFormatterStore"/> 添加字节数组与相关集合的格式化器。 （私有实现细节，仅在内部使用。）
        /// </summary>
        /// <param name="store">要添加格式化器的格式化器存储实例。</param>
        private static void AddByteArrayFormatter(this IBinaryFormatterStore store)
        {
            store.AddNullableFormatter<byte>();
            store.Add<byte[], ByteArrayFormatter>();
            store.AddListFormatter<byte>();
            store.AddLinkedListFormatter<byte>();
            store.AddQueueFormatter<byte>();
            store.AddStackFormatter<byte>();
        }

        private static void AddUnManagedFormatter<T>(this IBinaryFormatterStore store) where T : unmanaged
        {
            store.Add<T, UnManagedFornatter<T>>();
        }
    }
}