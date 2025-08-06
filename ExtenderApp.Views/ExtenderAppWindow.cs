using ExtenderApp.Abstract;
using ExtenderApp.Data;
using System.Windows;

namespace ExtenderApp.Views
{
    public class ExtenderAppWindow : Window, IWindow
    {
        public ViewInfo ViewInfo { get; }

        public virtual IView? CurrentView
        {
            get
            {
                if (DataContext is not IWindowViewModel viewModel)
                    return null;
                return viewModel.CurrentView;
            }
        }

        IWindow? IWindow.Owner
        {
            get => Owner as IWindow;
            set => Owner = value as Window;
        }

        public IWindow? Window => throw new NotImplementedException($"无法重复获取Window的Window:{Title}");

        int IWindow.WindowStartupLocation
        {
            get
            {
                return (int)WindowStartupLocation;
            }
            set
            {
                WindowStartupLocation = (WindowStartupLocation)value;
            }
        }

        protected T? ViewModel<T>() where T : class, IViewModel
            => DataContext as T;

        public ExtenderAppWindow(IViewModel? dataContext = null)
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

        public virtual void ShowView(IView view)
        {
            if (DataContext is not IWindowViewModel viewModel)
                return;

            viewModel.ShowView(view);
        }

        public void InjectWindow(IWindow window)
        {
            throw new NotImplementedException();
        }
    }
}
