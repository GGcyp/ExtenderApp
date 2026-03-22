using System.Diagnostics.CodeAnalysis;
using System.Text;
using ExtenderApp.Abstract.Networks;
using HttpHeader = ExtenderApp.Abstract.Networks.HttpHeader;

namespace ExtenderApp.Common.Networks
{
    /// <summary>
    /// 提供用于解析 HTTP 起始行与头部的辅助方法。
    /// - 以 <see cref="Encoding.Latin1"/> 解码字节到文本以兼容 RFC7230 的 header 编码习惯；
    /// - 支持从连续字节流中读取起始行（request-line / status-line）和逐行解析头部字段名/字段值。
    /// </summary>
    internal static class HttpHeaderHelps
    {
        /// <summary>
        /// 检查缓冲区中是否包含完整的 HTTP 头部块（即是否出现头部终止符 CRLFCRLF）。
        /// </summary>
        /// <param name="unread">要检查的字节片段。</param>
        /// <param name="headerLength">若返回 <c>true</c>，输出头部块结束位置相对于片段起始的字节偏移（包含终止符长度）。</param>
        /// <returns>如果找到头部终止符则返回 <c>true</c>，否则返回 <c>false</c>。</returns>
        public static bool TryGetHasHttpHeader(ReadOnlySpan<byte> unread, out int headerLength)
        {
            int headerEnd = unread.IndexOf(HttpConstants.HeaderTerminator);
            if (headerEnd < 0) { headerLength = 0; return false; }
            headerLength = headerEnd + HttpConstants.HeaderTerminator.Length;
            return true;
        }

        /// <summary>
        /// 尝试从缓冲区的起始处读取一行 HTTP 起始行（request-line 或 status-line）。
        /// </summary>
        /// <param name="unread">包含待解析数据的字节片段（从流的当前位置开始）。</param>
        /// <param name="startLine">若返回 <c>true</c>，输出解析得到的起始行文本（不包含 CRLF）。</param>
        /// <param name="lineLength">输出起始行的长度（以字节为单位），包含行尾的 LF，但不包含后续数据。</param>
        /// <returns>若成功读取到完整的一行起始行则返回 <c>true</c>；否则返回 <c>false</c>（例如未找到换行符）。</returns>
        public static bool TryGetHttpStartLine(ReadOnlySpan<byte> unread, [NotNullWhen(true)] out string startLine, out int lineLength)
        {
            startLine = GetLineString(unread, Encoding.Latin1, out lineLength);
            return string.IsNullOrEmpty(startLine) == false;
        }

        /// <summary>
        /// 根据提供的起始行与数据缓冲解析出 <see cref="HttpHeader"/> 对象。
        /// </summary>
        /// <param name="unread">包含整个头部块的字节片段（从起始行开始）。</param>
        /// <param name="header">输出解析得到的 <see cref="HttpHeader"/> 实例，包含已注册并设置的头选项。</param>
        /// <param name="startLine">调用方可提供已解析的起始行文本，也可传入空字符串；方法会读取并忽略实际起始行文本（仅用于流位置移动）。</param>
        /// <param name="headerLength">输出头部块长度（字节），包含结尾的 CRLFCRLF。</param>
        /// <returns>若缓冲包含完整头部块则返回 <c>true</c> 并输出 header 与 headerLength；否则返回 <c>false</c>（需要更多数据）。</returns>
        public static bool TryGetHttpHeader(ReadOnlySpan<byte> unread, [NotNullWhen(true)] out HttpHeader header, [NotNullWhen(true)] out string startLine, [NotNullWhen(true)] out int headerLength)
        {
            header = null!;
            headerLength = 0;
            startLine = string.Empty;
            var headerTerminator = HttpConstants.HeaderTerminator;
            int headerEnd = unread.IndexOf(headerTerminator);
            if (headerEnd < 0)
                return false;

            headerLength = headerEnd + headerTerminator.Length;
            var encoding = Encoding.Latin1;

            int pos = 0;
            var remaining = unread.Slice(pos, headerLength - pos);
            startLine = GetLineString(remaining, encoding, out var nLength);

            // advance past start line
            pos += nLength + 1;
            header = new();
            while (pos < headerLength)
            {
                remaining = unread.Slice(pos, headerLength - pos);
                string line = GetLineString(remaining, encoding, out nLength);
                pos += nLength + 1;
                if (string.IsNullOrEmpty(line)) continue;

                int idx = line.IndexOf(':');
                if (idx <= 0) continue;
                string name = line.Substring(0, idx).Trim();
                string value = line.Substring(idx + 1).Trim();
                header.ApplyOption(name, value);
            }

            return true;
        }

