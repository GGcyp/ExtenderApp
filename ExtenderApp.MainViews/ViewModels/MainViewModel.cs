using ExtenderApp.Abstract;
using ExtenderApp.ViewModels;

namespace ExtenderApp.MainViews.ViewModels
{
    public class MainViewModel : ExtenderAppViewModel
    {
        /// <summary>
        /// 当前视图接口
        /// </summary>
        public IViewModel? CurrentViewModel { get; set; }

        public MainViewModel(IServiceStore serviceStore) : base(serviceStore)
        {
            CurrentViewModel = GetViewModel<PluginViewModle>();
        }
    }
}