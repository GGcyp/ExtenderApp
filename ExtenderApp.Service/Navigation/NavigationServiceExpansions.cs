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
    }
}