        /// <summary>
        /// 从一个不包含起始行的头部字节块解析出 <see cref="HttpHeader"/> 对象。 通常用于在已知起始行长度后直接解析随后的一段头部文本（例如当起始行已被单独读取）。
        /// </summary>
        /// <param name="unread">包含头部数据（从头部首行为止或从第一条头字段开始）的字节片段。</param>
        /// <param name="header">输出解析出的 <see cref="HttpHeader"/> 实例。</param>
        /// <returns>若解析成功返回 <c>true</c>；若缓冲不包含完整头部则返回 <c>false</c>。</returns>
        public static bool TryGetHttpHeaderNotStartLine(ReadOnlySpan<byte> unread, [NotNullWhen(true)] out HttpHeader header)
        {
            if (!TryGetHasHttpHeader(unread, out int headerLength))
            {
                header = null!;
                return false;
            }

            int pos = 0;
            header = new();
            while (pos < headerLength)
            {
                var remaining = unread.Slice(pos, headerLength - pos);
                string line = GetLineString(remaining, Encoding.Latin1, out int nLength);
                pos += nLength + 1;
                if (string.IsNullOrEmpty(line)) continue;

                int idx = line.IndexOf(':');
                if (idx <= 0) continue;
                string name = line.Substring(0, idx).Trim();
                string value = line.Substring(idx + 1).Trim();
                header.ApplyOption(name, value);
            }

            return true;
        }

        /// <summary>
        /// 判断给定文本是否为 HTTP 请求的起始行（request-line），并解析出 METHOD、Request-URI 与版本号（as <see cref="System.Version"/>）。
        /// - 支持起始行形如: "GET /path HTTP/1.1" 或 "POST /foo HTTP/2.0"。
        /// - 解析时会容错去掉前缀 "HTTP/" 并尝试使用 <see cref="Version.TryParse(string,out Version)"/>。
        /// </summary>
        /// <param name="startLine">起始行文本（不包含 CRLF）。</param>
        /// <param name="method">输出的方法名（例如 "GET"）。</param>
        /// <param name="path">输出请求的目标路径/URI（原文）。</param>
        /// <param name="version">输出解析得到的版本（例如 1.1）。</param>
        /// <returns>当且仅当成功识别为请求行并解析出版本时返回 <c>true</c>。</returns>
        public static bool TryParseRequestStartLine(string startLine, [NotNullWhen(true)] out string method, [NotNullWhen(true)] out string path, out Version version)
        {
            method = path = null!;
            version = default!;
            if (string.IsNullOrWhiteSpace(startLine))
                return false;

            var parts = startLine.Split(' ', 3, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 3)
                return false;

            method = parts[0];
            path = parts[1];
            string verPart = parts[2];

            if (verPart.StartsWith("HTTP/", StringComparison.OrdinalIgnoreCase))
                verPart = verPart.Substring(5);

            if (Version.TryParse(verPart, out var parsed))
            {
                version = parsed;
                return true;
            }

            return false;
        }

        /// <summary>
        /// 从字节流起始处读取一行并判断其是否为请求起始行；若是则解析并返回 METHOD、Request-URI 与 Version。
        /// </summary>
        /// <param name="unread">包含起始行的字节片段（从行首开始）。</param>
        /// <param name="method">输出的方法名。</param>
        /// <param name="path">输出请求目标。</param>
        /// <param name="version">输出解析得到的版本。</param>
        /// <param name="lineLength">输出起始行占用的字节长度（包含 LF）。</param>
        /// <returns>若成功读取并解析为请求行则返回 <c>true</c>，否则返回 <c>false</c>。</returns>
        public static bool TryParseRequestStartLine(ReadOnlySpan<byte> unread, [NotNullWhen(true)] out string method, [NotNullWhen(true)] out string path, out Version version, out int lineLength)
        {
            method = path = null!;
            version = default!;
            var startLine = GetLineString(unread, Encoding.Latin1, out lineLength);
            if (string.IsNullOrEmpty(startLine))
                return false;

            return TryParseRequestStartLine(startLine, out method, out path, out version);
        }

