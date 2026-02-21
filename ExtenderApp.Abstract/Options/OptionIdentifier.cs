namespace ExtenderApp.Abstract.Options
{
    /// <summary>
    /// 表示一个选项的唯一标识基类。
    /// </summary>
    public abstract class OptionIdentifier : IEquatable<OptionIdentifier>
    {
        /// <summary>
        /// 获取选项名称。
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// 获取选项的全局唯一标识符。
        /// </summary>
        public Guid Id { get; }

        /// <summary>
        /// 获取选项的获取公开程度。
        /// </summary>
        public OptionVisibility GetVisibility { get; }

        /// <summary>
        /// 获取选项的设置公开程度。
        /// </summary>
        public OptionVisibility SetVisibility { get; }

        /// <summary>
        /// 使用指定名称和公开程度初始化 <see cref="OptionIdentifier"/> 类的新实例。
        /// </summary>
        /// <param name="name">选项名称。</param>
        /// <param name="getVisibility">选项获取公开程度。</param>
        /// <param name="setVisibility">选项设置公开程度。</param>
        public OptionIdentifier(string name, OptionVisibility getVisibility, OptionVisibility setVisibility)
        {
            Name = name;
            Id = Guid.NewGuid();
            GetVisibility = getVisibility;
            SetVisibility = setVisibility;
        }

        /// <summary>
        /// 获取选项的值对象。
        /// </summary>
        /// <returns>选项的值对象。</returns>
        internal abstract OptionValue GetOptionValue();

        /// <summary>
        /// 判断当前标识与另一个标识是否相等。
        /// </summary>
        /// <param name="other">要比较的另一个标识。</param>
        /// <returns>若相等则为 true，否则为 false。</returns>
        public bool Equals(OptionIdentifier? other)
        {
            if (ReferenceEquals(this, other)) return true;
            if (other is null) return false;
            return Id.Equals(other.Id) &&
                Name == other.Name &&
                GetVisibility == other.GetVisibility &&
                SetVisibility == other.SetVisibility;
        }

        /// <inheritdoc/>
        public override bool Equals(object? obj) => Equals(obj as OptionIdentifier);

        /// <inheritdoc/>
        public override int GetHashCode() => HashCode.Combine(Id, Name, GetVisibility, SetVisibility);

        /// <summary>
        /// 判断两个 <see cref="OptionIdentifier"/> 是否相等。
        /// </summary>
        public static bool operator ==(OptionIdentifier? left, OptionIdentifier? right) =>
            left is null ? right is null : left.Equals(right);

        /// <summary>
        /// 判断两个 <see cref="OptionIdentifier"/> 是否不相等。
        /// </summary>
        public static bool operator !=(OptionIdentifier? left, OptionIdentifier? right) => !(left == right);

        /// <summary>
        /// 返回选项名称的字符串表示。
        /// </summary>
        /// <returns>选项名称。</returns>
        public override string ToString() => $"{Name} (Get: {GetVisibility}, Set: {SetVisibility})";
    }

    /// <summary>
    /// 表示一个类型安全的选项唯一标识。
    /// </summary>
    /// <typeparam name="T">选项值的类型。</typeparam>
    public class OptionIdentifier<T> : OptionIdentifier
    {
        /// <summary>
        /// 使用指定名称和公开程度初始化 <see cref="OptionIdentifier{T}"/> 类的新实例。
        /// </summary>
        /// <param name="name">选项名称。</param>
        /// <param name="getVisibility">选项获取公开程度。</param>
        /// <param name="setVisibility">选项设置公开程度。</param>
        public OptionIdentifier(string name, OptionVisibility getVisibility = OptionVisibility.Public, OptionVisibility setVisibility = OptionVisibility.Public)
            : base(name, getVisibility, setVisibility)
        {
        }

        internal override OptionValue<T> GetOptionValue()
            => GetOptionValue(default!);

        /// <summary>
        /// 获取选项值对象。
        /// </summary>
        /// <param name="value">选项的值。</param>
        /// <returns>选项值对象。</returns>
        internal static OptionValue<T> GetOptionValue(T value)
            => new OptionValue<T>(value);
    }
}