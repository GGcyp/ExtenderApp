using System.Reflection;
using ExtenderApp.Abstract;

namespace ExtenderApp.Common.Serializations.Binary.Formatters
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

            foreach (MemberInfo member in type.GetMembers())
            {
                if (member.TryGetSerializationsMemberAttribute(out bool include))
                {
                    if (include)
                        store.Add(member);

                    continue;
                }

                if (member is FieldInfo field && field.IsPublic ||
                    member is PropertyInfo property && HasPublicGetterAndSetter(property))
                {
                    store.Add(member);
                }
            }
        }

        private static bool HasPublicGetterAndSetter(PropertyInfo property)
        {
            return property.CanRead
                && property.CanWrite
                && property.GetMethod?.IsPublic == true
                && property.SetMethod?.IsPublic == true;
        }
    }
}