using AppHost.Extensions.DependencyInjection;
using ExtenderApp.Data;

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
        /// 尝试根据指定类型获取对应的格式化器类型集合
        /// </summary>
        /// <param name="type">需要获取格式化器的目标类型</param>
        /// <param name="formatters">输出参数，返回找到的格式化器类型集合，封装在 <see cref="ValueOrList{Type}"/> 中。</param>
        bool TryGetValue(Type type, out ValueOrList<Type> formatters);
        bool TryGetValue(Type type, out Type formatter);
    }
}
