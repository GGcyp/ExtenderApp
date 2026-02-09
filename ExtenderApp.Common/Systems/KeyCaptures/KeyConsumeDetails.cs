using ExtenderApp.Contracts;

namespace ExtenderApp.Common.Systems.KeyCaptures
{
    /// <summary>
    /// 按键消费详情，包含目标对象的弱引用及其对应的按键事件处理器。
    /// </summary>
    public class KeyConsumeDetails
    {
        /// <summary>
        /// 目标对象的弱引用，用于避免强引用造成的内存泄漏。 仅用于标识订阅者存在性，不参与事件负载。
        /// </summary>
        private readonly WeakReference<object> _weakReference;

        /// <summary>
        /// 获取目标对象的强引用（如果仍然存活）。
        /// </summary>
        public object? Target =>
            _weakReference.TryGetTarget(out var target) ? target : null;

        /// <summary>
        /// 按键按下时的事件处理器（可选）。
        /// </summary>
        public Action<KeyUpEvent>? KeyDownHandler { get; set; }

        /// <summary>
        /// 按键抬起时的事件处理器（可选）。
        /// </summary>
        public Action<KeyUpEvent>? KeyUpHandler { get; set; }

        /// <summary>
        /// 订阅者期望匹配的修饰键掩码。 当触发事件的修饰键与该值匹配时，才应调用相应的处理器。
        /// </summary>
        public ModifierKeys ModifierKeys { get; }

        /// <summary>
        /// 指示弱引用目标对象是否仍然存活。 为 false 时表明订阅者已被回收，应当从订阅列表中移除。
        /// </summary>
        public bool IsAlive => Target is not null;

        public KeyConsumeDetails(object targetObj, ModifierKeys modifier,
            Action<KeyUpEvent>? keyDownHandler = null,
            Action<KeyUpEvent>? keyUpHandler = null)
        {
            _weakReference = new(targetObj);
            KeyDownHandler = keyDownHandler;
            KeyUpHandler = keyUpHandler;
            ModifierKeys = modifier;
        }
    }
}