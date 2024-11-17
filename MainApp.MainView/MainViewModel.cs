using MainApp.Abstract;
using MainApp.ViewModels;

namespace MainApp.MainView
{
    public class MainViewModel : BaseViewModel<MainModel>
    {
        public MainViewModel(MainModel model, IDispatcher dispatcher) : base(model, dispatcher)
        {
        }
    }
}
