using ExtenderApp.Abstract;
using ExtenderApp.Contracts;

namespace ExtenderApp.Common.Startups
{
    internal class StartupExecuter : DisposableObject, IStartupExecuter
    {
        private readonly IEnumerable<IStartupExecute> _startupExecutes;

        public StartupExecuter(IEnumerable<IStartupExecute> startupExecutes)
        {
            _startupExecutes = startupExecutes;
        }

        public async ValueTask ExecuteAsync()
        {
            foreach (var startupExecute in _startupExecutes)
            {
                await startupExecute.ExecuteAsync();
            }
        }

        protected override void DisposeManagedResources()
        {
            foreach (var startupExecute in _startupExecutes)
            {
                startupExecute.Dispose();
            }
        }
    }
}