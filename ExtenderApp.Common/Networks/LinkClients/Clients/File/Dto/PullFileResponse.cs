using ExtenderApp.Data;

namespace ExtenderApp.Common.Networks.LinkClients
{
    internal readonly struct PullFileResponse
    {
        /// <summary>
        /// 文件的唯一标识符。
        /// </summary>
        public Guid FileId { get; }

        public BitFieldData FileBitField { get; }

        /// <summary>
        /// 接收方是否准备好接收。
        /// </summary>
        public bool IsReady { get; }
    }
}