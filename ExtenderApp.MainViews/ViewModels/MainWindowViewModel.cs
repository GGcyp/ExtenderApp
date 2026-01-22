using ExtenderApp.Abstract;
using ExtenderApp.ViewModels;

namespace ExtenderApp.MainViews.ViewModels
{
    public class MainWindowViewModel : ExtenderAppViewModel
    {
        /// <summary>
        /// 当前主视图接口
        /// </summary>
        public IViewModel? CurrentViewModel { get; set; }

        /// <summary>
        /// 当前过场动画视图接口
        /// </summary>
        public IView? CurrentCutsceneView { get; set; }

        public MainWindowViewModel(MainViewNavigation navigation)
        {
            navigation.NavigationEvent += (viewModel) =>
            {
                CurrentViewModel = viewModel;
            };
            navigation.NavigateToHome();
        }
    }
}