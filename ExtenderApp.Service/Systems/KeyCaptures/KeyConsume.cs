using ExtenderApp.Data;

namespace ExtenderApp.Services.Systems.KeyCaptures
{
    /// <summary>
    /// 表示对某个按键组合（主键 + 修饰键）的消费配置与状态。
    /// 提供重复按键判定及生成 <see cref="KeyEvent"/> 的能力，并维护订阅此按键组合的处理器集合。
    /// </summary>
    internal class KeyConsume
    {
        /// <summary>
        /// 重复按键的时间阈值（毫秒）。
        /// 在该时间窗口内再次调用通知（按下）将被视为重复（<see cref="KeyEvent.IsRepeat"/> = true）。
        /// </summary>
        private const int RepeatThresholdMs = 50; // 重复按键的时间阈值，单位为毫秒

        /// <summary>
        /// 目标按键（主键）。
        /// </summary>
        public Key Key { get; }

        /// <summary>
        /// 上一次“按下”时间戳（仅在按下时更新），用于重复判定。
        /// </summary>
        DateTime lastDownTime;

        /// <summary>
        /// 按键消费详情列表。
        /// 每个元素保存一个目标对象的弱引用及其对应的按下/抬起处理器。
        /// </summary>
        List<KeyConsumeDetails> _keyConsumeDetails;

        /// <summary>
        /// 构造 <see cref="KeyConsume"/>。
        /// </summary>
        /// <param name="key">要捕获/消费的主键。</param>
        /// <param name="modifierKeys">组合修饰键（默认无）。</param>
        public KeyConsume(Key key)
        {
            Key = key;
            lastDownTime = DateTime.Now;
            _keyConsumeDetails = new();
        }

        /// <summary>
        /// 注册（或合并）订阅者的按键事件处理器。
        /// - 如果已存在同一 <paramref name="targetObj"/> 与 <paramref name="modifierKeys"/> 的订阅项，则将新的处理器合并到现有委托上（+=）。
        /// - 如果不存在，则新增一条订阅记录。
        /// 线程安全：通过对内部列表加锁避免并发修改。
        /// </summary>
        /// <param name="targetObj">订阅者目标对象（以弱引用形式存储，不会阻止其被回收）。</param>
        /// <param name="modifierKeys">该订阅项希望匹配的修饰键条件。</param>
        /// <param name="keyDownHandler">按下事件处理器；传入 null 表示不订阅按下事件。</param>
        /// <param name="keyupHandler">抬起事件处理器；传入 null 表示不订阅抬起事件。</param>
        public void Add(object targetObj,
            ModifierKeys modifierKeys,
            EventHandler<KeyEvent>? keyDownHandler,
            EventHandler<KeyEvent>? keyupHandler)
        {
            lock (_keyConsumeDetails)
            {
                for (int i = 0; i < _keyConsumeDetails.Count; i++)
                {
                    var d = _keyConsumeDetails[i];
                    if (d.Target == targetObj && d.ModifierKeys == modifierKeys)
                    {
                        if (d.KeyDownHandler is null && keyDownHandler is not null)
                            d.KeyDownHandler = keyDownHandler;
                        else if (d.KeyDownHandler is not null && keyDownHandler is not null)
                            d.KeyDownHandler += keyDownHandler;

                        if (d.KeyUpHandler is null && keyupHandler is not null)
                            d.KeyUpHandler = keyupHandler;
                        else if (d.KeyUpHandler is not null && keyupHandler is not null)
                            d.KeyUpHandler += keyupHandler;

                        return;
                    }
                }
                _keyConsumeDetails.Add(new KeyConsumeDetails(targetObj, modifierKeys, keyDownHandler, keyupHandler));
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
        /// - 对抬起事件：计算重复判定但不更新按下时间戳。
        /// 会自动清理已失效的订阅者。
        /// </summary>
        /// <param name="isKeyDown">true 表示按下（KeyDown）；false 表示抬起（KeyUp）。</param>
        /// <param name="modifierKeys">当前修饰键状态。</param>
        public void Notify(bool isKeyDown, ModifierKeys modifierKeys)
        {
            // 先清理失效订阅者
            PruneDeadSubscribers();

            // 创建事件数据（按需更新 lastDownTime）
            var evt = CreateKeyEvent(isKeyDown, modifierKeys);

            lock (_keyConsumeDetails)
            {
                for (int i = 0; i < _keyConsumeDetails.Count; i++)
                {
                    var d = _keyConsumeDetails[i];
                    if (!d.IsAlive)
                        continue;
                    if (d.ModifierKeys == ModifierKeys.None || d.ModifierKeys == modifierKeys)
                    {
                        EventHandler<KeyEvent>? eventHandler = isKeyDown ? d.KeyDownHandler : d.KeyUpHandler;
                        eventHandler?.Invoke(d.Target, evt);
                    }
                }
            }

        }

        /// <summary>
        /// 基于当前配置生成一次键盘事件数据。
        /// IsRepeat 判定依据为“当前时间 - lastDownTime &lt; RepeatThresholdMs”。
        /// 仅在按下时更新 lastDownTime；抬起不更新。
        /// </summary>
        /// <param name="isKeyDown">按键状态是否为按下</param>
        /// param name="ModifierKeys">当前修饰键状态</param>
        private KeyEvent CreateKeyEvent(bool isKeyDown, ModifierKeys modifierKeys)
        {
            var now = DateTime.Now;
            bool isRepeat = (now - lastDownTime).TotalMilliseconds < RepeatThresholdMs;
            if (isKeyDown)
                lastDownTime = now;
            return new(Key, modifierKeys, isRepeat);
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
