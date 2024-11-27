namespace ExtenderApp.Data
{
    /// <summary>
    /// 网络请求消息结构体
    /// </summary>
    public struct NetworkRequestMessage
    {
        /// <summary>
        /// 请求方法
        /// </summary>
        private HttpMethod method;
        /// <summary>
        /// 获取或设置请求方法
        /// </summary>
        public HttpMethod Method
        {
            get => method;
            set
            {
                method = value;
                IsChange = true;
            }
        }

        /// <summary>
        /// 目标URI
        /// </summary>
        private Uri targetUri;
        /// <summary>
        /// 获取或设置目标URI
        /// </summary>
        public Uri TargetUri
        {
            get => targetUri;
            set
            {
                targetUri = value;
                IsChange = true;
            }
        }
        /// <summary>
        /// 请求头列表
        /// </summary>
        public ValueList<KeyValuePair<string, string>> Headers { get; }

        /// <summary>
        /// HTTP内容
        /// </summary>
        private HttpContent content;
        /// <summary>
        /// 获取或设置HTTP内容
        /// </summary>
        public HttpContent Content
        {
            get => content;
            set
            {
                content = value;
                IsChange = true;
            }
        }

        /// <summary>
        /// 指示请求消息是否已更改
        /// </summary>
        public bool IsChange { get; set; }

        /// <summary>
        /// 请求消息对象
        /// </summary>
        public object? RequestMessage { get; set; }

        /// <summary>
        /// 使用指定的请求方法和URI字符串初始化NetworkRequestMessage结构体
        /// </summary>
        /// <param name="method">请求方法</param>
        /// <param name="uri">目标URI字符串</param>
        /// <param name="content">HTTP内容（可选）</param>
        public NetworkRequestMessage(HttpMethod method, string uri, HttpContent content = null) : this(method, new Uri(uri), content)
        {

        }

        /// <summary>
        /// 使用指定的请求方法和URI初始化NetworkRequestMessage结构体
        /// </summary>
        /// <param name="method">请求方法</param>
        /// <param name="uri">目标URI</param>
        /// <param name="content">HTTP内容（可选）</param>
        public NetworkRequestMessage(HttpMethod method, Uri uri, HttpContent content = null)
        {
            this.method = method;
            targetUri = uri;
            Headers = new();
            IsChange = true;
            RequestMessage = null;
            this.content = content;
        }

        /// <summary>
        /// 添加请求头
        /// </summary>
        /// <param name="name">请求头名称</param>
        /// <param name="value">请求头值（可选）</param>
        public void AddHeader(string name, string? value)
        {
            Headers.Add(new KeyValuePair<string, string>(name, value));
            IsChange = true;
        }
    }
}
