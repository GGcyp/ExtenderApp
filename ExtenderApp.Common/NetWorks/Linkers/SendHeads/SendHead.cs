

namespace ExtenderApp.Common.Networks
{
    /// <summary>
    /// 发送头部信息结构体
    /// </summary>
    internal struct SendHead
    {
        /// <summary>
        /// 是否包含发送头部信息
        /// </summary>
        public bool HasSendHead;

        /// <summary>
        /// 类型代码
        /// </summary>
        public int TypeCode;

        /// <summary>
        /// 数据长度
        /// </summary>
        public int Length;

        public SendHead(bool hasSendHead, int typeCode, int length)
        {
            HasSendHead = hasSendHead;
            TypeCode = typeCode;
            Length = length;
        }
    }
}
