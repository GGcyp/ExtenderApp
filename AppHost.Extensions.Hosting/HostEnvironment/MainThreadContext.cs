

namespace AppHost.Extensions.Hosting
{
    internal class MainThreadContext : IMainThreadContext
    {
        public Thread? MainThread { get; private set; }

        public SynchronizationContext? Context { get; private set; }

        private bool Initialized => MainThread != null && Context != null;

        public void InitMainThreadContext()
        {
            if (Initialized)
            {
                return;
            }
            MainThread = Thread.CurrentThread;
            Context = SynchronizationContext.Current;
        }
    }
}
