using ExtenderApp.Data;

namespace ExtenderApp.Common.Systems.KeyCaptures
{
    /// <summary>
    /// 表示对某个按键组合（主键 + 修饰键）的消费配置与状态。 提供重复按键判定及生成 <see cref="KeyEvent"/> 的能力，并维护订阅此按键组合的处理器集合。
    /// </summary>
    public class KeyConsume
    {
        /// <summary>
        /// 目标按键（主键）。
        /// </summary>
        public Key Key { get; }

        /// <summary>
        /// 按键消费详情列表。 每个元素保存一个目标对象的弱引用及其对应的按下/抬起处理器。
        /// </summary>
        private readonly List<KeyConsumeDetails> _keyConsumeDetails;

        /// <summary>
        /// 获取已经注册按键的订阅者数量。
        /// </summary>
        public int Count => _keyConsumeDetails.Count;

        /// <summary>
        /// 构造 <see cref="KeyConsume"/>。
        /// </summary>
        /// <param name="key">要捕获/消费的主键。</param>
        /// <param name="modifierKeys">组合修饰键（默认无）。</param>
        public KeyConsume(Key key)
        {
            Key = key;
            _keyConsumeDetails = new();
        }

        /// <summary>
        /// 注册（或合并）订阅者的按键事件处理器。
        /// - 如果已存在同一 <paramref name="targetObj"/> 与 <paramref name="modifierKeys"/> 的订阅项，则将新的处理器合并到现有委托上（+=）。
        /// - 如果不存在，则新增一条订阅记录。 线程安全：通过对内部列表加锁避免并发修改。
        /// </summary>
        /// <param name="targetObj">订阅者目标对象（以弱引用形式存储，不会阻止其被回收）。</param>
        /// <param name="modifierKeys">该订阅项希望匹配的修饰键条件。</param>
        /// <param name="keyDownHandler">按下事件处理器；传入 null 表示不订阅按下事件。</param>
        /// <param name="keyUpHandler">抬起事件处理器；传入 null 表示不订阅抬起事件。</param>
        public void Add(KeyConsumeDetails details)
        {
            lock (_keyConsumeDetails)
            {
                for (int i = 0; i < _keyConsumeDetails.Count; i++)
                {
                    var d = _keyConsumeDetails[i];
                    if (d.Target == details.Target && d.ModifierKeys == details.ModifierKeys)
                    {
                        throw new InvalidOperationException("不能重复添加相同目标对象和修饰键的订阅记录。");
                    }
                }
                _keyConsumeDetails.Add(details);
            }
        }

        /// <summary>
        /// 删除指定目标对象的所有订阅记录。
        /// </summary>
        /// <param name="targetObj">需要被删除的对象</param>
        public void Remove(object targetObj)
        {
            lock (_keyConsumeDetails)
            {
                _keyConsumeDetails.RemoveAll(d => d.Target == targetObj);
            }
        }

        /// <summary>
        /// 向订阅者发送通知：按下或抬起。
        /// - 对按下事件：计算重复判定并更新内部按下时间戳。
        /// - 对抬起事件：计算重复判定但不更新按下时间戳。 会自动清理已失效的订阅者。
        /// </summary>
        /// <param name="isKeyDown">true 表示按下（KeyDown）；false 表示抬起（KeyUp）。</param>
        /// <param name="modifierKeys">当前修饰键状态。</param>
        public void Publish(bool isKeyDown, KeyEvent keyEvent)
        {
            // 在通知之前，首先清理那些目标对象已被垃圾回收的无效订阅。
            PruneDeadSubscribers();

            // 锁定订阅列表以确保线程安全，防止在遍历时集合被修改。
            lock (_keyConsumeDetails)
            {
                // 遍历所有订阅项，以通知符合条件的订阅者。
                for (int i = 0; i < _keyConsumeDetails.Count; i++)
                {
                    var d = _keyConsumeDetails[i];
                    // 如果订阅者的目标对象已不存在（弱引用已失效），则跳过。
                    if (!d.IsAlive)
                        continue;
                    // 检查修饰键是否匹配：
                    // 1. 如果订阅时未指定修饰键（ModifierKeys.None），则匹配所有修饰键组合。
                    // 2. 如果订阅时指定了修饰键，则要求当前事件的修饰键状态完全一致。
                    if (d.ModifierKeys == ModifierKeys.None || d.ModifierKeys == keyEvent.ModifierKeys)
                    {
                        // 根据是按键按下还是抬起，选择相应的事件处理器。
                        Action<KeyEvent>? action = isKeyDown ? d.KeyDownHandler : d.KeyUpHandler;
                        // 如果存在对应的处理器，则调用它并传入事件参数。
                        action?.Invoke(keyEvent);
                    }
                }
            }
        }

        /// <summary>
        /// 移除失效订阅者或无处理器的条目。
        /// </summary>
        private void PruneDeadSubscribers()
        {
            _keyConsumeDetails.RemoveAll(d =>
                !d.IsAlive || (d.KeyDownHandler is null && d.KeyUpHandler is null));
        }
    }
}