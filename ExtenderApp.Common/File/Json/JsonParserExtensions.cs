using ExtenderApp.Abstract;
using ExtenderApp.Data;

namespace ExtenderApp.Common
{
    public static class JsonParserExtensions
    {
        /// <summary>
        /// 从文件中反序列化对象，并通过回调返回结果
        /// </summary>
        /// <param name="infoData">文件信息数据</param>
        /// <param name="type">目标类型</param>
        /// <param name="callback">回调方法，用于处理反序列化后的对象</param>
        /// <param name="options">文件设置（可选）</param>
        public static void Deserialize(this IJsonParser parser, FileOperate infoData, Type type, Action<object?> callback, object options = default)
        {
            object? result = parser.Deserialize(infoData, type, options);
            callback?.Invoke(result);
        }

        /// <summary>
        /// 从文件中反序列化对象
        /// </summary>
        /// <param name="infoData">文件信息数据</param>
        /// <param name="type">目标类型</param>
        /// <param name="options">文件设置（可选）</param>
        /// <returns>反序列化后的对象</returns>
        public static object? Deserialize(this IJsonParser parser, FileOperate infoData, Type type, object options = default)
        {
            return parser.Deserialize(infoData, type, options);
        }

        /// <summary>
        /// 从文件中反序列化特定类型的对象，并通过回调返回结果
        /// </summary>
        /// <typeparam name="T">目标类型</typeparam>
        /// <param name="infoData">文件信息数据</param>
        /// <param name="callback">回调方法，用于处理反序列化后的对象</param>
        /// <param name="options">文件设置（可选）</param>
        public static void Deserialize<T>(this IJsonParser parser, FileOperate infoData, Action<T?> callback, object options = default)
        {
            T? result = parser.Deserialize<T>(infoData, options);
            callback?.Invoke(result);
        }

        ///// <summary>
        ///// 从文件中反序列化特定类型的对象
        ///// </summary>
        ///// <typeparam name="T">目标类型</typeparam>
        ///// <param name="infoData">文件信息数据</param>
        ///// <param name="options">文件设置（可选）</param>
        ///// <returns>反序列化后的对象</returns>
        //public static T? Deserialize<T>(this IJsonParser parser, FileInfoData infoData, object options = default)
        //{
        //    object? result = parser.Deserialize(infoData, typeof(T), options);
        //    return (T)result;
        //}

        /// <summary>
        /// 从JSON字符串中反序列化对象
        /// </summary>
        /// <param name="json">JSON字符串</param>
        /// <param name="type">目标类型</param>
        /// <param name="options">文件设置（可选）</param>
        /// <returns>反序列化后的对象</returns>
        public static object? Deserialize(this IJsonParser parser, string json, Type type, object options = default)
        {
            return parser.Deserialize(json, type, options);
        }

        ///// <summary>
        ///// 从JSON字符串中反序列化对象
        ///// </summary>
        ///// <param name="json">JSON字符串</param>
        ///// <param name="type">目标类型</param>
        ///// <param name="options">文件设置（可选）</param>
        ///// <returns>反序列化后的对象</returns>
        //public static T? Deserialize<T>(this IJsonParser parser, string json, object options = default)
        //{
        //    return (T)parser.Deserialize(json, typeof(T), options);
        //}

        /// <summary>
        /// 异步反序列化JSON数据到指定类型对象。
        /// </summary>
        /// <param name="parser">JSON解析器提供者。</param>
        /// <param name="infoData">文件信息数据。</param>
        /// <param name="type">目标类型。</param>
        /// <param name="options">文件解析选项（可选）。</param>
        /// <returns>包含反序列化对象的ValueTask。</returns>
        public static ValueTask<object?> DeserializeAsync(this IJsonParser parser, FileOperate infoData, Type type, object options = default)
        {
            ArgumentNullException.ThrowIfNull(parser, nameof(parser));
            return parser.DeserializeAsync(infoData, type, options);
        }

        /// <summary>
        /// 异步反序列化JSON数据到指定类型对象，并调用回调函数处理结果。
        /// </summary>
        /// <param name="parser">JSON解析器提供者。</param>
        /// <param name="infoData">文件信息数据。</param>
        /// <param name="type">目标类型。</param>
        /// <param name="callback">处理反序列化结果的回调函数。</param>
        /// <param name="options">文件解析选项（可选）。</param>
        public static async void DeserializeAsync(this IJsonParser parser, FileOperate infoData, Type type, Action<object?> callback, object options = default)
        {
            var obj = await parser.DeserializeAsync(infoData, type, options);
            callback?.Invoke(obj);
        }

