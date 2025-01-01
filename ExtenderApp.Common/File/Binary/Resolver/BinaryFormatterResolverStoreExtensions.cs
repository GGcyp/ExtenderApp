using ExtenderApp.Abstract;
using ExtenderApp.Data;

namespace ExtenderApp.Common
{
    /// <summary>
    /// BinaryFormatterResolverStore 类的扩展方法。
    /// </summary>
    public static class BinaryFormatterResolverStoreExtensions
    {
        /// <summary>
        /// 向 BinaryFormatterResolverStore 添加一个类型为 TType 的数据对应的格式化器信息。
        /// </summary>
        /// <typeparam name="TType">要序列化的数据类型。</typeparam>
        /// <typeparam name="TFormatter">用于序列化和反序列化 TType 类型的格式化器类型，必须实现 IBinaryFormatter 接口。</typeparam>
        /// <param name="store">当前的 BinaryFormatterResolverStore 实例。</param>
        /// <param name="scope">可选参数，指定格式化器的作用域。</param>
        /// <returns>返回当前修改后的 BinaryFormatterResolverStore 实例。</returns>
        public static BinaryFormatterResolverStore AddInfo<TType, TFormatter>(this BinaryFormatterResolverStore store, string scope = null) where TFormatter : IBinaryFormatter
        {
            store.Add(typeof(TType), new BinaryFormatterInfo(scope, typeof(TFormatter)));
            return store;
        }
    }
}
