using ExtenderApp.Abstract;
using ExtenderApp.Data;

namespace ExtenderApp.Common.Networks.SNMP
{
    internal class SimpleSnmpClient : LinkClient<IUdpLinker>
    {
        public SimpleSnmpClient(IUdpLinker linker) : base(linker)
        {

        }
    }
}