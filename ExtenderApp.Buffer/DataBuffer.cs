namespace ExtenderApp.Buffer
{
    /// <summary>
    /// 数据缓冲区基类，提供跨缓冲区类型的相等性比较入口。
    /// </summary>
    public abstract class DataBuffer : IEquatable<DataBuffer>
    {
        /// <summary>
        /// 获取一个表示空的静态数据缓冲区实例。
        /// </summary>
        public static DataBuffer Empty { get; } = new EmptyDataBuffer();

        /// <summary>
        /// 比较当前缓冲区与另一个缓冲区是否相等。
        /// </summary>
        /// <param name="other">要比较的另一个缓冲区。</param>
        /// <returns>相等返回 <c>true</c>；否则返回 <c>false</c>。</returns>
        public abstract bool Equals(DataBuffer? other);

        /// <summary>
        /// 将当前实例重置并归还到对象池。
        /// </summary>
        public abstract void Release();

        /// <summary>
        /// 返回一个值，指示指定的缓冲区是否为 <c>null</c> 或空缓冲区实例。
        /// </summary>
        /// <param name="buffer">指示的缓冲区</param>
        /// <returns>是空返回true，否者返回flase</returns>
        public static bool IsEmptyOrNull(DataBuffer? buffer)
        {
            return buffer == null || buffer is EmptyDataBuffer;
        }

        /// <summary>
        /// 是否包含与指定类型相等的类型。
        /// </summary>
        /// <typeparam name="T">指定类型</typeparam>
        /// <returns>是则返回true，否则返回false</returns>
        public abstract bool HasValueType<T>();

        /// <summary>
        /// 从单个值创建并获取一个 <see cref="DataBuffer{T1}"/> 实例。
        /// </summary>
        /// <typeparam name="T">值的类型。</typeparam>
        /// <param name="value">要存储在缓冲区中的值。</param>
        /// <returns>包含指定值的 <see cref="DataBuffer{T1}"/> 新实例。</returns>
        public static DataBuffer<T> FromValue<T>(T value)
        {
            return DataBuffer<T>.Get(value);
        }

        /// <summary>
        /// 从两个值创建并获取一个 <see cref="DataBuffer{T1, T2}"/> 实例。
        /// </summary>
        /// <typeparam name="T1">第一个值的类型。</typeparam>
        /// <typeparam name="T2">第二个值的类型。</typeparam>
        /// <param name="item1">要存储的第一个值。</param>
        /// <param name="item2">要存储的第二个值。</param>
        /// <returns>包含指定值的 <see cref="DataBuffer{T1, T2}"/> 新实例。</returns>
        public static DataBuffer<T1, T2> FromValue<T1, T2>(T1 item1, T2 item2)
        {
            return DataBuffer<T1, T2>.Get(item1, item2);
        }

        /// <summary>
        /// 从三个值创建并获取一个 <see cref="DataBuffer{T1, T2, T3}"/> 实例。
        /// </summary>
        /// <typeparam name="T1">第一个值的类型。</typeparam>
        /// <typeparam name="T2">第二个值的类型。</typeparam>
        /// <typeparam name="T3">第三个值的类型。</typeparam>
        /// <param name="item1">要存储的第一个值。</param>
        /// <param name="item2">要存储的第二个值。</param>
        /// <param name="item3">要存储的第三个值。</param>
        /// <returns>包含指定值的 <see cref="DataBuffer{T1, T2, T3}"/> 新实例。</returns>
        public static DataBuffer<T1, T2, T3> FromValue<T1, T2, T3>(T1 item1, T2 item2, T3 item3)
        {
            return DataBuffer<T1, T2, T3>.Get(item1, item2, item3);
        }

        /// <summary>
        /// 从四个值创建并获取一个 <see cref="DataBuffer{T1, T2, T3, T4}"/> 实例。
        /// </summary>
        /// <typeparam name="T1">第一个值的类型。</typeparam>
        /// <typeparam name="T2">第二个值的类型。</typeparam>
        /// <typeparam name="T3">第三个值的类型。</typeparam>
        /// <typeparam name="T4">第四个值的类型。</typeparam>
        /// <param name="item1">要存储的第一个值。</param>
        /// <param name="item2">要存储的第二个值。</param>
        /// <param name="item3">要存储的第三个值。</param>
        /// <param name="item4">要存储的第四个值。</param>
        /// <returns>包含指定值的 <see cref="DataBuffer{T1, T2, T3, T4}"/> 新实例。</returns>
        public static DataBuffer<T1, T2, T3, T4> FromValue<T1, T2, T3, T4>(T1 item1, T2 item2, T3 item3, T4 item4)
        {
            return DataBuffer<T1, T2, T3, T4>.Get(item1, item2, item3, item4);
        }
    }

    /// <summary>
    /// 表示一个专门的“空”数据缓冲区实例，用于表示无数据或占位。 不参与对象池管理， <see cref="Release"/> 是空操作。
    /// </summary>
    internal class EmptyDataBuffer : DataBuffer
    {
        /// <inheritdoc/>
        public override bool Equals(DataBuffer? other)
        {
            return other is EmptyDataBuffer;
        }

        /// <inheritdoc/>
        public override bool Equals(object? obj)
        {
            return obj is EmptyDataBuffer;
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return 0;
        }

        /// <summary>
        /// EmptyDataBuffer 无需回收到池中，此方法为空实现以保持接口一致性。
        /// </summary>
        public override void Release()
        {
        }

        /// <summary>
        /// 确定此空缓冲区实例是否包含指定的值类型。
        /// </summary>
        /// <typeparam name="T">要检查的值类型。</typeparam>
        /// <returns>总是返回 <c>false</c>，因为空缓冲区不包含任何值。</returns>
        public override bool HasValueType<T>()
        {
            return false;
        }

        /// <summary>
        /// 返回用于显示的文本表示： <c>&lt;empty&gt;</c>。
        /// </summary>
        public override string ToString()
        {
            return "<empty>";
        }
    }

    /// <summary>
    /// 单值泛型数据缓冲区。
    /// </summary>
    /// <typeparam name="T1">缓冲区存储的数据类型。</typeparam>
    public class DataBuffer<T1> : DataBuffer, IEquatable<DataBuffer<T1>>
    {
        /// <summary>
        /// 当前类型缓冲区的全局对象池。
        /// </summary>
        private static readonly ObjectPool<DataBuffer<T1>> pool
            = ObjectPool.Create<DataBuffer<T1>>();

        /// <summary>
        /// 用于比较 <typeparamref name="T1"/> 类型值的默认相等比较器。
        /// </summary>
        public static readonly EqualityComparer<T1?> comparer = EqualityComparer<T1?>.Default;

        /// <summary>
        /// 从对象池获取一个 <see cref="DataBuffer{T1}"/> 实例，并可选初始化其值。
        /// </summary>
        /// <param name="value">初始值，默认 <c>default</c>。</param>
        /// <returns>获取到的缓冲区实例。</returns>
        /// <remarks>使用完成后请调用 <see cref="Release"/> 将实例归还对象池。</remarks>
        public static DataBuffer<T1> Get(T1? value = default)
        {
            var buffer = pool.Get();
            buffer.Item1 = value;
            return buffer;
        }

        /// <summary>
        /// 将当前实例重置并归还到对象池。
        /// </summary>
        /// <remarks>会先将 <see cref="Item1"/> 重置为默认值，然后释放回对象池。</remarks>
        public override void Release()
        {
            Item1 = default;
            pool.Release(this);
        }

        /// <summary>
        /// 当前缓冲区承载的值。
        /// </summary>
        public T1? Item1 { get; set; }

        /// <summary>
        /// 确定此缓冲区实例是否包含指定的值类型。
        /// </summary>
        /// <typeparam name="T">要检查的值类型。</typeparam>
        /// <returns>如果 <typeparamref name="T"/> 与 <typeparamref name="T1"/> 是相同类型，则为 <c>true</c>；否则为 <c>false</c>。</returns>
        public override bool HasValueType<T>()
        {
            return typeof(T1) == typeof(T);
        }

        /// <summary>
        /// 比较与另一个缓冲区是否相等（基于值语义）。
        /// </summary>
        /// <param name="other">另一个缓冲区。</param>
        /// <returns>当且仅当 <paramref name="other"/> 为相同泛型参数的缓冲区且两者 <see cref="Item1"/> 相等时返回 <c>true</c>。</returns>
        public override bool Equals(DataBuffer? other)
        {
            return other is DataBuffer<T1> otherBuffer && Equals(otherBuffer);
        }

        /// <summary>
        /// 确定当前对象是否等于同一类型的另一个对象。
        /// </summary>
        /// <param name="other">要与当前对象进行比较的对象。</param>
        /// <returns>如果指定的对象等于当前对象，则为 <c>true</c>；否则为 <c>false</c>。</returns>
        public bool Equals(DataBuffer<T1>? other)
        {
            if (other is null) return false;
            return ItemEquals(this, other);
        }

        private static bool ItemEquals(DataBuffer<T1> left, DataBuffer<T1> right)
        {
            return comparer.Equals(left.Item1, right.Item1);
        }

        /// <summary>
        /// 使用值语义比较两个 <see cref="DataBuffer{T1}"/> 实例。
        /// </summary>
        public static bool operator ==(DataBuffer<T1>? left, DataBuffer<T1>? right)
        {
            if (left is null) return right is null;
            return left.Equals(right);
        }

        /// <summary>
        /// 使用值语义判断两个 <see cref="DataBuffer{T1}"/> 实例不相等。
        /// </summary>
        public static bool operator !=(DataBuffer<T1>? left, DataBuffer<T1>? right)
            => !(left == right);

        /// <summary>
        /// 确定指定的对象是否等于当前对象。
        /// </summary>
        /// <param name="obj">要与当前对象进行比较的对象。</param>
        /// <returns>如果指定的对象是 <see cref="DataBuffer{T1}"/> 的实例并且其值与此实例相等，则为 <c>true</c>；否则为 <c>false</c>。</returns>
        public override bool Equals(object? obj)
            => obj is DataBuffer<T1> buffer && ItemEquals(this, buffer);

        /// <summary>
        /// 返回此实例的哈希码。
        /// </summary>
        /// <returns>一个 32 位带符号整数，它是此实例的哈希码。</returns>
        public override int GetHashCode()
        {
            return Item1?.GetHashCode() ?? 0;
        }

        /// <summary>
        /// 返回表示当前对象的字符串。
        /// </summary>
        /// <returns>表示当前对象的字符串。</returns>
        public override string ToString()
        {
            return Item1?.ToString() ?? "<null>";
        }

        /// <summary>
        /// 定义从 <see cref="DataBuffer{T1}"/> 到其承载值 <typeparamref name="T1"/> 的隐式转换。
        /// </summary>
        /// <param name="buffer">要转换的数据缓冲区。</param>
        /// <returns><see cref="Item1"/> 的值。</returns>
        public static implicit operator T1?(DataBuffer<T1> buffer)
            => buffer.Item1;

        /// <summary>
        /// 定义从值 <typeparamref name="T1"/> 到 <see cref="DataBuffer{T1}"/> 的隐式转换。
        /// </summary>
        /// <param name="value">要包装在数据缓冲区中的值。</param>
        /// <returns>包含该值的新 <see cref="DataBuffer{T1}"/> 实例。</returns>
        public static implicit operator DataBuffer<T1>(T1? value)
            => Get(value);
    }

    /// <summary>
    /// 双值泛型数据缓冲区。
    /// </summary>
    /// <typeparam name="T1">第一个泛型参数类型。</typeparam>
    /// <typeparam name="T2">第二个泛型参数类型。</typeparam>
    public class DataBuffer<T1, T2> : DataBuffer, IEquatable<DataBuffer<T1, T2>>
    {
        /// <summary>
        /// 数据缓冲区对象池。
        /// </summary>
        private static readonly ObjectPool<DataBuffer<T1, T2>> pool
            = ObjectPool.Create<DataBuffer<T1, T2>>();

        /// <summary>
        /// 从对象池获取实例，并初始化两个数据项。
        /// </summary>
        /// <param name="item1">第一个数据项，默认 <c>default</c>。</param>
        /// <param name="item2">第二个数据项，默认 <c>default</c>。</param>
        /// <returns>数据缓冲区实例。</returns>
        public static DataBuffer<T1, T2> Get(T1? item1 = default, T2? item2 = default)
        {
            var buffer = pool.Get();
            buffer.Item1 = item1;
            buffer.Item2 = item2;
            return buffer;
        }

        /// <summary>
        /// 第一个泛型参数的数据项。
        /// </summary>
        public T1? Item1 { get; set; }

        /// <summary>
        /// 第二个泛型参数的数据项。
        /// </summary>
        public T2? Item2 { get; set; }

        /// <summary>
        /// 释放内部后备缓冲并将当前实例归还对象池。
        /// </summary>
        public override void Release()
        {
            Item1 = default;
            Item2 = default;
            pool.Release(this);
        }

        private static bool ItemEquals(DataBuffer<T1, T2> left, DataBuffer<T1, T2> right)
        {
            return DataBuffer<T1>.comparer.Equals(left.Item1, right.Item1)
                && DataBuffer<T2>.comparer.Equals(left.Item2, right.Item2);
        }

        /// <summary>
        /// 确定两个 <see cref="DataBuffer{T1, T2}"/> 实例是否相等。
        /// </summary>
        public static bool operator ==(DataBuffer<T1, T2>? left, DataBuffer<T1, T2>? right)
        {
            if (left is null) return right is null;
            return left.Equals(right);
        }

        /// <summary>
        /// 确定两个 <see cref="DataBuffer{T1, T2}"/> 实例是否不相等。
        /// </summary>
        public static bool operator !=(DataBuffer<T1, T2>? left, DataBuffer<T1, T2>? right)
            => !(left == right);

        /// <summary>
        /// 值语义比较两个缓冲区。
        /// </summary>
        public override bool Equals(DataBuffer? other)
        {
            return other is DataBuffer<T1, T2> otherBuffer && Equals(otherBuffer);
        }

        /// <summary>
        /// 确定当前对象是否等于同一类型的另一个对象。
        /// </summary>
        /// <param name="other">要与当前对象进行比较的对象。</param>
        /// <returns>如果指定的对象等于当前对象，则为 <c>true</c>；否则为 <c>false</c>。</returns>
        public bool Equals(DataBuffer<T1, T2>? other)
        {
            if (other is null) return false;
            return ItemEquals(this, other);
        }

        /// <summary>
        /// 确定指定的对象是否等于当前对象。
        /// </summary>
        /// <param name="obj">要与当前对象进行比较的对象。</param>
        /// <returns>如果指定的对象是 <see cref="DataBuffer{T1, T2}"/> 的实例并且其值与此实例相等，则为 <c>true</c>；否则为 <c>false</c>。</returns>
        public override bool Equals(object? obj)
        {
            return Equals(obj as DataBuffer<T1, T2>);
        }

        /// <summary>
        /// 返回此实例的哈希码。
        /// </summary>
        /// <returns>一个 32 位带符号整数，它是此实例的哈希码。</returns>
        public override int GetHashCode()
        {
            return HashCode.Combine(Item1, Item2);
        }

        /// <summary>
        /// 返回表示当前对象的字符串。
        /// </summary>
        /// <returns>表示当前对象的字符串。</returns>
        public override string ToString()
        {
            return $"({Item1?.ToString() ?? "<null>"}, {Item2?.ToString() ?? "<null>"})";
        }

        /// <summary>
        /// 确定此缓冲区实例是否包含指定的值类型。
        /// </summary>
        /// <typeparam name="T">要检查的值类型。</typeparam>
        /// <returns>如果 <typeparamref name="T"/> 与 <typeparamref name="T1"/> 或 <typeparamref name="T2"/> 是相同类型，则为 <c>true</c>；否则为 <c>false</c>。</returns>
        public override bool HasValueType<T>()
        {
            return typeof(T) == typeof(T1) || typeof(T) == typeof(T2);
        }

        /// <summary>
        /// 定义从 <see cref="DataBuffer{T1, T2}"/> 到其承载值的元组的隐式转换。
        /// </summary>
        /// <param name="buffer">要转换的数据缓冲区。</param>
        /// <returns>包含缓冲区值的元组。</returns>
        public static implicit operator (T1?, T2?)(DataBuffer<T1, T2> buffer)
            => (buffer.Item1, buffer.Item2);

        /// <summary>
        /// 定义从值的元组到 <see cref="DataBuffer{T1, T2}"/> 的隐式转换。
        /// </summary>
        /// <param name="value">要包装在数据缓冲区中的值元组。</param>
        /// <returns>包含该值元组的新 <see cref="DataBuffer{T1, T2}"/> 实例。</returns>
        public static implicit operator DataBuffer<T1, T2>((T1? item1, T2? item2) value)
            => Get(value.item1, value.item2);
    }

    /// <summary>
    /// 三值泛型数据缓冲区。
    /// </summary>
    /// <typeparam name="T1">第一个泛型参数的类型。</typeparam>
    /// <typeparam name="T2">第二个泛型参数的类型。</typeparam>
    /// <typeparam name="T3">第三个泛型参数的类型。</typeparam>
    public class DataBuffer<T1, T2, T3> : DataBuffer, IEquatable<DataBuffer<T1, T2, T3>>
    {
        /// <summary>
        /// 数据缓冲区对象池。
        /// </summary>
        private static readonly ObjectPool<DataBuffer<T1, T2, T3>> _pool
            = ObjectPool.Create<DataBuffer<T1, T2, T3>>();

        /// <summary>
        /// 从对象池获取实例，并初始化三个数据项。
        /// </summary>
        /// <param name="item1">第一个数据项，默认 <c>default</c>。</param>
        /// <param name="item2">第二个数据项，默认 <c>default</c>。</param>
        /// <param name="item3">第三个数据项，默认 <c>default</c>。</param>
        /// <returns>数据缓冲区实例。</returns>
        public static DataBuffer<T1, T2, T3> Get(T1? item1 = default, T2? item2 = default, T3? item3 = default)
        {
            var buffer = _pool.Get();
            buffer.Item1 = item1;
            buffer.Item2 = item2;
            buffer.Item3 = item3;
            return buffer;
        }

        /// <summary>
        /// 第一个泛型参数的数据项。
        /// </summary>
        public T1? Item1 { get; set; }

        /// <summary>
        /// 第二个泛型参数的数据项。
        /// </summary>
        public T2? Item2 { get; set; }

        /// <summary>
        /// 第三个泛型参数的数据项。
        /// </summary>
        public T3? Item3 { get; set; }

        /// <summary>
        /// 释放内部后备缓冲并将当前实例归还对象池。
        /// </summary>
        public override void Release()
        {
            Item1 = default;
            Item2 = default;
            Item3 = default;
            _pool.Release(this);
        }

        private static bool ItemEquals(DataBuffer<T1, T2, T3> left, DataBuffer<T1, T2, T3> right)
        {
            return DataBuffer<T1>.comparer.Equals(left.Item1, right.Item1)
                && DataBuffer<T2>.comparer.Equals(left.Item2, right.Item2)
                && DataBuffer<T3>.comparer.Equals(left.Item3, right.Item3);
        }

        /// <summary>
        /// 值语义比较两个缓冲区。
        /// </summary>
        public override bool Equals(DataBuffer? other)
        {
            return other is DataBuffer<T1, T2, T3> otherBuffer && Equals(otherBuffer);
        }

        /// <summary>
        /// 确定当前对象是否等于同一类型的另一个对象。
        /// </summary>
        /// <param name="other">要与当前对象进行比较的对象。</param>
        /// <returns>如果指定的对象等于当前对象，则为 <c>true</c>；否则为 <c>false</c>。</returns>
        public bool Equals(DataBuffer<T1, T2, T3>? other)
        {
            if (other is null) return false;
            return ItemEquals(this, other);
        }

        /// <summary>
        /// 返回此实例的哈希码。
        /// </summary>
        /// <returns>一个 32 位带符号整数，它是此实例的哈希码。</returns>
        public override int GetHashCode()
        {
            return HashCode.Combine(Item1, Item2, Item3);
        }

        /// <summary>
        /// 确定两个 <see cref="DataBuffer{T1, T2, T3}"/> 实例是否相等。
        /// </summary>
        public static bool operator ==(DataBuffer<T1, T2, T3>? left, DataBuffer<T1, T2, T3>? right)
        {
            if (left is null) return right is null;
            return left.Equals(right);
        }

        /// <summary>
        /// 确定两个 <see cref="DataBuffer{T1, T2, T3}"/> 实例是否不相等。
        /// </summary>
        public static bool operator !=(DataBuffer<T1, T2, T3>? left, DataBuffer<T1, T2, T3>? right)
            => !(left == right);

        /// <summary>
        /// 确定指定的对象是否等于当前对象。
        /// </summary>
        /// <param name="obj">要与当前对象进行比较的对象。</param>
        /// <returns>如果指定的对象是 <see cref="DataBuffer{T1, T2, T3}"/> 的实例并且其值与此实例相等，则为 <c>true</c>；否则为 <c>false</c>。</returns>
        public override bool Equals(object? obj)
        {
            return Equals(obj as DataBuffer<T1, T2, T3>);
        }

        /// <summary>
        /// 返回表示当前对象的字符串。
        /// </summary>
        /// <returns>表示当前对象的字符串。</returns>
        public override string ToString()
        {
            return $"({Item1?.ToString() ?? "<null>"}, {Item2?.ToString() ?? "<null>"}, {Item3?.ToString() ?? "<null>"})";
        }

        /// <summary>
        /// 确定此缓冲区实例是否包含指定的值类型。
        /// </summary>
        /// <typeparam name="T">要检查的值类型。</typeparam>
        /// <returns>
        /// 如果 <typeparamref name="T"/> 与 <typeparamref name="T1"/>、 <typeparamref name="T2"/> 或 <typeparamref name="T3"/> 中任何一个类型相同，则为
        /// <c>true</c>；否则为 <c>false</c>。
        /// </returns>
        public override bool HasValueType<T>()
        {
            return typeof(T) == typeof(T1)
                || typeof(T) == typeof(T2)
                || typeof(T) == typeof(T3);
        }

        /// <summary>
        /// 定义从 <see cref="DataBuffer{T1, T2, T3}"/> 到其承载值的元组的隐式转换。
        /// </summary>
        /// <param name="buffer">要转换的数据缓冲区。</param>
        /// <returns>包含缓冲区值的元组。</returns>
        public static implicit operator (T1?, T2?, T3?)(DataBuffer<T1, T2, T3> buffer)
            => (buffer.Item1, buffer.Item2, buffer.Item3);

        /// <summary>
        /// 定义从值的元组到 <see cref="DataBuffer{T1, T2, T3}"/> 的隐式转换。
        /// </summary>
        /// <param name="value">要包装在数据缓冲区中的值元组。</param>
        /// <returns>包含该值元组的新 <see cref="DataBuffer{T1, T2, T3}"/> 实例。</returns>
        public static implicit operator DataBuffer<T1, T2, T3>((T1? item1, T2? item2, T3? item3) value)
            => Get(value.item1, value.item2, value.item3);
    }

    /// <summary>
    /// 四值泛型数据缓冲区。
    /// </summary>
    /// <typeparam name="T1">缓冲区中第一个元素的类型。</typeparam>
    /// <typeparam name="T2">缓冲区中第二个元素的类型。</typeparam>
    /// <typeparam name="T3">缓冲区中第三个元素的类型。</typeparam>
    /// <typeparam name="T4">缓冲区中第四个元素的类型。</typeparam>
    public class DataBuffer<T1, T2, T3, T4> : DataBuffer, IEquatable<DataBuffer<T1, T2, T3, T4>>
    {
        /// <summary>
        /// 静态对象池，用于存储和重用 <see cref="DataBuffer{T1, T2, T3, T4}"/> 实例。
        /// </summary>
        private static readonly ObjectPool<DataBuffer<T1, T2, T3, T4>> pool
            = ObjectPool.Create<DataBuffer<T1, T2, T3, T4>>();

        /// <summary>
        /// 从对象池获取一个 <see cref="DataBuffer{T1, T2, T3, T4}"/> 实例，并使用提供的项对其进行初始化。
        /// </summary>
        /// <param name="item1">要存储在缓冲区的第一个项。</param>
        /// <param name="item2">要存储在缓冲区的第二个项。</param>
        /// <param name="item3">要存储在缓冲区的第三个项。</param>
        /// <param name="item4">要存储在缓冲区的第四个项。</param>
        /// <returns>一个初始化的 <see cref="DataBuffer{T1, T2, T3, T4}"/> 实例。</returns>
        public static DataBuffer<T1, T2, T3, T4> Get(
            T1? item1 = default,
            T2? item2 = default,
            T3? item3 = default,
            T4? item4 = default)
        {
            var buffer = pool.Get();
            buffer.Item1 = item1;
            buffer.Item2 = item2;
            buffer.Item3 = item3;
            buffer.Item4 = item4;
            return buffer;
        }

        /// <summary>
        /// 第一个泛型参数的数据项。
        /// </summary>
        public T1? Item1 { get; set; }

        /// <summary>
        /// 第二个泛型参数的数据项。
        /// </summary>
        public T2? Item2 { get; set; }

        /// <summary>
        /// 第三个泛型参数的数据项。
        /// </summary>
        public T3? Item3 { get; set; }

        /// <summary>
        /// 缓冲区中的第四个元素。
        /// </summary>
        public T4? Item4 { get; set; }

        /// <summary>
        /// 值语义比较两个缓冲区。
        /// </summary>
        public override bool Equals(DataBuffer? other)
        {
            return other is DataBuffer<T1, T2, T3, T4> otherBuffer && Equals(otherBuffer);
        }

        /// <summary>
        /// 释放内部后备缓冲并将当前实例归还对象池。
        /// </summary>
        public override void Release()
        {
            Item1 = default;
            Item2 = default;
            Item3 = default;
            Item4 = default;
            pool.Release(this);
        }

        private static bool ItemEquals(DataBuffer<T1, T2, T3, T4> left, DataBuffer<T1, T2, T3, T4> right)
        {
            return DataBuffer<T1>.comparer.Equals(left.Item1, right.Item1)
                && DataBuffer<T2>.comparer.Equals(left.Item2, right.Item2)
                && DataBuffer<T3>.comparer.Equals(left.Item3, right.Item3)
                && DataBuffer<T4>.comparer.Equals(left.Item4, right.Item4);
        }

        /// <summary>
        /// 确定两个 <see cref="DataBuffer{T1, T2, T3, T4}"/> 实例是否相等。
        /// </summary>
        public static bool operator ==(DataBuffer<T1, T2, T3, T4>? left, DataBuffer<T1, T2, T3, T4>? right)
        {
            if (left is null) return right is null;
            return left.Equals(right);
        }

        /// <summary>
        /// 确定两个 <see cref="DataBuffer{T1, T2, T3, T4}"/> 实例是否不相等。
        /// </summary>
        public static bool operator !=(DataBuffer<T1, T2, T3, T4>? left, DataBuffer<T1, T2, T3, T4>? right)
            => !(left == right);

        /// <summary>
        /// 确定当前对象是否等于同一类型的另一个对象。
        /// </summary>
        /// <param name="other">要与当前对象进行比较的对象。</param>
        /// <returns>如果指定的对象等于当前对象，则为 <c>true</c>；否则为 <c>false</c>。</returns>
        public bool Equals(DataBuffer<T1, T2, T3, T4>? other)
        {
            if (other is null) return false;
            return ItemEquals(this, other);
        }

        /// <summary>
        /// 确定指定的对象是否等于当前对象。
        /// </summary>
        /// <param name="obj">要与当前对象进行比较的对象。</param>
        /// <returns>如果指定的对象是 <see cref="DataBuffer{T1, T2, T3, T4}"/> 的实例并且其值与此实例相等，则为 <c>true</c>；否则为 <c>false</c>。</returns>
        public override bool Equals(object? obj)
        {
            return Equals(obj as DataBuffer<T1, T2, T3, T4>);
        }

        /// <summary>
        /// 返回此实例的哈希码。
        /// </summary>
        /// <returns>一个 32 位带符号整数，它是此实例的哈希码。</returns>
        public override int GetHashCode()
        {
            return HashCode.Combine(Item1, Item2, Item3, Item4);
        }

        /// <summary>
        /// 返回表示当前对象的字符串。
        /// </summary>
        /// <returns>表示当前对象的字符串。</returns>
        public override string ToString()
        {
            return $"({Item1?.ToString() ?? "<null>"}, {Item2?.ToString() ?? "<null>"}, {Item3?.ToString() ?? "<null>"}, {Item4?.ToString() ?? "<null>"})";
        }

        /// <summary>
        /// 确定此缓冲区实例是否包含指定的值类型。
        /// </summary>
        /// <typeparam name="T">要检查的值类型。</typeparam>
        /// <returns>
        /// 如果 <typeparamref name="T"/> 与 <typeparamref name="T1"/>、 <typeparamref name="T2"/>、 <typeparamref name="T3"/> 或 <typeparamref name="T4"/>
        /// 中任何一个类型相同，则为 <c>true</c>；否则为 <c>false</c>。
        /// </returns>
        public override bool HasValueType<T>()
        {
            return typeof(T) == typeof(T1)
                || typeof(T) == typeof(T2)
                || typeof(T) == typeof(T3)
                || typeof(T) == typeof(T4);
        }

        /// <summary>
        /// 定义从 <see cref="DataBuffer{T1, T2, T3, T4}"/> 到其承载值的元组的隐式转换。
        /// </summary>
        /// <param name="buffer">要转换的数据缓冲区。</param>
        /// <returns>包含缓冲区值的元组。</returns>
        public static implicit operator (T1?, T2?, T3?, T4?)(DataBuffer<T1, T2, T3, T4> buffer)
            => (buffer.Item1, buffer.Item2, buffer.Item3, buffer.Item4);

        /// <summary>
        /// 定义从值的元组到 <see cref="DataBuffer{T1, T2, T3, T4}"/> 的隐式转换。
        /// </summary>
        /// <param name="value">要包装在数据缓冲区中的值元组。</param>
        /// <returns>包含该值元组的新 <see cref="DataBuffer{T1, T2, T3, T4}"/> 实例。</returns>
        public static implicit operator DataBuffer<T1, T2, T3, T4>((T1? item1, T2? item2, T3? item3, T4? item4) value)
            => Get(value.item1, value.item2, value.item3, value.item4);
    }
}