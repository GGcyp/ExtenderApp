using ExtenderApp.Data;

namespace ExtenderApp.Abstract
{
    /// <summary>
    /// 定义一个用于全局键盘事件捕获的服务接口。 该接口允许注册和注销对特定按键（可带修饰键）的按下和抬起事件的回调。 实现此接口的类必须管理底层钩子或监听器的生命周期，并通过
    /// IDisposable 模式提供资源释放机制。
    /// </summary>
    /// <remarks>注意：回调的执行不保证在UI线程上。如果回调需要与UI元素交互，调用者有责任将操作封送到正确的UI线程。</remarks>
    public interface IKeyCapture
    {
        /// <summary>
        /// 为指定的“主键”与“修饰键”组合注册事件处理器。
        /// </summary>
        /// <param name="key">要监听的目标主键。</param>
        /// <param name="obj">订阅者的唯一标识对象，用于后续的精确注销。</param>
        /// <param name="keyDownHandler">按键按下时触发的回调委托。如果为 null，则不订阅按下事件。</param>
        /// <param name="keyupHandler">按键抬起时触发的回调委托。如果为 null，则不订阅抬起事件。</param>
        /// <param name="modifierKeys">要求匹配的修饰键组合。默认为 <see cref="ModifierKeys.None"/>，表示匹配任意修饰键组合。</param>
        /// <remarks>
        /// 如果使用相同的 <paramref name="obj"/> 为同一 <paramref name="key"/> 和 <paramref
        /// name="modifierKeys"/> 组合重复注册，将会抛出异常，以防止意外的重复订阅。
        /// </remarks>
        void RegisterKeyCapture(Key key, object obj, Action<KeyUpEvent>? keyDownHandler, Action<KeyUpEvent>? keyupHandler, ModifierKeys modifierKeys = ModifierKeys.None);

        /// <summary>
        /// 注册一个“任意键”处理器，它会响应所有按键事件，只要当前的修饰键状态与指定条件匹配。
        /// </summary>
        /// <param name="obj">订阅者的唯一标识对象。</param>
        /// <param name="keyDownHandler">按键按下时触发的回调。</param>
        /// <param name="keyupHandler">按键抬起时触发的回调。</param>
        /// <param name="modifierKeys">要求匹配的修饰键组合。</param>
        /// <remarks>
        /// 此重载用于捕获不关心具体主键、只关心修饰键的场景。 接口的实现者可以选择是否支持此功能。如果不支持，应在其文档中明确说明行为（例如，忽略此注册或抛出 <see cref="NotSupportedException"/>）。
        /// </remarks>
        void RegisterKeyCapture(object obj, Action<KeyUpEvent>? keyDownHandler, Action<KeyUpEvent>? keyupHandler, ModifierKeys modifierKeys = ModifierKeys.None);

        /// <summary>
        /// 注销指定订阅者在特定主键上的所有事件处理器（无论修饰键如何）。
        /// </summary>
        /// <param name="key">要取消订阅的目标主键。</param>
        /// <param name="obj">要注销的订阅者标识对象。</param>
        void UnRegisterKeyCapture(Key key, object obj);

        /// <summary>
        /// 注销指定订阅者注册的所有按键事件处理器。
        /// </summary>
        /// <param name="obj">要注销的订阅者标识对象。</param>
        void UnRegisterKeyCapture(object obj);
    }
}