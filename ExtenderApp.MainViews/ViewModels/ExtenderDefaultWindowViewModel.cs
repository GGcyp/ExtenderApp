using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExtenderApp.Abstract;
using ExtenderApp.MainViews.Windows;
using ExtenderApp.ViewModels;

namespace ExtenderApp.MainViews.ViewModels
{
    /// <summary>
    /// 表示默认窗口视图模型的类，继承自 <see cref="ExtenderAppViewModel{T}"/> 并实现了 <see cref="IWindowViewModel"/> 接口。
    /// </summary>
    public class ExtenderDefaultWindowViewModel : ExtenderAppViewModel<ExtenderDefaultWindow>, IWindowViewModel
    {
        /// <summary>
        /// 获取或设置当前视图。
        /// </summary>
        public IView? CurrentView { get; set; }

        /// <summary>
        /// 初始化 <see cref="ExtenderDefaultWindowViewModel"/> 类的新实例。
        /// </summary>
        /// <param name="serviceStore">服务存储。</param>
        public ExtenderDefaultWindowViewModel(IServiceStore serviceStore) : base(serviceStore)
        {
        }

        /// <summary>
        /// 显示指定的视图。
        /// </summary>
        /// <param name="view">要显示的视图。</param>
        public void ShowView(IView view)
        {
            CurrentView = view;
        }
    }
}
