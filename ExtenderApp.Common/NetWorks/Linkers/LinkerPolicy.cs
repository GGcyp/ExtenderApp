using System.Net.Sockets;
using ExtenderApp.Common.ConcurrentOperates;
using ExtenderApp.Common.NetWorks.LinkOperates;
using ExtenderApp.Data;

namespace ExtenderApp.Common.NetWorks
{
    /// <summary>
    /// 默认链接策略
    /// </summary>
    public abstract class LinkerPolicy : LinkOperatePolicy<LinkerData>
    {

    }

    /// <summary>
    /// LinkOperatePolicy<TData> 类是一个泛型类，继承自 ConcurrentOperatePolicy<Socket, TData> 类。
    /// 它专门用于处理与 LinkOperateData 相关的操作策略。
    /// </summary>
    /// <typeparam name="TData">泛型参数，必须继承自 LinkOperateData 类并且具有无参构造函数。</typeparam>
    public abstract class LinkOperatePolicy<TData> : ConcurrentOperatePolicy<Socket, TData>
        where TData : LinkerData
    {
        /// <summary>
        /// 根据提供的数据创建一个新的 Socket 对象。
        /// </summary>
        /// <param name="data">包含创建 Socket 所需的数据。</param>
        /// <returns>返回创建的新 Socket 对象。</returns>
        public override Socket Create(TData data)
        {
            var result = new Socket(data.AddressFamily, data.SocketType, data.ProtocolType);
            result.NoDelay = true;
            return result;
            //return new Socket(data.AddressFamily, data.SocketType, data.ProtocolType);
        }

        public override void AfterExecute(Socket operate, TData data)
        {
            if (data.IsClose)
            {
                operate.Close();
                data.CloseCallback?.Invoke();
            }
        }
    }
}
