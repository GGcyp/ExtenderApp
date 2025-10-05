using ExtenderApp.Abstract;
using ExtenderApp.Data;

namespace ExtenderApp.Services.Systems.KeyCaptures
{
    /// <summary>
    /// 为 <see cref="IKeyCapture"/> 提供的扩展方法。
    /// 用于便捷地仅注册“按下”或“抬起”的事件处理器。
    /// </summary>
    public static class KeyCaptureExtensions
    {
        /// <summary>
        /// 仅为指定键注册“按下（KeyDown）”事件处理器。
        /// 内部通过调用 <see cref="IKeyCapture.Register"/>，并将抬起处理器置为 null。
        /// </summary>
        /// <param name="keyCapture">键盘捕获实例。</param>
        /// <param name="key">要监听的主键。</param>
        /// <param name="obj">订阅者目标对象（用于标识与后续注销）。</param>
        /// <param name="eventHandler">按下事件处理器。</param>
        /// <param name="modifierKeys">匹配所需的修饰键组合（默认无）。</param>
        public static void RegisterDown(this IKeyCapture keyCapture, Key key, object obj, EventHandler<KeyEvent> eventHandler, ModifierKeys modifierKeys = ModifierKeys.None)
        {
            keyCapture.Register(key, obj, eventHandler, null, modifierKeys);
        }

        /// <summary>
        /// 仅为指定键注册“抬起（KeyUp）”事件处理器。
        /// 内部通过调用 <see cref="IKeyCapture.Register"/>，并将按下处理器置为 null。
        /// </summary>
        /// <param name="keyCapture">键盘捕获实例。</param>
        /// <param name="key">要监听的主键。</param>
        /// <param name="obj">订阅者目标对象（用于标识与后续注销）。</param>
        /// <param name="eventHandler">抬起事件处理器。</param>
        /// <param name="modifierKeys">匹配所需的修饰键组合（默认无）。</param>
        public static void RegisterUp(this IKeyCapture keyCapture, Key key, object obj, EventHandler<KeyEvent> eventHandler, ModifierKeys modifierKeys = ModifierKeys.None)
        {
            keyCapture.Register(key, obj, null, eventHandler, modifierKeys);
        }

        /// <summary>
        /// 为“任意主键”注册“按下（KeyDown）”事件处理器（按修饰键过滤）。
        /// 当任何按键触发且其修饰键与 <paramref name="modifierKeys"/> 匹配时，调用处理器。
        /// </summary>
        /// <param name="keyCapture">键盘捕获实例。</param>
        /// <param name="obj">订阅者目标对象（用于标识与后续注销）。</param>
        /// <param name="eventHandler">按下事件处理器。</param>
        /// <param name="modifierKeys">匹配所需的修饰键组合（默认无）。</param>
        public static void RegisterDown(this IKeyCapture keyCapture, object obj, EventHandler<KeyEvent> eventHandler, ModifierKeys modifierKeys = ModifierKeys.None)
        {
            keyCapture.Register(obj, eventHandler, null, modifierKeys);
        }

        /// <summary>
        /// 为“任意主键”注册“抬起（KeyUp）”事件处理器（按修饰键过滤）。
        /// 当任何按键触发且其修饰键与 <paramref name="modifierKeys"/> 匹配时，调用处理器。
        /// </summary>
        /// <param name="keyCapture">键盘捕获实例。</param>
        /// <param name="obj">订阅者目标对象（用于标识与后续注销）。</param>
        /// <param name="eventHandler">抬起事件处理器。</param>
        /// <param name="modifierKeys">匹配所需的修饰键组合（默认无）。</param>
        public static void RegisterUp(this IKeyCapture keyCapture, object obj, EventHandler<KeyEvent> eventHandler, ModifierKeys modifierKeys = ModifierKeys.None)
        {
            keyCapture.Register(obj, null, eventHandler, modifierKeys);
        }
    }
}
