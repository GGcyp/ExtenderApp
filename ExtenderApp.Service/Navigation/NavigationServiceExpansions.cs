using ExtenderApp.Abstract;
namespace ExtenderApp.Services
{
    public static class NavigationServiceExpansions
    {
        public static T NavigateTo<T>(this INavigationService service, IView oldView = null) where T : class, IView
        {
            return service.NavigateTo(typeof(T), oldView) as T;
        }
    }
}
