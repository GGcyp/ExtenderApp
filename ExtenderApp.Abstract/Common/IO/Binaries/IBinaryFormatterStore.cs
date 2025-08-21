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

        /// <summary>
        /// 尝试获取与指定类型关联的格式化器类型
        /// </summary>
        /// <param name="type">要查询的数据类型</param>
        /// <param name="formatter">
        /// 当方法返回true时，输出参数包含与指定类型关联的格式化器类型；
        /// 当方法返回false时，输出参数为null
        /// </param>
        /// <returns>
        /// 如果找到与指定类型关联的格式化器类型则返回true，否则返回false
        /// </returns>
        bool TryGetValue(Type type, out Type formatter);
    }
}
