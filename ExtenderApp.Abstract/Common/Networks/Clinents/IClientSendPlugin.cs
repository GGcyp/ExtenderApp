
using ExtenderApp.Data;

namespace ExtenderApp.Abstract
{
    public interface IClientSendPlugin : IClientPlugin
    {
        void SendOperateContext(LinkerClientContext context);
    }
}
