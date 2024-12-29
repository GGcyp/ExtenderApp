using System.Windows.Controls;
using ExtenderApp.Abstract;
using ExtenderApp.Data;

namespace ExtenderApp.Views
{
    /// <summary>
    /// ExtenderAppView 类是一个用户控件类，实现了 IView 接口。
    /// </summary>
    public class ExtenderAppView : UserControl, IView
    {
        /// <summary>
        /// 获取当前视图的视图信息。
        /// </summary>
        /// <returns>返回当前视图的视图信息。</returns>
        public ViewInfo ViewInfo { get; }

        public ExtenderAppView()
        {
            ViewInfo = new ViewInfo(GetType().Name);
        }

        public virtual void Enter(ViewInfo oldViewInfo)
        {

        }

        public virtual void Exit(ViewInfo newViewInfo)
        {

        }
    }
}
