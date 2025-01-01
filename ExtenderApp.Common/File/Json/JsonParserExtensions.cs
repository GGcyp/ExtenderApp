using ExtenderApp.Abstract;
using ExtenderApp.Data;

namespace ExtenderApp.Common
{
    /// <summary>
    /// Json解析器扩展类
    /// </summary>
    public static class JsonParserExtensions
    {
        /// <summary>
        /// 反序列化本地文件
        /// </summary>
        /// <param name="parser">Json解析器</param>
        /// <param name="info">本地文件信息</param>
        /// <param name="type">目标类型</param>
        /// <param name="callback">回调函数，参数为反序列化后的对象</param>
        /// <param name="options">选项</param>
        public static void Deserialize(this IJsonParser parser, LocalFileInfo info, Type type, Action<object?> callback, object options = default)
        {
            parser.Deserialize(new FileOperate(info), type, callback, options);
        }

        /// <summary>
        /// 反序列化文件操作对象
        /// </summary>
        /// <param name="parser">Json解析器</param>
        /// <param name="info">文件操作对象</param>
        /// <param name="type">目标类型</param>
        /// <param name="callback">回调函数，参数为反序列化后的对象</param>
        /// <param name="options">选项</param>
        public static void Deserialize(this IJsonParser parser, FileOperate info, Type type, Action<object?> callback, object options = default)
        {
            ArgumentNullException.ThrowIfNull(parser, nameof(parser));
            object? result = parser.Deserialize(info, type, options);
            callback?.Invoke(result);
        }

        /// <summary>
        /// 反序列化本地文件并返回结果
        /// </summary>
        /// <param name="parser">Json解析器</param>
        /// <param name="info">本地文件信息</param>
        /// <param name="type">目标类型</param>
        /// <param name="options">选项</param>
        /// <returns>反序列化后的对象</returns>
        public static object? Deserialize(this IJsonParser parser, LocalFileInfo info, Type type, object options = default)
        {
            ArgumentNullException.ThrowIfNull(parser, nameof(parser));
            return parser.Deserialize(new FileOperate(info, FileMode.Open, FileAccess.Read), type, options);
        }

        /// <summary>
        /// 反序列化本地文件并调用回调函数
        /// </summary>
        /// <param name="parser">Json解析器</param>
        /// <param name="info">本地文件信息</param>
        /// <param name="callback">回调函数，参数为反序列化后的对象</param>
        /// <param name="options">选项</param>
        /// <typeparam name="T">目标类型</typeparam>
        public static void Deserialize<T>(this IJsonParser parser, LocalFileInfo info, Action<T?> callback, object options = default)
        {
            parser.Deserialize(new FileOperate(info, FileMode.Open, FileAccess.Read), callback, options);
        }

        /// <summary>
        /// 反序列化文件操作对象并调用回调函数
        /// </summary>
        /// <param name="parser">Json解析器</param>
        /// <param name="info">文件操作对象</param>
        /// <param name="callback">回调函数，参数为反序列化后的对象</param>
        /// <param name="options">选项</param>
        /// <typeparam name="T">目标类型</typeparam>
        public static void Deserialize<T>(this IJsonParser parser, FileOperate info, Action<T?> callback, object options = default)
        {
            ArgumentNullException.ThrowIfNull(parser, nameof(parser));
            T? result = parser.Deserialize<T>(info, options);
            callback?.Invoke(result);
        }

        /// <summary>
        /// 反序列化本地文件并返回泛型结果
        /// </summary>
        /// <param name="parser">Json解析器</param>
        /// <param name="info">本地文件信息</param>
        /// <param name="options">选项</param>
        /// <typeparam name="T">目标类型</typeparam>
        /// <returns>反序列化后的对象</returns>
        public static T? Deserialize<T>(this IJsonParser parser, LocalFileInfo info, object options = default)
        {
            ArgumentNullException.ThrowIfNull(parser, nameof(parser));
            return parser.Deserialize<T>(new FileOperate(info, FileMode.Open, FileAccess.Read), options);
        }

        /// <summary>
        /// 异步反序列化本地文件并调用回调函数
        /// </summary>
        /// <param name="parser">Json解析器</param>
        /// <param name="info">本地文件信息</param>
        /// <param name="type">目标类型</param>
        /// <param name="callback">回调函数，参数为反序列化后的对象</param>
        /// <param name="options">选项</param>
        public static async void DeserializeAsync(this IJsonParser parser, LocalFileInfo info, Type type, Action<object?> callback, object options = default)
        {
            ArgumentNullException.ThrowIfNull(parser, nameof(parser));
            await parser.DeserializeAsync(new FileOperate(info, FileMode.Open, FileAccess.Read), type, options);
        }

        /// <summary>
        /// 异步反序列化文件操作对象并调用回调函数
        /// </summary>
        /// <param name="parser">Json解析器</param>
        /// <param name="info">文件操作对象</param>
        /// <param name="type">目标类型</param>
        /// <param name="callback">回调函数，参数为反序列化后的对象</param>
        /// <param name="options">选项</param>
        public static async void DeserializeAsync(this IJsonParser parser, FileOperate info, Type type, Action<object?> callback, object options = default)
        {
            ArgumentNullException.ThrowIfNull(parser, nameof(parser));
            var obj = await parser.DeserializeAsync(info, type, options);
            callback?.Invoke(obj);
        }

