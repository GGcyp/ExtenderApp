using ExtenderApp.Abstract;
using ExtenderApp.ViewModels;

namespace ExtenderApp.MainView
{
    public class MainViewModel : BaseViewModel<MainModel>
    {
        public MainViewModel(MainModel model, IDispatcher dispatcher) : base(model, dispatcher)
        {
        }
    }
}
