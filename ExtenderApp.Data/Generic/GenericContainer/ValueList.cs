using System.Collections;
using System.Data;

namespace ExtenderApp.Data
{
    /// <summary>
    /// ValueList 泛型结构体，实现了 IList<T> 接口，用于管理值类型的集合。
    /// 类内部使用类，最好不要传递
    /// </summary>
    /// <typeparam name="T">集合中元素的类型。</typeparam>
    public struct ValueList<T> : IList<T>, IEquatable<ValueList<T>>
    {
        /// <summary>
        /// 默认的数组长度。
        /// </summary>
        private const int m_DefaultLength = 4;

        /// <summary>
        /// 存储集合元素的数组。
        /// </summary>
        private T[] array;

        /// <summary>
        /// 获取一个对象，用于锁定。
        /// </summary>
        public object LockObject => array;

        /// <summary>
        /// 获取一个值，指示集合是否为空。
        /// </summary>
        public bool IsEmpty => LockObject == null;

        /// <summary>
        /// 获取一个值，指示集合是否为只读。
        /// </summary>
        public bool IsReadOnly => false;

        /// <summary>
        /// 获取表示数组元素的 UnreadSpan<T>。
        /// </summary>
        public Span<T> SpanArray => array;

        /// <summary>
        /// 获取或设置集合中的元素个数。
        /// </summary>
        public int Count { get; set; }

        /// <summary>
        /// 获取或设置指定索引处的元素。
        /// </summary>
        /// <param name="index">要获取或设置的元素的从零开始的索引。</param>
        /// <returns>指定索引处的元素。</returns>
        public T this[int index]
        {
            get
            {
                CheckIndex(index);
                return SpanArray[index];
            }
            set
            {
                CheckIndex(index);
                SpanArray[index] = value;
            }
        }

        /// <summary>
        /// 获取或设置用于比较元素的 EqualityComparer<T>。
        /// </summary>
        private EqualityComparer<T>? _equalityComparer;
        private EqualityComparer<T> equalityComparer
        {
            get
            {
                if (_equalityComparer is null)
                {
                    _equalityComparer = EqualityComparer<T>.Default;
                }
                return _equalityComparer;
            }
        }

        /// <summary>
        /// 初始化 ValueList<T> 的新实例，使用默认长度。
        /// </summary>
        public ValueList()
            : this(m_DefaultLength) { }

        /// <summary>
        /// 初始化 ValueList<T> 的新实例，指定容量。
        /// </summary>
        /// <param name="capacity">集合的初始容量。</param>
        public ValueList(int capacity)
        {
            Count = 0;
            array = new T[capacity];
        }

        /// <summary>
        /// 初始化 ValueList<T> 的新实例，使用指定的数组元素。
        /// </summary>
        /// <param name="array">包含集合元素的数组。</param>
        public ValueList(T[] array)
        {
            Count = array.Length;
            this.array = array;
        }

        /// <summary>
        /// 初始化 ValueList<T> 的新实例，使用指定集合的元素。
        /// </summary>
        /// <param name="array">包含集合元素的 IEnumerable<T>。</param>
        public ValueList(IEnumerable<T> array)
        {
            this.array = new T[m_DefaultLength];
            foreach (T item in array)
            {
                Add(item);
            }
        }

        /// <summary>
        /// 检查索引是否有效。
        /// </summary>
        /// <param name="index">要检查的索引。</param>
        private void CheckIndex(int index)
        {
            if (index < 0 || index > Count)
            {
                throw new IndexOutOfRangeException("插入数据位置超过数据界限");
            }
        }

        /// <summary>
        /// 检查数组是否为空。
        /// </summary>
        private void CheckArrayEmpty()
        {
            ArgumentNullException.ThrowIfNull(LockObject, nameof(ValueList<T>));
        }

        /// <summary>
        /// 扩展数组容量。
        /// </summary>
        private void Expansion()
        {
            if (IsEmpty)
            {
                array = new T[m_DefaultLength];
                return;
            }

            if (Count + 1 < array.Length)
                return;

            int length = array.Length * 2;

            T[] temp = array;
            array = new T[length];
            temp.CopyTo(array, 0);
        }

        /// <summary>
        /// 在集合的末尾添加一个新元素。
        /// </summary>
        /// <param name="item">要添加到集合末尾的元素。</param>
        public void Add(T item)
        {
            Expansion();

            SpanArray[Count] = item;
            Count++;
        }

        /// <summary>
        /// 移除集合中满足指定条件的第一个元素。
        /// </summary>
        /// <param name="predicate">定义要搜索的条件的谓词。</param>
        /// <returns>如果找到元素，则为该元素；否则为类型的默认值。</returns>
        public T? Remove(Predicate<T> predicate)
        {
            for (int i = 0; i < Count; i++)
            {
                T temp = array[i];
                if (predicate(temp))
                {
                    RemoveAt(i);
                    return temp;
                }
            }

            return default(T);
        }

