using System.Collections.Concurrent;
using ExtenderApp.Abstract;

namespace ExtenderApp.Common.Networks
{
    /// <summary>
    /// 客户端格式化器管理器：负责注册与查找 <see cref="IClientFormatter"/>。
    /// </summary>
    /// <remarks>
    /// - 提供两种索引：按数据类型哈希（DataTypeHash）与按泛型类型 <c>T</c>；
    /// - 当同一 <c>T</c> 注册多个实现时，会自动以 <see cref="ClientFormatterTypeSwitch{T}"/> 聚合，
    ///   以支持基于 FormatterTypeHash 的后续分发。
    /// </remarks>
    internal class ClientFormatterManager : IClientFormatterManager
    {
        /// <summary>
        /// 按数据类型哈希索引到格式化器（或聚合开关）的字典。
        /// </summary>
        private readonly ConcurrentDictionary<int, IClientFormatter> _hashToFormatterDict;

        /// <summary>
        /// 按泛型类型 <c>T</c> 索引到格式化器（或聚合开关）的字典。
        /// </summary>
        private readonly ConcurrentDictionary<Type, IClientFormatter> _typeToFormatterDict;

        /// <summary>
        /// 初始化管理器实例。
        /// </summary>
        public ClientFormatterManager()
        {
            _hashToFormatterDict = new ConcurrentDictionary<int, IClientFormatter>();
            _typeToFormatterDict = new ConcurrentDictionary<Type, IClientFormatter>();
        }

        public void AddFormatter<T>(IClientFormatter<T> formatter)
        {
            if (formatter == null)
                throw new ArgumentNullException(nameof(formatter));

            Type type = typeof(T);
            if (_typeToFormatterDict.TryGetValue(type, out var lastFormatter))
            {
                throw new InvalidOperationException(string.Format("当前转换器已经被注册：{0}", type.FullName));
            }
            _typeToFormatterDict.TryAdd(type, formatter);
            _hashToFormatterDict.TryAdd(formatter.DataTypeHash, formatter);
        }

        public void RemoveFormatter<T>()
        {
            Type type = typeof(T);
            if (_typeToFormatterDict.TryRemove(type, out var formatter))
            {
                _hashToFormatterDict.TryRemove(formatter.DataTypeHash, out var _);
            }
        }

        public IClientFormatter? GetFormatter(int dataTypeHash)
        {
            _hashToFormatterDict.TryGetValue(dataTypeHash, out var formatter);
            return formatter;
        }

        public IClientFormatter<T>? GetFormatter<T>()
        {
            Type type = typeof(T);
            if (_typeToFormatterDict.TryGetValue(type, out var formatter))
            {
                return formatter as IClientFormatter<T>;
            }
            return null;
        }
    }
}
