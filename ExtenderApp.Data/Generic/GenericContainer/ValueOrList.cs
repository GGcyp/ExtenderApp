﻿using System.Collections;

namespace ExtenderApp.Data
{
    /// <summary>
    /// 表示一个值或者值的列表，可以存储单个值或值的列表。
    /// </summary>
    /// <typeparam name="T">值的类型。</typeparam>
    public class ValueOrList<T> : IList<T>
    {
        public static ValueOrList<T> Empty { get; } = new ValueOrList<T>(0);

        /// <summary>
        /// 存储单个值。
        /// </summary>
        private T? single;

        /// <summary>
        /// 存储值的列表。
        /// </summary>
        private List<T>? list;

        /// <summary>
        /// 获取集合中的元素数量。
        /// </summary>
        public int Count
        {
            get
            {
                if (list != null) return list.Count;
                return single is not null ? 1 : 0;
            }
        }

        /// <summary>
        /// 获取一个值，该值指示集合是只读的。
        /// </summary>
        public bool IsReadOnly => false;

        public bool IsList => list is not null;

        /// <summary>
        /// 获取或设置指定索引处的元素。
        /// </summary>
        /// <param name="index">要获取或设置的元素的从零开始的索引。</param>
        /// <returns>指定索引处的元素。</returns>
        public T this[int index]
        {
            get
            {
                if (list != null) return list[index];
                if (index == 0 && single is not null) return single;
                throw new IndexOutOfRangeException();
            }
            set
            {
                if (list != null)
                {
                    list[index] = value;
                }
                else if (index == 0 && single is not null)
                {
                    single = value;
                }
                else
                {
                    throw new IndexOutOfRangeException();
                }
            }
        }

        public ValueOrList() : this(0)
        {

        }

        public ValueOrList(int capacity)
        {
            if (capacity < 0)
                throw new ArgumentOutOfRangeException();

            if (capacity > 1)
                list = new List<T>(capacity);
        }

        /// <summary>
        /// 将对象添加到集合的末尾。
        /// </summary>
        /// <param name="item">要添加到集合末尾的对象。</param>
        public void Add(T item)
        {
            if (list != null)
            {
                list.Add(item);
            }
            else if (single is null)
            {
                single = item;
            }
            else
            {
                list = new List<T>(2) { single!, item };
                single = default;
            }
        }

        /// <summary>
        /// 从集合中移除所有元素。
        /// </summary>
        public void Clear()
        {
            single = default;
            list = null;
        }

        /// <summary>
        /// 确定集合中是否包含特定元素。
        /// </summary>
        /// <param name="item">要在集合中定位的对象。</param>
        /// <returns>如果集合包含指定的元素，则为 true；否则为 false。</returns>
        public bool Contains(T item)
        {
            if (list != null) return list.Contains(item);
            if (single is not null) return EqualityComparer<T>.Default.Equals(single, item);
            return false;
        }

        /// <summary>
        /// 将整个集合复制到兼容的一维数组中，从指定索引处开始复制。
        /// </summary>
        /// <param name="array">一维 System.Array，它是目标数组的起始位置。</param>
        /// <param name="arrayIndex">array 中从零开始的索引，从此处开始复制集合中的元素。</param>
        public void CopyTo(T[] array, int arrayIndex)
        {
            if (list != null)
            {
                list.CopyTo(array, arrayIndex);
            }
            else if (single is not null)
            {
                array[arrayIndex] = single;
            }
        }

        /// <summary>
        /// 返回循环访问集合的枚举器。
        /// </summary>
        /// <returns>一个可用于循环访问集合的枚举器。</returns>
        public IEnumerator<T> GetEnumerator()
        {
            if (list != null)
            {
                for (int i = 0; i < list.Count; i++)
                {
                    yield return list[i];
                }
            }
            else if (single is not null)
            {
                yield return single;
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
            if (list != null) return list.IndexOf(item);
            if (single is not null && EqualityComparer<T>.Default.Equals(single, item)) return 0;
            return -1;
        }

        /// <summary>
        /// 在集合的指定索引处插入一个元素。
        /// </summary>
        /// <param name="index">在集合中插入元素的位置的索引。</param>
        /// <param name="item">要插入集合的元素。</param>
        public void Insert(int index, T item)
        {
            if (list != null)
            {
                list.Insert(index, item);
            }
            else if (single is null && index == 0)
            {
                single = item;
            }
            else if (single is not null && index == 0)
            {
                list = new List<T>(2) { item, single! };
                single = default;
            }
            else if (single is not null && index == 1)
            {
                list = new List<T>(2) { single!, item };
                single = default;
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
            if (list != null)
            {
                return list.Remove(item);
            }
            else if (single is not null && EqualityComparer<T>.Default.Equals(single, item))
            {
                single = default;
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
            if (list != null)
            {
                list.RemoveAt(index);
                if (list.Count == 1)
                {
                    single = list[0];
                    list = null;
                }
            }
            else if (single is not null && index == 0)
            {
                single = default;
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