        /// <summary>
        /// 从集合中移除特定对象的第一个匹配项。
        /// </summary>
        /// <param name="item">要从集合中移除的对象。</param>
        /// <returns>如果已从集合中成功移除 item，则为 true；否则为 false。如果在集合中未找到 item，该方法也返回 false。</returns>
        public bool Remove(T item)
        {
            int index = IndexOf(item);
            if (index < 0)
                return false;

            RemoveAt(index);
            return true;
        }

        /// <summary>
        /// 移除集合中指定位置的元素。
        /// </summary>
        /// <param name="index">要移除的元素的从零开始的索引。</param>
        public void RemoveAt(int index)
        {
            CheckArrayEmpty();

            CheckIndex(index);

            for (int i = index; i < Count - 1; i++)
            {
                SpanArray[i] = SpanArray[i + 1];
            }
            Count--;
        }

        /// <summary>
        /// 从集合中移除所有元素。
        /// </summary>
        public void Clear()
        {
            for (int i = 0; i < Count; i++)
            {
                SpanArray[i] = default;
            }
            Count = 0;
        }

        /// <summary>
        /// 在集合中搜索指定的对象，并返回该对象的从零开始的索引。
        /// </summary>
        /// <param name="item">要在集合中定位的对象。</param>
        /// <returns>如果找到 item，则为该对象的从零开始的索引；否则为 -1。</returns>
        public int IndexOf(T item)
        {
            return IndexOf(item, null);
        }

        /// <summary>
        /// 在集合中搜索指定的对象，并返回该对象的从零开始的索引。
        /// </summary>
        /// <param name="item">要在集合中定位的对象。</param>
        /// <param name="comparer">用于比较元素的 IEqualityComparer<T> 实现。</param>
        /// <returns>如果找到 item，则为该对象的从零开始的索引；否则为 -1。</returns>
        public int IndexOf(T item, EqualityComparer<T> comparer)
        {
            CheckArrayEmpty();

            if (Count == 0) return -1;

            comparer = comparer ?? equalityComparer;

            for (int i = 0; i < Count; i++)
            {
                if (comparer.Equals(item, SpanArray[i]))
                {
                    return i;
                }
            }

            return -1;
        }

        /// <summary>
        /// 在集合的指定位置插入一个新元素。
        /// </summary>
        /// <param name="index">要在其中插入新元素的从零开始的索引。</param>
        /// <param name="item">要插入集合中的元素。</param>
        public void Insert(int index, T item)
        {
            Expansion();

            for (int i = Count; i > index; i--)
            {
                SpanArray[i] = SpanArray[i - 1];
            }

            SpanArray[index] = item;
            Count++;
        }

        /// <summary>
        /// 确定集合是否包含指定的元素。
        /// </summary>
        /// <param name="item">要在集合中定位的对象。</param>
        /// <returns>如果在集合中找到 item，则为 true；否则为 false。</returns>
        public bool Contains(T item)
        {
            return Contains(item, null);
        }

        /// <summary>
        /// 确定集合是否包含指定的元素。
        /// </summary>
        /// <param name="item">要在集合中定位的对象。</param>
        /// <param name="comparer">用于比较元素的 IEqualityComparer<T> 实现。</param>
        /// <returns>如果在集合中找到 item，则为 true；否则为 false。</returns>
        public bool Contains(T item, EqualityComparer<T> comparer)
        {
            return IndexOf(item, comparer) >= 0;
        }

