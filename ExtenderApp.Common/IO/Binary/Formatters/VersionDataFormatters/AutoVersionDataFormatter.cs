using ExtenderApp.Abstract;

namespace ExtenderApp.Common.IO.Binary.Formatters
{
    /// <summary>
    /// 带版本信息的自动二进制格式化器基类。
    /// 基于 <see cref="AutoFormatter{T}"/> 生成序列化/反序列化委托，并通过 <see cref="FormatterVersion"/> 暴露当前格式版本，
    /// 以支持模型的前后兼容与迁移。
    /// </summary>
    /// <typeparam name="T">要格式化的对象类型。</typeparam>
    public abstract class AutoVersionDataFormatter<T> : AutoFormatter<T>, IVersionDataFormatter<T>
    {
        /// <summary>
        /// 当前格式化器的格式版本。
        /// 用于区分不同版本的二进制布局，协助读写端进行兼容处理与迁移。
        /// </summary>
        public abstract Version FormatterVersion { get; }

        /// <summary>
        /// 使用给定的对象存储上下文初始化实例。
        /// 派生类通常在构造函数中收集需要处理的属性/字段并调用基类的 AddInfo 完成委托编译。
        /// </summary>
        /// <param name="store">格式化表达式/方法的共享存储与工厂。</param>
        public AutoVersionDataFormatter(DefaultObjectStore store) : base(store)
        {
        }
    }
}
