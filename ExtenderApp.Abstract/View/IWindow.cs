using System.ComponentModel;

namespace ExtenderApp.Abstract
{
    /// <summary>
    /// 定义一个窗口接口。
    /// </summary>
    public interface IWindow : IView
    {
        #region Properties

        /// <summary>
        /// 获取或设置窗口的最小宽度。
        /// </summary>
        double MinWidth { get; set; }

        /// <summary>
        /// 获取或设置窗口的最大宽度。
        /// </summary>
        double MaxWidth { get; set; }

        /// <summary>
        /// 获取或设置窗口的宽度。
        /// </summary>
        double Width { get; set; }

        /// <summary>
        /// 获取或设置窗口的最小高度。
        /// </summary>
        double MinHeight { get; set; }

        /// <summary>
        /// 获取或设置窗口的最大高度。
        /// </summary>
        double MaxHeight { get; set; }

        /// <summary>
        /// 获取或设置窗口的高度。
        /// </summary>
        double Height { get; set; }

        /// <summary>
        /// 获取或设置标题
        /// </summary>
        string Title { get; set; }

        /// <summary>
        /// Manual = 0 <br/> CenterScreen = 1 <br/> CenterOwner = 2 <br/>
        /// 当选择Manual时，窗口的位置不是自动确定的。开发者需要在代码中明确指定窗口的位置，或者如果不指定，则窗口可能会出现在由操作系统决定的默认位置。 <br/>
        /// 当选择CenterScreen时，窗口会在当前鼠标所在的屏幕中央启动。这对于确保窗口在用户的视线范围内很有用，特别是在多显示器设置中。 <br/>
        /// 当选择CenterOwner时，窗口会在其拥有者窗口的中央启动。这对于创建模态对话框或子窗口很有用，这些窗口应该相对于它们的主窗口居中显示。 <br/>
        /// </summary>
        public int WindowStartupLocation { get; set; }

        /// <summary>
        /// 获取或设置所有者窗口。
        /// </summary>
        /// <returns>返回表示所有者窗口的 <see cref="IWindow"/> 对象，如果没有所有者窗口，则为 null。</returns>
        IWindow? Owner { get; set; }

        /// <summary>
        /// 获取当前显示视图的视图模型
        /// </summary>
        IViewModel? CurrentShowViewModel { get; }

        /// <summary>
        /// 获取窗口是否处于活动状态。 如果窗口是前台窗口并且正在接收用户输入，则为 true；否则为 false。
        /// </summary>
        bool IsActive { get; }

        /// <summary>
        /// 获取或设置窗口是否始终位于其他窗口之上。
        /// </summary>
        bool Topmost { get; set; }

        #endregion Properties

        #region Show

        /// <summary>
        /// 显示窗口。
        /// </summary>
        void Show();

        /// <summary>
        /// 以对话框形式显示窗口，并返回用户交互的结果。
        /// </summary>
        /// <returns>如果用户接受对话框中的操作，则返回 true；如果用户取消操作，则返回 false；如果用户关闭对话框，则返回 null。</returns>
        bool? ShowDialog();

        ///// <summary>
        ///// 显示视图。
        ///// </summary>
        ///// <param name="view">要显示的视图。</param>
        //void ShowView(IView? view);

        #endregion Show

        /// <summary>
        /// 关闭视图。
        /// </summary>
        void Close();

        #region FullScreen

        /// <summary>
        /// 指示窗口当前是否处于全屏状态（由实现定义全屏的语义）。
        /// </summary>
        /// <remarks>
        /// 全屏的具体行为由实现决定：可能是仅调整窗口大小并覆盖工作区/屏幕，或同时移除窗口边框/置顶等。 使用 <see cref="EnterFullScreen(bool)"/> /
        /// <see cref="ExitFullScreen"/> / <see cref="ToggleFullScreen(bool)"/> 控制全屏状态。
        /// </remarks>
        bool IsFullScreen { get; }

        /// <summary>
        /// 进入全屏模式。
        /// </summary>
        /// <param name="coverTaskbar">
        /// 如果为 <c>true</c>，实现应尽量将窗口扩展到显示器的像素边界，包含任务栏（强制覆盖任务栏）。 如果为 <c>false</c>，实现通常使用屏幕工作区或最大化（不一定覆盖任务栏）。
        /// </param>
        /// <remarks>
        /// 调用此方法应保存当前窗口的可恢复状态（例如位置、大小、WindowState 等，具体由实现决定）， 以便后续调用 <see cref="ExitFullScreen"/> 能恢复到调用前的状态。
        /// </remarks>
        void EnterFullScreen(bool coverTaskbar = false);

        /// <summary>
        /// 退出全屏模式并恢复到进入全屏前的窗口状态。
        /// </summary>
        /// <remarks>如果实现保存了额外的窗口属性（例如 WindowStyle、ResizeMode、Topmost 等），应在此处一并恢复。</remarks>
        void ExitFullScreen();

        /// <summary>
        /// 切换全屏状态：如果当前不是全屏则进入全屏，否则退出全屏。
        /// </summary>
        /// <param name="coverTaskbar">
        /// 传递给 <see cref="EnterFullScreen(bool)"/> 的 <paramref name="coverTaskbar"/> 参数。
        /// </param>
        void ToggleFullScreen(bool coverTaskbar = false);

        #endregion FullScreen

        #region Events

        /// <summary>
        /// 当窗口被激活时触发的事件。
        /// </summary>
        event EventHandler Activated;

        /// <summary>
        /// 当窗口即将关闭时触发的事件，允许取消关闭操作。
        /// </summary>
        event CancelEventHandler Closing;

        /// <summary>
        /// 当窗口的位置发生变化时触发的事件。
        /// </summary>
        event EventHandler LocationChanged;

        /// <summary>
        /// 当窗口关闭时触发的事件。
        /// </summary>
        event EventHandler Closed;

        /// <summary>
        /// 当窗口的状态发生变化时触发的事件（例如，最大化、最小化等）。
        /// </summary>
        event EventHandler StateChanged;

        #endregion Events
    }
}