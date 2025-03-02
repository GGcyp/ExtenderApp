


using ExtenderApp.Data;

namespace ExtenderApp.Abstract
{
    /// <summary>
    /// 定义网络数据解析接口
    /// </summary>
    public interface INetWorkParse
    {
        /// <summary>
        /// 解析代码
        /// </summary>
        int Code { get; }

        /// <summary>
        /// 解析网络数据
        /// </summary>
        /// <param name="packet">待解析的数据包</param>
        void Parse(NetworkPacket packet);
    }

    /// <summary>
    /// 定义一个泛型接口 INetWorkParse，该接口继承自 INetWorkParse，并添加了泛型参数 T。
    /// </summary>
    /// <typeparam name="T">泛型参数，用于指定输入值的类型。</typeparam>
    public interface INetWorkParse<T> : INetWorkParse
    {
        /// <summary>
        /// 将输入值解析为 NetworkPacket 对象。
        /// </summary>
        /// <param name="value">需要解析的输入值。</param>
        /// <returns>解析后的 NetworkPacket 对象。</returns>
        NetworkPacket Parse(T value);
    }
}
