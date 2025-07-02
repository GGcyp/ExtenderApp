

using System.Net;
using ExtenderApp.Data;

namespace ExtenderApp.Abstract
{
    /// <summary>
    /// 表示一个 UDP 链接器接口，继承自 <see cref="ILinker"/> 接口。
    /// </summary>
    public interface IUdpLinker : ILinker
    {
        /// <summary>
        /// 将数据发送到指定的终结点。
        /// </summary>
        /// <param name="data">要发送的数据字节数组。</param>
        /// <param name="endPoint">目标终结点。</param>
        void SendTo(byte[] data, EndPoint endPoint);

        /// <summary>
        /// 从指定的起始位置开始，将数据发送到指定的终结点。
        /// </summary>
        /// <param name="data">要发送的数据字节数组。</param>
        /// <param name="start">数据的起始位置。</param>
        /// <param name="endPoint">目标终结点。</param>
        void SendTo(byte[] data, int start, EndPoint endPoint);

        /// <summary>
        /// 从指定的起始位置和长度开始，将数据发送到指定的终结点。
        /// </summary>
        /// <param name="data">要发送的数据字节数组。</param>
        /// <param name="start">数据的起始位置。</param>
        /// <param name="length">要发送的数据长度。</param>
        /// <param name="endPoint">目标终结点。</param>
        void SendTo(byte[] data, int start, int length, EndPoint endPoint);

        /// <summary>
        /// 将内存中的数据发送到指定的终结点。
        /// </summary>
        /// <param name="memory">要发送的内存。</param>
        /// <param name="endPoint">目标终结点。</param>
        void SendTo(Memory<byte> memory, EndPoint endPoint);

        /// <summary>
        /// 异步地将数据发送到指定的终结点。
        /// </summary>
        /// <param name="data">要发送的数据字节数组。</param>
        /// <param name="endPoint">目标终结点。</param>
        void SendToAsync(byte[] data, EndPoint endPoint);

        /// <summary>
        /// 异步地从指定的起始位置和长度开始，将数据发送到指定的终结点。
        /// </summary>
        /// <param name="data">要发送的数据字节数组。</param>
        /// <param name="start">数据的起始位置。</param>
        /// <param name="length">要发送的数据长度。</param>
        /// <param name="endPoint">目标终结点。</param>
        void SendToAsync(byte[] data, int start, int length, EndPoint endPoint);

        /// <summary>
        /// 使用<see cref="ExtenderBinaryWriter"/>将数据异步发送到指定的终结点。
        /// </summary>
        /// <param name="writer">包含要发送数据的<see cref="ExtenderBinaryWriter"/>实例。</param>
        /// <param name="endPoint">目标终结点。</param>
        void SendToAsyncWriter(ExtenderBinaryWriter writer, EndPoint endPoint);

        /// <summary>
        /// 使用<see cref="ExtenderBinaryWriter"/>将数据同步发送到指定的终结点。
        /// </summary>
        /// <param name="writer">包含要发送数据的<see cref="ExtenderBinaryWriter"/>实例。</param>
        /// <param name="endPoint">目标终结点。</param>
        void SendToWriter(ExtenderBinaryWriter writer, EndPoint endPoint);
    }
}
