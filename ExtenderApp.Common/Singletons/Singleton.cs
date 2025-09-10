
namespace ExtenderApp.Common
{
    /// <summary>
    /// 泛型单例模式实现类
    /// 提供线程安全的泛型单例实例获取方式
    /// </summary>
    /// <typeparam name="T">
    /// 要创建单例的类类型
    /// 必须为引用类型且包含无参构造函数
    /// </typeparam>
    public class Singleton<T> where T : class, new()
    {
        // 使用Lazy<T>实现延迟初始化
        // readonly修饰保证线程安全
        // 静态构造函数特性保证首次访问前初始化
        private static readonly Lazy<T> _lazyInstance =
            new Lazy<T>(() => new T());

        /// <summary>
        /// 获取单例实例的静态属性
        /// 采用表达式体成员语法简化实现
        /// </summary>
        /// <value>
        /// 返回类型为T的唯一实例
        /// 首次访问时自动创建实例
        /// </value>
        public static T Instance => _lazyInstance.Value;
    }
}
