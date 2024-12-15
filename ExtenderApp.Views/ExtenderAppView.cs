using System.Windows.Controls;
using ExtenderApp.Abstract;
using ExtenderApp.Data;

namespace ExtenderApp.Views
{
    public class ExtenderAppView : UserControl, IView
    {
        public ViewInfo ViewInfo => new ViewInfo(GetType().Name);

        public virtual void Enter(ViewInfo oldViewInfo)
        {
            
        }

        public virtual void Exit(ViewInfo newViewInfo)
        {
            
        }
    }
}
