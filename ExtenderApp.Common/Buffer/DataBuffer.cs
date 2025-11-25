using ExtenderApp.Common.ObjectPools;

namespace ExtenderApp.Common.DataBuffers
{
    /// <summary>
    /// 数据缓冲区基类，提供跨缓冲区类型的相等性比较入口。
    /// </summary>
    public abstract class DataBuffer : IEquatable<DataBuffer>
    {
        public static DataBuffer Empty { get; } = new EmptyDataBuffer();

        /// <summary>
        /// 比较当前缓冲区与另一个缓冲区是否相等。
        /// </summary>
        /// <param name="other">要比较的另一个缓冲区。</param>
        /// <returns>
        /// 相等返回 <c>true</c>；否则返回 <c>false</c>。
        /// </returns>
        public abstract bool Equals(DataBuffer? other);

        /// <summary>
        /// 将当前实例重置并归还到对象池。
        /// </summary>
        /// <remarks>
        /// 会先将 <see cref="Item1"/> 重置为默认值，然后释放回对象池。
        /// </remarks>
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

        public static DataBuffer<T> FromValue<T>(T value)
        {
            return DataBuffer<T>.Get(value);
        }

        public static DataBuffer<T1, T2> FromValues<T1, T2>(T1 item1, T2 item2)
        {
            return DataBuffer<T1, T2>.Get(item1, item2);
        }
    }

    /// <summary>
    /// 表示一个专门的“空”数据缓冲区实例，用于表示无数据或占位。
    /// 不参与对象池管理，<see cref="Release"/> 是空操作。
    /// </summary>
    public class EmptyDataBuffer : DataBuffer
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

        public override bool HasValueType<T>()
        {
            return false;
        }

