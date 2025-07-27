
using System.Collections;
using System.Xml.Linq;

namespace ExtenderApp.Data
{
    /// <summary>
    /// 泛型节点类，其中 T 是继承自 Node<T> 的类型。
    /// </summary>
    /// <typeparam name="T">节点类型，必须是 Node<T> 的子类。</typeparam>
    public class Node<T> : IEnumerable<T> where T : Node<T>, IEnumerable<T>
    {
        /// <summary>
        /// 默认大小常量
        /// </summary>
        private const int c_DefaultSize = 4;

        /// <summary>
        /// 节点数量
        /// </summary>
        private int nodeCount;

        /// <summary>
        /// 获取节点数量
        /// </summary>
        public int Count => nodeCount;

        /// <summary>
        /// 父节点
        /// </summary>
        public T? ParentNode { get; set; }

        /// <summary>
        /// 子节点数组
        /// </summary>
        private T[]? nodes;

        /// <summary>
        /// 通过索引获取或设置子节点
        /// </summary>
        /// <param name="index">索引</param>
        /// <returns>子节点</returns>
        public T this[int index]
        {
            get
            {
                if (!HasChildNodes || CheckIndexOut(index))
                {
                    throw new IndexOutOfRangeException($"传入Index：{index},当前Count：{Count}");
                }
                return nodes![index];
            }
            set
            {
                if (!HasChildNodes || CheckIndexOut(index))
                {
                    throw new IndexOutOfRangeException($"传入Index：{index},当前Count：{Count}");
                }
                nodes![index] = value;
            }
        }

        /// <summary>
        /// 判断是否有子节点
        /// </summary>
        /// <returns>是否有子节点</returns>
        public bool HasChildNodes => nodes != null && Count > 0;

        /// <summary>
        /// 获取当前节点的子节点数组。
        /// </summary>
        /// <returns>包含当前节点的子节点的数组，如果子节点为空则返回null。</returns>
        public Node<T>[]? Children
        {
            get
            {
                if (nodes == null)
                    return null;

                T[]? children = new T[Count];
                Array.Copy(nodes, children, Count);
                return children;
            }
        }

        /// <summary>
        /// 检查索引是否越界
        /// </summary>
        /// <param name="index">索引</param>
        /// <returns>是否越界</returns>
        protected bool CheckIndexOut(int index) => index < 0 || index > nodeCount - 1;

        /// <summary>
        /// 创建或扩展子节点数组
        /// </summary>
        protected void CreateOrExpansionNodes()
        {
            if (!HasChildNodes)
            {
                nodes = new T[c_DefaultSize];
                return;
            }

            if (nodeCount + 1 < nodes!.Length) return;

            var array = nodes;
            nodes = new T[nodes.Length * 2];
            array.CopyTo(nodes, 0);
        }

        /// <summary>
        /// 添加节点
        /// </summary>
        /// <param name="node"></param>
        public virtual void Add(T node)
        {
            if (node == null) throw new ArgumentNullException("Can't be add null node");

            if (node.ParentNode != null) throw new ArgumentNullException("Node have parent node, Can't add to node");

            CreateOrExpansionNodes();

            node.ParentNode = this as T;
            nodes![nodeCount] = node;
            nodeCount++;
        }

        /// <summary>
        /// 如过有自己的父类，就从父类中删除自己
        /// </summary>
        public void RemoveParentNode()
        {
            ParentNode?.Remove(this as T);
        }

