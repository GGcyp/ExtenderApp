using ExtenderApp.Abstract;

namespace ExtenderApp.ViewModels
{
    public abstract class ExtenderAppViewModel : IViewModel
    {
        /// <summary>
        /// 服务存储接口实例
        /// </summary>
        protected readonly IServiceStore _serviceStore;

        private readonly string _viewModelName;

        /// <summary>
        /// 视图接口实例
        /// </summary>
        protected readonly IView _view;

        /// <summary>
        /// 获取指定类型的视图实例
        /// </summary>
        /// <typeparam name="TView">视图类型，需要继承自IView接口</typeparam>
        /// <returns>返回指定类型的视图实例</returns>
        /// <exception cref="ArgumentNullException">如果_view为null，则抛出此异常</exception>
        /// <exception cref="InvalidCastException">如果_view无法转换为指定类型，则抛出此异常</exception>
        protected TView GetView<TView>() where TView : class, IView
        {
            ArgumentNullException.ThrowIfNull(_view, string.Format("视图为空 : {0}", GetType().Name));

            var view = _view as TView;
            if (view is null)
                throw new InvalidCastException(string.Format("类型无效，{0}无法转换为{1}", _view.GetType().Name, typeof(TView)));

            return view;
        }

        protected ExtenderAppViewModel(IServiceStore serviceStore)
        {
            _serviceStore = serviceStore;
            _viewModelName = GetType().Name;
        }

        /// <summary>
        /// 导航到指定的视图。
        /// </summary>
        /// <typeparam name="TView">目标视图的类型，必须实现 IView 接口。</typeparam>
        public TView NavigateTo<TView>() where TView : class, IView
        {
            return NavigateTo(typeof(TView)) as TView;
        }

        /// <summary>
        /// 导航到指定的视图类型。
        /// </summary>
        /// <param name="targetView">目标视图的类型。</param>
        public IView NavigateTo(Type targetView)
        {
            return _serviceStore.NavigationService.NavigateTo(targetView, _view);
        }
    }
}
