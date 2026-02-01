using System.Buffers;
using ExtenderApp.Abstract;
using ExtenderApp.Common.Serializations.Binary;
using ExtenderApp.Common.Serializations.Json;
using ExtenderApp.Data;
using Microsoft.Extensions.DependencyInjection;

namespace ExtenderApp.Common
{
    /// <summary>
    /// 序列化相关的扩展方法集合。
    /// <para>包含将序列化实现注册到依赖注入容器，以及针对 <see cref="ISerialization"/> 的便捷读写扩展方法。</para>
    /// </summary>
    public static class SerializationExtensions
    {
        /// <summary>
        /// 将序列化服务注册到指定的 <see cref="IServiceCollection"/> 中。
        /// </summary>
        /// <param name="services">目标 <see cref="IServiceCollection"/> 实例。</param>
        /// <returns>返回传入的 <see cref="IServiceCollection"/>，以支持链式调用。</returns>
        public static IServiceCollection AddSerializations(this IServiceCollection services)
        {
            services.AddSingleton<IJsonSerialization, JsonSerialization>();
            services.AddBinary();
            return services;
        }

        #region Serialization

        /// <summary>
        /// 将对象序列化并写入由 <see cref="IFileOperateProvider"/> 根据 <see cref="FileOperateInfo"/> 创建的文件操作器中。
        /// </summary>
        /// <typeparam name="T">要序列化的对象类型。</typeparam>
        /// <param name="serialization">用于序列化的 <see cref="ISerialization"/> 实例。</param>
        /// <param name="obj">要序列化并写入的对象。</param>
        /// <param name="provider">用于获取 <see cref="IFileOperate"/> 的提供者。</param>
        /// <param name="info">用于创建文件操作器的文件操作配置信息。</param>
        /// <param name="compression">可选的压缩实现；若提供则在写入前尝试压缩序列化结果。</param>
        /// <param name="position">写入文件的起始位置（字节偏移）。</param>
        /// <exception cref="ArgumentNullException">当 <paramref name="provider"/> 为 <c>null</c> 时抛出。</exception>
        public static void Serialize<T>(this ISerialization serialization, T obj, IFileOperateProvider provider, FileOperateInfo info, ICompression? compression = null, long position = 0)
        {
            ArgumentNullException.ThrowIfNull(provider);
            serialization.Serialize(obj, provider.GetOperate(info), compression, position);
        }

        /// <summary>
        /// 将对象序列化并写入指定的 <see cref="IFileOperate"/> 中（同步）。
        /// </summary>
        /// <typeparam name="T">要序列化的对象类型。</typeparam>
        /// <param name="serialization">用于序列化的 <see cref="ISerialization"/> 实例。</param>
        /// <param name="obj">要序列化并写入的对象。</param>
        /// <param name="operate">用于执行写入操作的 <see cref="IFileOperate"/> 实例。</param>
        /// <param name="compression">可选的压缩实现；若提供则在写入前尝试压缩序列化结果。</param>
        /// <param name="position">写入文件的起始位置（字节偏移）。</param>
        /// <exception cref="ArgumentNullException">当 <paramref name="serialization"/> 或 <paramref name="operate"/> 为 <c>null</c> 时抛出。</exception>
        /// <remarks>
        /// 方法内部：调用 <see cref="ISerialization.Serialize{T}(T?, out ByteBuffer)"/> 获取序列化缓冲，
        /// 若提供 <paramref name="compression"/> 则尝试压缩缓冲并在必要时替换为压缩结果，最后使用 <see cref="IFileOperate.Write(ByteBuffer)"/> 写入并释放缓冲。
        /// </remarks>
        public static void Serialize<T>(this ISerialization serialization, T obj, IFileOperate operate, ICompression? compression = null, long position = 0)
        {
            ArgumentNullException.ThrowIfNull(serialization);
            ArgumentNullException.ThrowIfNull(operate);

            serialization.Serialize(obj, out var buffer);
            if (compression != null && compression.TryCompress(buffer, out var compressedBuffer))
            {
                buffer.Dispose();
                buffer = compressedBuffer;
            }
            operate.Write(buffer, position);
            buffer.Dispose();
        }

