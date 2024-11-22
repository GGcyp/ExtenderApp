using System.Windows.Threading;
using ExtenderApp.Abstract;

namespace ExtenderApp.Views
{
    internal class WPF_Dispatcher : IDispatcher
    {
        private readonly Dispatcher _dispatcher;

        public WPF_Dispatcher(Dispatcher dispatcher)
        {
            _dispatcher = dispatcher;
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
