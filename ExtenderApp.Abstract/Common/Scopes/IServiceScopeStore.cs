using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;

namespace ExtenderApp.Abstract
{
    /// <summary>
    /// 存储与管理以名称区分的 IServiceScope / IServiceProvider。
    /// 用于宿主在不同插件或模块间按作用域（scopeName）保存和检索依赖注入作用域。
    /// </summary>
    public interface IServiceScopeStore
    {
        /// <summary>
        /// 根据作用域名称获取对应的 <see cref="IServiceProvider"/>。
        /// </summary>
        /// <param name="scopeName">作用域名称。</param>
        /// <returns>找到则返回对应的 <see cref="IServiceProvider"/>；未找到则返回 <c>null</c>。</returns>
        IServiceProvider? Get(string scopeName);

        /// <summary>
        /// 尝试根据作用域名称获取对应的 <see cref="IServiceProvider"/>。
        /// </summary>
        /// <param name="scopeName">作用域名称。</param>
        /// <param name="provider">当返回值为 <c>true</c> 时输出对应的 <see cref="IServiceProvider"/>；否则为 <c>null</c>。</param>
        /// <returns>若找到则为 <c>true</c>，否则为 <c>false</c>。</returns>
        bool TryGet(string scopeName, [MaybeNullWhen(false)] out IServiceProvider provider);

        /// <summary>
        /// 尝试添加一个以名称标识的 <see cref="IServiceScope"/>。
        /// </summary>
        /// <param name="scope">作用域名称。</param>
        /// <param name="serviceScope">要添加的 <see cref="IServiceScope"/> 实例。</param>
        /// <returns>添加成功（即该名称尚未存在）返回 <c>true</c>，否则返回 <c>false</c>。</returns>
        bool TryAdd(string scope, IServiceScope serviceScope);

        /// <summary>
        /// 尝试移除指定名称的 <see cref="IServiceScope"/> 并输出被移除的实例。
        /// </summary>
        /// <param name="scopeName">要移除的作用域名称。</param>
        /// <param name="serviceScope">当返回值为 <c>true</c> 时输出被移除的 <see cref="IServiceScope"/>；否则为 <c>null</c>。</param>
        /// <returns>移除成功返回 <c>true</c>，否则返回 <c>false</c>。</returns>
        bool TryRemove(string scopeName, [MaybeNullWhen(false)] out IServiceScope serviceScope);
    }
}