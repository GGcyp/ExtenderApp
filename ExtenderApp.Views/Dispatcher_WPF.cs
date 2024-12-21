using System.Windows.Threading;
using ExtenderApp.Abstract;

namespace ExtenderApp.Views
{
    internal class Dispatcher_WPF : IDispatcherService
    {
        private readonly Dispatcher _dispatcher;

        public Dispatcher_WPF()
        {
            _dispatcher = Dispatcher.CurrentDispatcher;
        }

        public void Invoke(Action action)
        {
            _dispatcher.Invoke(action);
        }

        public void BeginInvoke(Action action)
        {
            _dispatcher.BeginInvoke(action);
        }
    }
}
