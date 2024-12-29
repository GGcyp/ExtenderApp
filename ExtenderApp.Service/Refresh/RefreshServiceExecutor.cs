using AppHost.Extensions.Hosting;

namespace ExtenderApp.Services
{
    /// <summary>
    /// 刷新服务执行器类，继承自BackgroundService类，用于在后台执行刷新任务。
    /// </summary>
    internal class RefreshServiceExecutor : BackgroundService
    {
        /// <summary>
        /// 固定刷新时间间隔，单位为秒。
        /// </summary>
        private const int FIX_REFRESH_TIME = 20;

        /// <summary>
        /// 需要刷新的项列表。
        /// </summary>
        private readonly RefreshStore _store;

        /// <summary>
        /// 用于取消任务的令牌。
        /// </summary>
        private CancellationToken stoppingToken;

        /// <summary>
        /// 刷新服务开始的时间。
        /// </summary>
        private DateTime startTime;

        /// <summary>
        /// 表示当前服务执行器是否处于激活状态。
        /// </summary>
        private bool isActive;

        /// <summary>
        /// 初始化刷新服务执行器。
        /// </summary>
        /// <param name="items">需要刷新的项集合。</param>
        public RefreshServiceExecutor(RefreshStore refreshStore)
        {
            _store = refreshStore;
            _store.ChangeEvent += CheckUpdate;
            startTime = DateTime.Now;
        }

        /// <summary>
        /// 执行刷新任务。
        /// </summary>
        /// <param name="stoppingToken">用于取消任务的令牌。</param>
        /// <returns>异步任务。</returns>
        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            this.stoppingToken = stoppingToken;
            return Task.CompletedTask;
        }

        /// <summary>
        /// 检查并更新数据。
        /// </summary>
        private void CheckUpdate()
        {
            if (_store.UpdateCount == 0 && _store.FixUpdateCount == 0)
            {
                isActive = false;
                return;
            }

            if (isActive) return;

            isActive = true;
            Task.Run(Refresh, stoppingToken);
        }

        /// <summary>
        /// 刷新方法。
        /// </summary>
        private void Refresh()
        {
            while (!stoppingToken.IsCancellationRequested || !isActive)
            {
                for (int i = 0; i < _store.UpdateCount; i++)
                {
                    _store.GetUpdateAction(i).Invoke();
                }

                if (DateTime.Now - startTime > TimeSpan.FromSeconds(FIX_REFRESH_TIME)) continue;
                startTime = DateTime.Now;

                for (int i = 0; i < _store.FixUpdateCount; i++)
                {
                    _store.GetFixUpdateAction(i).Invoke();
                }
            }
        }
    }
}
