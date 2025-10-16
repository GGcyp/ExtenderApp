using System.Reflection;
using ExtenderApp.Abstract;

namespace ExtenderApp.Common.IO.Binary.Formatters
{
    /// <summary>
    /// 默认的对象格式化器类，继承自 <see cref="ResolverFormatter{T}"/> 类。
    /// </summary>
    /// <typeparam name="T">要格式化的对象的类型。</typeparam>
    internal class DefaultObjectFormatter<T> : AutoFormatter<T>
    {
        public DefaultObjectFormatter(IBinaryFormatterResolver resolver) : base(resolver)
        {
        }

        protected override void Init(AutoMemberDetailsStore store)
        {
            Type type = typeof(T);
            PropertyInfo[] properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetProperty | BindingFlags.SetProperty);
            FieldInfo[] fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetField | BindingFlags.SetField);

            for (int i = 0; i < properties.Length; i++)
            {
                store.Add(properties[i]);
            }

            for (int i = 0; i < fields.Length; i++)
            {
                store.Add(fields[i]);
            }
        }
    }
}
