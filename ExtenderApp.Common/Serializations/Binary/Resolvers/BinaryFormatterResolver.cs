using ExtenderApp.Abstract;
using ExtenderApp.Common.Serializations.Binary.Formatters;
using Microsoft.Extensions.DependencyInjection;

namespace ExtenderApp.Common
{
    /// <summary>
    /// 内部类 BinaryFormatterResolver，实现了
    /// IBinaryFormatterResolver 接口。
    /// </summary>
    internal class BinaryFormatterResolver : IBinaryFormatterResolver
    {
        /// <summary>
        /// 存储BinaryFormatter解析器的仓库
        /// </summary>
        private readonly Dictionary<Type, IBinaryFormatter> _formmaterDict;

        /// <summary>
        /// 二进制格式化器存储接口实例
        /// </summary>
        private readonly IBinaryFormatterStore _store;

        /// <summary>
        /// 服务提供者实例
        /// </summary>
        private readonly IServiceProvider _serviceProvider;

        /// <summary>
        /// 二进制格式化器创建器实例
        /// </summary>
        private readonly BinaryFormatterCreator _formatCreator;

        /// <summary>
        /// 使用指定的BinaryFormatter解析器仓库创建BinaryFormatter解析器实例
        /// </summary>
        /// <param name="store">BinaryFormatter解析器仓库</param>
        public BinaryFormatterResolver(IBinaryFormatterStore store, IServiceProvider provider)
        {
            _formmaterDict = new();
            _serviceProvider = provider;

            _store = store;
            _formatCreator = new(store);
        }

        public IBinaryFormatter<T> GetFormatter<T>()
        {
            return (IBinaryFormatter<T>)GetFormatter(typeof(T));
        }

        public IBinaryFormatter GetFormatter(Type type)
        {
            if (!_formmaterDict.TryGetValue(type, out var formatter))
            {
                lock (_formmaterDict)
                {
                    if (_formmaterDict.TryGetValue(type, out formatter))
                    {
                        return formatter;
                    }

                    Type? formatterType = null;
                    if (!_store.TryGetValue(type, out var details))
                    {
                        formatterType = _formatCreator.CreatFormatter(type);
                        if (formatterType is null)
                            throw new InvalidOperationException($"未找到转换器类型：{type.FullName}.");

                        formatter = formatter = ActivatorUtilities.CreateInstance(_serviceProvider, formatterType!) as IBinaryFormatter;
                    }
                    else
                    {
                        if (details!.FormatterTypes.Count == 0)
                        {
                            throw new ArgumentNullException($"转换器类型为空：{type}");
                        }

                        //检查是不是版本数据转换器
                        if (details.IsVersionDataFormatter)
                        {
                            var managerType = typeof(VersionDataFormatterManager<>).MakeGenericType(details.BinaryType);
                            var obj = Activator.CreateInstance(managerType, this);

                            if (obj is not IVersionDataFormatterManager manager)
                                throw new InvalidOperationException($"无法创建版本数据转换器管理器：{managerType.FullName}");

                            formatter = manager;
                            var formatterTypes = details.FormatterTypes;
                            for (int i = 0; i < formatterTypes.Count; i++)
                            {
                                var vdFormatter = ActivatorUtilities.CreateInstance(_serviceProvider, formatterTypes[i])!;
                                manager.AddFormatter(vdFormatter);
                            }

                            _formmaterDict.Add(details.VersionDataBinaryType!, formatter!);
                            type = details.BinaryType;
                        }
                        else
                        {
                            formatterType = details.FormatterTypes[0];
                            formatter = ActivatorUtilities.CreateInstance(_serviceProvider, formatterType!) as IBinaryFormatter;
                        }
                    }
                    if (formatter == null)
                        throw new ArgumentNullException($"未找到：{type.FullName} 的格式转换器");

                    _formmaterDict.Add(type, formatter);
                }
            }
            return formatter;
        }
    }
}