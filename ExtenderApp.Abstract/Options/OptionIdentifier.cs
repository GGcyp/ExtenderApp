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

        /// <summary>
        /// 获取默认选项值。 该方法由派生类实现，返回与当前标识相关联的默认选项值。 默认选项值表示当选项未被显式设置时的默认状态或配置。 具体的默认值取决于派生类的实现和选项的语义。
        /// </summary>
        /// <returns>默认选项值。</returns>
        public abstract OptionValue GetDefaultOptionValue();

        /// <inheritdoc/>
        public override bool Equals(object? obj) => Equals(obj as OptionIdentifier);

        /// <inheritdoc/>
        public override int GetHashCode() => HashCode.Combine(Id, Name, GetVisibility, SetVisibility);

        /// <summary>
        /// 判断当前标识与另一个标识是否兼容，即它们具有相同的名称和公开程度，但不要求它们的 Id 相同。
        /// </summary>
        /// <param name="other">要比较的另一个标识。</param>
        /// <returns>若兼容则为 true，否则为 false。</returns>
        public bool IsCompatibleWith(OptionIdentifier other)
        {
            return Name == other.Name &&
                   GetVisibility == other.GetVisibility &&
                   SetVisibility == other.SetVisibility;
        }

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
        /// 默认值，表示当选项未被显式设置时的默认选项值。 该属性在实例化时通过构造函数参数进行初始化，并且在整个生命周期内保持不变。
        /// </summary>
        public T DefaultValue { get; }

        /// <summary>
        /// 使用指定名称和公开程度初始化 <see cref="OptionIdentifier{T}"/> 类的新实例。
        /// </summary>
        /// <param name="name">选项名称。</param>
        /// <param name="defaultValue">选项的默认值。</param>
        /// <param name="getVisibility">选项获取公开程度。</param>
        /// <param name="setVisibility">选项设置公开程度。</param>
        public OptionIdentifier(string name, T defaultValue, OptionVisibility getVisibility = OptionVisibility.Public, OptionVisibility setVisibility = OptionVisibility.Public)
            : base(name, getVisibility, setVisibility)
        {
            DefaultValue = defaultValue;
        }

        /// <summary>
        /// 使用指定名称和公开程度初始化 <see cref="OptionIdentifier{T}"/> 类的新实例。
        /// </summary>
        /// <param name="name">选项名称。</param>
        /// <param name="getVisibility">选项获取公开程度。</param>
        /// <param name="setVisibility">选项设置公开程度。</param>
        public OptionIdentifier(string name, OptionVisibility getVisibility = OptionVisibility.Public, OptionVisibility setVisibility = OptionVisibility.Public)
            : base(name, getVisibility, setVisibility)
        {
            DefaultValue = default!;
        }

        /// <summary>
        /// 尝试将给定的变更处理器绑定到指定的 <see cref="OptionValue"/> 上。 仅当当前 <see cref="OptionIdentifier{T}"/> 与 <paramref name="optionValue"/> 的标识符相同时才会绑定。
        /// </summary>
        /// <param name="optionValue">目标选项值对象，期望为与当前标识匹配的 <see cref="OptionValue"/> 实例。</param>
        /// <param name="changeHandler">要绑定的变更事件处理器，允许为 null（表示不绑定）。</param>
        /// <returns>绑定成功返回 <c>true</c>；若标识不匹配或类型不正确则返回 <c>false</c>。</returns>
        public bool TryBindChangedHandler(OptionValue optionValue, EventHandler<(OptionIdentifier, T)> changeHandler)
            => TryBindChangedHandler(optionValue, changeHandler, out _);

        /// <summary>
        /// 尝试将给定的变更处理器绑定到指定的 <see cref="OptionValue"/> 上。 仅当当前 <see cref="OptionIdentifier{T}"/> 与 <paramref name="optionValue"/> 的标识符相同时才会绑定。
        /// </summary>
        /// <param name="optionValue">目标选项值对象，期望为与当前标识匹配的 <see cref="OptionValue"/> 实例。</param>
        /// <param name="changeHandler">要绑定的变更事件处理器，允许为 null（表示不绑定）。</param>
        /// <param name="validOptionValue">输出参数，返回有效的类型安全选项值对象。</param>
        /// <returns>绑定成功返回 <c>true</c>；若标识不匹配或类型不正确则返回 <c>false</c>。</returns>
        public bool TryBindChangedHandler(OptionValue optionValue, EventHandler<(OptionIdentifier, T)> changeHandler, out OptionValue<T> validOptionValue)
        {
            validOptionValue = default!;
            if (optionValue is null || !ReferenceEquals(this, optionValue.Identifier))
            {
                return false;
            }

            if (TryConvertOptionValue(optionValue, out validOptionValue))
            {
                validOptionValue.ChangedHandler += changeHandler;
                return true;
            }
            return false;
        }

        /// <summary>
        /// 尝试将一个非类型安全的选项值转换为类型安全的选项值。 仅当 <paramref name="optionValue"/> 的 <see cref="OptionIdentifier"/> 与当前实例匹配且类型为 <see cref="OptionValue{T}"/> 时转换成功。
        /// </summary>
        /// <param name="optionValue">要转换的非类型安全选项值。</param>
        /// <param name="typedOptionValue">转换后的类型安全选项值（若返回 <c>true</c> 则为有效对象）。</param>
        /// <returns>如果转换成功，则为 <c>true</c>；否则为 <c>false</c>。</returns>
        public bool TryConvertOptionValue(OptionValue optionValue, out OptionValue<T> typedOptionValue)
        {
            typedOptionValue = default!;
            if (optionValue is null || !ReferenceEquals(this, optionValue.Identifier))
            {
                return false;
            }

            if (optionValue is OptionValue<T> validOptionValue)
            {
                typedOptionValue = validOptionValue;
                return true;
            }
            return false;
        }

        /// <summary>
        /// 转换一个非类型安全的选项值为类型安全的选项值。 仅当 <paramref name="optionValue"/> 的 <see cref="OptionIdentifier"/> 与当前实例匹配且类型为 <see cref="OptionValue{T}"/>
        /// 时转换成功，否则抛出 <see cref="InvalidCastException"/>。
        /// </summary>
        /// <param name="optionValue">要转换的非类型安全选项值。</param>
        /// <returns>转换后的类型安全选项值。</returns>
        /// <exception cref="InvalidCastException">当转换失败时抛出。</exception>
        public OptionValue<T> ConvertOptionValue(OptionValue optionValue)
            => TryConvertOptionValue(optionValue, out var typedOptionValue)
            ? typedOptionValue :
            throw new InvalidCastException($"无法将选项值转换为类型安全的选项值。期望标识: {this}, 实际标识: {optionValue.Identifier}");

        /// <summary>
        /// 尝试从非类型安全的 <see cref="OptionValue"/> 中获取类型为 <typeparamref name="T"/> 的值。 当 <paramref name="optionValue"/> 与当前标识匹配且能够成功转换为 <see
        /// cref="OptionValue{T}"/> 时， 将输出转换后的值并返回 <c>true</c>，否则返回 <c>false</c>。
        /// </summary>
        /// <param name="optionValue">要读取的非类型安全选项值对象。</param>
        /// <param name="value">若成功则返回对应的类型安全值，否则返回默认值。</param>
        /// <returns>若成功读取并转换则为 <c>true</c>，否则为 <c>false</c>。</returns>
        public bool TryGetValue(OptionValue optionValue, out T value)
        {
            value = default!;
            if (TryConvertOptionValue(optionValue, out var typedOptionValue))
            {
                value = typedOptionValue.Value;
                return true;
            }
            return false;
        }

        /// <summary>
        /// 从非类型安全的 <see cref="OptionValue"/> 中获取类型为 <typeparamref name="T"/> 的值。 当转换失败时抛出 <see cref="InvalidCastException"/>。
        /// </summary>
        /// <param name="optionValue">要读取的非类型安全选项值对象。</param>
        /// <returns>转换后的类型安全值。</returns>
        /// <exception cref="InvalidCastException">当 <paramref name="optionValue"/> 的标识不匹配或类型不为 <see cref="OptionValue{T}"/> 时抛出。</exception>
        public T GetValue(OptionValue optionValue)
            => TryGetValue(optionValue, out var value)
            ? value :
            throw new InvalidCastException($"无法获取选项值。期望标识: {this}, 实际标识: {optionValue.Identifier}");

        public override OptionValue<T> GetDefaultOptionValue()
            => new OptionValue<T>(this, DefaultValue);
    }
}