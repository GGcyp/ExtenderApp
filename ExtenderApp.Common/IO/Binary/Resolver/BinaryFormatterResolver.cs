using AppHost.Extensions.DependencyInjection;
using ExtenderApp.Abstract;
using ExtenderApp.Common.IO.Binary.Formatter;

namespace ExtenderApp.Common
{
    /// <summary>
    /// 内部类 BinaryFormatterResolver，实现了 IBinaryFormatterResolver 接口。
    /// </summary>
    internal class BinaryFormatterResolver : IBinaryFormatterResolver
    {
        /// <summary>
        /// 存储BinaryFormatter解析器的仓库
        /// </summary>
        private readonly Dictionary<Type, IBinaryFormatter> _formmaterDict;
        private readonly IBinaryFormatterStore _store;
        private readonly ITentativeProvider _serviceProvider;
        private readonly BinaryFormatterCreator _formatCreator;

        /// <summary>
        /// 使用指定的BinaryFormatter解析器仓库创建BinaryFormatter解析器实例
        /// </summary>
        /// <param name="store">BinaryFormatter解析器仓库</param>
        public BinaryFormatterResolver(IBinaryFormatterStore store, ITentativeProvider provider)
        {
            _formmaterDict = new();
            _serviceProvider = provider;

            _store = store;
            _formatCreator = new(store);
        }

        /// <summary>
        /// 获取指定类型的BinaryFormatter解析器
        /// </summary>
        /// <typeparam name="T">要解析的类型</typeparam>
        /// <returns>返回指定类型的BinaryFormatter解析器</returns>
        /// <exception cref="InvalidOperationException">如果未找到指定类型的解析方法，则抛出此异常</exception>
        public IBinaryFormatter<T> GetFormatter<T>()
        {
            var resultType = typeof(T);
            if (!_formmaterDict.TryGetValue(resultType, out var formatter))
            {
                if (!_store.TryGetValue(resultType, out var formatterType))
                {
                    formatterType = _formatCreator.CreatFormatter(resultType);
                    if (formatterType is null)
                        throw new ArgumentNullException(resultType.FullName);
                }


                formatter = _serviceProvider.GetService(formatterType!) as IBinaryFormatter;
                if (formatter == null)
                    throw new ArgumentNullException(resultType.FullName);

                _formmaterDict.Add(resultType, formatter);
            }


            return (IBinaryFormatter<T>)formatter;
        }
    }
}
