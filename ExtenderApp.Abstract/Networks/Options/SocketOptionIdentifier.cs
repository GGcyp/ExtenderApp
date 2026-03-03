using System.Net.Sockets;
using ExtenderApp.Abstract.Options;

namespace ExtenderApp.Abstract.Networks
{
    /// <summary>
    /// 表示一个绑定到套接字级别和名称的选项标识符。 用于将通用的 <see cref="OptionIdentifier{T}"/> 扩展为可直接映射到套接字选项的标识符。
    /// </summary>
    /// <typeparam name="T">选项值的类型。</typeparam>
    public class SocketOptionIdentifier<T> : OptionIdentifier<T>
    {
        /// <summary>
        /// 获取套接字选项的级别（例如 SocketOptionLevel.Socket、SocketOptionLevel.IP 等）。
        /// </summary>
        public SocketOptionLevel SocketLevel { get; }

        /// <summary>
        /// 获取套接字选项的名称（如 ReuseAddress、SendBuffer 等）。
        /// </summary>
        public SocketOptionName SocketName { get; }

        /// <summary>
        /// 使用指定名称、套接字级别和套接字选项名称创建 <see cref="SocketOptionIdentifier{T}"/> 实例。
        /// </summary>
        /// <param name="name">标识符名称（通常与属性名或选项名相同）。</param>
        /// <param name="optionLevel">套接字选项级别。</param>
        /// <param name="optionName">套接字选项名称。</param>
        /// <param name="getVisibility">读取该选项的可见性，默认为 <see cref="OptionVisibility.Public"/>。</param>
        /// <param name="setVisibility">设置该选项的可见性，默认为 <see cref="OptionVisibility.Public"/>。</param>
        public SocketOptionIdentifier(string name, SocketOptionLevel optionLevel, SocketOptionName optionName, OptionVisibility getVisibility = OptionVisibility.Public, OptionVisibility setVisibility = OptionVisibility.Public) : base(name, getVisibility, setVisibility)
        {
            this.SocketLevel = optionLevel;
            this.SocketName = optionName;
        }
    }
}