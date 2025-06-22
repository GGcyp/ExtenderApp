using AppHost.Extensions.DependencyInjection;

namespace ExtenderApp.Abstract
{
    /// <summary>
    /// 表示一个用于存储二进制格式化器的接口。
    /// </summary>
    public interface IBinaryFormatterStore : IConfiguration
    {
        /// <summary>
        /// 向格式化器存储中添加指定类型的格式化器。
        /// </summary>
        /// <typeparam name="Type">需要格式化的类型。</typeparam>
        /// <typeparam name="TypeFormatter">格式化器类型，必须实现 <see cref="IBinaryFormatter{T}"/> 接口。</typeparam>
        public void AddFormatter(Type type, Type TypeFormatter);

        /// <summary>
        /// 根据给定的类型获取相应的格式化器类型。
        /// </summary>
        /// <typeparam name="T">泛型参数类型。</typeparam>
        /// <param name="type">需要获取格式化器类型的目标类型。</param>
        /// <param name="formatter">输出参数，用于返回找到的格式化器类型。</param>
        /// <returns>如果找到对应的格式化器类型，则返回true；否则返回false。</returns>
        bool TryGetValue(Type type, out Type formatter);
    }
}
