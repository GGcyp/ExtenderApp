using ExtenderApp.Abstract;

namespace ExtenderApp.Common.IO.Binaries.Formatters
{
    /// <summary>
    /// 本地数据格式化器的抽象基类，提供版本控制能力和解析器支持
    /// </summary>
    /// <typeparam name="T">需要格式化的数据类型</typeparam>
    /// <remarks>
    /// 1. 继承自 ResolverFormatter&lt;T&gt;，复用其基础格式化功能
    /// 2. 实现 ILocalFormatter&lt;T&gt; 接口，强制要求声明格式化器版本
    /// 3. 通过构造函数注入二进制格式化解析器
    /// </remarks>
    public abstract class VersionDataFormatter<T> : ResolverFormatter<T>, IVersionDataFormatter<T>
    {
        /// <summary>
        /// 获取当前格式化器的版本信息
        /// </summary>
        /// <value>
        /// 返回 System.Version 对象，表示格式化器的版本号
        /// </value>
        /// <remarks>
        /// 1. 必须由派生类实现具体版本号
        /// 2. 用于版本兼容性检查
        /// </remarks>
        public abstract Version FormatterVersion { get; }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="resolver">二进制格式化解析器实例</param>
        /// <exception cref="ArgumentNullException">当 resolver 为 null 时抛出</exception>
        protected VersionDataFormatter(IBinaryFormatterResolver resolver) : base(resolver)
        {

        }
    }
}
