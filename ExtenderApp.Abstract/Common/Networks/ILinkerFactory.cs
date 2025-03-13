

using System.Net.Sockets;

namespace ExtenderApp.Abstract
{
    /// <summary>
    /// 链接器工厂接口
    /// </summary>
    public interface ILinkerFactory
    {
        /// <summary>
        /// 根据协议类型创建链接器
        /// </summary>
        /// <param name="type">协议类型</param>
        /// <returns>返回创建的链接器</returns>
        ILinker CreateLinker(ProtocolType type);
    }
}
