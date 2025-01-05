﻿using ExtenderApp.Abstract;
using ExtenderApp.Common.File.Binary.Formatter;
using ExtenderApp.Common.File.Binary.Formatter.Collection;
using ExtenderApp.Data;

namespace ExtenderApp.Common
{
    /// <summary>
    /// BinaryFormatterResolverStore 类的扩展方法。
    /// </summary>
    public static class BinaryFormatterResolverStoreExtensions
    {
        /// <summary>
        /// 向BinaryFormatterResolverStore添加指定类型的格式化器
        /// </summary>
        /// <typeparam name="Type">要序列化的类型，必须为类类型</typeparam>
        /// <typeparam name="TFormatter">用于序列化和反序列化Type类型的格式化器，必须实现IBinaryFormatter接口</typeparam>
        /// <param name="store">BinaryFormatterResolverStore实例</param>
        /// <returns>返回更新后的BinaryFormatterResolverStore实例</returns>
        public static BinaryFormatterResolverStore AddClassFormatter<Type, TFormatter>(this BinaryFormatterResolverStore store) where Type : class where TFormatter : IBinaryFormatter<Type>
        {
            store.Add<Type, TFormatter>();
            store.AddCollectionFormatter<Type>();
            return store;
        }

        /// <summary>
        /// 为给定的 <see cref="BinaryFormatterResolverStore"/> 添加一个针对结构体类型的格式化器。
        /// </summary>
        /// <typeparam name="Type">结构体类型。</typeparam>
        /// <typeparam name="TFormatter">实现 <see cref="IBinaryFormatter{T}"/> 接口的格式化器类型。</typeparam>
        /// <param name="store">要添加格式化器的 <see cref="BinaryFormatterResolverStore"/> 实例。</param>
        /// <returns>返回添加格式化器后的 <see cref="BinaryFormatterResolverStore"/> 实例。</returns>
        public static BinaryFormatterResolverStore AddStructFormatter<Type, TFormatter>(this BinaryFormatterResolverStore store) where Type : struct where TFormatter : IBinaryFormatter<Type>
        {
            store.Add<Type, TFormatter>();
            store.AddNullableFormatter<Type>();
            store.AddCollectionFormatter<Type>();
            return store;
        }

        /// <summary>
        /// 为指定类型的集合添加格式化器。
        /// </summary>
        /// <typeparam name="Type">集合元素的类型。</typeparam>
        /// <param name="store">BinaryFormatterResolverStore 实例。</param>
        /// <returns>返回添加了格式化器的 BinaryFormatterResolverStore 实例。</returns>
        public static BinaryFormatterResolverStore AddCollectionFormatter<Type>(this BinaryFormatterResolverStore store)
        {
            store.AddArrayFormatter<Type>();
            store.AddListFormatter<Type>();
            store.AddLinkedListFormatter<Type>();
            store.AddQueueFormatter<Type>();
            store.AddStackFormatter<Type>();
            return store;
        }

        /// <summary>
        /// 向 BinaryFormatterResolverStore 中添加一个可空类型的格式化器。
        /// </summary>
        /// <typeparam name="Type">要添加格式化器的类型，该类型必须为值类型。</typeparam>
        /// <param name="store">当前的 BinaryFormatterResolverStore 实例。</param>
        /// <returns>返回当前的 BinaryFormatterResolverStore 实例。</returns>
        public static BinaryFormatterResolverStore AddNullableFormatter<Type>(this BinaryFormatterResolverStore store) where Type : struct
        {
            return store.Add<Type?, NullableFormatter<Type>>();
        }

        /// <summary>
        /// 为给定的<see cref="BinaryFormatterResolverStore"/>添加数组类型的格式化器。
        /// </summary>
        /// <typeparam name="Type">数组元素的类型。</typeparam>
        /// <param name="store">需要添加格式化器的<see cref="BinaryFormatterResolverStore"/>实例。</param>
        /// <returns>返回添加格式化器后的<see cref="BinaryFormatterResolverStore"/>实例。</returns>
        public static BinaryFormatterResolverStore AddArrayFormatter<Type>(this BinaryFormatterResolverStore store)
        {
            return store.Add<Type[], ArrayFormatter<Type>>();
        }

        /// <summary>
        /// 向二进制格式化器解析器存储中添加枚举格式化器。
        /// </summary>
        /// <typeparam name="Type">枚举类型。</typeparam>
        /// <param name="store">二进制格式化器解析器存储实例。</param>
        /// <returns>添加枚举格式化器后的二进制格式化器解析器存储实例。</returns>
        public static BinaryFormatterResolverStore AddEnumFormatter<Type>(this BinaryFormatterResolverStore store) where Type : struct, Enum
        {
            return store.Add<Type, EnumFormatter<Type>>();
        }

        /// <summary>
        /// 为给定的<see cref="BinaryFormatterResolverStore"/>添加列表类型的格式化器。
        /// </summary>
        /// <typeparam name="Type">列表元素的类型。</typeparam>
        /// <param name="store">需要添加格式化器的<see cref="BinaryFormatterResolverStore"/>实例。</param>
        /// <returns>返回添加格式化器后的<see cref="BinaryFormatterResolverStore"/>实例。</returns>
        public static BinaryFormatterResolverStore AddListFormatter<Type>(this BinaryFormatterResolverStore store)
        {
            return store.Add<List<Type>, ListFormatter<Type>>();
        }

