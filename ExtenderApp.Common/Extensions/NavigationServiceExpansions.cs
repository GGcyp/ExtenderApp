using ExtenderApp.Abstract;

namespace ExtenderApp.Common
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
        public static void NavigateTo<T>(this INavigationService service) where T : class, IView
        {
            service.NavigateTo(typeof(T), string.Empty);
        }

        /// <summary>
        /// 使用指定作用域和默认参数导航到指定视图。
        /// </summary>
        /// <typeparam name="T">要导航到的视图类型。</typeparam>
        /// <param name="service">导航服务实例。</param>
        /// <param name="scope">作用域。</param>
        public static void NavigateTo<T>(this INavigationService service, string scope) where T : class, IView
        {
            service.NavigateTo(typeof(T), scope);
        }
    }
}