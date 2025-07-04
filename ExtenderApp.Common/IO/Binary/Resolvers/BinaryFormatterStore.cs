using ExtenderApp.Abstract;
using ExtenderApp.Data;

namespace ExtenderApp.Common
{
    internal class BinaryFormatterStore : Dictionary<Type, Type>, IBinaryFormatterStore
    {
        public void AddFormatter(Type type, Type TypeFormatter)
        {
            if (ContainsKey(type))
                throw new Exception(string.Format("重复添加转换器类型：{0}", type.FullName));

            Add(type, TypeFormatter);
        }

        public bool TryGetValue<T>(Type type, out Type formatter)
        {
            return TryGetValue(type, out formatter);
        }
    }
}
