using System.Reflection;
using ExtenderApp.Abstract;
using ExtenderApp.Common.Serializations.Binary;
using ExtenderApp.Common.Serializations.Json;
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

        #region Reflection Helpers

        /// <summary>
        /// 尝试获取成员上的 <see cref="SerializationsMemberAttribute"/>，如果存在则返回其包含的序列化包含信息。
        /// </summary>
        /// <param name="memberInfo">要检查的成员信息。</param>
        /// <param name="include">如果找到属性，则返回其包含信息；否则返回 <c>false</c>。</param>
        /// <returns>返回是否找到</returns>
        internal static bool TryGetSerializationsMemberAttribute(this MemberInfo memberInfo, out bool include)
        {
            var attribute = memberInfo.GetCustomAttribute<SerializationsMemberAttribute>(true);
            if (attribute is null)
            {
                include = false;
                return false;
            }

            include = attribute.Include;
            return true;
        }

        #endregion Reflection Helpers
    }
}