using ExtenderApp.Data;

namespace ExtenderApp.Common.Network
{
    /// <summary>
    /// NetworkRequestMessage 类的扩展方法类
    /// </summary>
    public static class NetworkRequestMessageExtensions
    {
        /// <summary>
        /// 将 NetworkRequestMessage 转换为 HttpRequestMessage 对象
        /// </summary>
        /// <param name="message">待转换的 NetworkRequestMessage 对象</param>
        /// <returns>转换后的 HttpRequestMessage 对象</returns>
        public static HttpRequestMessage ToHttpRequestMessage(this NetworkRequestMessage message)
        {
            HttpRequestMessage result;
            if (!message.IsChange)
            {
                result = message.RequestMessage as HttpRequestMessage;
                if (result != null) return result;
            }

            result = new HttpRequestMessage(message.Method, message.TargetUri);
            result.Content = message.Content;
            for (int i = 0; i < message.Headers.Count; i++)
            {
                var header = message.Headers[i];
                result.Headers.Add(header.Key, header.Value);
            }

            message.IsChange = false;
            message.RequestMessage = result;
            return result;
        }
    }
}
