

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
        /// Manual = 0 <br/> 
        /// CenterScreen = 1 <br/> 
        /// CenterOwner = 2 <br/> 
        /// 当选择Manual时，窗口的位置不是自动确定的。开发者需要在代码中明确指定窗口的位置，或者如果不指定，则窗口可能会出现在由操作系统决定的默认位置。 <br/> 
        /// 当选择CenterScreen时，窗口会在当前鼠标所在的屏幕中央启动。这对于确保窗口在用户的视线范围内很有用，特别是在多显示器设置中。 <br/> 
        /// 当选择CenterOwner时，窗口会在其拥有者窗口的中央启动。这对于创建模态对话框或子窗口很有用，这些窗口应该相对于它们的主窗口居中显示。 <br/> 
        /// </summary>
        public int WindowStartupLocation { get; set; }

        /// <summary>
        /// 获取或设置所有者窗口。
        /// </summary>
        /// <returns>
        /// 返回表示所有者窗口的 <see cref="IWindow"/> 对象，如果没有所有者窗口，则为 null。
        /// </returns>
        IWindow? Owner { get; set; }

        /// <summary>
        /// 获取当前视图接口。
        /// </summary>
        /// <value>返回当前视图接口。</value>
        IView? CurrentView { get; }

        /// <summary>
        /// 获取窗口是否处于活动状态。
        /// 如果窗口是前台窗口并且正在接收用户输入，则为 true；否则为 false。
        /// </summary>
        bool IsActive { get; }

        /// <summary>
        /// 获取或设置窗口是否始终位于其他窗口之上。    
        /// </summary>
        bool Topmost { get; set; }

        #endregion

        /// <summary>
        /// 显示窗口。
        /// </summary>
        void Show();

        /// <summary>
        /// 以对话框形式显示窗口，并返回用户交互的结果。
        /// </summary>
        /// <returns>如果用户接受对话框中的操作，则返回 true；如果用户取消操作，则返回 false；如果用户关闭对话框，则返回 null。</returns>
        bool? ShowDialog();

        /// <summary>
        /// 显示视图。
        /// </summary>
        /// <param name="view">要显示的视图。</param>
        void ShowView(IView view);

        /// <summary>
        /// 关闭视图。
        /// </summary>
        void Close();

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

        #endregion 
    }
}
