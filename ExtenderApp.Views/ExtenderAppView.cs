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

        protected T? ViewModel<T>() where T : class, IViewModel
            => DataContext as T;

        public ExtenderAppView(IViewModel? dataContext = null)
        {
            ViewInfo = new ViewInfo(GetType().Name);
            DataContext = dataContext;
        }

        public virtual void Enter(ViewInfo oldViewInfo)
        {
            var viewModel = DataContext as IViewModel;
            viewModel?.InjectView(this);
        }

        public virtual void Exit(ViewInfo newViewInfo)
        {
            var viewModel = DataContext as IViewModel;
            viewModel?.Close();
        }
    }
}