        /// <summary>
        /// 为指定的 <see cref="BinaryFormatterResolverStore"/> 添加 <see cref="StackFormatter{T}"/> 格式化器。
        /// </summary>
        /// <typeparam name="Type">泛型类型。</typeparam>
        /// <param name="store"><see cref="BinaryFormatterResolverStore"/> 实例。</param>
        /// <returns>返回修改后的 <see cref="BinaryFormatterResolverStore"/> 实例。</returns>
        /// <exception cref="ArgumentNullException"><paramref name="store"/> 为 <c>null</c>。</exception>
        public static BinaryFormatterResolverStore AddStackFormatter<Type>(this BinaryFormatterResolverStore store)
        {
            return store.Add<Stack<Type>, StackFormatter<Type>>();
        }

        /// <summary>
        /// 为给定的 <see cref="BinaryFormatterResolverStore"/> 添加 <see cref="LinkedListFormatter{T}"/> 格式化器。
        /// </summary>
        /// <typeparam name="Type">链表中存储的元素类型。</typeparam>
        /// <param name="store">当前的 <see cref="BinaryFormatterResolverStore"/> 实例。</param>
        /// <returns>返回添加格式化器后的 <see cref="BinaryFormatterResolverStore"/> 实例。</returns>
        public static BinaryFormatterResolverStore AddLinkedListFormatter<Type>(this BinaryFormatterResolverStore store)
        {
            return store.Add<LinkedList<Type>, LinkedListFormatter<Type>>();
        }

        /// <summary>
        /// 为指定的 <see cref="BinaryFormatterResolverStore"/> 添加一个队列格式化器。
        /// </summary>
        /// <typeparam name="Type">队列中元素的类型。</typeparam>
        /// <param name="store">要添加队列格式化器的 <see cref="BinaryFormatterResolverStore"/> 实例。</param>
        /// <returns>返回添加队列格式化器后的 <see cref="BinaryFormatterResolverStore"/> 实例。</returns>
        public static BinaryFormatterResolverStore AddQueueFormatter<Type>(this BinaryFormatterResolverStore store)
        {
            return store.Add<Queue<Type>, QueueFormatter<Type>>();
        }

        /// <summary>
        /// 向给定的 BinaryFormatterResolverStore 添加 DictionaryFormatter。
        /// </summary>
        /// <typeparam name="TKey">字典键的类型。</typeparam>
        /// <typeparam name="TValue">字典值的类型。</typeparam>
        /// <param name="store">要添加格式化器的 BinaryFormatterResolverStore。</param>
        /// <returns>添加了 DictionaryFormatter 的 BinaryFormatterResolverStore。</returns>
        public static BinaryFormatterResolverStore AddDictionaryFormatter<TKey, TValue>(this BinaryFormatterResolverStore store) where TKey : notnull
        {
            return store.Add<Dictionary<TKey, TValue>, DictionaryFormatter<TKey, TValue>>();
        }

        /// <summary>
        /// 向 <see cref="BinaryFormatterResolverStore"/> 添加一个自定义的字典格式化器。
        /// </summary>
        /// <typeparam name="TKey">字典键的类型。</typeparam>
        /// <typeparam name="TValue">字典值的类型。</typeparam>
        /// <typeparam name="TDictionary">字典的类型，需要实现 <see cref="IDictionary{TKey, TValue}"/> 接口。</typeparam>
        /// <param name="store">要添加格式化器的 <see cref="BinaryFormatterResolverStore"/> 实例。</param>
        /// <returns>返回添加格式化器后的 <see cref="BinaryFormatterResolverStore"/> 实例。</returns>
        public static BinaryFormatterResolverStore AddInterfaceDictionaryFormatter<TKey, TValue, TDictionary>(this BinaryFormatterResolverStore store) where TDictionary : IDictionary<TKey, TValue>, new()
        {
            return store.Add<TDictionary, CustomizeDictionaryFormatter<TKey, TValue, TDictionary>>();
        }

        /// <summary>
        /// 为<see cref="BinaryFormatterResolverStore"/>添加接口列表格式化器。
        /// </summary>
        /// <typeparam name="T">接口列表中的元素类型。</typeparam>
        /// <typeparam name="TList">实现了<see cref="IList{T}"/>接口的列表类型。</typeparam>
        /// <param name="store">当前<see cref="BinaryFormatterResolverStore"/>实例。</param>
        /// <returns>返回添加了接口列表格式化器的<see cref="BinaryFormatterResolverStore"/>实例。</returns>
        public static BinaryFormatterResolverStore AddInterfaceListFormatter<T, TList>(this BinaryFormatterResolverStore store) where TList : class, IList<T>, new()
        {
            return store.Add<TList, InterfaceListFormatter<T, TList>>();
        }

        /// <summary>
        /// 为 <see cref="BinaryFormatterResolverStore"/> 添加指定类型的格式化器。
        /// </summary>
        /// <typeparam name="Type">需要添加格式化器的类型。</typeparam>
        /// <typeparam name="TFormatter">实现 <see cref="IBinaryFormatter"/> 接口的格式化器类型。</typeparam>
        /// <param name="store">当前的 <see cref="BinaryFormatterResolverStore"/> 实例。</param>
        /// <returns>返回添加格式化器后的 <see cref="BinaryFormatterResolverStore"/> 实例。</returns>
        public static BinaryFormatterResolverStore Add<Type, TFormatter>(this BinaryFormatterResolverStore store) where TFormatter : IBinaryFormatter
        {
            store.Add(typeof(Type), typeof(TFormatter));
            return store;
        }
    }
}
