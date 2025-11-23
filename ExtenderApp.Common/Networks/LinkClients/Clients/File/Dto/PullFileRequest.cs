
namespace ExtenderApp.Common.Networks.LinkClients
{
    /// <summary>
    /// 拉取文件请求，由接收方发起，希望从发送方获取文件。
    /// </summary>
    internal readonly struct PullFileRequest
    {
        /// <summary>
        /// 文件的唯一标识符。
        /// </summary>
        public Guid FileId { get; }

        /// <summary>
        /// 文件名。
        /// </summary>
        public string FileName { get; }
    }
}