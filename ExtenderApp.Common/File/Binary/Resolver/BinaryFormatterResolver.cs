using ExtenderApp.Abstract;

namespace ExtenderApp.Common.File.Binary
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
        private readonly BinaryFormatterResolverStore _store;

        /// <summary>
        /// 使用指定的BinaryFormatter解析器仓库创建BinaryFormatter解析器实例
        /// </summary>
        /// <param name="store">BinaryFormatter解析器仓库</param>
        public BinaryFormatterResolver(BinaryFormatterResolverStore store)
        {
            _formmaterDict = new();
            _store = store;
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
                throw new InvalidOperationException(string.Format("未找到{0}的解析方法", resultType.Name));

            return (IBinaryFormatter<T>)formatter;
        }
    }
}
