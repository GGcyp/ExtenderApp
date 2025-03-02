
namespace ExtenderApp.Common.NetWorks
{
    /// <summary>
    /// LinkTypeStore 类用于存储和管理通过整数代码与类型之间的映射关系。
    /// </summary>
    public class LinkTypeStore
    {
        /// <summary>
        /// 私有字段，存储整数代码与类型之间的映射关系。
        /// </summary>
        private readonly Dictionary<int, Type> _dict;

        /// <summary>
        /// LinkTypeStore 类的构造函数。
        /// </summary>
        public LinkTypeStore()
        {
            _dict = new();
        }

        /// <summary>
        /// 根据整数代码获取对应的类型。
        /// </summary>
        /// <param name="code">整数代码。</param>
        /// <returns>返回对应的类型。</returns>
        /// <exception cref="KeyNotFoundException">如果找不到对应的整数代码，则抛出此异常。</exception>
        public Type Get(int code)
        {
            if (!_dict.TryGetValue(code, out var type))
            {
                throw new KeyNotFoundException();
            }

            return type;
        }

        /// <summary>
        /// 添加一个类型，并将其整数代码设置为类型的哈希码。
        /// </summary>
        /// <typeparam name="T">要添加的类型。</typeparam>
        public void Add<T>()
        {
            Type type = typeof(T);
            _dict.Add(type.Name.GetHashCode(), type);
        }
    }
}
