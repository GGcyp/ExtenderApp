using ExtenderApp.ViewModels;
using ExtenderApp.Abstract;
using System.Collections.ObjectModel;
using ExtenderApp.Common;

namespace ExtenderApp.LAN
{
    public class LANMainViewModel : ExtenderAppViewModel<LANMainView, LANModel>
    {
        public LANMainViewModel(IServiceStore serviceStore) : base(serviceStore)
        {

        }
    }
}
