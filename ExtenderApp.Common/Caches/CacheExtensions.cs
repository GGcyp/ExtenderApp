using System.Buffers;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ExtenderApp.Common.Caches
{
    /// <summary>
    /// 缓存扩展类
    /// </summary>
    internal static class CacheExtensions
    {
        /// <summary>
        /// 添加缓存服务到IServiceCollection中
        /// </summary>
        /// <param name="services">
        /// IServiceCollection 实例
        /// </param>
        /// <returns>扩展后的IServiceCollection实例</returns>
        internal static IServiceCollection AddCache(this IServiceCollection services)
        {
            services.AddSingleton<StringCache>();
            services.AddSingleton<IPAddressCache>();
            return services;
        }

        /// <summary>
        /// 获取字符串对应的Guid
        /// </summary>
        /// <param name="value">需要获取Guid的字符串</param>
        /// <returns>对应的Guid</returns>
        public static Guid GetGuid(this string value)
        {
            Encoding encoding = Encoding.UTF8;
            int length = encoding.GetByteCount(value);
            byte[] bytes = ArrayPool<byte>.Shared.Rent(length);
            encoding.GetBytes(value, 0, value.Length, bytes, 0);
            Span<byte> guidBytes = stackalloc byte[16];

            int guidLength = SHA256.HashData(bytes.AsSpan(), guidBytes);
            ArrayPool<byte>.Shared.Return(bytes);
            return new Guid(guidBytes);
        }
    }
}