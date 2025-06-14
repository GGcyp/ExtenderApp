using System.Net.Sockets;
using ExtenderApp.Common.ConcurrentOperates;
using ExtenderApp.Common.ObjectPools;

namespace ExtenderApp.Common.Networks
{
    /// <summary>
    /// 链接操作数据类，继承自ConcurrentOperateData类
    /// </summary>
    public class LinkOperateData : ConcurrentOperateData
    {
        /// <summary>
        /// 地址族
        /// </summary>
        public AddressFamily AddressFamily { get; set; }

        /// <summary>
        /// 套接字类型
        /// </summary>
        public SocketType SocketType { get; set; }

        /// <summary>
        /// 协议类型
        /// </summary>
        public ProtocolType ProtocolType { get; set; }

        /// <summary>
        /// 是否关闭
        /// </summary>
        public bool IsClose { get; set; }

        /// <summary>
        /// 关闭回调
        /// </summary>
        public Action? CloseCallback { get; set; }

        public Socket Socket { get; set; }

        public LinkOperateData(AddressFamily addressFamily, SocketType socketType, ProtocolType protocolType)
        {
            AddressFamily = addressFamily;
            SocketType = socketType;
            ProtocolType = protocolType;
            IsClose = false;
            CloseCallback = null;
            Socket = new Socket(AddressFamily, SocketType, ProtocolType);
        }

        public override bool TryReset()
        {
            IsClose = false;
            CloseCallback = null;
            return base.TryReset();
        }
    }
}
