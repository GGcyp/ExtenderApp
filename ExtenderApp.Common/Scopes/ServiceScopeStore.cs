using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using ExtenderApp.Abstract;
using Microsoft.Extensions.DependencyInjection;

namespace ExtenderApp.Common.Scopes
{
    /// <summary>
    /// 基于名称管理 <see cref="IServiceScope"/> 的线程安全存储。
    /// 使用内部的 <see cref="ConcurrentDictionary{TKey, TValue}"/> 保存作用域，提供按名称的添加/获取/移除操作。
    /// </summary>
    internal class ServiceScopeStore : IServiceScopeStore
    {
        private readonly ConcurrentDictionary<string, IServiceScope> _scopeServiceDict;

        /// <summary>
        /// 创建一个新的 <see cref="ServiceScopeStore"/> 实例。
        /// </summary>
        public ServiceScopeStore()
        {
            _scopeServiceDict = new();
        }

        /// <summary>
        /// 根据作用域名称获取对应的 <see cref="IServiceProvider"/>，若不存在返回 <c>null</c>。
        /// </summary>
        /// <param name="scopeName">作用域名称，不能为空。</param>
        /// <returns>对应的 <see cref="IServiceProvider"/> 或 <c>null</c>（未找到）。</returns>
        /// <exception cref="ArgumentNullException">当 <paramref name="scopeName"/> 为 <c>null</c> 或空字符串时抛出（由内部调用的 <see cref="TryGet(string, out IServiceProvider)"/> 抛出）。</exception>
        public IServiceProvider? Get(string scopeName)
        {
            TryGet(scopeName, out var provider);
            return provider;
        }

        /// <summary>
        /// 尝试将给定的 <see cref="IServiceScope"/> 以指定名称添加到存储。
        /// </summary>
        /// <param name="scope">作用域名称，不能为空或空字符串。</param>
        /// <param name="serviceScope">要添加的 <see cref="IServiceScope"/> 实例，不能为空。</param>
        /// <returns>添加成功返回 <c>true</c>（即该名称尚不存在），否则返回 <c>false</c>。</returns>
        /// <exception cref="ArgumentNullException">当 <paramref name="scope"/> 为空或 <paramref name="serviceScope"/> 为 <c>null</c> 时抛出。</exception>
        public bool TryAdd(string scope, IServiceScope serviceScope)
        {
            if (string.IsNullOrEmpty(scope))
                throw new ArgumentNullException(nameof(scope));
            ArgumentNullException.ThrowIfNull(serviceScope);

            return _scopeServiceDict.TryAdd(scope, serviceScope);
        }

        /// <summary>
        /// 尝试根据名称获取对应的 <see cref="IServiceProvider"/>。
        /// </summary>
        /// <param name="scopeName">作用域名称，不能为空或空字符串。</param>
        /// <param name="provider">当返回值为 <c>true</c> 时输出对应的 <see cref="IServiceProvider"/>，否则为 <c>null</c>。</param>
        /// <returns>找到则返回 <c>true</c>，否则返回 <c>false</c>。</returns>
        /// <exception cref="ArgumentNullException">当 <paramref name="scopeName"/> 为空或空字符串时抛出。</exception>
        public bool TryGet(string scopeName, [MaybeNullWhen(false)] out IServiceProvider provider)
        {
            provider = default!;
            if (string.IsNullOrEmpty(scopeName))
                throw new ArgumentNullException(nameof(scopeName));

            if (_scopeServiceDict.TryGetValue(scopeName, out var serviceScope))
            {
                provider = serviceScope.ServiceProvider;
                return true;
            }
            return false;
        }

        /// <summary>
        /// 尝试移除指定名称的 <see cref="IServiceScope"/>，并返回被移除的实例（若存在）。
        /// </summary>
        /// <param name="scopeName">要移除的作用域名称，不能为空或空字符串。</param>
        /// <param name="serviceScope">当返回值为 <c>true</c> 时输出被移除的 <see cref="IServiceScope"/>，否则为 <c>null</c>。</param>
        /// <returns>移除成功返回 <c>true</c>，否则返回 <c>false</c>。</returns>
        /// <exception cref="ArgumentNullException">当 <paramref name="scopeName"/> 为空或空字符串时抛出。</exception>
        public bool TryRemove(string scopeName, [MaybeNullWhen(false)] out IServiceScope serviceScope)
        {
            if (string.IsNullOrEmpty(scopeName))
                throw new ArgumentNullException(nameof(scopeName));

            return _scopeServiceDict.TryRemove(scopeName, out serviceScope);
        }
    }
}