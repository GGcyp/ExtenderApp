using ExtenderApp.Abstract;
using ExtenderApp.MainViews.Models;
using ExtenderApp.ViewModels;

namespace ExtenderApp.MainViews.ViewModels
{
    public class MainViewModel : ExtenderAppViewModel<MainModel>
    {
        /// <summary>
        /// 当前视图接口
        /// </summary>
        public IViewModel? CurrentViewModel { get; set; }

        public MainViewModel(MainModel model, IServiceStore serviceStore) : base(model, serviceStore)
        {
        }
    }
}