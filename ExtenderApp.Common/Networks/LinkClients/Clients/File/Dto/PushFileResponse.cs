
namespace ExtenderApp.Common.Networks.LinkClients
{
    /// <summary>
    /// 文件传输响应，由接收方回应。
    /// </summary>
    internal readonly struct PushFileResponse
    {
        /// <summary>
        /// 文件的唯一标识符。
        /// </summary>
        public Guid FileId { get; }

        /// <summary>
        /// 接收方是否准备好接收。
        /// </summary>
        public bool IsReady { get; }
    }
}