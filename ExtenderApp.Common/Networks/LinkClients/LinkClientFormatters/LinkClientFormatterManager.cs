using System.Collections.Concurrent;
using ExtenderApp.Abstract;
using ExtenderApp.Common.Hash;

namespace ExtenderApp.Common.Networks
{
    /// <summary>
    /// 客户端格式化器管理器：负责注册与查找 <see cref="ILinkClientFormatter"/>。
    /// </summary>
    /// <remarks>
    /// - 提供两种索引：按数据类型哈希（MessageType）与按泛型类型 <c>TLinkClient</c>；
    /// - 当同一 <c>TLinkClient</c> 注册多个实现时，会自动以 <see cref="ClientFormatterTypeSwitch{T}"/> 聚合，
    ///   以支持基于 FormatterTypeHash 的后续分发。
    /// </remarks>
    internal class LinkClientFormatterManager : ILinkClientFormatterManager
    {
        /// <summary>
        /// 按数据类型哈希索引到格式化器（或聚合开关）的字典。
        /// </summary>
        private readonly ConcurrentDictionary<int, ILinkClientFormatter> _hashToFormatterDict;

        /// <summary>
        /// 初始化管理器实例。
        /// </summary>
        public LinkClientFormatterManager()
        {
            _hashToFormatterDict = new ConcurrentDictionary<int, ILinkClientFormatter>();
        }

        public void AddFormatter<T>(IClientFormatter<T> formatter)
        {
            if (formatter == null)
                throw new ArgumentNullException(nameof(formatter));

            Type type = typeof(T);
            if (_hashToFormatterDict.TryGetValue(formatter.DataType, out var lastFormatter))
            {
                throw new InvalidOperationException(string.Format("当前转换器已经被注册：{0}", type.FullName));
            }
            _hashToFormatterDict.TryAdd(formatter.DataType, formatter);
        }

        public void RemoveFormatter<T>()
        {
            _hashToFormatterDict.TryRemove(typeof(T).ComputeHash_FNV_1a(), out var _);
        }

        public ILinkClientFormatter? GetFormatter(int dataTypeHash)
        {
            _hashToFormatterDict.TryGetValue(dataTypeHash, out var formatter);
            return formatter;
        }

        public IClientFormatter<T>? GetFormatter<T>()
        {
            if (_hashToFormatterDict.TryGetValue(typeof(T).ComputeHash_FNV_1a(), out var formatter))
            {
                return formatter as IClientFormatter<T>;
            }
            return null;
        }
    }
}
