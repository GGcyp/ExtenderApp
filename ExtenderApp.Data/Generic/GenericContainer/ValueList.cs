using System.Collections;

namespace ExtenderApp.Data
{
    /// <summary>
    /// 类内部使用类，最好不要传递
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public struct ValueList<T> : IList<T>
    {
        private const int m_DefaultLength = 4;

        private T[] array;
        public object LockObject => array;
        public bool IsEmpty => LockObject == null;
        public bool IsReadOnly => false;
        public Span<T> SpanArray => array;

        public int Count { get; set; }
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

        public ValueList()
            : this(m_DefaultLength) { }

        public ValueList(int capacity)
        {
            Count = 0;
            array = new T[capacity];
        }

        public ValueList(T[] array)
        {
            Count = array.Length;
            this.array = array;
        }

        public ValueList(IEnumerable<T> array)
        {
            this.array = new T[m_DefaultLength];
            foreach (T item in array)
            {
                Add(item);
            }
        }

        private void CheckIndex(int index)
        {
            if(index < 0 || index > Count)
            {
                throw new IndexOutOfRangeException("插入数据位置超过数据界限");
            }
        }

        private void CheckArrayEmpty()
        {
            ArgumentNullException.ThrowIfNull(LockObject, nameof(ValueList<T>));
        }

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

        public void Add(T item)
        {
            Expansion();

            SpanArray[Count] = item;
            Count++;
        }

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

        public bool Remove(T item)
        {
            int index = IndexOf(item);
            if (index < 0)
                return false;

            RemoveAt(index);
            return true;
        }

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

        public void Clear()
        {
            for(int i = 0; i < Count; i++)
            {
                SpanArray[i] = default;
            }
            Count = 0;
        }

        public int IndexOf(T item)
        {
            CheckArrayEmpty();

            if((object)item is null)
            {
                for (int i = 0; i < Count; i++)
                {
                    if (item.Equals(SpanArray[i]))
                    {
                        return i;
                    }
                }
            }
            else
            {
                EqualityComparer<T> equalityComparer = EqualityComparer<T>.Default;
                for(int i = 0; i < Count; i++)
                {
                    if (equalityComparer.Equals(SpanArray[i], item)) return i;
                }
            }
            return -1;
        }

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

        public bool Contains(T item)
        {
            return IndexOf(item) >= 0;
        }

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

        public void LoopList(Action<T> action)
        {
            CheckArrayEmpty();

            for (int i = 0; i < Count; i++)
            {
                action(SpanArray[i]);
            }
        }

        public bool Equals(ValueList<T> list)
        {
            CheckArrayEmpty();
            return array.Equals(list.array);
        }

        public T[] ToArray()
        {
            T[] result = new T[Count];
            CopyTo(result, 0);
            return result;
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            //无法装下全部数据
            if (array.Length - arrayIndex > Count)
                throw new ArgumentException(nameof(array));

            Array.Copy(array, 0, array, arrayIndex, Count);
        }

        public IEnumerator<T> GetEnumerator()
        {
            for (int i = 0; i < Count; i++)
            {
                yield return SpanArray[i];
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

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

        public void ForEach(Action<T> action)
        {
            for (int i = 0; i < Count; i++)
            {
                action(SpanArray[i]);
            }
        }

        /// <summary>
        /// 获取最后一个
        /// </summary>
        /// <returns><see cref="T"/></returns>
        public T GetLast()
        {
            if (Count == 0)
                return default;
            return SpanArray[Count - 1];
        }
    }
}