        /// <summary>
        /// 判断给定文本是否为 HTTP 响应的起始行（status-line），并解析出 Version、StatusCode 与 Reason-Phrase。
        /// - 支持形如: "HTTP/1.1 200 OK" 的 status-line。
        /// </summary>
        /// <param name="startLine">响应起始行文本（不包含 CRLF）。</param>
        /// <param name="version">输出解析得到的版本（例如 1.1）。</param>
        /// <param name="statusCode">输出解析得到的数字状态码（例如 200）。</param>
        /// <param name="reasonPhrase">输出 Reason-Phrase（可能包含空格）。</param>
        /// <returns>当且仅当成功识别为响应行并解析出版本和状态码时返回 <c>true</c>。</returns>
        public static bool TryParseResponseStartLine(string startLine, out Version version, out int statusCode, out string reasonPhrase)
        {
            version = default!;
            statusCode = 0;
            reasonPhrase = string.Empty;

            if (string.IsNullOrWhiteSpace(startLine))
                return false;

            var parts = startLine.Split(' ', 3, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 2)
                return false;

            // parts[0] should be like HTTP/1.1
            var verPart = parts[0];
            if (!verPart.StartsWith("HTTP/", StringComparison.OrdinalIgnoreCase))
                return false;

            verPart = verPart.Substring(5);
            if (!Version.TryParse(verPart, out var parsedVer))
                return false;

            if (!int.TryParse(parts[1], out var code))
                return false;

            version = parsedVer;
            statusCode = code;
            reasonPhrase = parts.Length >= 3 ? parts[2] : string.Empty;
            return true;
        }

        /// <summary>
        /// 从字节流起始处读取一行并判断其是否为响应起始行；若是则解析并返回 Version、StatusCode 与 Reason-Phrase。
        /// </summary>
        /// <param name="unread">包含起始行的字节片段（从行首开始）。</param>
        /// <param name="version">输出解析得到的版本。</param>
        /// <param name="statusCode">输出解析得到的数字状态码。</param>
        /// <param name="reasonPhrase">输出 Reason-Phrase。</param>
        /// <param name="lineLength">输出起始行占用的字节长度（包含 LF）。</param>
        /// <returns>若成功读取并解析为响应行则返回 <c>true</c>，否则返回 <c>false</c>。</returns>
        public static bool TryParseResponseStartLine(ReadOnlySpan<byte> unread, out Version version, out int statusCode, out string reasonPhrase, out int lineLength)
        {
            version = default!;
            statusCode = 0;
            reasonPhrase = string.Empty;
            var startLine = GetLineString(unread, Encoding.Latin1, out lineLength);
            if (string.IsNullOrEmpty(startLine))
                return false;

            return TryParseResponseStartLine(startLine, out version, out statusCode, out reasonPhrase);
        }

        /// <summary>
        /// 获取给定字节片段的首行文本（以指定编码解码），并输出该行占用的字节长度（包含行尾 LF）。 行末的 CR 会被自动去除（如果存在）。如果未找到换行符，则返回空字符串并将长度设置为 -1。 该方法适用于逐行解析 HTTP 头部字段时从连续字节流中读取每一行文本。
        /// </summary>
        /// <param name="span">包含待解析字节的片段。</param>
        /// <param name="encoding">用于解码字节的编码。</param>
        /// <param name="nLength">输出行占用的字节长度（包含 LF）。</param>
        /// <returns>返回解析得到的行文本，如果未找到换行符则返回空字符串。</returns>
        private static string GetLineString(ReadOnlySpan<byte> span, Encoding encoding, out int nLength)
        {
            const byte LF = (byte)'\n';
            const byte CR = (byte)'\r';

            nLength = span.IndexOf(LF);
            if (nLength < 0)
                return string.Empty;//没有找到换行符

            // 计算不含 CRLF 的行长度
            int lineLen = nLength;
            if (lineLen > 0 && span[lineLen - 1] == CR)
                lineLen--;

            // 取得行的 byte slice
            ReadOnlySpan<byte> lineSpan = span.Slice(0, lineLen);

            // 将行按 ASCII 解成 string（这里只对单行分配）
            return encoding.GetString(lineSpan);
        }
    }
}