        /// <summary>
        /// 异步反序列化JSON字符串并调用回调函数
        /// </summary>
        /// <param name="parser">Json解析器</param>
        /// <param name="json">JSON字符串</param>
        /// <param name="type">目标类型</param>
        /// <param name="callback">回调函数，参数为反序列化后的对象</param>
        /// <param name="options">选项</param>
        public static async void DeserializeAsync(this IJsonParser parser, string json, Type type, Action<object?> callback, object options = default)
        {
            ArgumentNullException.ThrowIfNull(parser, nameof(parser));
            object? result = await parser.DeserializeAsync(json, type, options);
            callback?.Invoke(result);
        }

        /// <summary>
        /// 异步反序列化JSON字符串
        /// </summary>
        /// <param name="parser">Json解析器</param>
        /// <param name="json">JSON字符串</param>
        /// <param name="type">目标类型</param>
        /// <param name="options">选项</param>
        /// <returns>异步任务，返回反序列化后的对象</returns>
        public static async Task<object?> DeserializeAsync(this IJsonParser parser, string json, Type type, object options = default)
        {
            ArgumentNullException.ThrowIfNull(parser, nameof(parser));
            return await Task.Run(() =>
            {
                object? result = parser.Deserialize(json, type, options);
                return result;
            });
        }

        /// <summary>
        /// 序列化对象为本地文件
        /// </summary>
        /// <param name="parser">Json解析器</param>
        /// <param name="jsonObject">要序列化的对象</param>
        /// <param name="info">本地文件信息</param>
        /// <param name="options">选项</param>
        /// <returns>是否序列化成功</returns>
        public static bool Serialize(this IJsonParser parser, object jsonObject, LocalFileInfo info, object options = default)
        {
            ArgumentNullException.ThrowIfNull(parser, nameof(parser));
            return parser.Serialize(jsonObject, new FileOperate(info, FileMode.OpenOrCreate, FileAccess.ReadWrite), options);
        }

        /// <summary>
        /// 序列化对象为本地文件并调用回调函数
        /// </summary>
        /// <param name="parser">Json解析器</param>
        /// <param name="jsonObject">要序列化的对象</param>
        /// <param name="info">本地文件信息</param>
        /// <param name="callback">回调函数，参数为序列化是否成功</param>
        /// <param name="options">选项</param>
        public static void Serialize(this IJsonParser parser, object jsonObject, LocalFileInfo info, Action<bool> callback, object options = default)
        {
            parser.Serialize(jsonObject, new FileOperate(info, FileMode.OpenOrCreate, FileAccess.ReadWrite), callback, options);
        }

        /// <summary>
        /// 序列化对象为文件操作对象并调用回调函数
        /// </summary>
        /// <param name="parser">Json解析器</param>
        /// <param name="jsonObject">要序列化的对象</param>
        /// <param name="info">文件操作对象</param>
        /// <param name="callback">回调函数，参数为序列化是否成功</param>
        /// <param name="options">选项</param>
        public static void Serialize(this IJsonParser parser, object jsonObject, FileOperate info, Action<bool> callback, object options = default)
        {
            ArgumentNullException.ThrowIfNull(parser, nameof(parser));
            bool result = parser.Serialize(jsonObject, info, options);
            callback?.Invoke(result);
        }

        /// <summary>
        /// 序列化对象为字符串并调用回调函数
        /// </summary>
        /// <param name="parser">Json解析器</param>
        /// <param name="jsonObject">要序列化的对象</param>
        /// <param name="callback">回调函数，参数为序列化后的JSON字符串</param>
        /// <param name="options">选项</param>
        public static void Serialize(this IJsonParser parser, object jsonObject, Action<string> callback, object options = default)
        {
            ArgumentNullException.ThrowIfNull(parser, nameof(parser));
            string json = parser.SerializeToString(jsonObject, options);
            callback?.Invoke(json);
        }

        /// <summary>
        /// 异步序列化对象为本地文件并调用回调函数
        /// </summary>
        /// <param name="parser">Json解析器</param>
        /// <param name="jsonObject">要序列化的对象</param>
        /// <param name="info">本地文件信息</param>
        /// <param name="callback">回调函数，参数为序列化是否成功</param>
        /// <param name="options">选项</param>
        public static async void SerializeAsync(this IJsonParser parser, object jsonObject, LocalFileInfo info, Action<bool> callback, object options = default)
        {
            ArgumentNullException.ThrowIfNull(parser, nameof(parser));
            bool result = await parser.SerializeAsync(jsonObject, new FileOperate(info, FileMode.OpenOrCreate, FileAccess.ReadWrite), options);
            callback?.Invoke(result);
        }

        /// <summary>
        /// 异步序列化对象为本地文件
        /// </summary>
        /// <param name="parser">Json解析器</param>
        /// <param name="jsonObject">要序列化的对象</param>
        /// <param name="info">本地文件信息</param>
        /// <param name="options">选项</param>
        /// <returns>异步任务，返回序列化是否成功</returns>
        public static ValueTask<bool> SerializeAsync(this IJsonParser parser, object jsonObject, LocalFileInfo info, object options = default)
        {
            ArgumentNullException.ThrowIfNull(parser, nameof(parser));
            return parser.SerializeAsync(jsonObject, new FileOperate(info, FileMode.OpenOrCreate, FileAccess.ReadWrite), options);
        }
    }
}
