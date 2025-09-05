

namespace AppHost.Extensions.DependencyInjection
{
    /// <summary>
    /// 作用域选项。
    /// </summary>
    public class ScopeOptions
    {
        /// <summary>
        /// 依赖的其他作用域
        /// </summary>
        internal List<string> ReloScopes { get; set; }

        /// <summary>
        /// 作用域名
        /// </summary>
        public string ScopeName { get; set; }

        /// <summary>
        /// ScopeOptions 类的构造函数。
        /// </summary>
        public ScopeOptions()
        {
            ReloScopes = new List<string>();
        }

        /// <summary>
        /// 向 ReloScopes 列表中添加一个作用域。
        /// </summary>
        /// <param name="scope">要添加的作用域名称。</param>
        /// <exception cref="ArgumentNullException">如果传入的 scope 参数为空或仅包含空白字符，则抛出此异常。</exception>
        public void AddReloScope(string scope)
        {
            if (string.IsNullOrEmpty(scope))
            {
                throw new ArgumentNullException(nameof(scope));
            }
            ReloScopes.Add(scope);
        }
    }
}
