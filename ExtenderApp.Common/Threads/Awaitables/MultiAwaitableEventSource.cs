using ExtenderApp.Buffer;

namespace ExtenderApp.Common.Threads
{
    /// <summary>
    /// 多消费者/多生产者的可等待事件源。 用于在生产者和消费者之间传递单个值：当生产者调用 <see cref="SetValue"/> 时， 若有等待的消费者则立即完成最早的等待者；若无等待者则将值入队，供后续消费者直接获取。 该类为线程安全实现并复用内部节点以减少分配。
    /// </summary>
    /// <typeparam name="T">传递值的类型。</typeparam>
    public sealed class MultiAwaitableEventSource<T>
    {
        /// <summary>
        /// 内部节点对象池，用于复用队列节点以减少 GC 分配。
        /// </summary>
        private static readonly ObjectPool<AwaitableSourceItem> itemPool = ObjectPool.Create<AwaitableSourceItem>();

        /// <summary>
        /// 同步锁对象，用于保护队列的并发访问。
        /// </summary>
        private readonly object lockObject = new object();

        /// <summary>
        /// 队列头节点。
        /// </summary>
        private AwaitableSourceItem? head;

        /// <summary>
        /// 队列尾节点。
        /// </summary>
        private AwaitableSourceItem? tail;

        /// <summary>
        /// 获取下一个可用值的 ValueTask。若已有生产者提供了值，将立即返回已完成的任务；否则返回一个挂起的 awaitable，等待生产者调用 <see cref="SetValue"/> 进行完成。
        /// </summary>
        public ValueTask<T> GetValueAsync()
        {
            lock (lockObject)
            {
                // 如果队列头存在且是已完成的值节点，直接出队并返回其已完成的 Source
                if (head != null)
                {
                    var node = head;
                    var src = node.Source!;
                    if (node.IsCompleted)
                    {
                        head = node.Next;
                        if (head == null)
                            tail = null;
                        node.Reset();
                        itemPool.Release(node);
                    }
                    return src.GetValueTask();
                }

                // 否则，创建一个等待者节点并入队，等待生产者来完成
                var waitNode = itemPool.Get();
                waitNode.Source = AwaitableEventSource.GetAwaitableEventSource<T>();
                waitNode.Next = null;

                if (tail == null)
                {
                    head = tail = waitNode;
                }
                else
                {
                    tail.Next = waitNode;
                    tail = waitNode;
                }

                return waitNode.Source!;
            }
        }

        /// <summary>
        /// 由生产者设置一个值。若已有等待的消费者，则立即完成最早的等待者；否则将值作为已完成项入队，供后续消费者立即获取。
        /// </summary>
        public void SetValue(T value)
        {
            lock (lockObject)
            {
                // 若有等待的消费者（即头节点存在且未完成），将其出队并完成
                if (head != null)
                {
                    var node = head;
                    head = node.Next;
                    if (head == null)
                        tail = null;

                    var src = node.Source;
                    try
                    {
                        src?.SetResult(value);
                    }
                    finally
                    {
                        node.Reset();
                        itemPool.Release(node);
                    }

                    return;
                }

                // 否则，将值作为已完成项入队，等待消费者来取
                var valueNode = itemPool.Get();
                valueNode.IsCompleted = true;
                valueNode.Source = AwaitableEventSource.FromResult(value);
                valueNode.Next = null;

                if (tail == null)
                {
                    head = tail = valueNode;
                }
                else
                {
                    tail.Next = valueNode;
                    tail = valueNode;
                }
            }
        }

        /// <summary>
        /// 可等待事件源的内部节点类，表示队列中的一个项。每个节点包含一个是否已完成的标志、一个可等待事件源（仅当未完成时使用）以及指向下一个节点的引用。
        /// </summary>
        private class AwaitableSourceItem
        {
            /// <summary> 
            /// 当前节点的状态标志，指示该节点是否已完成（即是否包含一个可直接获取的值）。如果为 true，则 Source 字段无效；如果为 false，则 Source 字段必须是一个有效的 AwaitableEventSource<T> 实例，等待被完成。 
            /// </summary>
            public bool IsCompleted { get; set; }

            /// <summary>
            /// 获取当前节点的可等待事件源，仅当 IsCompleted 为 false 时有效。生产者将通过该事件源完成等待的消费者；消费者将通过该事件源等待生产者的完成。当节点被重置或回收时，该字段将被置 null。
            /// </summary>
            public AwaitableEventSource<T>? Source { get; set; }

            /// <summary>
            /// 获取当前节点的下一个节点引用，形成一个单向链表结构。队列中的每个节点都通过该字段链接到下一个节点，直到链表末尾（即 Next 为 null）。当节点被重置或回收时，该字段将被置 null。
            /// </summary>
            public AwaitableSourceItem? Next { get; set; }

            /// <summary>
            /// 重置当前节点。
            /// </summary>
            public void Reset()
            {
                IsCompleted = false;
                Source = null;
                Next = null;
            }
        }
    }
}