        /// <summary>
        /// 将对象序列化到 <see cref="ByteBuffer"/> 中，并可选使用压缩器对结果进行压缩（输出时返回最终缓冲）。
        /// </summary>
        /// <typeparam name="T">要序列化的对象类型。</typeparam>
        /// <param name="serialization">用于序列化的 <see cref="ISerialization"/> 实例（不可为空）。</param>
        /// <param name="obj">要序列化的对象。</param>
        /// <param name="compression">用于压缩的 <see cref="ICompression"/> 实例（不可为空）。</param>
        /// <param name="buffer">输出的序列化缓冲，若压缩成功则为压缩后缓冲。</param>
        /// <exception cref="ArgumentNullException">当 <paramref name="serialization"/> 或 <paramref name="compression"/> 为 <c>null</c> 时抛出。</exception>
        public static void Serialize<T>(this ISerialization serialization, T obj, ICompression compression, out ByteBuffer buffer)
        {
            ArgumentNullException.ThrowIfNull(serialization);
            ArgumentNullException.ThrowIfNull(compression);

            serialization.Serialize(obj, out buffer);
            if (compression.TryCompress(buffer, out var compressedBuffer))
            {
                buffer.Dispose();
                buffer = compressedBuffer;
            }
        }

        #endregion Serialization

        #region Deserialization

        /// <summary>
        /// 从 <paramref name="span"/> 中读取数据；若提供的 <paramref name="compression"/> 能解压数据则先解压再反序列化，否则直接反序列化原始数据（适用于内存块）。
        /// </summary>
        /// <typeparam name="T">目标对象类型。</typeparam>
        /// <param name="serialization">用于反序列化的 <see cref="ISerialization"/> 实例（不可为空）。</param>
        /// <param name="span">输入的字节切片。</param>
        /// <param name="compression">用于尝试解压的压缩实现（不可为空）。</param>
        /// <returns>反序列化得到的对象；若输入为空或解析失败由具体实现决定是否返回 <c>null</c>。</returns>
        /// <exception cref="ArgumentNullException">当 <paramref name="serialization"/> 或 <paramref name="compression"/> 为 <c>null</c> 时抛出。</exception>
        public static T? Deserialize<T>(this ISerialization serialization, ReadOnlySpan<byte> span, ICompression compression)
        {
            ArgumentNullException.ThrowIfNull(serialization);
            ArgumentNullException.ThrowIfNull(compression);

            if (compression.TryDecompress(span, out var block))
            {
                var result = serialization.Deserialize<T>(block.UnreadMemory);
                block.Dispose();
                return result;
            }
            return serialization.Deserialize<T>(span);
        }

        /// <summary>
        /// 从 <paramref name="memory"/> 中读取数据；若提供的 <paramref name="compression"/> 能解压数据则先解压再反序列化，否则直接反序列化原始数据（适用于 <see cref="ReadOnlyMemory{byte}"/>）。
        /// </summary>
        /// <typeparam name="T">目标对象类型。</typeparam>
        /// <param name="serialization">用于反序列化的 <see cref="ISerialization"/> 实例（不可为空）。</param>
        /// <param name="memory">输入的只读内存。</param>
        /// <param name="compression">用于尝试解压的压缩实现（不可为空）。</param>
        /// <returns>反序列化得到的对象；若输入为空或解析失败由具体实现决定是否返回 <c>null</c>。</returns>
        /// <exception cref="ArgumentNullException">当 <paramref name="serialization"/> 或 <paramref name="compression"/> 为 <c>null</c> 时抛出。</exception>
        public static T? Deserialize<T>(this ISerialization serialization, ReadOnlyMemory<byte> memory, ICompression compression)
        {
            ArgumentNullException.ThrowIfNull(serialization);
            ArgumentNullException.ThrowIfNull(compression);

            if (compression.TryDecompress(memory.Span, out var block))
            {
                var result = serialization.Deserialize<T>(block.UnreadMemory);
                block.Dispose();
                return result;
            }
            return serialization.Deserialize<T>(memory);
        }

