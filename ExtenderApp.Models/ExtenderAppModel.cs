using System.ComponentModel;
using System.Runtime.CompilerServices;
using ExtenderApp.Abstract;
using ExtenderApp.Common;

namespace ExtenderApp.Models
{
    /// <summary>
    /// 扩展应用程序模型基类，继承自<see cref="INotifyPropertyChanged"/>，用于支持属性变更通知和资源释放。
    /// </summary>
    public class ExtenderAppModel : DisposableObject, INotifyPropertyChanged
    {
        /// <summary>
        /// 标记模型是否已初始化。
        /// </summary>
        private bool _isInitialized;

        /// <summary>
        /// 属性变更事件，当属性值发生变化时触发。
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// 触发属性变更通知。
        /// </summary>
        /// <param name="propertyName">属性名称，自动获取调用成员名。</param>
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// 初始化模型，确保只初始化一次。
        /// </summary>
        /// <param name="store">服务仓库，用于依赖注入和服务获取。</param>
        public void Initialize(IPuginServiceStore store)
        {
            if (_isInitialized) return;
            _isInitialized = true;

            if (store == null)
                throw new ArgumentNullException(nameof(store));

            Init(store);
        }

        /// <summary>
        /// 派生类可重写的初始化方法。
        /// </summary>
        /// <param name="store">服务仓库。</param>
        protected virtual void Init(IPuginServiceStore store)
        {
        }
    }
}
