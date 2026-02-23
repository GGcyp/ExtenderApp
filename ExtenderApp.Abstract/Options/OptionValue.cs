using ExtenderApp.Contracts;

namespace ExtenderApp.Abstract.Options
{
    /// <summary>
    /// 表示一个选项的具体值基类。
    /// </summary>
    public abstract class OptionValue : DisposableObject, IEquatable<OptionValue>
    {
        /// <summary>
        /// 当前选项值所属的选项标识符。每个选项值都与一个唯一的选项标识符相关联，以确保选项值的正确性和一致性。
        /// </summary>
        public OptionIdentifier Identifier { get; }

        public OptionValue(OptionIdentifier identifier)
        {
            Identifier = identifier;
        }

        /// <summary>
        /// 获取选项的具体值。
        /// </summary>
        /// <returns>选项的具体值。</returns>
        public abstract object GetValue();

        /// <summary>
        /// 复制当前选项值，返回一个新的实例。新实例的值与当前实例相同，但它们是独立的对象。
        /// </summary>
        /// <param name="needRegisterChange">指示是否需要注册值变化事件。对于某些类型的选项值，可能需要在复制时注册事件以确保正确的行为。</param>
        /// <returns>一个新的 <see cref="OptionValue"/> 实例，包含与当前实例相同的值。</returns>
        public abstract OptionValue Clone(bool needRegisterChange);

        public bool Equals(OptionValue? other)
        {
            return ReferenceEquals(this, other) ||
                (other is not null && Identifier.Equals(other.Identifier));
        }

        public override bool Equals(object? obj) => Equals(obj as OptionValue);

        public override int GetHashCode() => Identifier.GetHashCode();

        public override string ToString() => Identifier.ToString();

        public static bool operator ==(OptionValue? left, OptionValue? right) =>
            left is null ? right is null : left.Equals(right);

        public static bool operator !=(OptionValue? left, OptionValue? right) => !(left == right);
    }

    /// <summary>
    /// 表示一个类型安全的选项具体值。
    /// </summary>
    /// <typeparam name="T">选项值的类型。</typeparam>
    public class OptionValue<T> : OptionValue, IEquatable<OptionValue<T>>
    {
        /// <summary>
        /// 获取选项的具体值。
        /// </summary>
        public T Value { get; private set; }

        /// <summary>
        /// 当选项值发生变化时触发的事件。事件参数为新的选项值。
        /// </summary>
        public event EventHandler<(OptionIdentifier, T)>? ChangedHandler;

        public OptionValue(OptionIdentifier identifier) : this(identifier, default!)
        {
        }

        /// <summary>
        /// 初始化 <see cref="OptionValue{T}"/> 类的新实例。
        /// </summary>
        /// <param name="identifier">选项标识符。</param>
        /// <param name="value">选项的具体值。</param>
        public OptionValue(OptionIdentifier identifier, T value) : base(identifier)
        {
            Value = value;
        }

        /// <summary>
        /// 更新选项的具体值，并在值发生变化时触发 <see cref="ChangedHandler"/> 事件。
        /// </summary>
        /// <param name="sender">触发事件的发送者。</param>
        /// <param name="item">包含选项标识符和新的选项值的元组。</param>
        public void UpdateValue(object? sender, (OptionIdentifier, T) item)
        {
            var (identifier, newValue) = item;
            if (!Identifier.Equals(identifier))
                throw new ArgumentException($"选项标识符不匹配。当前标识符: {Identifier}, 提供的标识符: {identifier}");

            if (!EqualityComparer<T>.Default.Equals(Value, newValue))
            {
                Value = newValue;
                ChangedHandler?.Invoke(this, (Identifier, newValue));
            }
        }

        /// <summary>
        /// 判断当前值与另一个类型安全值是否相等。
        /// </summary>
        public bool Equals(OptionValue<T>? other)
        {
            if (ReferenceEquals(this, other)) return true;
            if (other is null) return false;
            return EqualityComparer<T>.Default.Equals(Value, other.Value);
        }

        public override bool Equals(object? obj) => Equals(obj as OptionValue<T>);

        public override int GetHashCode() => HashCode.Combine(Value);

        public override object GetValue() => Value!;

        public override string ToString() => $"{Identifier.Name}: {Value}";

        public static bool operator ==(OptionValue<T>? left, OptionValue<T>? right) =>
            left is null ? right is null : left.Equals(right);

        public static bool operator !=(OptionValue<T>? left, OptionValue<T>? right) => !(left == right);

        protected override void DisposeManagedResources()
        {
            if (Value is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }

        public override OptionValue Clone(bool needRegisterChange)
        {
            var clone = new OptionValue<T>(Identifier, Value);
            if (needRegisterChange)
            {
                ChangedHandler += clone.UpdateValue;
                clone.ChangedHandler += UpdateValue;
            }

            return clone;
        }
    }
}