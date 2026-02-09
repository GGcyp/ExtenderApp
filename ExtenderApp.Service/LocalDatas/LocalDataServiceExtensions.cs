using ExtenderApp.Abstract;
using ExtenderApp.Common;
using ExtenderApp.Contracts;

namespace ExtenderApp.Services
{
    /// <summary>
    /// LocalDataServiceExtensions 类提供了对 ILocalDataService 接口的扩展方法。
    /// </summary>
    public static class LocalDataServiceExtensions
    {
        /// <summary>
        /// 为指定的本地数据类型添加一个本地数据格式化器到格式化器存储中
        /// </summary>
        /// <typeparam name="TLoclaData">本地数据类型</typeparam>
        /// <typeparam name="TFormatter">数据格式化器类型，必须实现IVersionDataFormatter&lt;TLoclaData&gt;接口</typeparam>
        /// <param name="store">格式化器存储实例，通过扩展方法方式调用</param>
        public static void AddLocalDataFormatter<TLoclaData, TFormatter>(this IBinaryFormatterStore store)
            where TFormatter : IVersionDataFormatter<TLoclaData>
        {
            // 调用格式化器存储的AddVersionData方法，将本地数据类型和对应的格式化器类型添加到存储中
            store.AddVersionData<TLoclaData, TFormatter>();
        }
    }
}