        /// <summary>
        /// 从 <paramref name="sequence"/> 中读取数据；若提供的 <paramref name="compression"/> 能解压数据则先解压再反序列化，否则直接反序列化原始序列（适用于分段序列）。
        /// </summary>
        /// <typeparam name="T">目标对象类型。</typeparam>
        /// <param name="serialization">用于反序列化的 <see cref="ISerialization"/> 实例（不可为空）。</param>
        /// <param name="sequence">输入的只读序列。</param>
        /// <param name="compression">用于尝试解压的压缩实现（不可为空）。</param>
        /// <returns>反序列化得到的对象；若输入为空或解析失败由具体实现决定是否返回 <c>null</c>。</returns>
        /// <exception cref="ArgumentNullException">当 <paramref name="serialization"/> 或 <paramref name="compression"/> 为 <c>null</c> 时抛出。</exception>
        public static T? Deserialize<T>(this ISerialization serialization, ReadOnlySequence<byte> sequence, ICompression compression)
        {
            ArgumentNullException.ThrowIfNull(serialization);
            ArgumentNullException.ThrowIfNull(compression);

            if (compression.TryDecompress(sequence, out var buffer))
            {
                var result = serialization.Deserialize<T>(buffer);
                buffer.Dispose();
                return result;
            }
            return serialization.Deserialize<T>(sequence);
        }

        /// <summary>
        /// 从由 <see cref="IFileOperateProvider"/> 根据 <see cref="FileOperateInfo"/> 创建的文件操作器中读取数据并反序列化为目标类型（从指定偏移到文件末尾）。
        /// </summary>
        /// <typeparam name="T">目标对象类型。</typeparam>
        /// <param name="serialization">用于反序列化的 <see cref="ISerialization"/> 实例。</param>
        /// <param name="provider">用于获取 <see cref="IFileOperate"/> 的提供者。</param>
        /// <param name="info">用于创建文件操作器的文件操作配置信息。</param>
        /// <param name="position">读取的起始位置（字节偏移）。</param>
        /// <returns>反序列化得到的对象实例；若数据为空或解析失败可返回 <c>null</c>（由具体实现决定）。</returns>
        /// <exception cref="ArgumentNullException">当 <paramref name="provider"/> 为 <c>null</c> 时抛出。</exception>
        public static T? Deserialize<T>(this ISerialization serialization, IFileOperateProvider provider, FileOperateInfo info, long position = 0)
        {
            ArgumentNullException.ThrowIfNull(provider);
            return serialization.Deserialize<T>(provider.GetOperate(info), position);
        }

        /// <summary>
        /// 从由 <see cref="IFileOperateProvider"/> 根据 <see cref="FileOperateInfo"/> 创建的文件操作器中读取指定长度数据并反序列化为目标类型。
        /// </summary>
        /// <typeparam name="T">目标对象类型。</typeparam>
        /// <param name="serialization">用于反序列化的 <see cref="ISerialization"/> 实例。</param>
        /// <param name="provider">用于获取 <see cref="IFileOperate"/> 的提供者。</param>
        /// <param name="info">用于创建文件操作器的文件操作配置信息。</param>
        /// <param name="position">读取的起始位置（字节偏移）。</param>
        /// <param name="length">要读取的字节数。</param>
        /// <returns>反序列化得到的对象实例；若数据为空或解析失败可返回 <c>null</c>（由具体实现决定）。returns>
        /// <exception cref="ArgumentNullException">当 <paramref name="provider"/> 为 <c>null</c> 时抛出。</exception>
        public static T? Deserialize<T>(this ISerialization serialization, IFileOperateProvider provider, FileOperateInfo info, long position, int length)
        {
            ArgumentNullException.ThrowIfNull(provider);
            return serialization.Deserialize<T>(provider.GetOperate(info), position, length);
        }