        public virtual bool Remove(T node)
        {
            if (node == null) throw new ArgumentNullException("Can't be remove null node");

            if (!HasChildNodes) return false;

            for (int i = 0; i < nodeCount; i++)
            {
                if (nodes?[i] == node)
                {
                    RemoveAt(i);
                    node.RemoveParentNode();
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 根据指定的条件移除元素。
        /// </summary>
        /// <param name="predicate">用于确定是否移除元素的条件。</param>
        /// <returns>如果成功移除元素，则返回 true；否则返回 false。</returns>
        public virtual T? Remove(Predicate<T> predicate)
        {
            if (!TryFind(predicate, out var node)) return node;
            node.RemoveParentNode();
            return node;
        }

        public virtual T RemoveAt(int index)
        {
            T resultNode = null;
            if (CheckIndexOut(index)) return resultNode;
            resultNode = nodes[index];
            for (int i = index; i < nodeCount - 1; i++)
            {
                nodes[i] = nodes[i + 1];
            }
            nodeCount--;
            resultNode.RemoveParentNode();
            return resultNode;
        }

        /// <summary>
        /// 根据给定的条件函数从集合中获取第一个匹配的元素。
        /// </summary>
        /// <param name="predicate">条件函数，用于判断元素是否匹配。</param>
        /// <returns>返回匹配的元素，若未找到则返回默认值。</returns>
        public virtual T? Find(Predicate<T> predicate)
        {
            if (predicate == null) return default;
            T? node = null;
            PrivateTryFind(predicate, out node);
            return node;
        }

        /// <summary>
        /// 根据给定的函数判断条件，从树形结构中查找满足条件的节点。
        /// </summary>
        /// <param name="predicate">用于判断节点是否满足条件的函数，参数为树形结构中的节点类型T，返回值为bool。</param>
        /// <param name="node">如果找到满足条件的节点，则返回该节点；否则返回null。</param>
        /// <returns>如果找到满足条件的节点，则返回true；否则返回false。</returns>
        public virtual bool TryFind(Predicate<T> predicate, out T? node)
        {
            node = null;
            if (predicate == null) return false;

            return PrivateTryFind(predicate, out node);
        }

        private bool PrivateTryFind(Predicate<T> predicate, out T? node)
        {
            node = null;
            for (int i = 0; i < nodeCount; i++)
            {
                //找到了
                var n = this[i];
                if (predicate.Invoke(n))
                {
                    node = n;
                    return true;
                }
            }

            for (int i = 0; i < nodeCount; i++)
            {
                //找到了
                var n = this[i];
                if (n.PrivateTryFind(predicate, out node)) return true;
            }

            return false;
        }

        /// <summary>
        /// 清除所有记录或数据
        /// </summary>
        public void Clear()
        {
            if (nodes == null) return;

            for (int i = 0; i < nodeCount; i++)
            {
                nodes[i] = default;
            }
            nodeCount = 0;
            ParentNode = default;
        }


        #region Loop

        /// <summary>
        /// 遍历这个节点的子项
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <param name="action"></param>
        public void LoopChildNodes(Action<T> action)
        {
            if (!HasChildNodes) return;

            for (int i = 0; i < Count; i++)
            {
                action(this[i]);
            }
        }

        /// <summary>
        /// 遍历这个节点的子节点和之后的所有子项
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="action"></param>
        public void LoopAllChildNodes(Action<T> action)
        {
            if (!HasChildNodes) return;

            for (int i = 0; i < Count; i++)
            {
                action(this[i]);
                this[i].LoopAllChildNodes(action);
            }
        }

        /// <summary>
        /// 遍历这个节点的子项
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="T1"></typeparam>
        /// <param name="action"></param>
        /// <param name="value"></param>
        public void LoopChildNodes<T1>(Action<T, T1> action, T1 value)
        {
            if (!HasChildNodes) return;

            for (int i = 0; i < Count; i++)
            {
                action(this[i], value);
            }
        }

        /// <summary>
        /// 遍历这个节点的子节点和之后的所有子项
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="T1"></typeparam>
        /// <param name="action"></param>
        /// <param name="value"></param>
        public void LoopAllChildNodes<T1>(Action<T, T1> action, T1 value)
        {
            if (!HasChildNodes) return;

            for (int i = 0; i < Count; i++)
            {
                action(this[i], value);
                this[i].LoopAllChildNodes(action, value);
            }
        }

        /// <summary>
        /// 遍历这个节点的子项
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="T1"></typeparam>
        /// <param name="action"></param>
        /// <param name="value"></param>
        public void LoopChildNodes<T1>(Action<T, T1> action, ref T1 value)
        {
            if (!HasChildNodes) return;

            for (int i = 0; i < Count; i++)
            {
                action(this[i], value);
            }
        }

        /// <summary>
        /// 遍历这个节点的子节点和之后的所有子项
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="T1"></typeparam>
        /// <param name="action"></param>
        /// <param name="value"></param>
        public void LoopAllChildNodes<T1>(Action<T, T1> action, ref T1 value)
        {
            if (!HasChildNodes) return;

            for (int i = 0; i < Count; i++)
            {
                action(this[i], value);
                this[i].LoopAllChildNodes(action, ref value);
            }
        }

        /// <summary>
        /// 遍历这个节点的子项
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <param name="action"></param>
        /// <param name="value1"></param>
        /// <param name="value2"></param>
        public void LoopChildNodes<T1, T2>(Action<T, T1, T2> action, T1 value1, T2 value2)
        {
            if (!HasChildNodes) return;

            for (int i = 0; i < Count; i++)
            {
                action(this[i], value1, value2);
            }
        }

        /// <summary>
        /// 遍历这个节点的子节点和之后的所有子项
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <param name="action"></param>
        /// <param name="value1"></param>
        /// <param name="value2"></param>
        public void LoopAllChildNodes<T1, T2>(Action<T, T1, T2> action, T1 value1, T2 value2)
        {
            if (!HasChildNodes) return;

            for (int i = 0; i < Count; i++)
            {
                action(this[i], value1, value2);
                this[i].LoopAllChildNodes(action, value1, value2);
            }
        }

        #endregion

        #region Enumerator

        public IEnumerator<T> GetEnumerator()
        {
            return new NodeEnumerator((T)this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private class NodeEnumerator : IEnumerator<T>
        {
            private readonly Queue<T> _queue;
            private readonly T _items;

            public NodeEnumerator(T items)
            {
                _queue = new();
                this._items = items;
                _queue.Enqueue(items);
            }

            public T Current { get; set; }

            object IEnumerator.Current => Current;

            public bool MoveNext()
            {
                if (_queue.Count == 0)
                    return false;

                Current = _queue.Dequeue();
                if (!Current.HasChildNodes)
                    return true;

                for (int i = 0; i < Current.Count; i++)
                {
                    _queue.Enqueue(Current[i]);
                }

                return true;
            }

            public void Reset()
            {
                _queue.Clear();
                _queue.Enqueue(_items);
                Current = default;
            }

            public void Dispose()
            {
                // 清理资源，如果需要的话
            }
        }

        #endregion
    }
}
