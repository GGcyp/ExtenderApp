using System.Net.Sockets;
using System.Runtime.Versioning;

namespace ExtenderApp.Abstract
{
    /// <summary>
    /// 扩展的链路器接口，提供对底层 socket 的自定义控制与选项设置能力。
    /// </summary>
    public interface ICustomizeLinker : ILinker
    {
        /// <summary>
        /// 对套接字执行原始 IO 控制调用（整数版）。
        /// </summary>
        /// <param name="ioControlCode">IO 控制代码（整数形式）。</param>
        /// <param name="optionInValue">可选的输入缓冲；可为 null。</param>
        /// <param name="optionOutValue">用于接收输出的缓冲；可为 null。</param>
        /// <returns>返回原始 IOControl 的结果代码（通常为 0 表示成功）。</returns>
        int IOControl(int ioControlCode, byte[]? optionInValue, byte[]? optionOutValue);

        /// <summary>
        /// 对套接字执行原始 IO 控制调用（枚举版）。
        /// </summary>
        /// <param name="ioControlCode">IO 控制代码（<see cref="IOControlCode"/> 枚举）。</param>
        /// <param name="optionInValue">可选的输入缓冲；可为 null。</param>
        /// <param name="optionOutValue">用于接收输出的缓冲；可为 null。</param>
        /// <returns>返回原始 IOControl 的结果代码（通常为 0 表示成功）。</returns>
        int IOControl(IOControlCode ioControlCode, byte[]? optionInValue, byte[]? optionOutValue);

        /// <summary>
        /// 设置套接字选项（整型值）。
        /// </summary>
        /// <param name="optionLevel">选项级别（例如 <see cref="SocketOptionLevel.Socket"/>）。</param>
        /// <param name="optionName">要设置的选项名。</param>
        /// <param name="optionValue">整型选项值。</param>
        void SetSocketOption(SocketOptionLevel optionLevel, SocketOptionName optionName, int optionValue);

        /// <summary>
        /// 设置套接字选项（字节数组值）。
        /// </summary>
        /// <param name="optionLevel">选项级别。</param>
        /// <param name="optionName">要设置的选项名。</param>
        /// <param name="optionValue">字节数组形式的选项值。</param>
        void SetSocketOption(SocketOptionLevel optionLevel, SocketOptionName optionName, byte[] optionValue);

        /// <summary>
        /// 设置套接字选项（布尔值）。
        /// </summary>
        /// <param name="optionLevel">选项级别。</param>
        /// <param name="optionName">要设置的选项名。</param>
        /// <param name="optionValue">布尔型选项值。</param>
        void SetSocketOption(SocketOptionLevel optionLevel, SocketOptionName optionName, bool optionValue);

        /// <summary>
        /// 设置套接字选项（任意对象值，需由实现方解释和处理）。
        /// </summary>
        /// <param name="optionLevel">选项级别。</param>
        /// <param name="optionName">要设置的选项名。</param>
        /// <param name="optionValue">任意对象类型的选项值。</param>
        void SetSocketOption(SocketOptionLevel optionLevel, SocketOptionName optionName, object optionValue);

        /// <summary>
        /// 设置原始套接字选项（以原始字节表示）。适用于需要直接传递结构体字节布局的场景。
        /// </summary>
        /// <param name="optionLevel">原始选项级别（平台/协议相关整数值）。</param>
        /// <param name="optionName">原始选项名（平台/协议相关整数值）。</param>
        /// <param name="optionValue">以 <see cref="ReadOnlySpan{byte}"/> 形式传入的选项字节。</param>
        public void SetRawSocketOption(int optionLevel, int optionName, ReadOnlySpan<byte> optionValue);

        /// <summary>
        /// 获取指定套接字选项的对象表示（实现可返回适当类型，如 int 或 byte[]）。
        /// </summary>
        /// <param name="optionLevel">选项级别。</param>
        /// <param name="optionName">要读取的选项名。</param>
        /// <returns>选项值的对象表示；若不可用可返回 null。</returns>
        object? GetSocketOption(SocketOptionLevel optionLevel, SocketOptionName optionName);

        /// <summary>
        /// 将读取到的套接字选项写入调用方提供的字节数组（阻塞直到写满或异常）。
        /// </summary>
        /// <param name="optionLevel">选项级别。</param>
        /// <param name="optionName">要读取的选项名。</param>
        /// <param name="optionValue">用于接收选项字节的目标数组。</param>
        void GetSocketOption(SocketOptionLevel optionLevel, SocketOptionName optionName, byte[] optionValue);

        /// <summary>
        /// 读取指定长度的套接字选项并以字节数组返回。
        /// </summary>
        /// <param name="optionLevel">选项级别。</param>
        /// <param name="optionName">要读取的选项名。</param>
        /// <param name="optionLength">期望读取的字节长度。</param>
        /// <returns>包含读取到的字节数组；若读取失败可抛出异常或返回空数组（由实现决定）。</returns>
        byte[] GetSocketOption(SocketOptionLevel optionLevel, SocketOptionName optionName, int optionLength);

        /// <summary>
        /// 读取原始套接字选项到调用方提供的 <see cref="Span{byte}"/> 中。
        /// </summary>
        /// <param name="optionLevel">原始选项级别（整数）。</param>
        /// <param name="optionName">原始选项名（整数）。</param>
        /// <param name="optionValue">目标缓冲，用于接收返回的字节。</param>
        /// <returns>实际写入到 <paramref name="optionValue"/> 的字节数。</returns>
        int GetRawSocketOption(int optionLevel, int optionName, Span<byte> optionValue);

        /// <summary>
        /// 设置 IP 层的保护级别（用于跨网络/蜂窝策略等场景）。
        /// </summary>
        /// <param name="level">要设置的 <see cref="IPProtectionLevel"/>。</param>
        [SupportedOSPlatform("windows")]
        void SetIPProtectionLevel(IPProtectionLevel level);
    }
}