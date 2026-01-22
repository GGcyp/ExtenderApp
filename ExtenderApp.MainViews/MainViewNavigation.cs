using ExtenderApp.Abstract;
using ExtenderApp.MainViews.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace ExtenderApp.MainViews
{
    /// <summary>
    /// 提供主视图内的导航功能：按需通过依赖注入解析并切换目标 <see cref="IViewModel"/> 实例，
    /// 并通过 <see cref="NavigationEvent"/> 通知订阅方（通常是窗口层或容器视图模型）更新显示。
    /// </summary>
    public class MainViewNavigation
    {
        /// <summary>
        /// 服务提供者，用于按需解析视图模型实例。
        /// </summary>
        private readonly IServiceProvider _serviceProvider;

        /// <summary>
        /// 当前激活的视图模型（可为空）。
        /// </summary>
        public IViewModel? CurrentViewModel { get; set; }

        /// <summary>
        /// 导航通知事件：当发生导航（切换 <see cref="CurrentViewModel"/>）时调用，参数为目标 <see cref="IViewModel"/>（可为 null）。
        /// 建议订阅者在 UI 线程更新绑定的显示内容。
        /// </summary>
        public Action<IViewModel?>? NavigationEvent { get; set; }

        /// <summary>
        /// 使用指定的 <see cref="IServiceProvider"/> 初始化 <see cref="MainViewNavigation"/> 的新实例。
        /// </summary>
        /// <param name="serviceProvider">用于解析视图模型的服务提供者，不能为空。</param>
        public MainViewNavigation(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        /// <summary>
        /// 导航到由依赖注入容器解析的指定视图模型类型 <typeparamref name="T"/> 的实例，并触发 <see cref="NavigationEvent"/>。
        /// </summary>
        /// <typeparam name="T">要导航到的视图模型类型，必须实现 <see cref="IViewModel"/>。</typeparam>
        public void NavigateToView<T>() where T : IViewModel
        {
            var viewModel = _serviceProvider.GetRequiredService<T>();

            CurrentViewModel = viewModel;
            NavigationEvent?.Invoke(viewModel);
        }

        public void NavigateToRun()
        {
            NavigateToView<MainViewRunViewModel>();
        }

        public void NavigateToHome()
        {
            NavigateToView<MainViewModel>();
        }
    }
}