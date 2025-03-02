using AppHost.Extensions.DependencyInjection;
using ExtenderApp.Abstract;

namespace ExtenderApp.Common.NetWorks
{
    internal class NetWorkFormatterResolver
    {
        private readonly NetWrokFormatterStore _store;
        private readonly Dictionary<int, INetWorkParse> _parseDict;
        private readonly ITentativeProvider _tentativeProvider;

        public NetWorkFormatterResolver(NetWrokFormatterStore store, ITentativeProvider provider)
        {
            _store = store;
            _tentativeProvider = provider;

            _parseDict = new();
        }

        /// <summary>
        /// 获取指定类型的网络解析器
        /// </summary>
        /// <typeparam name="T">网络解析器对应的类型</typeparam>
        /// <returns>返回指定类型的网络解析器</returns>
        /// <exception cref="KeyNotFoundException">如果未找到指定类型的网络解析器，则抛出此异常</exception>
        public INetWorkParse<T> GetParse<T>()
        {
            var resultType = typeof(T);
            var parse = GetParse(resultType.FullName.GetHashCode());
            if (parse == null)
                throw new KeyNotFoundException(resultType.FullName);

            return (INetWorkParse<T>)parse;
        }

        /// <summary>
        /// 根据哈希码获取网络解析器
        /// </summary>
        /// <param name="code">哈希码</param>
        /// <returns>返回对应的网络解析器</returns>
        /// <exception cref="ArgumentNullException">如果指定哈希码在存储中未找到对应类型或解析器，则抛出此异常</exception>
        public INetWorkParse GetParse(int code)
        {
            if (!_parseDict.TryGetValue(code, out var parse))
            {
                if (!_store.TryGetValue(code, out var type))
                {
                    if (type is null)
                        throw new ArgumentNullException(code.ToString());
                }

                parse = _tentativeProvider.GetService(type) as INetWorkParse;
                if (parse == null)
                    throw new ArgumentNullException(code.ToString());

                _parseDict.Add(code, parse);
            }
            return parse;
        }
    }
}