        /// <summary>
        /// 从指定的 <see cref="IFileOperate"/> 中读取（从 <paramref name="position"/> 到文件末尾）并反序列化为目标类型。
        /// </summary>
        /// <typeparam name="T">目标对象类型。</typeparam>
        /// <param name="serialization">用于反序列化的 <see cref="ISerialization"/> 实例。</param>
        /// <param name="operate">用于执行读取操作的 <see cref="IFileOperate"/> 实例。</param>
        /// <param name="position">起始读取位置（字节偏移）。</param>
        /// <returns>反序列化得到的对象实例；若数据为空或解析失败可返回 <c>null</c>（由具体实现决定）。</returns>
        /// <exception cref="ArgumentNullException">当 <paramref name="serialization"/> 或 <paramref name="operate"/> 为 <c>null</c> 时抛出。</exception>
        public static T? Deserialize<T>(this ISerialization serialization, IFileOperate operate, long position = 0)
        {
            ArgumentNullException.ThrowIfNull(serialization);
            ArgumentNullException.ThrowIfNull(operate);

            operate.Read(out ByteBuffer buffer, position);
            var result = serialization.Deserialize<T>(buffer.UnreadSequence);
            buffer.Dispose();
            return result;
        }

        /// <summary>
        /// 从指定的 <see cref="IFileOperate"/> 中读取指定长度的数据并反序列化为目标类型（同步）。
        /// </summary>
        /// <typeparam name="T">目标对象类型。</typeparam>
        /// <param name="serialization">用于反序列化的 <see cref="ISerialization"/> 实例。</param>
        /// <param name="operate">用于执行读取操作的 <see cref="IFileOperate"/> 实例。</param>
        /// <param name="position">起始读取位置（字节偏移）。</param>
        /// <param name="length">要读取的字节数。</param>
        /// <returns>反序列化得到的对象实例；若数据为空或解析失败可返回 <c>null</c>（由具体实现决定）。</returns>
        /// <exception cref="ArgumentNullException">当 <paramref name="serialization"/> 或 <paramref name="operate"/> 为 <c>null</c> 时抛出。</exception>
        /// <remarks>
        /// 内部流程：从 <paramref name="operate"/> 读取到 <see cref="ByteBuffer"/>，调用 <see cref="ISerialization.Deserialize{T}(ReadOnlySequence{byte})"/> 反序列化，并释放缓冲。
        /// </remarks>
        public static T? Deserialize<T>(this ISerialization serialization, IFileOperate operate, long position, int length)
        {
            ArgumentNullException.ThrowIfNull(serialization);
            ArgumentNullException.ThrowIfNull(operate);

            operate.Read(length, out ByteBuffer buffer, position);
            T? result = serialization.Deserialize<T>(buffer);
            buffer.Dispose();
            return result;
        }

        /// <summary>
        /// 使用提供的压缩器先尝试解压内存数据，然后反序列化为目标类型。
        /// </summary>
        /// <typeparam name="T">目标对象类型。</typeparam>
        /// <param name="serialization">用于反序列化的 <see cref="ISerialization"/> 实例（不可为空）。</param>
        /// <param name="compression">用于解压的 <see cref="ICompression"/> 实例（不可为空）。</param>
        /// <param name="memory">输入的只读内存。</param>
        /// <returns>反序列化得到的对象实例；若数据为空或解析失败可返回 <c>null</c>（由具体实现决定）。</returns>
        /// <exception cref="ArgumentNullException">当 <paramref name="serialization"/> 或 <paramref name="compression"/> 为 <c>null</c> 时抛出。</exception>
        public static T? Deserialize<T>(this ISerialization serialization, ICompression compression, ReadOnlyMemory<byte> memory)
        {
            ArgumentNullException.ThrowIfNull(serialization);
            ArgumentNullException.ThrowIfNull(compression);
            if (compression.TryDecompress(memory.Span, out var block))
            {
                var result = serialization.Deserialize<T>(block.UnreadMemory);
                block.Dispose();
                return result;
            }
            return serialization.Deserialize<T>(memory);
        }

        /// <summary>
        /// 使用提供的压缩器先尝试解压序列数据，然后反序列化为目标类型。
        /// </summary>
        /// <typeparam name="T">目标对象类型。</typeparam>
        /// <param name="serialization">用于反序列化的 <see cref="ISerialization"/> 实例（不可为空）。</param>
        /// <param name="compression">用于解压的 <see cref="ICompression"/> 实例（不可为空）。</param>
        /// <param name="sequence">输入的 <see cref="ReadOnlySequence{byte}"/>。</param>
        /// <returns>反序列化得到的对象实例；若数据为空或解析失败可返回 <c>null</c>（由具体实现决定）。</returns>
        /// <exception cref="ArgumentNullException">当 <paramref name="serialization"/> 或 <paramref name="compression"/> 为 <c>null</c> 时抛出。</exception>
        public static T? Deserialize<T>(this ISerialization serialization, ICompression compression, ReadOnlySequence<byte> sequence)
        {
            ArgumentNullException.ThrowIfNull(serialization);
            ArgumentNullException.ThrowIfNull(compression);
            if (compression.TryDecompress(sequence, out var buffer))
            {
                var result = serialization.Deserialize<T>(buffer);
                buffer.Dispose();
                return result;
            }
            return serialization.Deserialize<T>(sequence);
        }

