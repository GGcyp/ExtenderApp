using System;
using System.Net;
using System.Net.Sockets;
using ExtenderApp.Abstract;
using ExtenderApp.Data;

namespace ExtenderApp.Common.Networks
{
    /// <summary>
    /// 表示一个抽象的LinkClient类，该类继承自DisposableObject并实现ILinker接口。
    /// </summary>
    public abstract class LinkClient : DisposableObject
    {

    }

    /// <summary>
    /// 泛型链接客户端类，用于处理特定类型的链接器和链接解析器。
    /// </summary>
    /// <typeparam name="TLinker">实现ILinker接口的链接器类型。</typeparam>
    /// <typeparam name="TLinkParser">实现LinkParser接口的链接解析器类型。</typeparam>
    public class LinkClient<TLinker, TLinkParser> : LinkClient
        where TLinker : ILinker
        where TLinkParser : LinkParser
    {
       
    }
}
