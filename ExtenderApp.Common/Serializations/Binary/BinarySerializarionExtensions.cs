using System.Net;
using ExtenderApp.Abstract;
using ExtenderApp.Common.Serializations.Binary.Formatters;
using ExtenderApp.Common.Serializations.Binary.Formatters.Class;
using ExtenderApp.Common.Serializations.Binary.Formatters.Collection;
using ExtenderApp.Data;
using Microsoft.Extensions.DependencyInjection;

namespace ExtenderApp.Common.Serializations.Binary
{
    /// <summary>
    /// 提供用于注册二进制序列化组件和常用序列化/反序列化扩展方法的静态辅助类。
    /// 此类负责将格式化器、转换器和 <see cref="IBinarySerialization"/> 服务注册到 DI 容器。
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
            services.AddSingleton<BinaryConvert>();
            services.AddSingleton<BinaryOptions>();
            services.AddSingleton<ByteBufferConvert>();
            services.AddSingleton<IBinarySerialization, BinarySerialization>();

            services.AddBinaryFormatter();

            return services;
        }

        /// <summary>
        /// 向服务集合中添加内置的二进制格式化器并将格式化器存储注册为单例。
        /// （私有实现细节，仅在内部使用。）
        /// </summary>
        /// <param name="services">目标服务集合。</param>
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

            store.AddStructFormatter<BitFieldData, BitFieldDataFormatter>();
            store.AddStructFormatter<LocalFileInfo, LocalFileInfoFormatter>();

            store.Add<ByteBlock, ByteBlockFormatter>();
            store.Add<Result, ResultFormatter>();
            store.AddByteArrayFormatter();

            return services.AddSingleton<IBinaryFormatterStore>(store);
        }

        /// <summary>
        /// 为 <see cref="IBinaryFormatterStore"/> 添加字节数组与相关集合的格式化器。
        /// （私有实现细节，仅在内部使用。）
        /// </summary>
        /// <param name="store">要添加格式化器的格式化器存储实例。</param>
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

        #region Serialize

        /// <summary>
        /// 使用已配置的 <see cref="IBinarySerialization"/> 将指定值序列化并写入到 <see cref="ByteBuffer"/>。
        /// </summary>
        /// <typeparam name="T">要序列化的值类型或引用类型。</typeparam>
        /// <param name="binarySerialization">用于执行序列化的实例。</param>
        /// <param name="buffer">目标 <see cref="ByteBuffer"/>（以 ref 传递，将被写入数据）。</param>
        /// <param name="value">要序列化的值。</param>
        /// <exception cref="ArgumentNullException"><paramref name="binarySerialization"/> 为 null 时抛出。</exception>
        public static void Serialize<T>(this IBinarySerialization binarySerialization, ref ByteBuffer buffer, T value)
        {
            ArgumentNullException.ThrowIfNull(binarySerialization);

            binarySerialization.Serialize(value, out var outBuffer);
            buffer.Write(outBuffer);
            outBuffer.Dispose();
        }

        /// <summary>
        /// 使用已配置的 <see cref="IBinarySerialization"/> 将指定值序列化并写入到 <see cref="ByteBlock"/>。
        /// </summary>
        /// <typeparam name="T">要序列化的值类型或引用类型。</typeparam>
        /// <param name="binarySerialization">用于执行序列化的实例。</param>
        /// <param name="block">目标 <see cref="ByteBlock"/>（以 ref 传递，将被写入数据）。</param>
        /// <param name="value">要序列化的值。</param>
        /// <exception cref="ArgumentNullException"><paramref name="binarySerialization"/> 为 null 时抛出。</exception>
        public static void Serialize<T>(this IBinarySerialization binarySerialization, ref ByteBlock block, T value)
        {
            ArgumentNullException.ThrowIfNull(binarySerialization);

            binarySerialization.Serialize(value, out var outBuffer);
            block.Write(outBuffer);
            outBuffer.Dispose();
        }

        #endregion Serialize

        #region Deserialize

        /// <summary>
        /// 从 <see cref="ByteBuffer"/> 中反序列化出指定类型的值。
        /// </summary>
        /// <typeparam name="T">要反序列化的目标类型。</typeparam>
        /// <param name="binarySerialization">用于执行反序列化的实例。</param>
        /// <param name="buffer">包含序列化数据的 <see cref="ByteBuffer"/>（以 ref 传递，格式化器可能会读取位置）。</param>
        /// <returns>反序列化得到的值；当没有可用格式化器或失败时返回默认值。</returns>
        /// <exception cref="ArgumentNullException"><paramref name="binarySerialization"/> 为 null 时抛出。</exception>
        public static T? Deserialize<T>(this IBinarySerialization binarySerialization, ref ByteBuffer buffer)
        {
            ArgumentNullException.ThrowIfNull(binarySerialization);

            if (binarySerialization.TryGetFormatter<T>(out var formatter))
            {
                return formatter.Deserialize(ref buffer);
            }
            return default;
        }

        /// <summary>
        /// 从 <see cref="ByteBlock"/> 中反序列化出指定类型的值，并按反序列化消耗量推进 <see cref="ByteBlock"/> 的读取位置。
        /// </summary>
        /// <typeparam name="T">要反序列化的目标类型。</typeparam>
        /// <param name="binarySerialization">用于执行反序列化的实例。</param>
        /// <param name="block">包含序列化数据的 <see cref="ByteBlock"/>（以 ref 传递，调用后已按消耗量推进读取位置）。</param>
        /// <returns>反序列化得到的值；当没有可用格式化器或失败时返回默认值。</returns>
        /// <exception cref="ArgumentNullException"><paramref name="binarySerialization"/> 为 null 时抛出。</exception>
        public static T? Deserialize<T>(this IBinarySerialization binarySerialization, ref ByteBlock block)
        {
            ArgumentNullException.ThrowIfNull(binarySerialization);

            ByteBuffer buffer = new(block);
            var result = binarySerialization.Deserialize<T>(ref buffer);
            block.ReadAdvance((int)buffer.Consumed);
            buffer.Dispose();
            return result;
        }

        #endregion Deserialize
    }
}