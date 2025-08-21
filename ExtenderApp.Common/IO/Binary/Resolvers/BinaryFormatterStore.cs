using ExtenderApp.Abstract;
using ExtenderApp.Common.IO.Binaries.Formatters;
using ExtenderApp.Data;

namespace ExtenderApp.Common.IO.Binaries
{
    internal class BinaryFormatterStore : Dictionary<Type, ValueOrList<Type>>, IBinaryFormatterStore
    {
        public void AddFormatter(Type type, Type typeFormatter)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));
            if (typeFormatter == null)
                throw new ArgumentNullException(nameof(typeFormatter));

            if (TryGetValue(type, out ValueOrList<Type> valueOrList))
            {
                if (!typeFormatter.IsAssignableTo(typeof(IVersionDataFormatter)))
                {
                    throw new InvalidOperationException($"只有继承IVersionDataFormatter的转换器才能重复添加：{type.FullName} : {typeFormatter.FullName}");
                }

                if (valueOrList.Contains(typeFormatter))
                {
                    throw new Exception($"转换器已存在：{type.FullName} : {typeFormatter.FullName}");
                }
                valueOrList.Add(typeFormatter);
                return;
            }

            valueOrList = new ValueOrList<Type>();
            valueOrList.Add(typeFormatter);
            Add(type, valueOrList);
        }

        public bool TryGetValue(Type type, out Type formatter)
        {
            formatter = default;
            if (!TryGetValue(type, out ValueOrList<Type> valueOrList))
                return false;

            formatter = valueOrList[valueOrList.Count - 1];
            return true;
        }
    }
}
