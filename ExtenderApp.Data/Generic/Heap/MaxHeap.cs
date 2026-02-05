namespace ExtenderApp.Data
{
    /// <summary>
    /// 最大堆类，泛型类型 T 必须实现 IComparable<T> 接口。
    /// </summary>
    /// <typeparam name="T">堆中元素的类型，必须实现 IComparable<T> 接口。</typeparam>
    public class MaxHeap<T> where T : IComparable<T>
    {
        /// <summary>
        /// 最大堆的私有成员变量，用于存储堆中的数据。
        /// </summary>
        private ValueList<T> heap;

        /// <summary>
        /// 获取堆中的元素数量。
        /// </summary>
        public int Count => heap.Count;

        /// <summary>
        /// 堆的容量。
        /// </summary>
        private int capacity;

        /// <summary>
        /// 获取堆的容量。如果堆是可扩展的，则返回堆中元素的数量；否则返回固定容量。
        /// </summary>
        public int Capacity => Scalable ? heap.Count : capacity;

        /// <summary>
        /// 获取或设置堆是否可扩展。
        /// </summary>
        public bool Scalable { get; private set; }

        /// <summary>
        /// 使用指定的容量和可扩展性初始化最大堆。
        /// </summary>
        /// <param name="capacity">堆的初始容量。</param>
        /// <param name="scalable">堆是否可扩展。</param>
        public MaxHeap(int capacity, bool scalable = true)
        {
            heap = new(capacity + 1);
            this.capacity = capacity;
            this.Scalable = scalable;
        }

        /// <summary>
        /// 使用指定的元素数组和可扩展性初始化最大堆。
        /// </summary>
        /// <param name="values">用于初始化堆的元素数组。</param>
        /// <param name="scalable">堆是否可扩展。</param>
        public MaxHeap(T[] values, bool scalable = true)
        {
            //heap = new(values);
            //this.capacity = values.WrittenCount;
            //this.Scalable = scalable;
            //heap.Add(default(T));
            BuildHeap(values, scalable);
        }

        /// <summary>
        /// 获取指定索引的父节点索引
        /// </summary>
        /// <param name="index">当前节点的索引</param>
        /// <returns>父节点的索引</returns>
        private int Parent(int index)
        {
            return (index - 1) / 2;
        }

        /// <summary>
        /// 获取指定索引的左子节点索引
        /// </summary>
        /// <param name="index">当前节点的索引</param>
        /// <returns>左子节点的索引</returns>
        private int LeftChild(int index)
        {
            return 2 * index + 1;
        }

        /// <summary>
        /// 获取指定索引的右子节点索引
        /// </summary>
        /// <param name="index">当前节点的索引</param>
        /// <returns>右子节点的索引</returns>
        private int RightChild(int index)
        {
            return 2 * index + 2;
        }

        /// <summary>
        /// 交换数组中两个元素的位置
        /// </summary>
        /// <param name="i">第一个元素的索引</param>
        /// <param name="j">第二个元素的索引</param>
        private void Swap(int i, int j)
        {
            T temp = heap[i];
            heap[i] = heap[j];
            heap[j] = temp;
        }

        /// <summary>
        /// 将指定索引位置的元素向上堆化，确保堆的性质。
        /// </summary>
        /// <param name="index">需要堆化的元素的索引。</param>
        private void HeapifyUp(int index)
        {
            int parentIndex = Parent(index);
            while (index > 0 && heap[parentIndex].CompareTo(heap[index]) < 0)
            {
                Swap(index, parentIndex);
                index = parentIndex;
                parentIndex = Parent(index);
            }

            //如果堆不可扩容，数组最后一位在调整后删除
            if (!Scalable)
            {
                heap.RemoveAt(heap.Count - 1);
            }
        }

        /// <summary>
        /// 向堆中插入一个新元素。
        /// </summary>
        /// <param name="value">要插入的值。</param>
        /// <remarks>
        /// 如果新加进来时，堆是不可扩张并且达到容量上线，
        /// 且堆内最小值大于新加的数时返回。
        /// 如果堆不可扩容，数组最后添加一位进行调整。
        /// </remarks>
        public void Insert(T value)
        {
            //如果新加进来时，堆是不可扩张并且达到容量上线
            if (!Scalable && Count >= capacity)
            {
                //而且堆内最小值大于新加的数时返回
                if (heap[heap.Count - 1].CompareTo(value) < 0) return;
            }
            //如果不可扩容，数组最后添加一位进行调整

            heap.Add(value);
            HeapifyUp(heap.Count - 1);
        }

        /// <summary>
        /// 从下往上调整堆结构，确保堆的性质。
        /// </summary>
        /// <param name="index">从哪个索引开始调整堆。</param>
        private void HeapifyDown(int index)
        {
            int left;
            int right;
            int largest;
            while (true)
            {
                largest = index;
                left = LeftChild(index);
                right = RightChild(index);

                if (left < heap.Count && heap[left].CompareTo(heap[largest]) > 0)
                {
                    largest = left;
                }

                if (right < heap.Count && heap[right].CompareTo(heap[largest]) > 0)
                {
                    largest = right;
                }

                if (largest == index)
                {
                    break;
                }

                Swap(index, largest);
                index = largest;
            }
        }

        /// <summary>
        /// 删除并返回堆顶元素（大顶堆中是最大值）
        /// </summary>
        public T ExtractMax()
        {
            if (Count == 0)
            {
                throw new InvalidOperationException("Heap is empty");
            }

            int index = Scalable ? heap.Count - 1 : heap.Count - 2;
            T max = heap[0];
            heap[0] = heap[index];
            heap.RemoveAt(index);
            HeapifyDown(0);
            return max;
        }

        /// <summary>
        /// 构建堆
        /// </summary>
        /// <param name="arr">需要构建成堆的数组</param>
        /// <param name="scalable">是否可扩展堆的容量，默认为true</param>
        /// <remarks>
        /// 使用该方法可以将一个数组构建成堆，以便于进行堆排序等操作。
        /// 参数 scalable 指定堆的容量是否可以动态扩展，默认为 true。
        /// 如果 scalable 为 false，则堆的容量固定为输入数组的长度。
        /// </remarks>
        public void BuildHeap(T[] arr, bool scalable = true)
        {
            heap.Clear();
            for (int i = 0; i < arr.Length; i++)
            {
                heap.Add(arr[i]);
            }
            capacity = arr.Length;
            this.Scalable = scalable;
            //如果不可扩容，最后一位当作临时存储
            if (!scalable)
                heap.Add(default(T));

            for (int i = heap.Count / 2 - 1; i >= 0; i--)
            {
                HeapifyDown(i);
            }
        }
    }
}
