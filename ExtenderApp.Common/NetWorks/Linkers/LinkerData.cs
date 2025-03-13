using System.Net.Sockets;
using ExtenderApp.Common.ConcurrentOperates;
using ExtenderApp.Common.ObjectPools;

namespace ExtenderApp.Common.Networks
{
    /// <summary>
    /// 链接操作数据类，继承自ConcurrentOperateData类
    /// </summary>
    public class LinkerData : ConcurrentOperateData
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

        /// <summary>
        /// 使用指定的地址族、套接字类型和协议类型初始化LinkOperateData实例
        /// </summary>
        /// <param name="addressFamily">地址族</param>
        /// <param name="socketType">套接字类型</param>
        /// <param name="protocolType">协议类型</param>
        public LinkerData(AddressFamily addressFamily, SocketType socketType, ProtocolType protocolType)
        {
            AddressFamily = addressFamily;
            SocketType = socketType;
            ProtocolType = protocolType;
        }

        /// <summary>
        /// 尝试重置LinkOperateData实例
        /// </summary>
        /// <returns>如果重置成功则返回true，否则返回false</returns>
        public override bool TryReset()
        {
            IsClose = false;
            CloseCallback = null;
            return base.TryReset();
        }
    }
}
