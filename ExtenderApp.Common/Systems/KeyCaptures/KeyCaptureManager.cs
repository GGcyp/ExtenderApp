using ExtenderApp.Common.Caches;
using ExtenderApp.Data;

namespace ExtenderApp.Common.Systems.KeyCaptures
{
    /// <summary>
    /// 管理全局按键捕获和分发。 继承自 EvictionCache，自动清理没有订阅者的按键监听。
    /// </summary>
    public class KeyCaptureManager : EvictionCache<Key, KeyConsume>
    {
        /// <summary>
        /// 按键枚举的所有可能值。
        /// </summary>
        private static readonly Key[] keys = Enum.GetValues<Key>();

        /// <summary>
        /// 初始化 KeyCaptureManager 的新实例。
        /// </summary>
        public KeyCaptureManager() : base()
        {
        }

        /// <summary>
        /// 确定是否应从缓存中逐出某个 KeyConsume 实例。
        /// </summary>
        /// <param name="value">要检查的 KeyConsume 实例。</param>
        /// <param name="now">当前时间。</param>
        /// <returns>如果 KeyConsume 实例不再有任何订阅者，则返回 true；否则返回 false。</returns>
        protected override bool ShouldEvict(KeyConsume value, DateTime now)
        {
            // 当一个按键的消费订阅者为0时，将其从缓存中移除。
            return value.Count == 0;
        }

        /// <summary>
        /// 将按键事件（按下或抬起）推送给对应的消费者。
        /// </summary>
        /// <param name="key">触发的按键。</param>
        /// <param name="isKeyDown">如果按键是按下状态，则为 true；如果是抬起状态，则为 false。</param>
        /// <param name="keyEvent">按键事件的详细信息。</param>
        public void PushConsume(Key key, bool isKeyDown, KeyEvent keyEvent)
        {
            if (TryGet(key, out var consume))
            {
                consume.Publish(isKeyDown, keyEvent);
            }
        }

        /// <summary>
        /// 注册一个按键事件的订阅。
        /// </summary>
        /// <param name="key">要订阅的按键。</param>
        /// <param name="consumeDetails">包含订阅者信息和处理器的详情对象。</param>
        public void RegisterConsume(Key key, KeyConsumeDetails consumeDetails)
        {
            if (TryGet(key, out var consume))
            {
                consume.Add(consumeDetails);
            }
            else
            {
                var newConsume = new KeyConsume(key);
                newConsume.Add(consumeDetails);
                AddOrUpdate(key, newConsume);
            }
        }

        /// <summary>
        /// 为多个按键注册同一个订阅。
        /// </summary>
        /// <param name="consumeDetails">案件消费详情</param>
        public void RegisterConsume(KeyConsumeDetails consumeDetails)
        {
            foreach (var key in keys)
            {
                RegisterConsume(key, consumeDetails);
            }
        }

        /// <summary>
        /// 注销指定对象对某个按键的所有订阅。
        /// </summary>
        /// <param name="key">要取消订阅的按键。</param>
        /// <param name="obj">要取消订阅的目标对象。</param>
        public void UnregisterConsume(Key key, object obj)
        {
            if (TryGet(key, out var consume))
            {
                consume.Remove(obj);
            }
        }

        /// <summary>
        /// 取消指定对象对所有按键的订阅。
        /// </summary>
        /// <param name="obj">指定对象</param>
        public void UnregisterConsume(object obj)
        {
            foreach (var item in Values)
            {
                item.Remove(obj);
            }
        }
    }
}