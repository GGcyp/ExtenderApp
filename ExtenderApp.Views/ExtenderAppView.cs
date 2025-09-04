using System.Windows.Controls;
using ExtenderApp.Abstract;
using ExtenderApp.Data;

namespace ExtenderApp.Views
{
    /// <summary>
    /// ExtenderAppView 类是一个用户控件类，实现了 IView 接口。
    /// </summary>
    public class ExtenderAppView : UserControl, IView
    {
        /// <summary>
        /// 获取当前视图的视图信息。
        /// </summary>
        /// <returns>返回当前视图的视图信息。</returns>
        public ViewInfo ViewInfo { get; }

        public IWindow? Window { get; private set; }

        public T? ViewModel<T>() where T : class, IViewModel
            => DataContext as T;

        public ExtenderAppView(IViewModel? dataContext = null)
        {
            ViewInfo = new ViewInfo(GetType().Name, dataContext);
            DataContext = dataContext;
        }

        public virtual void InjectWindow(IWindow window)
        {
            Window = window;
            Window.Closed += (s, e) =>
            {
                Exit(new());
            };
        }

        public virtual void Enter(ViewInfo oldViewInfo)
        {
            var viewModel = DataContext as IViewModel;
            viewModel?.InjectView(this);
            viewModel?.Enter(oldViewInfo);
        }

        public virtual void Exit(ViewInfo newViewInfo)
        {
            var viewModel = DataContext as IViewModel;
            viewModel?.Exit(newViewInfo);
        }
    }
}
