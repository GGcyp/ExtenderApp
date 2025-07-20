using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExtenderApp.Abstract
{
    /// <summary>
    /// 定义创建监听器连接器的工厂接口。
    /// </summary>
    public interface IListenerLinkerFactory
    {
        /// <summary>
        /// 创建一个监听器连接器实例。
        /// </summary>
        /// <typeparam name="T">指定连接器的类型，必须实现<see cref="ILinker"/>接口。</typeparam>
        /// <returns>返回创建的监听器连接器实例。</returns>
        IListenerLinker<T> CreateListenerLinker<T>() where T : ILinker;
    }
}
