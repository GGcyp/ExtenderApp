namespace ExtenderApp.Abstract
{
    /// <summary>
    /// 标记成员在序列化中的包含/排除状态。 当类型中存在任何标记该特性的成员时，默认仅序列化标记为包含的成员。
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
    public sealed class SerializationsMemberAttribute : Attribute
    {
        /// <summary>
        /// 指示是否参与序列化。
        /// </summary>
        public bool Include { get; }

        /// <summary>
        /// 创建序列化成员标记。
        /// </summary>
        /// <param name="include">是否参与二进制序列化。</param>
        public SerializationsMemberAttribute(bool include = true)
        {
            Include = include;
        }
    }
}