

using ExtenderApp.Data;

namespace ExtenderApp.Abstract
{
    public interface IClientReceivePlugin : IClientPlugin
    {
        void ReceiveOperateContext(LinkerClientContext context);
    }
}
