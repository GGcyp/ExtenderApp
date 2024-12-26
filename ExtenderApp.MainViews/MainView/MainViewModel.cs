using ExtenderApp.Abstract;
using ExtenderApp.ViewModels;

namespace ExtenderApp.MainViews
{
    public class MainViewModel : ExtenderAppViewModel
    {
        public DisplayDetailsStore DisplayDetailsStore { get; }
        private readonly MainModel _mainModel;

        public MainViewModel(DisplayDetailsStore store, MainModel mainModel, IServiceStore serviceStore) : base(serviceStore)
        {
            DisplayDetailsStore = store;
            _mainModel = mainModel;
            _mainModel.ToRunAction = ToRunView;
            _mainModel.ToHomeAction = ToHomeView;
        }

        public void NavigateTo(DisplayDetails details)
        {
            NavigateTo(details.ViewType);
        }

        public void Start(MainView mainView)
        {
            _mainModel.CurrentMainView = mainView;
            mainView.navigationControl.Content = NavigateTo<ModView>();
        }

        /// <summary>
        /// 返回主页
        /// </summary>
        public void ToHomeView()
        {
            var view = NavigateTo<MainView>();
            view.navigationControl = NavigateTo<ModView>();

            _mainModel.CurrentMainView = view;
            _mainModel.MainWindow.View = view;
        }

        /// <summary>
        /// 开始运行程序或模组，切换到另一个主页页面进行显示
        /// </summary>
        /// <param name="runViewType">要运行的程序或模组的类型</param>

        public void ToRunView(Type runViewType)
        {
            ArgumentNullException.ThrowIfNull(runViewType, nameof(runViewType));

            var view = NavigateTo<MainView_Run>();
            _mainModel.CurrentMainView = view;
            _mainModel.MainWindow.View = view;
            view.viewContenControl.Content = NavigateTo(runViewType);
        }
    }
}
