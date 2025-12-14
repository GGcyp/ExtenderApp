using System.ComponentModel;
using System.Runtime.CompilerServices;
using ExtenderApp.Data;

namespace ExtenderApp.Models
{
    /// <summary>
    /// 扩展应用程序数据模型基类，继承自 <see cref="INotifyPropertyChanged"/>
    /// </summary>
    public class DataModel : DisposableObject, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}