        /// <summary>
        /// 从指定的 <see cref="IFileOperate"/> 中读取数据（从 <paramref name="position"/> 到文件末尾），先尝试解压再反序列化为目标类型。
        /// </summary>
        /// <typeparam name="T">目标对象类型。</typeparam>
        /// <param name="serialization">用于反序列化的 <see cref="ISerialization"/> 实例。</param>
        /// <param name="compression">用于解压的 <see cref="ICompression"/> 实例。</param>
        /// <param name="operate">用于执行读取操作的 <see cref="IFileOperate"/> 实例。</param>
        /// <param name="position">起始读取位置（字节偏移）。</param>
        /// <returns>反序列化得到的对象实例；若数据为空或解析失败可返回 <c>null</c>（由具体实现决定）。</returns>
        public static T? Deserialize<T>(this ISerialization serialization, ICompression compression, IFileOperate operate, long position = 0)
        {
            ArgumentNullException.ThrowIfNull(operate);

            operate.Read(out ByteBuffer buffer, position);
            return serialization.Deserialize<T>(compression, buffer);
        }

        /// <summary>
        /// 从指定的 <see cref="IFileOperate"/> 中读取指定长度的数据，先尝试解压再反序列化为目标类型。
        /// </summary>
        /// <typeparam name="T">目标对象类型。</typeparam>
        /// <param name="serialization">用于反序列化的 <see cref="ISerialization"/> 实例。</param>
        /// <param name="compression">用于解压的 <see cref="ICompression"/> 实例。</param>
        /// <param name="operate">用于执行读取操作的 <see cref="IFileOperate"/> 实例。</param>
        /// <param name="position">起始读取位置（字节偏移）。</param>
        /// <param name="length">要读取的字节数。</param>
        /// <returns>反序列化得到的对象实例；若数据为空或解析失败可返回 <c>null</c>（由具体实现决定）。</returns>
        public static T? Deserialize<T>(this ISerialization serialization, ICompression compression, IFileOperate operate, long position, int length)
        {
            ArgumentNullException.ThrowIfNull(operate);

            operate.Read(length, out ByteBuffer buffer, position);
            return serialization.Deserialize<T>(compression, buffer);
        }

        /// <summary>
        /// 内部辅助：从给定的 <see cref="ByteBuffer"/> 中先尝试由指定压缩器解压，再调用序列化实现进行反序列化。
        /// </summary>
        /// <typeparam name="T">目标对象类型。</typeparam>
        /// <param name="serialization">用于反序列化的 <see cref="ISerialization"/> 实例（非空）。</param>
        /// <param name="compression">用于解压的 <see cref="ICompression"/> 实例（非空）。</param>
        /// <param name="buffer">包含可能被压缩或未压缩数据的 <see cref="ByteBuffer"/>，方法在结束时会释放该缓冲。</param>
        /// <returns>反序列化得到的对象实例；若数据为空或解析失败可返回 <c>null</c>（由具体实现决定）。</returns>
        private static T? Deserialize<T>(this ISerialization serialization, ICompression compression, ByteBuffer buffer)
        {
            ArgumentNullException.ThrowIfNull(serialization);
            ArgumentNullException.ThrowIfNull(compression);

            T? result = default;
            if (compression.TryDecompress(buffer.UnreadSequence, out var decompressedBuffer))
            {
                result = serialization.Deserialize<T>(decompressedBuffer.UnreadSequence);
                decompressedBuffer.Dispose();
            }
            else
            {
                result = serialization.Deserialize<T>(buffer.UnreadSequence);
            }
            buffer.Dispose();
            return result;
        }

        #endregion Deserialization
    }
}