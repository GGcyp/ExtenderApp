

using System.Text;

namespace ExtenderApp.Abstract
{
    /// <summary>
    /// 抽象的 HTTP 字节流解析器接口。
    /// 实现负责从原始字节流（ReadOnlySpan<byte>）中解析出完整的 <see cref="Data.HttpRequestMessage"/> / <see cref="Data.HttpResponseMessage"/>。
    /// 解析器应支持增量解析（当数据不足时返回 false，不消费字节），并在成功解析时通过 <paramref name="bytesConsumed"/> 返回已消费的字节数。
    /// </summary>
    public interface IHttpParser
    {
        /// <summary>
        /// 尝试从给定的字节缓冲解析出一个完整的 HTTP 请求消息（start-line + headers + 可选 body）。
        /// </summary>
        /// <param name="buffer">要解析的字节数据（只读切片）。解析器不得修改该切片。</param>
        /// <param name="message">
        /// 输出解析得到的 <see cref="Data.HttpRequestMessage"/> 实例。
        /// 当方法返回 false 或解析不完整时该值应为 <c>null</c>。
        /// 注意：返回非 null 的消息通常实现了 <see cref="System.IDisposable"/>，调用方应在适当时机调用 <c>Dispose()</c> 以释放可能的底层缓冲/资源。
        /// </param>
        /// <param name="bytesConsumed">
        /// 输出被解析并可丢弃的字节数（等于请求的总字节长度）。
        /// 当返回 false 时，该值通常为 0（或表示当前已安全消费的长度，取决于实现），调用方可据此管理剩余数据。
        /// </param>
        /// <param name="encoding">
        /// 可选的字符编码，用于将字节解码为字符串（例如请求行、头部或文本主体）。
        /// 若为 <c>null</c>，实现应至少使用 ASCII/ISO-8859-1 解码头部；文本主体的默认编码可由实现约定或通过 Content-Type/charset 头决定。
        /// </param>
        /// <returns>
        /// 如果成功解析出完整请求并填充 <paramref name="message"/>，返回 <c>true</c>；否则返回 <c>false</c>（表示数据不足或不是有效请求）。
        /// </returns>
        bool TryParseRequest(ReadOnlySpan<byte> buffer, out Data.HttpRequestMessage? message, out int bytesConsumed, Encoding? encoding = null);

        /// <summary>
        /// 尝试从给定的字节缓冲解析出一个完整的 HTTP 响应消息（status-line + headers + 可选 body）。
        /// </summary>
        /// <param name="buffer">要解析的字节数据（只读切片）。解析器不得修改该切片。</param>
        /// <param name="requestMessage">
        /// 与该响应关联的请求消息（若有），实现可以使用请求信息（例如 Method）来判断响应主体的存在性或解析策略（如对 HEAD 请求响应通常无主体）。
        /// 可以为 <c>null</c>，但在可用时应提供以便更准确地解析响应语义。
        /// </param>
        /// <param name="message">
        /// 输出解析得到的 <see cref="Data.HttpResponseMessage"/> 实例。
        /// 当方法返回 false 或解析不完整时该值应为 <c>null</c>。
        /// 注意：返回非 null 的消息通常实现了 <see cref="System.IDisposable"/>，调用方应在适当时机调用 <c>Dispose()</c> 以释放可能的底层缓冲/资源。
        /// </param>
        /// <param name="bytesConsumed">
        /// 输出被解析并可丢弃的字节数（等于响应的总字节长度）。
        /// 当返回 false 时，该值通常为 0（或表示当前已安全消费的长度，取决于实现）。
        /// </param>
        /// <param name="encoding">
        /// 可选的字符编码，用于将字节解码为字符串（例如状态行、头部或文本主体）。
        /// 若为 <c>null</c>，实现应至少使用 ASCII/ISO-8859-1 解码头部；文本主体的默认编码可由实现约定或通过 Content-Type/charset 头决定。
        /// </param>
        /// <returns>
        /// 如果成功解析出完整响应并填充 <paramref name="message"/>, 返回 <c>true</c>；否则返回 <c>false</c>。
        /// </returns>
        bool TryParseResponse(ReadOnlySpan<byte> buffer, Data.HttpRequestMessage requestMessage, out Data.HttpResponseMessage? message, out int bytesConsumed, Encoding? encoding = null);
    }
}
