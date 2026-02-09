using ExtenderApp.Contracts;

namespace ExtenderApp.Abstract
{
    /// <summary>
    /// 表示二进制格式化程序的存储接口，用于管理和获取不同类型的二进制格式化程序。
    /// 继承自 <see cref="IConfiguration"/> 接口。
    /// </summary>
    public interface IBinaryFormatterStore
    {
        /// <summary>
        /// 向格式化程序存储中添加指定类型的格式化程序。
        /// </summary>
        /// <typeparam name="T">需要格式化的类型。</typeparam>
        /// <typeparam name="TFormatter">格式化程序类型，必须实现 <see cref="IBinaryFormatter{T}"/> 接口。</typeparam>
        /// <param name="type">需要格式化的类型对象。</param>
        /// <param name="typeFormatter">格式化程序类型对象，必须实现 <see cref="IBinaryFormatter{T}"/> 接口。</param>
        /// <param name="isVersionDataFormatter">是否为版本格式化器</param>
        /// <exception cref="ArgumentNullException">当 <paramref name="type"/> 或 <paramref name="typeFormatter"/> 为 null 时抛出。</exception>
        /// <exception cref="ArgumentException">当 <paramref name="typeFormatter"/> 未实现 <see cref="IBinaryFormatter{T}"/> 接口时抛出。</exception>
        void AddFormatter(Type type, Type typeFormatter, bool isVersionDataFormatter = false);

        /// <summary>
        /// 尝试获取指定类型的二进制格式化程序详细信息。
        /// </summary>
        /// <param name="type">要查询的类型对象。</param>
        /// <param name="details">当方法返回时，包含与指定类型关联的 <see cref="BinaryFormatterDetails"/> 对象；如果未找到，则返回 null。</param>
        /// <returns>如果找到指定类型的格式化程序详细信息，则返回 true；否则返回 false。</returns>
        bool TryGetValue(Type type, out BinaryFormatterDetails? details);

        /// <summary>
        /// 尝试获取与指定类型关联的格式化程序类型。
        /// </summary>
        /// <param name="type">要查找的键类型。</param>
        /// <param name="formatterType">
        /// 当方法返回时，如果找到指定类型的格式化程序，则包含该格式化程序类型；
        /// 否则包含null。该参数未经初始化即被传递。
        /// </param>
        /// <returns>
        /// 如果找到指定类型的格式化程序，则返回true；
        /// 否则返回false。
        /// </returns>
        bool TryGetSingleFormatterType(Type type, out Type formatterType);
    }
}