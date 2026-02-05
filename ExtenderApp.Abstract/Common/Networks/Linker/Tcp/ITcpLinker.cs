namespace ExtenderApp.Abstract
{
    /// <summary>
    /// ITcpLinker 接口继承自 <see cref="ILinker"/> 接口，代表一个 TCP 链接器接口。
    /// </summary>
    public interface ITcpLinker : ILinker, ITcpLink
    {
    }
}