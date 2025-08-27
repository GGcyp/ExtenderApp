using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ExtenderApp.Models
{
    /// <summary>
    /// Model层，Model基类
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ExtenderAppModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
