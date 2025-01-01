using ExtenderApp.Abstract;
using ExtenderApp.Data;
using ExtenderApp.ViewModels;
using ExtenderApp.Common.Error;

namespace ExtenderApp.MainViews
{
    public class MainViewModel : ExtenderAppViewModel<IMainView>
    {
        public DisplayDetailsStore DisplayDetailsStore { get; }
        private readonly MainModel _mainModel;

        public MainViewModel(DisplayDetailsStore store, MainModel mainModel, IServiceStore serviceStore) : base(serviceStore)
        {
            DisplayDetailsStore = store;
            _mainModel = mainModel;
            _mainModel.ToRunAction = ToRunView;
            _mainModel.ToHomeAction = GoHomeView;
        }

        //public void NavigateTo(DisplayDetails details)
        //{
        //    NavigateTo(details.ViewType);
        //}

        public override void InjectView(IMainView view)
        {
            base.InjectView(view);

            _mainModel.CurrentMainView = View;
            if (View.GetType() == typeof(MainView)) 
                View.ShowView(NavigateTo<ModView>());
        }

        /// <summary>
        /// 返回主页
        /// </summary>
        public void GoHomeView()
        {
            var view = NavigateTo<IMainView>();

            _mainModel.CurrentMainView = view;
            _mainModel.MainWindow.ShowView(view);
        }

        /// <summary>
        /// 开始运行程序或模组，切换到另一个主页页面进行显示
        /// </summary>
        /// <param name="runViewType">要运行的程序或模组的类型</param>
        public void ToRunView()
        {
            var modDetails = _mainModel.CurrentModDetails;
            modDetails.ArgumentNull(nameof(modDetails));

            var view = NavigateTo<MainView_Run>();
            _mainModel.CurrentMainView = view;
            _mainModel.MainWindow.ShowView(view);
            view.ShowView(NavigateTo(modDetails));
        }
    }
}
