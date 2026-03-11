namespace ExtenderApp.Abstract
{
    /// <summary>
    /// 表示 HTTP 方法（如 GET/POST）的不可变值类型。
    /// 比较时不区分大小写（使用 <see cref="StringComparison.OrdinalIgnoreCase"/>）。
    /// </summary>
    /// <param name="Method"> 方法字符串（例如 "GET"、"POST"）。建议使用大写惯例，但比较不区分大小写。 </param>
    public readonly record struct HttpMethod(string Method)
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
        /// 获得是否为空方法（即 Method 为 null 或空字符串）。
        /// </summary>
        public bool IsEmpty => string.IsNullOrEmpty(Method);

        /// <summary>
        /// 与另一个 <see cref="HttpMethod"/> 比较是否表示同一 HTTP 方法（不区分大小写）。
        /// </summary>
        /// <param name="other">要比较的另一个实例。</param>
        /// <returns>若方法名称相同（不区分大小写）则返回 true，否则返回 false。</returns>
        public bool Equals(HttpMethod other) => other.Method.Equals(Method, StringComparison.OrdinalIgnoreCase);

        /// <summary>
        /// 返回与方法名称不区分大小写的哈希码，便于在字典/集合中使用。
        /// </summary>
        public override int GetHashCode() => Method.GetHashCode(StringComparison.OrdinalIgnoreCase);

        public override string ToString() => Method;

        public static implicit operator HttpMethod(string method)
            => new(method);

        public static implicit operator string(HttpMethod httpMethod)
            => httpMethod.Method;
    }
}