        /// <summary>
        /// 确定集合中是否包含与指定谓词所定义的条件匹配的元素。
        /// </summary>
        /// <param name="predicate">要应用于每个元素的谓词。</param>
        /// <returns>如果集合中包含与谓词匹配的元素，则为 true；否则为 false。</returns>
        public bool Contains(Predicate<T> predicate)
        {
            for (int i = 0; i < Count; i++)
            {
                T temp = SpanArray[i];
                if (predicate(temp))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 返回集合中满足指定条件的第一个元素的第一个匹配项。
        /// </summary>
        /// <param name="func">应用于每个元素的函数。</param>
        /// <returns>集合中满足条件的第一个元素；如果未找到任何元素，则为类型的默认值。</returns>
        public T FirstOrDefault(Func<T, bool> func)
        {
            CheckArrayEmpty();

            T item = default;
            for (int i = 0; i < Count; i++)
            {
                if (func(SpanArray[i]))
                {
                    item = SpanArray[i];
                    break;
                }
            }
            return item;
        }

        /// <summary>
        /// 返回集合中满足指定条件的第一个元素的第一个匹配项。
        /// </summary>
        /// <param name="func">应用于每个元素的函数。</param>
        /// <param name="contrast">用于与集合元素进行比较的值。</param>
        /// <returns>集合中满足条件的第一个元素；如果未找到任何元素，则为类型的默认值。</returns>
        public T FirstOrDefault<T1>(Func<T, T1, bool> func, T1 contrast)
        {
            CheckArrayEmpty();

            T item = default;
            for (int i = 0; i < Count; i++)
            {
                if (func(SpanArray[i], contrast))
                {
                    item = SpanArray[i];
                    break;
                }
            }
            return item;
        }

        /// <summary>
        /// 对集合中的每个元素执行指定的操作。
        /// </summary>
        /// <param name="action">要对集合中的每个元素执行的操作。</param>
        public void LoopList(Action<T> action)
        {
            CheckArrayEmpty();

            for (int i = 0; i < Count; i++)
            {
                action(SpanArray[i]);
            }
        }

        /// <summary>
        /// 确定指定的对象是否等于当前对象。
        /// </summary>
        /// <param name="list">要与当前对象进行比较的对象。</param>
        /// <returns>如果指定的对象等于当前对象，则为 true；否则为 false。</returns>
        public bool Equals(ValueList<T> list)
        {
            CheckArrayEmpty();
            return array.Equals(list.array);
        }

        /// <summary>
        /// 将集合的元素复制到新的数组中。
        /// </summary>
        /// <returns>包含集合元素的数组。</returns>
        public T[] ToArray()
        {
            T[] result = new T[Count];
            CopyTo(result, 0);
            return result;
        }

        /// <summary>
        /// 将集合的元素复制到兼容的一维数组中，从指定的数组索引开始。
        /// </summary>
        /// <param name="array">一维 Array，其长度至少与集合中的元素数一样多。</param>
        /// <param name="arrayIndex">array 中从零开始的索引，从该位置开始复制元素。</param>
        public void CopyTo(T[] array, int arrayIndex)
        {
            //无法装下全部数据
            if (array.Length - arrayIndex > Count)
                throw new ArgumentException(nameof(array));

            Array.Copy(array, 0, array, arrayIndex, Count);
        }

        /// <summary>
        /// 返回循环访问集合的枚举器。
        /// </summary>
        /// <returns>可用于循环访问集合的 IEnumerator<T>。</returns>
        public IEnumerator<T> GetEnumerator()
        {
            for (int i = 0; i < Count; i++)
            {
                yield return SpanArray[i];
            }
        }

        /// <summary>
        /// 返回循环访问集合的枚举器。
        /// </summary>
        /// <returns>可用于循环访问集合的 IEnumerator。</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// 搜索与指定谓词所定义的条件相匹配的元素，并返回整个集合中的第一个匹配项。
        /// </summary>
        /// <param name="predicate">定义要搜索的条件的谓词。</param>
        /// <returns>集合中与谓词匹配的第一个元素；如果未找到匹配项，则为类型的默认值。</returns>
        public T? Find(Predicate<T> predicate)
        {
            for (int i = 0; i < Count; i++)
            {
                T temp = SpanArray[i];
                if (predicate(temp))
                {
                    return temp;
                }
            }
            return default(T);
        }

        /// <summary>
        /// 搜索与指定谓词所定义的条件相匹配的元素，并返回整个集合中的第一个匹配项。
        /// </summary>
        /// <param name="predicate">定义要搜索的条件的谓词。</param>
        /// <param name="value">用于与集合元素进行比较的值。</param>
        /// <returns>集合中与谓词匹配的第一个元素；如果未找到匹配项，则为类型的默认值。</returns>
        public T? Find<TValue>(Func<T, TValue, bool> predicate, TValue value)
        {
            for (int i = 0; i < Count; i++)
            {
                T temp = SpanArray[i];
                if (predicate(temp, value))
                {
                    return temp;
                }
            }
            return default(T);
        }

        /// <summary>
        /// 返回集合中与指定谓词所定义的条件相匹配的元素的索引。
        /// </summary>
        /// <param name="predicate">定义要搜索的条件的谓词。</param>
        /// <param name="value">用于与集合元素进行比较的值。</param>
        /// <returns>集合中与谓词匹配的第一个元素的索引；如果未找到匹配项，则为 -1。</returns>
        public int FindIndex<TValue>(Func<T, TValue, bool> predicate, TValue value)
        {
            for (int i = 0; i < Count; i++)
            {
                T temp = SpanArray[i];
                if (predicate(temp, value))
                {
                    return i;
                }
            }
            return -1;
        }

        /// <summary>
        /// 对集合中的每个元素执行指定操作。
        /// </summary>
        /// <param name="action">要对集合中的每个元素执行的操作。</param>
        public void ForEach(Action<T> action)
        {
            for (int i = 0; i < Count; i++)
            {
                action(SpanArray[i]);
            }
        }

        /// <summary>
        /// 获取集合中的最后一个元素。
        /// </summary>
        /// <returns>集合中的最后一个元素。</returns>
        public T GetLast()
        {
            if (Count == 0)
                return default;
            return SpanArray[Count - 1];
        }
    }
}
