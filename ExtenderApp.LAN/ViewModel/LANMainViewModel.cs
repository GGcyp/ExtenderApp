using ExtenderApp.ViewModels;
using ExtenderApp.Abstract;
using System.Collections.ObjectModel;
using ExtenderApp.Common;

namespace ExtenderApp.LAN
{
    public class LANMainViewModel : ExtenderAppViewModel<LANMainView, LANModel>
    {
        public LANInteraceInfo CurrentLANInterace => Model.LANInteraces[Model.LANInteraces.Count - 1];

        public LANMainViewModel(IServiceStore serviceStore) : base(serviceStore)
        {

        }
    }
}
