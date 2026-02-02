using ExtenderApp.Abstract;
using ExtenderApp.Data;

namespace ExtenderApp.Common.Serializations.Binary.Formatters
{
    /// <summary>
    /// 队列格式化器类，继承自<see cref="CollectionFormatter{T, Queue{T}}" />。
    /// </summary>
    /// <typeparam name="T">队列中元素的类型。</typeparam>
    internal class QueueFormatter<T> : CollectionFormatter<T, Queue<T>>
    {
        public QueueFormatter(IBinaryFormatterResolver resolver) : base(resolver)
        {
        }

        protected override void Add(Queue<T> collection, T value)
        {
            collection.Enqueue(value);
        }

        protected override Queue<T> Create(int count)
        {
            return new Queue<T>(count);
        }
    }
}
