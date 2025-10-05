using ExtenderApp.Data;

namespace ExtenderApp.Abstract
{
    /// <summary>
    /// 全局/系统键盘捕获接口。
    /// 用于注册与注销指定按键（可带修饰键）的按下/抬起回调，并对外暴露释放资源的能力。
    /// 支持通过 <see cref="Start"/> 与 <see cref="Stop"/> 控制捕获生命周期。
    /// 注意：回调可能在非 UI 线程触发，实现方不保证线程上下文，请在处理器内自行调度到 UI 线程。
    /// </summary>
    public interface IKeyCapture : IDisposable
    {
        /// <summary>
        /// 是否已启动捕获。
        /// </summary>
        bool IsStart { get; }

        /// <summary>
        /// 为指定主键与修饰键组合注册回调。
        /// 同一订阅者对象（<paramref name="obj"/>）在同一键与修饰键组合下多次注册时，处理器将累加到同一委托链。
        /// </summary>
        /// <param name="key">要监听的主键。</param>
        /// <param name="obj">订阅者目标对象，用于标识与后续注销。</param>
        /// <param name="keyDownHandler">按下事件处理器（可为 null 表示不订阅按下）。</param>
        /// <param name="keyupHandler">抬起事件处理器（可为 null 表示不订阅抬起）。</param>
        /// <param name="modifierKeys">匹配所需的修饰键组合（默认无）。</param>
        void Register(Key key, object obj, EventHandler<KeyEvent>? keyDownHandler, EventHandler<KeyEvent>? keyupHandler, ModifierKeys modifierKeys = ModifierKeys.None);

        /// <summary>
        /// 为任意主键注册回调（按修饰键条件过滤）。
        /// 当任何按键触发且其修饰键与 <paramref name="modifierKeys"/> 匹配时，将调用相应处理器。
        /// </summary>
        /// <param name="obj">订阅者目标对象，用于标识与后续注销。</param>
        /// <param name="keyDownHandler">按下事件处理器（可为 null 表示不订阅按下）。</param>
        /// <param name="keyupHandler">抬起事件处理器（可为 null 表示不订阅抬起）。</param>
        /// <param name="modifierKeys">匹配所需的修饰键组合（默认无）。</param>
        /// <remarks>
        /// 实现可选择性支持“任意主键”订阅：即不限定具体 <see cref="Key"/>，仅按修饰键进行匹配。
        /// 如实现未支持，应在文档中说明其行为（例如忽略该重载或抛出不支持异常）。
        /// </remarks>
        void Register(object obj, EventHandler<KeyEvent>? keyDownHandler, EventHandler<KeyEvent>? keyupHandler, ModifierKeys modifierKeys = ModifierKeys.None);

        /// <summary>
        /// 注销指定对象在某主键上的全部回调（无论修饰键）。
        /// </summary>
        /// <param name="key">目标主键。</param>
        /// <param name="obj">订阅者目标对象。</param>
        void UnRegister(Key key, object obj);

        /// <summary>
        /// 注销指定对象在所有主键上的全部回调。
        /// </summary>
        /// <param name="obj">订阅者目标对象。</param>
        void UnRegister(object obj);

        /// <summary>
        /// 启动键盘捕获。
        /// 实现通常应安装底层钩子或开始监听系统键盘事件。
        /// </summary>
        /// <remarks>
        /// 建议实现为可重入/幂等（多次调用不会重复安装或抛出异常）。
        /// 如需自动启动，可在构造函数内调用本方法，但仍应允许显式调用。
        /// </remarks>
        void Start();

        /// <summary>
        /// 停止键盘捕获。
        /// 实现通常应卸载底层钩子或停止监听系统键盘事件，但保留已注册的回调以便后续再次 <see cref="Start"/>。
        /// </summary>
        /// <remarks>
        /// 建议实现为可重入/幂等（在未启动时调用不产生副作用）。
        /// 调用 <see cref="IDisposable.Dispose"/> 时也应确保停止捕获并释放相关资源。
        /// </remarks>
        void Stop();
    }
}
