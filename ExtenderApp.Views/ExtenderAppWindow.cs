using ExtenderApp.Abstract;
using System.Windows;

namespace ExtenderApp.Views
{
    public class ExtenderAppWindow : Window, IWindow
    {
        public virtual void ShowView(IView view)
        {
            throw new NotImplementedException();
        }
    }
}
