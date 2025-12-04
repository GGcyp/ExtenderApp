namespace ExtenderApp.Data.Data
{
    /// <summary>
    /// 泛型值队列结构
    /// </summary>
    /// <typeparam name="T">队列中元素的类型</typeparam>
    public struct ValueQueue<T> : IEquatable<ValueQueue<T>>
    {
        private const int m_DefaultLength = 4;

        private T[] array;
        public int Capacity => array.Length;
        public bool IsEmpty => array == null;
        public object LockObjcet => array;
        private Span<T> SpanArray => array;

        private int leftIndex;
        private int rightIndex;

        private int count;
        public int Count => count;

        public ValueQueue() : this(m_DefaultLength)
        {
        }

        public ValueQueue(int capacity)
        {
            array = new T[capacity];
            leftIndex = 0;
            rightIndex = 0;
        }

        public ValueQueue(T[] array)
        {
            this.array = array;
            leftIndex = 0;
            rightIndex = this.array.Length - 1;
            count = this.array.Length;
        }

        public ValueQueue(IEnumerable<T> array)
        {
            leftIndex = 0;
            count = 0;
            this.array = new T[count];
            foreach (var item in array)
            {
                Enqueue(item);
            }
        }

        /// <summary>
        /// 检查队列是否为空
        /// </summary>
        /// <exception cref="ArgumentNullException">当 GenericList 为空时引发</exception>
        private void CheckQueueIsEmpty()
        {
            if (IsEmpty)
            {
                throw new ArgumentNullException("the GenericList is null");
            }
        }

        /// <summary>
        /// 获取循环指针
        /// </summary>
        /// <param name="index">当前索引</param>
        /// <returns>下一个索引，如果超出容量则返回0</returns>
        private int LoopPointer(int index) => index + 1 >= Capacity ? 0 : index++;

        /// <summary>
        /// 扩容操作
        /// </summary>
        private void Expansion()
        {
            if (IsEmpty)
            {
                array = new T[m_DefaultLength];
                return;
            }

            if (Count + 1 < array.Length) return;

            int length = array.Length * 2;

            T[] temp = array;
            array = new T[length];
            temp.CopyTo(array, 0);
        }

        /// <summary>
        /// 从队列中出队一个元素
        /// </summary>
        /// <returns>出队的元素，如果队列为空则返回默认值</returns>
        public T Dequeue()
        {
            CheckQueueIsEmpty();

            T reslut = default(T);
            if (Count <= 0 || leftIndex == rightIndex) return reslut;

            reslut = SpanArray[leftIndex];
            leftIndex = LoopPointer(leftIndex);
            count--;
            return reslut;
        }

        /// <summary>
        /// 向队列中入队一个元素
        /// </summary>
        /// <param name="item">要入队的元素</param>
        public void Enqueue(T item)
        {
            Expansion();

            SpanArray[rightIndex] = item;
            rightIndex = LoopPointer(rightIndex);
            count++;
        }

        /// <summary>
        /// 清空队列
        /// </summary>
        public void Clear()
        {
            rightIndex = 0;
            leftIndex = 0;
            count = 0;
        }

        public bool Equals(ValueQueue<T> other)
        {
            return array.Equals(other.array);
        }
    }
}