using ExtenderApp.Abstract;
using ExtenderApp.Data;

namespace ExtenderApp.Services
{
    /// <summary>
    /// 刷新服务类，实现了IRefreshService接口。
    /// </summary>
    internal class RefreshService : IRefreshService
    {
        /// <summary>
        /// 刷新存储实例。
        /// </summary>
        private readonly RefreshStore _store;

        public RefreshService(RefreshStore store)
        {
            _store = store;
        }

        public void AddAction(Action action, RefreshType type)
        {
            lock (_store)
            {
                switch (type)
                {
                    case RefreshType.Update:
                        _store.AddUpdate(action);
                        break;
                    case RefreshType.FixUpdate:
                        _store.AddFixUpdate(action);
                        break;
                }
            }
        }

        public void RemoveAction(Action action, RefreshType type)
        {
            lock (_store)
            {
                switch (type)
                {
                    case RefreshType.Update:
                        _store.RemoveUpdate(action);
                        break;
                    case RefreshType.FixUpdate:
                        _store.RemoveFixUpdate(action);
                        break;
                }
            }
        }
    }
}
