using ExtenderApp.Abstract;
using ExtenderApp.MainViews.Models;
using ExtenderApp.ViewModels;

namespace ExtenderApp.MainViews.ViewModels
{
    public class MianWindowViewModel : ExtenderAppViewModel<MainViewWindow, MainModel>
    {
        public MianWindowViewModel(MainModel model, IServiceStore serviceStore) : base(model, serviceStore)
        {
        }

        public void ShowView(IMainView mainView)
        {
            Model.CurrentMainView = mainView;
        }
    }
}
