using System.Collections.ObjectModel;
using ExtenderApp.Abstract;
using ExtenderApp.Common;
using ExtenderApp.ViewModels;
using PacketDotNet;
using SharpPcap;

namespace ExtenderApp.LAN
{
    public class LANMainViewModel : ExtenderAppViewModel<LANMainView, LANModel>
    {
        public LANMainViewModel(IServiceStore serviceStore) : base(serviceStore)
        {
           //ArpPacket 
        }
    }
}
