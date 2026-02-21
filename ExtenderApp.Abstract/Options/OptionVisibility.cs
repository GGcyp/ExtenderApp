namespace ExtenderApp.Abstract.Options
{
    /// <summary>
    /// 表示选项的公开程度。
    /// </summary>
    [Flags]
    public enum OptionVisibility : byte
    {
        /// <summary>
        /// 公开，所有人可见。
        /// </summary>
        Public = 1 << 1,

        /// <summary>
        /// 内部，仅模块或组件内部可见。
        /// </summary>
        Internal = 1 << 2,

        /// <summary>
        /// 子类受保护，当前对象和子类可见。
        /// </summary>
        Protected = 1 << 3,

        /// <summary>
        /// 私有，仅当前对象可见。
        /// </summary>
        Private = 1 << 4,

        /// <summary>
        /// 只能在对象创建时设置，之后不可见或修改。
        /// </summary>
        Initial = 1 << 5,
    }
}