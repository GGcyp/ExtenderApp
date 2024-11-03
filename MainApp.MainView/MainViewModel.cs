using MainApp.Abstract;
using MainApp.Mods;

namespace MainApp.MainView
{
    public class MainViewModel : IViewModel
    {
        private readonly ModStore _store;

        public MainViewModel(ModStore store)
        {
            _store = store;
        }
    }
}
