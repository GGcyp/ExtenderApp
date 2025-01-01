using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using ExtenderApp.Abstract;

namespace ExtenderApp.Common
{
    /// <summary>
    /// FormatterResolverExtensions 类的内部静态类，提供对 IFormatterResolver 的扩展方法。
    /// </summary>
    internal static class BinaryFormatterResolverExtensions
    {
        /// <summary>
        /// 使用给定的 IFormatterResolver 获取并验证指定类型的二进制格式化器。
        /// </summary>
        /// <typeparam name="T">要格式化的类型。</typeparam>
        /// <param name="resolver">IFormatterResolver 实例。</param>
        /// <returns>返回指定类型的 IBinaryFormatter<T> 实例。</returns>
        /// <exception cref="ArgumentNullException">如果 resolver 为 null。</exception>
        /// <exception cref="Exception">如果未注册指定类型的格式化器。</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IBinaryFormatter<T> GetFormatterWithVerify<T>(this IBinaryFormatterResolver resolver)
        {
            if (resolver is null)
            {
                throw new ArgumentNullException(nameof(resolver));
            }

            IBinaryFormatter<T>? formatter;
            try
            {
                formatter = resolver.GetFormatter<T>();
            }
            catch (TypeInitializationException ex)
            {
                ExceptionDispatchInfo.Capture(ex.InnerException ?? ex).Throw();
                throw null;
            }

            if (formatter == null)
            {
                throw new Exception(string.Format("在转换二进制时发现未注册的类型：{0}", typeof(T).FullName));
            }

            return formatter;
        }
    }
}
