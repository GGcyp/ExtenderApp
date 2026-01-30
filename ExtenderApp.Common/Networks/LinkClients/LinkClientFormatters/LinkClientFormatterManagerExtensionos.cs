using ExtenderApp.Abstract;
using ExtenderApp.Data;
using Microsoft.Extensions.DependencyInjection;

namespace ExtenderApp.Common.Networks.LinkClients
{
    /// <summary>
    /// 为 <see cref="ILinkClientFormatterManager"/> 提供便捷的扩展方法，
    /// 用于注册/注销常见的格式化器（例如二进制和 JSON 格式化器）以及关联回调。
    /// </summary>
    internal static class LinkClientFormatterManagerExtensionos
    {
        /// <summary>
        /// 将格式化器相关的服务添加到依赖注入容器中。
        /// </summary>
        /// <param name="services">要配置的 <see cref="IServiceCollection"/> 实例。</param>
        /// <returns>传入的 <see cref="IServiceCollection"/>，以支持链式调用。</returns>
        internal static IServiceCollection AddService(this IServiceCollection services)
        {
            services.AddTransient(typeof(BinaryLinkClientFormatter<>));
            return services;
        }

        /// <summary>
        /// 为指定数据类型注册二进制格式化器，并可选地绑定接收回调。
        /// </summary>
        /// <typeparam name="T">要处理的数据类型。</typeparam>
        /// <param name="manager">格式化器管理器实例。</param>
        /// <param name="callback">可选的当接收到 <typeparamref name="T"/> 类型数据时调用的回调。</param>
        /// <returns>传入的格式化器管理器实例，以支持链式调用。</returns>
        public static ILinkClientFormatterManager AddBinaryFormatter<T>(this ILinkClientFormatterManager manager, Action<LinkClientReceivedValue<T>>? callback = null)
        {
            var formatter = manager.AddFormatter<BinaryLinkClientFormatter<T>>();
            if (formatter is not null && callback != null)
            {
                formatter.Received += callback;
            }
            return manager;
        }

        /// <summary>
        /// 从管理器中移除指定数据类型的二进制格式化器。
        /// </summary>
        /// <typeparam name="T">要移除的格式化器对应的数据类型。</typeparam>
        /// <param name="manager">格式化器管理器实例。</param>
        /// <returns>传入的格式化器管理器实例，以支持链式调用。</returns>
        public static ILinkClientFormatterManager RemoveBinaryFormatter<T>(this ILinkClientFormatterManager manager)
        {
            manager.RemoveFormatter<BinaryLinkClientFormatter<T>>();
            return manager;
        }

        /// <summary>
        /// 为指定数据类型注册 JSON 格式化器，并可选地绑定接收回调。
        /// </summary>
        /// <typeparam name="T">要处理的数据类型。</typeparam>
        /// <param name="manager">格式化器管理器实例。</param>
        /// <param name="callback">可选的当接收到 <typeparamref name="T"/> 类型数据时调用的回调。</param>
        /// <returns>传入的格式化器管理器实例，以支持链式调用。</returns>
        public static ILinkClientFormatterManager AddJsonFormatter<T>(this ILinkClientFormatterManager manager, Action<LinkClientReceivedValue<T>>? callback = null)
        {
            var formatter = manager.AddFormatter<JsonLinkClientFormatter<T>>();
            if (formatter is not null && callback != null)
            {
                formatter.Received += callback;
            }
            return manager;
        }

        /// <summary>
        /// 从管理器中移除指定数据类型的 JSON 格式化器。
        /// </summary>
        /// <typeparam name="T">要移除的格式化器对应的数据类型。</typeparam>
        /// <param name="manager">格式化器管理器实例。</param>
        /// <returns>传入的格式化器管理器实例，以支持链式调用。</returns>
        public static ILinkClientFormatterManager RemoveJsonFormatter<T>(this ILinkClientFormatterManager manager)
        {
            manager.RemoveFormatter<JsonLinkClientFormatter<T>>();
            return manager;
        }

        /// <summary>
        /// 为指定数据类型注册一个接收回调。如果目标格式化器已存在则直接订阅其 <c>Received</c> 事件。
        /// </summary>
        /// <typeparam name="T">要注册回调的数据类型。</typeparam>
        /// <param name="manager">格式化器管理器实例。</param>
        /// <param name="callback">当接收到 <typeparamref name="T"/> 类型数据时调用的回调。</param>
        /// <returns>传入的格式化器管理器实例，以支持链式调用。</returns>
        /// <exception cref="ArgumentNullException"><paramref name="callback"/> 为 <c>null</c> 时抛出。</exception>
        public static ILinkClientFormatterManager Register<T>(this ILinkClientFormatterManager manager, Action<LinkClientReceivedValue<T>> callback)
        {
            if (callback == null)
                throw new ArgumentNullException(nameof(callback));
            if (manager.TryGetFormatter<T>(out var formatter))
            {
                formatter.Received += callback;
            }
            return manager;
        }

        /// <summary>
        /// 注销先前为指定数据类型注册的接收回调。如果目标格式化器已存在则从其 <c>Received</c> 事件中移除回调。
        /// </summary>
        /// <typeparam name="T">要注销回调的数据类型。</typeparam>
        /// <param name="manager">格式化器管理器实例。</param>
        /// <param name="callback">要注销的回调委托实例。</param>
        /// <returns>传入的格式化器管理器实例，以支持链式调用。</returns>
        /// <exception cref="ArgumentNullException"><paramref name="callback"/> 为 <c>null</c> 时抛出。</exception>
        public static ILinkClientFormatterManager UnRegister<T>(this ILinkClientFormatterManager manager, Action<LinkClientReceivedValue<T>> callback)
        {
            if (callback == null)
                throw new ArgumentNullException(nameof(callback));
            if (manager.TryGetFormatter<T>(out var formatter))
            {
                formatter.Received -= callback;
            }
            return manager;
        }
    }
}