        /// <summary>
        /// 异步反序列化JSON字符串到指定类型对象，并调用回调函数处理结果。
        /// </summary>
        /// <param name="parser">JSON解析器提供者。</param>
        /// <param name="json">JSON字符串。</param>
        /// <param name="type">目标类型。</param>
        /// <param name="callback">处理反序列化结果的回调函数。</param>
        /// <param name="options">文件解析选项（可选）。</param>
        public static async void DeserializeAsync(this IJsonParser parser, string json, Type type, Action<object?> callback, object options = default)
        {
            object? result = await parser.DeserializeAsync(json, type, options);
            callback?.Invoke(result);
        }

        /// <summary>
        /// 异步反序列化JSON字符串到指定类型对象。
        /// </summary>
        /// <param name="parser">JSON解析器提供者。</param>
        /// <param name="json">JSON字符串。</param>
        /// <param name="type">目标类型。</param>
        /// <param name="options">文件解析选项（可选）。</param>
        /// <returns>包含反序列化对象的Task。</returns>
        public static async Task<object?> DeserializeAsync(this IJsonParser parser, string json, Type type, object options = default)
        {
            return await Task.Run(() =>
            {
                ArgumentNullException.ThrowIfNull(parser, nameof(parser));
                object? result = parser.Deserialize(json, type, options);
                return result;
            });
        }

        /// <summary>
        /// 将对象序列化为JSON字符串并保存到文件中
        /// </summary>
        /// <param name="jsonObject">要序列化的对象</param>
        /// <param name="infoData">文件信息数据</param>
        /// <param name="options">文件设置（可选）</param>
        /// <returns>序列化操作是否成功</returns>
        public static bool Serialize(this IJsonParser parser, object jsonObject, FileOperate infoData, object options = default)
        {
            ArgumentNullException.ThrowIfNull(parser, nameof(jsonObject));

            return parser.Serialize(jsonObject, infoData, options);
        }

        /// <summary>
        /// 将对象序列化为JSON字符串并保存到文件中，通过回调返回序列化结果
        /// </summary>
        /// <param name="jsonObject">要序列化的对象</param>
        /// <param name="infoData">文件信息数据</param>
        /// <param name="callback">回调方法，用于处理序列化结果</param>
        /// <param name="options">文件设置（可选）</param>
        public static void Serialize(this IJsonParser parser, object jsonObject, FileOperate infoData, Action<bool> callback, object options = default)
        {
            bool result = parser.Serialize(jsonObject, infoData, options);
            callback?.Invoke(result);
        }

        /// <summary>
        /// 将对象序列化为JSON字符串
        /// </summary>
        /// <param name="jsonObject">要序列化的对象</param>
        /// <param name="options">文件设置（可选）</param>
        /// <returns>序列化后的JSON字符串</returns>
        public static string Serialize(this IJsonParser parser, object jsonObject, object options = default)
        {
            ArgumentNullException.ThrowIfNull(parser, nameof(jsonObject));

            return parser.Serialize(jsonObject, options);
        }

        /// <summary>
        /// 将特定类型的对象序列化为JSON字符串，并通过回调返回序列化结果
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="jsonObject">要序列化的对象</param>
        /// <param name="callback">回调方法，用于处理序列化结果</param>
        /// <param name="options">文件设置（可选）</param>
        public static void Serialize(this IJsonParser parser, object jsonObject, Action<string> callback, object options = default)
        {
            string json = parser.Serialize(jsonObject, options);
            callback?.Invoke(json);
        }

        /// <summary>
        /// 异步将对象序列化为JSON字符串并保存到文件中，通过回调返回序列化结果。
        /// </summary>
        /// <param name="jsonObject">要序列化的对象。</param>
        /// <param name="infoData">文件信息数据。</param>
        /// <param name="callback">回调方法，用于处理序列化结果。</param>
        /// <param name="options">文件设置（可选）。</param>
        public static async void SerializeAsync(this IJsonParser parser, object jsonObject, FileOperate infoData, Action<bool> callback, object options = default)
        {
            bool result = await parser.SerializeAsync(jsonObject, infoData, options);
            callback?.Invoke(result);
        }

        /// <summary>
        /// 异步将对象序列化为JSON字符串并保存到文件中。
        /// </summary>
        /// <param name="jsonObject">要序列化的对象。</param>
        /// <param name="infoData">文件信息数据。</param>
        /// <param name="options">文件设置（可选）。</param>
        /// <returns>表示序列化操作是否成功的ValueTask。</returns>
        public static ValueTask<bool> SerializeAsync(this IJsonParser parser, object jsonObject, FileOperate infoData, object options = default)
        {
            ArgumentNullException.ThrowIfNull(parser, nameof(parser));
            return parser.SerializeAsync(jsonObject, infoData, options);
        }
    }
}