        /// <summary>
        /// 返回用于显示的文本表示：<c>&lt;empty&gt;</c>。
        /// </summary>
        public override string ToString()
        {
            return "<empty>";
        }
    }

    /// <summary>
    /// 单值泛型数据缓冲区。
    /// </summary>
    /// <typeparam name="T">缓冲区存储的数据类型。</typeparam>
    public class DataBuffer<T1> : DataBuffer, IEquatable<DataBuffer<T1>>
    {
        /// <summary>
        /// 当前类型缓冲区的全局对象池。
        /// </summary>
        private static ObjectPool<DataBuffer<T1>> pool
            = ObjectPool.CreateDefaultPool<DataBuffer<T1>>();

        public static EqualityComparer<T1?> comparer = EqualityComparer<T1?>.Default;

        /// <summary>
        /// 从对象池获取一个 <see cref="DataBuffer{T}"/> 实例，并可选初始化其值。
        /// </summary>
        /// <param name="value">初始值，默认 <c>default</c>。</param>
        /// <returns>获取到的缓冲区实例。</returns>
        /// <remarks>
        /// 使用完成后请调用 <see cref="Release"/> 将实例归还对象池。
        /// </remarks>
        public static DataBuffer<T1> Get(T1? value = default)
        {
            var buffer = pool.Get();
            buffer.Item1 = value;
            return buffer;
        }

        /// <summary>
        /// 将当前实例重置并归还到对象池。
        /// </summary>
        /// <remarks>
        /// 会先将 <see cref="Item1"/> 重置为默认值，然后释放回对象池。
        /// </remarks>
        public override void Release()
        {
            Item1 = default;
            pool.Release(this);
        }

        /// <summary>
        /// 当前缓冲区承载的值。
        /// </summary>
        public T1? Item1 { get; set; }

        public override bool HasValueType<T>()
        {
            return typeof(T) != typeof(T1);
        }

        /// <summary>
        /// 比较与另一个缓冲区是否相等（基于值语义）。
        /// </summary>
        /// <param name="other">另一个缓冲区。</param>
        /// <returns>
        /// 当且仅当 <paramref name="other"/>
        /// 为相同泛型参数的缓冲区且两者 <see cref="Item1"/>
        /// 相等时返回 <c>true</c>。
        /// </returns>
        public override bool Equals(DataBuffer? other)
        {
            if (other == null) return false;
            if (other is not DataBuffer<T1> otherBuffer)
            {
                return false;
            }
            return comparer.Equals(Item1, otherBuffer.Item1);
        }

        public bool Equals(DataBuffer<T1>? other)
        {
            if (other == null) return false;
            return ItemEquals(this, other);
        }

        private static bool ItemEquals(DataBuffer<T1> left, DataBuffer<T1> right)
        {
            return comparer.Equals(left.Item1, right.Item1);
        }

        /// <summary>
        /// 使用值语义比较两个 <see cref="DataBuffer{T}"/> 实例。
        /// </summary>
        public static bool operator ==(DataBuffer<T1>? left, DataBuffer<T1>? right)
        {
            if (left == null && right == null) return true;
            if (left == null || right == null) return false;
            return ItemEquals(left, right);
        }

        /// <summary>
        /// 使用值语义判断两个 <see cref="DataBuffer{T}"/> 实例不相等。
        /// </summary>
        public static bool operator !=(DataBuffer<T1>? left, DataBuffer<T1>? right)
            => !(left == right);

        public override bool Equals(object? obj)
            => obj is DataBuffer<T1> buffer && ItemEquals(this, buffer);

        public override int GetHashCode()
        {
            return Item1?.GetHashCode() ?? 0;
        }

        public override string ToString()
        {
            return Item1?.ToString() ?? "<null>";
        }

        /// <summary>
        /// 隐式转换为其承载的值。
        /// </summary>
        /// <param name="buffer">数据缓冲区。</param>
        /// <returns><see cref="Item1"/> 的值。</returns>
        public static implicit operator T1?(DataBuffer<T1> buffer)
            => buffer.Item1;

        public static implicit operator DataBuffer<T1>(T1? value)
            => Get(value);
    }

    /// <summary>
    /// 双值泛型数据缓冲区，内部通过两个 <see
    /// cref="DataBuffer{T}"/> 作为后备缓冲承载值。
    /// </summary>
    /// <typeparam name="T1">第一个泛型参数类型。</typeparam>
    /// <typeparam name="T2">第二个泛型参数类型。</typeparam>
    public class DataBuffer<T1, T2> : DataBuffer, IEquatable<DataBuffer<T1, T2>>
    {
        /// <summary>
        /// 数据缓冲区对象池。
        /// </summary>
        private static ObjectPool<DataBuffer<T1, T2>> pool
            = ObjectPool.CreateDefaultPool<DataBuffer<T1, T2>>();

        /// <summary>
        /// 从对象池获取实例，并初始化两个数据项。
        /// </summary>
        /// <param name="item1">第一个数据项，默认 <c>default</c>。</param>
        /// <param name="item2">第二个数据项，默认 <c>default</c>。</param>
        /// <returns>数据缓冲区实例。</returns>
        public static DataBuffer<T1, T2> Get(T1? item1 = default, T2? item2 = default)
        {
            var buffer = pool.Get();
            buffer.item1Buffer = FromValue(item1!);
            buffer.item2Buffer = FromValue(item2!);
            return buffer;
        }

        /// <summary>
        /// 第一个数据项的后备缓冲。
        /// </summary>
        private DataBuffer<T1> item1Buffer;

        /// <summary>
        /// 第一个泛型参数的数据项。
        /// </summary>
        public T1? Item1
        {
            get => item1Buffer.Item1;
            set => item1Buffer.Item1 = value;
        }

        /// <summary>
        /// 第二个数据项的后备缓冲。
        /// </summary>
        private DataBuffer<T2> item2Buffer;

        /// <summary>
        /// 第二个泛型参数的数据项。
        /// </summary>
        public T2? Item2
        {
            get => item2Buffer.Item1;
            set => item2Buffer.Item1 = value;
        }

        public DataBuffer()
        {
            item1Buffer = DataBuffer<T1>.Get();
            item2Buffer = DataBuffer<T2>.Get();
        }

        /// <summary>
        /// 释放内部后备缓冲并将当前实例归还对象池。
        /// </summary>
        public override void Release()
        {
            item1Buffer.Release();
            item2Buffer.Release();
            pool.Release(this);
        }

        private static bool ItemEquals(DataBuffer<T1, T2> left, DataBuffer<T1, T2> right)
        {
            return DataBuffer<T1>.comparer.Equals(left.Item1, right.Item1)
                && DataBuffer<T2>.comparer.Equals(left.Item2, right.Item2);
        }

        public static bool operator ==(DataBuffer<T1, T2>? left, DataBuffer<T1, T2>? right)
        {
            if (left == null && right == null) return true;
            if (left == null || right == null) return false;
            return ItemEquals(left, right);
        }

        public static bool operator !=(DataBuffer<T1, T2>? left, DataBuffer<T1, T2>? right)
            => !(left == right);

        /// <summary>
        /// 值语义比较两个缓冲区。
        /// </summary>
        public override bool Equals(DataBuffer? other)
        {
            if (other == null)
                return false;
            if (other is not DataBuffer<T1, T2> otherBuffer)
            {
                return false;
            }
            return Equals(otherBuffer);
        }

        public bool Equals(DataBuffer<T1, T2>? other)
        {
            if (other == null)
                return false;
            return ItemEquals(this, other);
        }

        public override bool Equals(object? obj)
        {
            return Equals(obj as DataBuffer<T1, T2>);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Item1, Item2);
        }

        public override string ToString()
        {
            return $"({Item1?.ToString() ?? "<null>"}, {Item2?.ToString() ?? "<null>"})";
        }

        public override bool HasValueType<T>()
        {
            return typeof(T) != typeof(T1) || typeof(T) != typeof(T2);
        }

        public static implicit operator (T1?, T2?)(DataBuffer<T1, T2> buffer)
            => (buffer.Item1, buffer.Item2);

        public static implicit operator DataBuffer<T1, T2>((T1? item1, T2? item2) value)
            => Get(value.item1, value.item2);

        public static implicit operator DataBuffer<T1, T2>(T1? item1)
            => Get(item1, default);

        public static implicit operator DataBuffer<T1, T2>(T2? item2)
            => Get(default, item2);

        public static implicit operator T1?(DataBuffer<T1, T2> value)
            => value.Item1;

        public static implicit operator T2?(DataBuffer<T1, T2> value)
            => value.Item2;
    }

    ///// <summary>
    ///// 三值泛型数据缓冲区，内部通过三个 <see
    ///// cref="DataBuffer{T}"/> 作为后备缓冲承载值。
    ///// </summary>
    ///// <typeparam name="T1">第一个泛型参数的类型。</typeparam>
    ///// <typeparam name="T2">第二个泛型参数的类型。</typeparam>
    ///// <typeparam name="T3">第三个泛型参数的类型。</typeparam>
    //public class DataBuffer<T1, T2, T3> : DataBuffer, IEquatable<DataBuffer<T1, T2, T3>>
    //{
    //    /// <summary>
    //    /// 数据缓冲区对象池。
    //    /// </summary>
    //    private static ObjectPool<DataBuffer<T1, T2, T3>> pool
    //        = ObjectPool.CreateDefaultPool<DataBuffer<T1, T2, T3>>();

    //    /// <summary>
    //    /// 从对象池获取实例，并初始化三个数据项。
    //    /// </summary>
    //    /// <param name="item1">第一个数据项，默认 <c>default</c>。</param>
    //    /// <param name="item2">第二个数据项，默认 <c>default</c>。</param>
    //    /// <param name="item3">第三个数据项，默认 <c>default</c>。</param>
    //    /// <returns>数据缓冲区实例。</returns>
    //    public static DataBuffer<T1, T2, T3> Get(T1? item1 = default, T2? item2 = default, T3? item3 = default)
    //    {
    //        var Buffer = pool.Get();
    //        Buffer.item1Buffer = DataBuffer<T1>.Get(item1);
    //        Buffer.item2Buffer = DataBuffer<T2>.Get(item2);
    //        Buffer.item3Buffer = DataBuffer<T3>.Get(item3);
    //        return Buffer;
    //    }

    //    /// <summary>
    //    /// 第一个数据项的后备缓冲。
    //    /// </summary>
    //    private DataBuffer<T1> item1Buffer;

    //    /// <summary>
    //    /// 第一个泛型参数的数据项。
    //    /// </summary>
    //    public T1? Item1
    //    {
    //        get => item1Buffer.Item1;
    //        set => item1Buffer.Item1 = Value;
    //    }

    //    /// <summary>
    //    /// 第二个数据项的后备缓冲。
    //    /// </summary>
    //    private DataBuffer<T2> item2Buffer;

    //    /// <summary>
    //    /// 第二个泛型参数的数据项。
    //    /// </summary>
    //    public T2? Item2
    //    {
    //        get => item2Buffer.Item1;
    //        set => item2Buffer.Item1 = Value;
    //    }

    //    /// <summary>
    //    /// 第三个数据项的后备缓冲。
    //    /// </summary>
    //    private DataBuffer<T3> item3Buffer;

    //    /// <summary>
    //    /// 第三个泛型参数的数据项。
    //    /// </summary>
    //    public T3? Item3
    //    {
    //        get => item3Buffer.Item1;
    //        set => item3Buffer.Item1 = Value;
    //    }

    //    public DataBuffer()
    //    {
    //        item1Buffer = DataBuffer<T1>.Get();
    //        item2Buffer = DataBuffer<T2>.Get();
    //        item3Buffer = DataBuffer<T3>.Get();
    //    }

    //    /// <summary>
    //    /// 释放内部后备缓冲并将当前实例归还对象池。
    //    /// </summary>
    //    public override void Release()
    //    {
    //        item1Buffer.Release();
    //        item2Buffer.Release();
    //        item3Buffer.Release();
    //        pool.Release(this);
    //    }

    //    private static bool ItemEquals(DataBuffer<T1, T2, T3> left, DataBuffer<T1, T2, T3> right)
    //    {
    //        return DataBuffer<T1>.comparer.Equals(left.Item1, right.Item1)
    //            && DataBuffer<T2>.comparer.Equals(left.Item2, right.Item2)
    //            && DataBuffer<T3>.comparer.Equals(left.Item3, right.Item3);
    //    }

    //    /// <summary>
    //    /// 值语义比较两个缓冲区。
    //    /// </summary>
    //    public override bool Equals(DataBuffer? other)
    //    {
    //        if (other == null)
    //            return false;
    //        if (other is not DataBuffer<T1, T2, T3> otherBuffer)
    //            return false;
    //        return ItemEquals(this, otherBuffer);
    //    }

    //    public bool Equals(DataBuffer<T1, T2, T3>? other)
    //    {
    //        if (other == null)
    //            return false;
    //        return ItemEquals(this, other);
    //    }

    //    public override int GetHashCode()
    //    {
    //        return HashCode.Combine(Item1, Item2, Item3);
    //    }

    //    public static bool operator ==(DataBuffer<T1, T2, T3>? left, DataBuffer<T1, T2, T3>? right)
    //    {
    //        if (left == null && right == null) return true;
    //        if (left == null || right == null) return false;
    //        return ItemEquals(left, right);
    //    }

    //    public static bool operator !=(DataBuffer<T1, T2, T3>? left, DataBuffer<T1, T2, T3>? right)
    //        => !(left == right);

    //    public override bool Equals(object? obj)
    //    {
    //        return Equals(obj as DataBuffer<T1, T2, T3>);
    //    }

    //    public override string ToString()
    //    {
    //        return $"({Item1?.ToString() ?? "<null>"}, {Item2?.ToString() ?? "<null>"}, {Item3?.ToString() ?? "<null>"})";
    //    }

    //    public static implicit operator (T1?, T2?, T3?)(DataBuffer<T1, T2, T3> Buffer)
    //        => (Buffer.Item1, Buffer.Item2, Buffer.Item3);

    //    public static implicit operator DataBuffer<T1, T2, T3>((T1? item1, T2? item2, T3? item3) Value)
    //        => Get(Value.item1, Value.item2, Value.item3);

    //    public static implicit operator DataBuffer<T1, T2, T3>(T1? item1)
    //        => Get(item1, default, default);

    //    public static implicit operator DataBuffer<T1, T2, T3>(T2? item2)
    //        => Get(default, item2, default);

    //    public static implicit operator DataBuffer<T1, T2, T3>(T3? item3)
    //        => Get(default, default, item3);

    //    public static implicit operator T1?(DataBuffer<T1, T2, T3> Value)
    //        => Value.Item1;

    //    public static implicit operator T2?(DataBuffer<T1, T2, T3> Value)
    //        => Value.Item2;

    //    public static implicit operator T3?(DataBuffer<T1, T2, T3> Value)
    //        => Value.Item3;
    //}

    ///// <summary>
    ///// 四值泛型数据缓冲区，内部通过四个 <see
    ///// cref="DataBuffer{T}"/> 作为后备缓冲承载值。
    ///// </summary>
    ///// <typeparam name="T1">缓冲区中第一个元素的类型。</typeparam>
    ///// <typeparam name="T2">缓冲区中第二个元素的类型。</typeparam>
    ///// <typeparam name="T3">缓冲区中第三个元素的类型。</typeparam>
    ///// <typeparam name="T4">缓冲区中第四个元素的类型。</typeparam>
    //public class DataBuffer<T1, T2, T3, T4> : DataBuffer
    //{
    //    /// <summary>
    //    /// 静态对象池，用于存储和重用 <see
    //    /// cref="DataBuffer{T1, T2, T3, T4}"/> 实例。
    //    /// </summary>
    //    private static ObjectPool<DataBuffer<T1, T2, T3, T4>> pool
    //        = ObjectPool.CreateDefaultPool<DataBuffer<T1, T2, T3, T4>>();

    //    public static DataBuffer<T1, T2, T3, T4> Get(
    //        T1? item1 = default,
    //        T2? item2 = default,
    //        T3? item3 = default,
    //        T4? item4 = default)
    //    {
    //        var Buffer = pool.Get();
    //        Buffer.item1Buffer = DataBuffer<T1>.Get(item1);
    //        Buffer.item2Buffer = DataBuffer<T2>.Get(item2);
    //        Buffer.item3Buffer = DataBuffer<T3>.Get(item3);
    //        Buffer.item4Buffer = DataBuffer<T4>.Get(item4);
    //        return Buffer;
    //    }

    //    /// <summary>
    //    /// 第一个数据项的后备缓冲。
    //    /// </summary>
    //    private DataBuffer<T1> item1Buffer;

    //    /// <summary>
    //    /// 第一个泛型参数的数据项。
    //    /// </summary>
    //    public T1? Item1
    //    {
    //        get => item1Buffer.Item1;
    //        set => item1Buffer.Item1 = Value;
    //    }

    //    /// <summary>
    //    /// 第二个数据项的后备缓冲。
    //    /// </summary>
    //    private DataBuffer<T2> item2Buffer;

    //    /// <summary>
    //    /// 第二个泛型参数的数据项。
    //    /// </summary>
    //    public T2? Item2
    //    {
    //        get => item2Buffer.Item1;
    //        set => item2Buffer.Item1 = Value;
    //    }

    //    /// <summary>
    //    /// 第三个数据项的后备缓冲。
    //    /// </summary>
    //    private DataBuffer<T3> item3Buffer;

    //    /// <summary>
    //    /// 第三个泛型参数的数据项。
    //    /// </summary>
    //    public T3? Item3
    //    {
    //        get => item3Buffer.Item1;
    //        set => item3Buffer.Item1 = Value;
    //    }

    //    /// <summary>
    //    /// 第四个数据项的后备缓冲。
    //    /// </summary>
    //    private DataBuffer<T4> item4Buffer;

    //    /// <summary>
    //    /// 缓冲区中的第四个元素。
    //    /// </summary>
    //    public T4? Item4
    //    {
    //        get => item4Buffer.Item1;
    //        set => item4Buffer.Item1 = Value;
    //    }

    //    public DataBuffer()
    //    {
    //        item1Buffer = DataBuffer<T1>.Get();
    //        item2Buffer = DataBuffer<T2>.Get();
    //        item3Buffer = DataBuffer<T3>.Get();
    //        item4Buffer = DataBuffer<T4>.Get();
    //    }

    //    /// <summary>
    //    /// 值语义比较两个缓冲区。
    //    /// </summary>
    //    public override bool Equals(DataBuffer? other)
    //    {
    //        if (other == null)
    //            return false;
    //        if (other is not DataBuffer<T1, T2, T3, T4> otherBuffer)
    //            return false;
    //        return ItemEquals(this, otherBuffer);
    //    }

    //    /// <summary>
    //    /// 释放内部后备缓冲并将当前实例归还对象池。
    //    /// </summary>
    //    public override void Release()
    //    {
    //        item1Buffer.Release();
    //        item2Buffer.Release();
    //        item3Buffer.Release();
    //        item4Buffer.Release();
    //        pool.Release(this);
    //    }

    //    private static bool ItemEquals(DataBuffer<T1, T2, T3, T4> left, DataBuffer<T1, T2, T3, T4> right)
    //    {
    //        return DataBuffer<T1>.comparer.Equals(left.Item1, right.Item1)
    //            && DataBuffer<T2>.comparer.Equals(left.Item2, right.Item2)
    //            && DataBuffer<T3>.comparer.Equals(left.Item3, right.Item3)
    //            && DataBuffer<T4>.comparer.Equals(left.Item4, right.Item4);
    //    }

    //    public static bool operator ==(DataBuffer<T1, T2, T3, T4>? left, DataBuffer<T1, T2, T3, T4>? right)
    //    {
    //        if (left == null && right == null) return true;
    //        if (left == null || right == null) return false;
    //        return ItemEquals(left, right);
    //    }

    //    public static bool operator !=(DataBuffer<T1, T2, T3, T4>? left, DataBuffer<T1, T2, T3, T4>? right)
    //        => !(left == right);

    //    public override bool Equals(object? obj)
    //    {
    //        if (ReferenceEquals(this, obj))
    //        {
    //            return true;
    //        }

    //        if (ReferenceEquals(obj, null))
    //        {
    //            return false;
    //        }

    //        if (obj is not DataBuffer<T1, T2, T3, T4> otherBuffer)
    //        {
    //            return false;
    //        }
    //        return ItemEquals(this, otherBuffer);
    //    }

    //    public override int GetHashCode()
    //    {
    //        return HashCode.Combine(Item1, Item2, Item3, Item4);
    //    }

    //    public override string ToString()
    //    {
    //        return $"({Item1?.ToString() ?? "<null>"}, {Item2?.ToString() ?? "<null>"}, {Item3?.ToString() ?? "<null>"}, {Item4?.ToString() ?? "<null>"})";
    //    }

    //    public static implicit operator (T1?, T2?, T3?, T4?)(DataBuffer<T1, T2, T3, T4> Buffer)
    //        => (Buffer.Item1, Buffer.Item2, Buffer.Item3, Buffer.Item4);

    //    public static implicit operator DataBuffer<T1, T2, T3, T4>((T1? item1, T2? item2, T3? item3, T4? item4) Value)
    //        => Get(Value.item1, Value.item2, Value.item3, Value.item4);

    //    public static implicit operator DataBuffer<T1, T2, T3, T4>(T1? item1)
    //        => Get(item1, default, default, default);

    //    public static implicit operator DataBuffer<T1, T2, T3, T4>(T2? item2)
    //        => Get(default, item2, default, default);

    //    public static implicit operator DataBuffer<T1, T2, T3, T4>(T3? item3)
    //        => Get(default, default, item3, default);

    //    public static implicit operator DataBuffer<T1, T2, T3, T4>(T4? item4)
    //        => Get(default, default, default, item4);

    //    public static implicit operator T1?(DataBuffer<T1, T2, T3, T4> Value)
    //        => Value.Item1;

    //    public static implicit operator T2?(DataBuffer<T1, T2, T3, T4> Value)
    //        => Value.Item2;

    //    public static implicit operator T3?(DataBuffer<T1, T2, T3, T4> Value)
    //        => Value.Item3;

    //    public static implicit operator T4?(DataBuffer<T1, T2, T3, T4> Value)
    //        => Value.Item4;
    //}
}