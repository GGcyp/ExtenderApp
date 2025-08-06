using ExtenderApp.Abstract;


namespace ExtenderApp.Services
{
    /// <summary>
    /// NavigationServiceExpansions 类提供了一组用于导航服务的扩展方法。
    /// </summary>
    public static class NavigationServiceExpansions
    {
        /// <summary>
        /// 使用默认参数导航到指定视图。
        /// </summary>
        /// <typeparam name="T">要导航到的视图类型。</typeparam>
        /// <param name="service">导航服务实例。</param>
        /// <returns>返回导航到的视图实例。</returns>
        public static T NavigateTo<T>(this INavigationService service) where T : class, IView
        {
            return (T)service.NavigateTo(typeof(T), string.Empty, null);
        }

        /// <summary>
        /// 使用指定作用域和默认参数导航到指定视图。
        /// </summary>
        /// <typeparam name="T">要导航到的视图类型。</typeparam>
        /// <param name="service">导航服务实例。</param>
        /// <param name="scope">作用域。</param>
        /// <returns>返回导航到的视图实例。</returns>
        public static T NavigateTo<T>(this INavigationService service, string scope) where T : class, IView
        {
            return (T)service.NavigateTo(typeof(T), scope, null);
        }

        /// <summary>
        /// 使用指定旧视图和默认参数导航到指定视图。
        /// </summary>
        /// <typeparam name="T">要导航到的视图类型。</typeparam>
        /// <param name="service">导航服务实例。</param>
        /// <param name="oldView">旧视图实例。</param>
        /// <returns>返回导航到的视图实例。</returns>
        public static T NavigateTo<T>(this INavigationService service, IView oldView) where T : class, IView
        {
            return (T)service.NavigateTo(typeof(T), string.Empty, oldView);
        }

        /// <summary>
        /// 导航到指定类型的视图。
        /// </summary>
        /// <param name="service">导航服务接口。</param>
        /// <param name="targetType">目标视图的类型。</param>
        /// <returns>返回导航到的视图接口。</returns>
        public static IView NavigateTo(this INavigationService service, Type targetType)
        {
            return service.NavigateTo(targetType, string.Empty, null);
        }

        /// <summary>
        /// 导航到指定类型的视图，并指定范围。
        /// </summary>
        /// <param name="service">导航服务接口。</param>
        /// <param name="targetType">目标视图的类型。</param>
        /// <param name="scope">视图范围。</param>
        /// <returns>返回导航到的视图接口。</returns>
        public static IView NavigateTo(this INavigationService service, Type targetType, string scope)
        {
            return service.NavigateTo(targetType, scope, null);
        }

        /// <summary>
        /// 导航到指定类型的视图，并指定旧视图。
        /// </summary>
        /// <param name="service">导航服务接口。</param>
        /// <param name="targetType">目标视图的类型。</param>
        /// <param name="oldView">旧视图接口。</param>
        /// <returns>返回导航到的视图接口。</returns>
        public static IView NavigateTo(this INavigationService service, Type targetType, IView oldView)
        {
            return service.NavigateTo(targetType, string.Empty, oldView);
        }

        /// <summary>
        /// 导航到指定视图对应的窗口，不指定旧视图。
        /// </summary>
        /// <typeparam name="T">要导航到的视图类型，必须实现IView接口。</typeparam>
        /// <param name="service">导航服务接口实例。</param>
        /// <returns>返回导航到的窗口。</returns>
        public static IWindow NavigateToWindow<T>(this INavigationService service) where T : class, IView
        {
            return NavigateToWindow<T>(service, string.Empty, null);
        }

        /// <summary>
        /// 导航到一个新的窗口，并在其中显示指定的视图。
        /// </summary>
        /// <typeparam name="T">要显示的视图的类型，必须继承自 IView 接口。</typeparam>
        /// <param name="service">用于导航的服务。</param>
        /// <param name="scope">导航的作用域。</param>
        /// <param name="oldView">旧视图，通常用于传递上下文信息。</param>
        /// <returns>返回包含新视图的窗口。</returns>
        public static IWindow NavigateToWindow<T>(this INavigationService service, string scope, IView oldView)
            where T : class, IView
        {
            IWindow window = service.NavigateTo<IWindow>();
            IView view = (T)service.NavigateTo(typeof(T), scope, oldView);
            window.ShowView(view);
            view.InjectWindow(window);
            return window;
        }
    }
}
