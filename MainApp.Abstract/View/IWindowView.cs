

namespace MainApp.Abstract
{
    public interface IWindowView : IView
    {
        void Show();
        bool? ShowDialog();
    }
}
