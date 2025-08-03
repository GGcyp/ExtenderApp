using ExtenderApp.Abstract;
using ExtenderApp.Data;
using System.Windows;

namespace ExtenderApp.Views
{
    public class ExtenderAppWindow : Window, IWindow
    {
        public ViewInfo ViewInfo => throw new NotImplementedException();

        public void Enter(ViewInfo oldViewInfo)
        {
            throw new NotImplementedException();
        }

        public void Exit(ViewInfo newViewInfo)
        {
            throw new NotImplementedException();
        }

        public void ShowView(IMainView mainView)
        {
            throw new NotImplementedException();
        }
    }
}
