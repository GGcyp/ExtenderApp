using System;

namespace ExtenderApp.Data
{
    /// <summary>
    /// 表示 HTTP 方法（如 GET/POST）的不可变值类型。
    /// 比较时不区分大小写（使用 <see cref="StringComparison.OrdinalIgnoreCase"/>）。
    /// </summary>
    public readonly struct HttpMethod : IEquatable<HttpMethod>
    {
        /// <summary>
        /// 表示 "GET" 方法。
        /// </summary>
        public static readonly HttpMethod Get = new("GET");

        /// <summary>
        /// 表示 "POST" 方法。
        /// </summary>
        public static readonly HttpMethod Post = new("POST");

        /// <summary>
        /// 表示 "PUT" 方法。
        /// </summary>
        public static readonly HttpMethod Put = new("PUT");

        /// <summary>
        /// 表示 "DELETE" 方法。
        /// </summary>
        public static readonly HttpMethod Delete = new("DELETE");

        /// <summary>
        /// 表示 "HEAD" 方法。
        /// </summary>
        public static readonly HttpMethod Head = new("HEAD");

        /// <summary>
        /// 方法字符串（例如 "GET"、"POST"）。建议使用大写惯例，但比较不区分大小写。
        /// </summary>
        public string Method { get; }

        /// <summary>
        /// 获得是否为空方法（即 Method 为 null 或空字符串）。
        /// </summary>
        public bool IsEmpty => string.IsNullOrEmpty(Method);

        /// <summary>
        /// 使用指定方法字符串构造一个 <see cref="HttpMethod"/> 实例。
        /// </summary>
        /// <param name="method">非空的 HTTP 方法名称。</param>
        /// <exception cref="ArgumentNullException">当 <paramref name="method"/> 为 null 时抛出。</exception>
        public HttpMethod(string method)
        {
            Method = method ?? throw new ArgumentNullException(nameof(method));
        }

        /// <summary>
        /// 与另一个 <see cref="HttpMethod"/> 比较是否表示同一 HTTP 方法（不区分大小写）。
        /// </summary>
        /// <param name="other">要比较的另一个实例。</param>
        /// <returns>若方法名称相同（不区分大小写）则返回 true，否则返回 false。</returns>
        public bool Equals(HttpMethod other)
        {
            return other.Method.Equals(Method, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// 相等运算符，等价于 <see cref="Equals(HttpMethod)"/>.
        /// </summary>
        public static bool operator ==(HttpMethod left, HttpMethod right)
            => left.Equals(right);

        /// <summary>
        /// 不等运算符，等价于取 <see cref="Equals(HttpMethod)"/> 的否定结果。
        /// </summary>
        public static bool operator !=(HttpMethod left, HttpMethod right)
            => !left.Equals(right);

        /// <summary>
        /// 覆盖自 <see cref="object"/> 的相等性比较；当且仅当对象为 <see cref="HttpMethod"/> 且方法等同时返回 true。
        /// </summary>
        public override bool Equals(object obj)
        {
            return obj is HttpMethod httpMethod && Equals(httpMethod);
        }

        /// <summary>
        /// 返回与方法名称不区分大小写的哈希码，便于在字典/集合中使用。
        /// </summary>
        public override int GetHashCode()
        {
            return Method.GetHashCode(StringComparison.OrdinalIgnoreCase);
        }

        public override string ToString()
        {
            return Method;
        }
    }
}