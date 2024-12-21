using System.Windows.Controls;
using ExtenderApp.Abstract;
using ExtenderApp.Data;

namespace ExtenderApp.Views
{
    /// <summary>
    /// ExtenderAppView 类是一个抽象的用户控件类，实现了 IView 接口。
    /// </summary>
    public abstract class ExtenderAppView : UserControl, IView
    {
        /// <summary>
        /// 获取当前视图的视图信息。
        /// </summary>
        /// <returns>返回当前视图的视图信息。</returns>
        public ViewInfo ViewInfo => new ViewInfo(GetType().Name);

        /// <summary>
        /// 进入当前视图。
        /// </summary>
        /// <param name="oldViewInfo">上一个视图的视图信息。</param>
        public virtual void Enter(ViewInfo oldViewInfo)
        {

        }

        /// <summary>
        /// 退出当前视图。
        /// </summary>
        /// <param name="newViewInfo">下一个视图的视图信息。</param>
        public virtual void Exit(ViewInfo newViewInfo)
        {

        }
    }
}
