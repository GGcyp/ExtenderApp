﻿using ExtenderApp.Abstract;

namespace ExtenderApp.Common.IO.Binaries.Formatter
{
    /// <summary>
    /// 二进制格式创建器类
    /// </summary>
    internal class BinaryFormatterCreator
    {
        /// <summary>
        /// 类型字典
        /// </summary>
        private IBinaryFormatterStore _store;

        /// <summary>
        /// 初始化二进制格式创建器
        /// </summary>
        /// <param name="store">二进制格式化解析器存储</param>
        public BinaryFormatterCreator(IBinaryFormatterStore store)
        {
            _store = store;

            _store.AddFormatter(typeof(Enum), typeof(EnumFormatter<>));
            _store.AddFormatter(typeof(List<>), typeof(ListFormatter<>));
            _store.AddFormatter(typeof(Stack<>), typeof(StackFormatter<>));
            _store.AddFormatter(typeof(Queue<>), typeof(QueueFormatter<>));
            _store.AddFormatter(typeof(Array), typeof(ArrayFormatter<>));

            _store.AddFormatter(typeof(Dictionary<,>), typeof(DictionaryFormatter<,>));
        }

        /// <summary>
        /// 创建格式化器
        /// </summary>
        /// <param name="type">类型</param>
        /// <returns>是否创建成功</returns>
        public Type? CreatFormatter(Type type)
        {
            if (type is null || type.IsAbstract)
            {
                return null;
            }

            if (type.IsArray)
            {
                return CreatArray(type);
            }

            if (type.IsEnum)
            {
                return CreatEnum(type);
            }

            if (type.IsGenericType)
            {
                return CreatGenericCollection(type);
            }

            Type genericType = type.BaseType;
            while (genericType != null && !genericType.IsGenericType)
            {
                genericType = genericType.BaseType;
            }
            if (genericType != null)
            {
                return CreatCollection(genericType, type);
            }

            //return typeof(DefaultObjectFormatter<>).MakeGenericType(type);

            return null;
        }

        /// <summary>
        /// 创建数组格式化器
        /// </summary>
        /// <param name="type">数组类型</param>
        /// <returns>是否创建成功</returns>
        private Type? CreatArray(Type type)
        {
            if (!_store.TryGetValue(typeof(Array), out var formatterType))
                return null;

            return formatterType.MakeGenericType(type);
        }

        /// <summary>
        /// 创建枚举格式化器
        /// </summary>
        /// <param name="type">枚举类型</param>
        /// <returns>是否创建成功</returns>
        private Type? CreatEnum(Type type)
        {
            if (!_store.TryGetValue(typeof(Enum), out var formatterType))
                return null;

            return formatterType.MakeGenericType(type);
        }

        /// <summary>
        /// 创建泛型格式化器(基础类型)
        /// </summary>
        /// <param name="type">泛型类型</param>
        /// <returns>是否创建成功</returns>
        private Type? CreatGenericCollection(Type type)
        {
            var genericTypeDefinition = type.GetGenericTypeDefinition();
            if (!_store.TryGetValue(genericTypeDefinition, out var formatterType))
                return null;

            Type[] typeArguments = type.GetGenericArguments();
            return formatterType.MakeGenericType(typeArguments);
        }

        /// <summary>
        /// 生成特定类型的转换器
        /// </summary>
        /// <param name="type">需要创建集合类型的类型。</param>
        /// <returns>如果类型是指定类型的集合，则返回相应的集合类型；否则返回null。</returns>
        private Type? CreatCollection(Type genericType, Type type)
        {
            var genericTypeDefinition = genericType.GetGenericTypeDefinition();
            if (typeof(List<>).IsAssignableFrom(genericTypeDefinition))
            {
                Type[] typeArguments = genericType.GetGenericArguments();
                return typeof(InterfaceListFormatter<,>).MakeGenericType(typeArguments[0], type);
            }
            else if (typeof(Dictionary<,>).IsAssignableFrom(genericTypeDefinition))
            {
                Type[] typeArguments = genericType.GetGenericArguments();
                return typeof(CustomizeDictionaryFormatter<,,>).MakeGenericType(typeArguments[0], typeArguments[1], type);
            }

            return null;
        }

    }
}
