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
        public static void Deserialize(this IJsonPareserProvider provider, FileInfoData infoData, Type type, Action<object?> callback, FileParserOptions options = default)
        {
            object? result = provider.Deserialize(infoData, type, options);
            callback?.Invoke(result);
        }

        /// <summary>
        /// 从文件中反序列化对象
        /// </summary>
        /// <param name="infoData">文件信息数据</param>
        /// <param name="type">目标类型</param>
        /// <param name="options">文件设置（可选）</param>
        /// <returns>反序列化后的对象</returns>
        public static object? Deserialize(this IJsonPareserProvider provider, FileInfoData infoData, Type type, FileParserOptions options = default)
        {
            return provider.GetParser<IJsonParser>(options.LibraryName)?.Deserialize(infoData, type, options.Options);
        }

        /// <summary>
        /// 从文件中反序列化特定类型的对象，并通过回调返回结果
        /// </summary>
        /// <typeparam name="T">目标类型</typeparam>
        /// <param name="infoData">文件信息数据</param>
        /// <param name="callback">回调方法，用于处理反序列化后的对象</param>
        /// <param name="options">文件设置（可选）</param>
        public static void Deserialize<T>(this IJsonPareserProvider provider, FileInfoData infoData, Action<T?> callback, FileParserOptions options = default) where T : class
        {
            T? result = provider.Deserialize<T>(infoData, options);
            callback?.Invoke(result);
        }

        /// <summary>
        /// 从文件中反序列化特定类型的对象
        /// </summary>
        /// <typeparam name="T">目标类型</typeparam>
        /// <param name="infoData">文件信息数据</param>
        /// <param name="options">文件设置（可选）</param>
        /// <returns>反序列化后的对象</returns>
        public static T? Deserialize<T>(this IJsonPareserProvider provider, FileInfoData infoData, FileParserOptions options = default) where T : class
        {
            object? result = provider.Deserialize(infoData, typeof(T), options);
            return result as T;
        }

        /// <summary>
        /// 从JSON字符串中反序列化对象
        /// </summary>
        /// <param name="json">JSON字符串</param>
        /// <param name="type">目标类型</param>
        /// <param name="options">文件设置（可选）</param>
        /// <returns>反序列化后的对象</returns>
        public static object? Deserialize(this IJsonPareserProvider provider, string json, Type type, FileParserOptions options = default)
        {
            return provider.GetParser<IJsonParser>(options.LibraryName)?.Deserialize(json, type, options.Options);
        }

        /// <summary>
        /// 从JSON字符串中反序列化对象
        /// </summary>
        /// <param name="json">JSON字符串</param>
        /// <param name="type">目标类型</param>
        /// <param name="options">文件设置（可选）</param>
        /// <returns>反序列化后的对象</returns>
        public static T? Deserialize<T>(this IJsonPareserProvider provider, string json, FileParserOptions options = default) where T : class
        {
            return provider.GetParser<IJsonParser>(options.LibraryName)?.Deserialize(json, typeof(T), options.Options) as T;
        }

        /// <summary>
        /// 异步反序列化JSON数据到指定类型对象。
        /// </summary>
        /// <param name="provider">JSON解析器提供者。</param>
        /// <param name="infoData">文件信息数据。</param>
        /// <param name="type">目标类型。</param>
        /// <param name="options">文件解析选项（可选）。</param>
        /// <returns>包含反序列化对象的ValueTask。</returns>
        public static ValueTask<object?> DeserializeAsync(this IJsonPareserProvider provider, FileInfoData infoData, Type type, FileParserOptions options = default)
        {
            IJsonParser? parser = provider.GetParser<IJsonParser>(options.LibraryName);
            ArgumentNullException.ThrowIfNull(parser, nameof(parser));
            return parser.DeserializeAsync(infoData, type, options.Options);
        }

        /// <summary>
        /// 异步反序列化JSON数据到指定类型对象，并调用回调函数处理结果。
        /// </summary>
        /// <param name="provider">JSON解析器提供者。</param>
        /// <param name="infoData">文件信息数据。</param>
        /// <param name="type">目标类型。</param>
        /// <param name="callback">处理反序列化结果的回调函数。</param>
        /// <param name="options">文件解析选项（可选）。</param>
        public static async void DeserializeAsync(this IJsonPareserProvider provider, FileInfoData infoData, Type type, Action<object?> callback, FileParserOptions options = default)
        {
            var obj = await provider.DeserializeAsync(infoData, type, options);
            callback?.Invoke(obj);
        }

        /// <summary>
        /// 异步反序列化JSON字符串到指定类型对象，并调用回调函数处理结果。
        /// </summary>
        /// <param name="provider">JSON解析器提供者。</param>
        /// <param name="json">JSON字符串。</param>
        /// <param name="type">目标类型。</param>
        /// <param name="callback">处理反序列化结果的回调函数。</param>
        /// <param name="options">文件解析选项（可选）。</param>
        public static async void DeserializeAsync(this IJsonPareserProvider provider, string json, Type type, Action<object?> callback, FileParserOptions options = default)
        {
            object? result = await provider.DeserializeAsync(json, type, options);
            callback?.Invoke(result);
        }

        /// <summary>
        /// 异步反序列化JSON字符串到指定类型对象。
        /// </summary>
        /// <param name="provider">JSON解析器提供者。</param>
        /// <param name="json">JSON字符串。</param>
        /// <param name="type">目标类型。</param>
        /// <param name="options">文件解析选项（可选）。</param>
        /// <returns>包含反序列化对象的Task。</returns>
        public static async Task<object?> DeserializeAsync(this IJsonPareserProvider provider, string json, Type type, FileParserOptions options = default)
        {
            try
            {
                return Task.Run(() =>
                {
                    IJsonParser? parser = provider.GetParser<IJsonParser>(options.LibraryName);
                    ArgumentNullException.ThrowIfNull(parser, nameof(parser));
                    object? result = parser.Deserialize(json, type, options);
                    return result;
                });
            }
            catch (Exception ex)
            {
                return Task.CompletedTask;
            }
        }

        /// <summary>
        /// 将对象序列化为JSON字符串并保存到文件中
        /// </summary>
        /// <param name="jsonObject">要序列化的对象</param>
        /// <param name="infoData">文件信息数据</param>
        /// <param name="options">文件设置（可选）</param>
        /// <returns>序列化操作是否成功</returns>
        public static bool Serialize(this IJsonPareserProvider provider, object jsonObject, FileInfoData infoData, FileParserOptions options = default)
        {
            var parser = provider.GetParser<IJsonParser>(options.LibraryName);

            ArgumentNullException.ThrowIfNull(parser, string.Format("not found the parser {0}", options.LibraryName));

            return parser.Serialize(jsonObject, infoData, options.Options);
        }

        /// <summary>
        /// 将对象序列化为JSON字符串并保存到文件中，通过回调返回序列化结果
        /// </summary>
        /// <param name="jsonObject">要序列化的对象</param>
        /// <param name="infoData">文件信息数据</param>
        /// <param name="callback">回调方法，用于处理序列化结果</param>
        /// <param name="options">文件设置（可选）</param>
        public static void Serialize(this IJsonPareserProvider provider, object jsonObject, FileInfoData infoData, Action<bool> callback, FileParserOptions options = default)
        {
            bool result = provider.Serialize(jsonObject, infoData, options);
            callback?.Invoke(result);
        }

        /// <summary>
        /// 将对象序列化为JSON字符串
        /// </summary>
        /// <param name="jsonObject">要序列化的对象</param>
        /// <param name="options">文件设置（可选）</param>
        /// <returns>序列化后的JSON字符串</returns>
        public static string Serialize(this IJsonPareserProvider provider, object jsonObject, FileParserOptions options = default)
        {
            var parser = provider.GetParser<IJsonParser>(options.LibraryName);

            ArgumentNullException.ThrowIfNull(parser, string.Format("not found the parser {0}", options.LibraryName));

            return parser.Serialize(jsonObject, options);
        }

        /// <summary>
        /// 将特定类型的对象序列化为JSON字符串，并通过回调返回序列化结果
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="jsonObject">要序列化的对象</param>
        /// <param name="callback">回调方法，用于处理序列化结果</param>
        /// <param name="options">文件设置（可选）</param>
        public static void Serialize(this IJsonPareserProvider provider, object jsonObject, Action<string> callback, FileParserOptions options = default)
        {
            string json = provider.Serialize(jsonObject, options);
            callback?.Invoke(json);
        }

        /// <summary>
        /// 异步将对象序列化为JSON字符串并保存到文件中，通过回调返回序列化结果。
        /// </summary>
        /// <param name="jsonObject">要序列化的对象。</param>
        /// <param name="infoData">文件信息数据。</param>
        /// <param name="callback">回调方法，用于处理序列化结果。</param>
        /// <param name="options">文件设置（可选）。</param>
        public static async void SerializeAsync(this IJsonPareserProvider provider, object jsonObject, FileInfoData infoData, Action<bool> callback, FileParserOptions options = default)
        {
            bool result = await provider.SerializeAsync(jsonObject, infoData, options);
            callback?.Invoke(result);
        }

        /// <summary>
        /// 异步将对象序列化为JSON字符串并保存到文件中。
        /// </summary>
        /// <param name="jsonObject">要序列化的对象。</param>
        /// <param name="infoData">文件信息数据。</param>
        /// <param name="options">文件设置（可选）。</param>
        /// <returns>表示序列化操作是否成功的ValueTask。</returns>
        public static ValueTask<bool> SerializeAsync(this IJsonPareserProvider provider, object jsonObject, FileInfoData infoData, FileParserOptions options = default)
        {
            IJsonParser? parser = provider.GetParser<IJsonParser>(options.LibraryName);
            ArgumentNullException.ThrowIfNull(parser, nameof(parser));
            return parser.SerializeAsync(jsonObject, infoData, options);
        }
    }
}
