using System.Collections;

namespace ExtenderApp.Data
{
    /// <summary>
    /// 表示一个值或者值的列表，可以存储单个值或值的列表。
    /// </summary>
    /// <typeparam name="T">值的类型。</typeparam>
    public class ValueOrList<T> : IList<T>
    {
        /// <summary>
        /// 存储单个值。
        /// </summary>
        private T? _single;

        /// <summary>
        /// 存储值的列表。
        /// </summary>
        private List<T>? _list;

        /// <summary>
        /// 获取集合中的元素数量。
        /// </summary>
        public int Count
        {
            get
            {
                if (_list != null) return _list.Count;
                return _single is not null ? 1 : 0;
            }
        }

        /// <summary>
        /// 获取一个值，该值指示集合是只读的。
        /// </summary>
        public bool IsReadOnly => false;

        /// <summary>
        /// 获取或设置指定索引处的元素。
        /// </summary>
        /// <param name="index">要获取或设置的元素的从零开始的索引。</param>
        /// <returns>指定索引处的元素。</returns>
        public T this[int index]
        {
            get
            {
                if (_list != null) return _list[index];
                if (index == 0 && _single is not null) return _single;
                throw new IndexOutOfRangeException();
            }
            set
            {
                if (_list != null)
                {
                    _list[index] = value;
                }
                else if (index == 0 && _single is not null)
                {
                    _single = value;
                }
                else
                {
                    throw new IndexOutOfRangeException();
                }
            }
        }

        /// <summary>
        /// 将对象添加到集合的末尾。
        /// </summary>
        /// <param name="item">要添加到集合末尾的对象。</param>
        public void Add(T item)
        {
            if (_list != null)
            {
                _list.Add(item);
            }
            else if (_single is null)
            {
                _single = item;
            }
            else
            {
                _list = new List<T>(2) { _single!, item };
                _single = default;
            }
        }

        /// <summary>
        /// 从集合中移除所有元素。
        /// </summary>
        public void Clear()
        {
            _single = default;
            _list = null;
        }

        /// <summary>
        /// 确定集合中是否包含特定元素。
        /// </summary>
        /// <param name="item">要在集合中定位的对象。</param>
        /// <returns>如果集合包含指定的元素，则为 true；否则为 false。</returns>
        public bool Contains(T item)
        {
            if (_list != null) return _list.Contains(item);
            if (_single is not null) return EqualityComparer<T>.Default.Equals(_single, item);
            return false;
        }

        /// <summary>
        /// 将整个集合复制到兼容的一维数组中，从指定索引处开始复制。
        /// </summary>
        /// <param name="array">一维 System.Array，它是目标数组的起始位置。</param>
        /// <param name="arrayIndex">array 中从零开始的索引，从此处开始复制集合中的元素。</param>
        public void CopyTo(T[] array, int arrayIndex)
        {
            if (_list != null)
            {
                _list.CopyTo(array, arrayIndex);
            }
            else if (_single is not null)
            {
                array[arrayIndex] = _single;
            }
        }

        /// <summary>
        /// 返回循环访问集合的枚举器。
        /// </summary>
        /// <returns>一个可用于循环访问集合的枚举器。</returns>
        public IEnumerator<T> GetEnumerator()
        {
            if (_list != null)
            {
                for (int i = 0; i < _list.Count; i++)
                {
                    yield return _list[i];
                }
            }
            else if (_single is not null)
            {
                yield return _single;
            }
        }

        /// <summary>
        /// 返回循环访问 System.Collections 的枚举器。
        /// </summary>
        /// <returns>一个可用于循环访问集合的枚举器。</returns>
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <summary>
        /// 在集合中搜索指定的对象，并返回其从零开始的索引。
        /// </summary>
        /// <param name="item">要在集合中定位的对象。</param>
        /// <returns>在集合中对象第一次出现的从零开始的索引；如果未找到对象，则为 -1。</returns>
        public int IndexOf(T item)
        {
            if (_list != null) return _list.IndexOf(item);
            if (_single is not null && EqualityComparer<T>.Default.Equals(_single, item)) return 0;
            return -1;
        }

        /// <summary>
        /// 在集合的指定索引处插入一个元素。
        /// </summary>
        /// <param name="index">在集合中插入元素的位置的索引。</param>
        /// <param name="item">要插入集合的元素。</param>
        public void Insert(int index, T item)
        {
            if (_list != null)
            {
                _list.Insert(index, item);
            }
            else if (_single is null && index == 0)
            {
                _single = item;
            }
            else if (_single is not null && index == 0)
            {
                _list = new List<T>(2) { item, _single! };
                _single = default;
            }
            else if (_single is not null && index == 1)
            {
                _list = new List<T>(2) { _single!, item };
                _single = default;
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }
        }

        /// <summary>
        /// 从集合中移除特定对象的第一个匹配项。
        /// </summary>
        /// <param name="item">要从集合中移除的对象。</param>
        /// <returns>如果已从集合中成功移除 item，则为 true；否则为 false。如果在集合中未找到 item，该方法也返回 false。</returns>
        public bool Remove(T item)
        {
            if (_list != null)
            {
                return _list.Remove(item);
            }
            else if (_single is not null && EqualityComparer<T>.Default.Equals(_single, item))
            {
                _single = default;
                return true;
            }
            return false;
        }

        /// <summary>
        /// 移除集合中指定索引处的元素。
        /// </summary>
        /// <param name="index">要移除的元素的从零开始的索引。</param>
        public void RemoveAt(int index)
        {
            if (_list != null)
            {
                _list.RemoveAt(index);
                if (_list.Count == 1)
                {
                    _single = _list[0];
                    _list = null;
                }
            }
            else if (_single is not null && index == 0)
            {
                _single = default;
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }
        }

        /// <summary>
        /// 在集合中查找满足指定条件的元素。
        /// </summary>
        /// <param name="predicate">一个用于测试每个元素的条件。</param>
        /// <returns>如果找到满足条件的元素，则返回该元素；否则返回默认值。</returns>
        public T Find(Predicate<T> predicate)
        {
            for (int i = 0; i < Count; i++)
            {
                if (predicate(this[i]))
                {
                    return this[i];
                }
            }
            return default;
        }
    }
}