using System.Drawing;
using MainApp.Abstract;

namespace MainApp.Mods
{
    /// <summary>
    /// 模组详情类
    /// </summary>
    public sealed class ModDetails
    {
        /// <summary>
        /// 模组标题
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// 模组描述
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// 模组图标
        /// </summary>
        public Image Icon { get; set; }

        /// <summary>
        /// 获取模组主窗口
        /// </summary>
        public Func<IView> FactoryMainView {  get; set; }

        public bool IsEmpty
        {
            get
            {
                return string.IsNullOrEmpty(Title);
            }
        }
    }
}
