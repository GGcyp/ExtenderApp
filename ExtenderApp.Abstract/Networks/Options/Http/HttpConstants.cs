namespace ExtenderApp.Abstract.Networks
{
    /// <summary>
    /// HTTP 协议相关常量（CRLF、头部分隔符等）。
    /// 供序列化/解析 HTTP 首行与头部时复用，避免魔法字符串散落在代码中。
    /// </summary>
    public static class HttpConstants
    {
        /// <summary>
        /// HTTP 行结束符（CRLF）。等于 "\r\n"。
        /// </summary>
        public const string CRLF = "\r\n";

        /// <summary>
        /// HTTP 头部与主体之间的分隔符（CRLF CRLF）。等于 "\r\n\r\n"。
        /// </summary>
        public static readonly byte[] HeaderTerminator = { (byte)'\r', (byte)'\n', (byte)'\r', (byte)'\n' };

        /// <summary>
        /// 空格字符常量，常用于构造/解析 HTTP 报文时作为分隔符。
        /// </summary>
        public const string Space = " ";

        /// <summary>
        /// 点字符常量，常用于文件扩展名、主机名或版本号等的拼接与解析。
        /// </summary>
        public const string Dot = ".";

        /// <summary>
        /// 斜杠字符常量，常用于 URL 路径的拼接与解析。
        /// </summary>
        public const string Slash = "/";

        /// <summary>
        /// HTTP 报文行结束符（CRLF），用于分隔头部行与行尾。
        /// </summary>
        public const string NextLine = CRLF;
    }
}