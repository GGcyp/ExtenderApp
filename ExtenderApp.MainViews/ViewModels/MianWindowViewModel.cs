using ExtenderApp.Abstract;
using ExtenderApp.MainViews.Models;
using ExtenderApp.ViewModels;

namespace ExtenderApp.MainViews.ViewModels
{
    public class MianWindowViewModel : ExtenderAppViewModel<MainViewWindow, MainModel>, IWindowViewModel
    {
        public IView? CurrentView => Model.CurrentMainView;

        public MianWindowViewModel(MainModel model, IServiceStore serviceStore) : base(model, serviceStore)
        {
        }

        public void ShowView(IView view)
        {
            Model.CurrentMainView = view;
        }
    }
}
