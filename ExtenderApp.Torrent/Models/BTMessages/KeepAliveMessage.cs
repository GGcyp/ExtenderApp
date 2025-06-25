

using System.Buffers;
using ExtenderApp.Data;

namespace ExtenderApp.Torrent
{
    /// <summary>
    /// 保持活跃消息（空消息）类
    /// </summary>
    public class KeepAliveMessage : BtMessage
    {
        /// <summary>
        /// 初始化 <see cref="KeepAliveMessage"/> 类的新实例
        /// </summary>
        public KeepAliveMessage()
        {
            LengthPrefix = 0;
            MessageId = BTMessageType.KeepAlive;
        }

        public override void Encode(ExtenderBinaryWriter writer)
        {
            writer.Advance(4); // 仅写入长度前缀 0
        }
    }
}
