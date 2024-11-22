using ExtenderApp.Abstract;

namespace ExtenderApp.Views
{
    public static class ViewExpansion
    {
        /// <summary>
        /// 获取视图模型
        /// </summary>
        /// <typeparam name="TViewModel"></typeparam>
        /// <param name="view"></param>
        /// <returns></returns>
        public static TViewModel GetViewMode<TViewModel>(this IView view) where TViewModel : class
        {
            return (TViewModel)view.ViewModel;
        }
    }
}
