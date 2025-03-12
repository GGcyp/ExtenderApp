using ExtenderApp.Abstract;
using ExtenderApp.Common.ObjectPools;
using System.Diagnostics.CodeAnalysis;

namespace ExtenderApp.Common.Networks
{
    public static class Http_httpClient
    {
        private static readonly HttpClient _httpClient = new HttpClient();

        public static HttpResponseMessage Send(HttpRequestMessage send)
        {
            HttpResponseMessage result;
            try
            {
                result = _httpClient.Send(send);
                // 检查响应是否成功
                result.EnsureSuccessStatusCode();

                if (result.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    throw new Exception(string.Format("未能成功链接目标{0}", send.Content?.ToString()));
                }

                return result;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public static async Task<HttpResponseMessage> SendAsync(HttpRequestMessage send)
        {
            HttpResponseMessage result;
            try
            {
                result = await _httpClient.SendAsync(send);
                // 检查响应是否成功
                result.EnsureSuccessStatusCode();

                if (result.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    throw new Exception(string.Format("未能成功链接目标{0}", send.Content?.ToString()));
                }

                return result;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        /// <summary>
        /// HttpRequestMessagePoolPolicy 类实现了 IPooledObjectPolicy<HttpRequestMessage> 接口，
        /// 用于创建和释放 HttpRequestMessage 对象。
        /// </summary>
        private class HttpRequestMessagePoolPolicy : PooledObjectPolicy<HttpRequestMessage>
        {
            /// <summary>
            /// 创建一个新的 HttpRequestMessage 实例。
            /// </summary>
            /// <returns>返回新创建的 HttpRequestMessage 实例。</returns>
            public override HttpRequestMessage Create()
            {
                return new HttpRequestMessage();
            }

            /// <summary>
            /// 释放 HttpRequestMessage 对象。
            /// </summary>
            /// <param name="obj">要释放的 HttpRequestMessage 对象。</param>
            /// <returns>如果对象成功释放，则返回 true；否则返回 false。</returns>
            public override bool Release(HttpRequestMessage obj)
            {
                obj.Content = null;
                obj.Headers.Clear();
                return true;
            }
        }

        /// <summary>
        /// 用于存储 HttpRequestMessage 对象的对象池。
        /// </summary>
        private static readonly ObjectPool<HttpRequestMessage> _requestPooled
            = ObjectPool.Create(new HttpRequestMessagePoolPolicy());

        #region Get

        /// <summary>
        /// 使用 GET 方法发送 HTTP 请求，URI 作为字符串参数。
        /// </summary>
        /// <param name="_httpClient">INetWork_httpClient 接口的实例。</param>
        /// <param name="requestUri">请求的 URI 字符串。</param>
        /// <returns>返回请求的响应。</returns>
        /// <exception cref="ArgumentNullException">如果 requestUri 为 null 或空字符串，则抛出此异常。</exception>
        public static HttpResponseMessage Get([StringSyntax("Uri")] string? requestUri)
        {
            if (string.IsNullOrEmpty(requestUri))
                throw new ArgumentNullException("requestUri");

            return Get(new Uri(requestUri));
        }

        /// <summary>
        /// 异步使用 GET 方法发送 HTTP 请求，URI 作为字符串参数。
        /// </summary>
        /// <param name="_httpClient">INetWork_httpClient 接口的实例。</param>
        /// <param name="requestUri">请求的 URI 字符串。</param>
        /// <returns>返回请求的响应。</returns>
        /// <exception cref="ArgumentNullException">如果 requestUri 为 null 或空字符串，则抛出此异常。</exception>
        public static async Task<HttpResponseMessage> GetAsync([StringSyntax("Uri")] string? requestUri)
        {
            if (string.IsNullOrEmpty(requestUri))
                throw new ArgumentNullException("requestUri");

            return await _httpClient.GetAsync(new Uri(requestUri));
        }

        /// <summary>
        /// 使用 GET 方法发送 HTTP 请求。
        /// </summary>
        /// <param name="_httpClient">INetWork_httpClient 接口的实例。</param>
        /// <param name="uri">请求的 URI。</param>
        /// <returns>返回请求的响应。</returns>
        /// <exception cref="ArgumentNullException">如果 _httpClient 或 uri 为 null，则抛出此异常。</exception>
        public static HttpResponseMessage Get([NotNull] Uri uri)
        {
            if (_httpClient is null)
                throw new ArgumentNullException("_httpClient");

            var request = _requestPooled.Get();
            try
            {
                request.Method = HttpMethod.Get;
                request.RequestUri = uri;

                var response = _httpClient.Send(request);
                return response;
            }
            catch (Exception ex)
            {
                throw;
            }
            finally
            {
                _requestPooled.Release(request);
            }
        }

        /// <summary>
        /// 异步使用 GET 方法发送 HTTP 请求。
        /// </summary>
        /// <param name="_httpClient">INetWork_httpClient 接口的实例。</param>
        /// <param name="uri">请求的 URI。</param>
        /// <returns>返回请求的响应。</returns>
        /// <exception cref="ArgumentNullException">如果 _httpClient 或 uri 为 null，则抛出此异常。</exception>
        public static async Task<HttpResponseMessage> GetAsync([NotNull] Uri uri)
        {
            if (_httpClient is null)
                throw new ArgumentNullException("_httpClient");

            var request = _requestPooled.Get();
            try
            {
                request.Method = HttpMethod.Get;
                request.RequestUri = uri;

                var response = await _httpClient.SendAsync(request);
                return response;
            }
            catch (Exception ex)
            {
                throw;
            }
            finally
            {
                _requestPooled.Release(request);
            }
        }

        #endregion

        #region Post

        /// <summary>
        /// 使用 POST 方法发送 HTTP 请求，URI 作为字符串参数。
        /// </summary>
        /// <param name="_httpClient">INetWork_httpClient 接口的实例。</param>
        /// <param name="requestUri">请求的 URI 字符串。</param>
        /// <param name="content">请求的内容。</param>
        /// <returns>返回请求的响应。</returns>
        /// <exception cref="ArgumentNullException">如果 requestUri 或 content 为 null 或空字符串，则抛出此异常。</exception>
        public static HttpResponseMessage Post([StringSyntax("Uri")] string? requestUri, HttpContent content)
        {
            if (string.IsNullOrEmpty(requestUri))
                throw new ArgumentNullException(nameof(requestUri));

            return Post(new Uri(requestUri), content);
        }

        /// <summary>
        /// 异步使用 POST 方法发送 HTTP 请求，URI 作为字符串参数。
        /// </summary>
        /// <param name="_httpClient">INetWork_httpClient 接口的实例。</param>
        /// <param name="requestUri">请求的 URI 字符串。</param>
        /// <param name="content">请求的内容。</param>
        /// <returns>返回请求的响应。</returns>
        /// <exception cref="ArgumentNullException">如果 requestUri 或 content 为 null 或空字符串，则抛出此异常。</exception>
        public static async Task<HttpResponseMessage> PostAsync([StringSyntax("Uri")] string? requestUri, HttpContent content)
        {
            if (string.IsNullOrEmpty(requestUri))
                throw new ArgumentNullException(nameof(requestUri));

            return await _httpClient.PostAsync(new Uri(requestUri), content);
        }

        /// <summary>
        /// 使用 POST 方法发送 HTTP 请求。
        /// </summary>
        /// <param name="_httpClient">INetWork_httpClient 接口的实例。</param>
        /// <param name="uri">请求的 URI。</param>
        /// <param name="content">请求的内容。</param>
        /// <returns>返回请求的响应。</returns>
        /// <exception cref="ArgumentNullException">如果 _httpClient、uri 或 content 为 null，则抛出此异常。</exception>
        public static HttpResponseMessage Post([NotNull] Uri uri, HttpContent content)
        {
            if (_httpClient is null)
                throw new ArgumentNullException(nameof(_httpClient));

            if (content is null)
                throw new ArgumentNullException(nameof(content));

            var request = _requestPooled.Get();
            try
            {
                request.Method = HttpMethod.Post;
                request.RequestUri = uri;
                request.Content = content;

                var response = _httpClient.Send(request);
                return response;
            }
            finally
            {
                _requestPooled.Release(request);
            }
        }

        /// <summary>
        /// 异步使用 POST 方法发送 HTTP 请求。
        /// </summary>
        /// <param name="_httpClient">INetWork_httpClient 接口的实例。</param>
        /// <param name="uri">请求的 URI。</param>
        /// <param name="content">请求的内容。</param>
        /// <returns>返回请求的响应。</returns>
        /// <exception cref="ArgumentNullException">如果 _httpClient、uri 或 content 为 null，则抛出此异常。</exception>
        public static async Task<HttpResponseMessage> PostAsync([NotNull] Uri uri, HttpContent content)
        {
            if (_httpClient is null)
                throw new ArgumentNullException(nameof(_httpClient));

            if (content is null)
                throw new ArgumentNullException(nameof(content));

            var request = _requestPooled.Get();
            try
            {
                request.Method = HttpMethod.Post;
                request.RequestUri = uri;
                request.Content = content;

                var response = await _httpClient.SendAsync(request);
                return response;
            }
            finally
            {
                _requestPooled.Release(request);
            }
        }

        #endregion

        #region Put

        /// <summary>
        /// 使用 PUT 方法发送 HTTP 请求，URI 作为字符串参数。
        /// </summary>
        /// <param name="_httpClient">INetWork_httpClient 接口的实例。</param>
        /// <param name="requestUri">请求的 URI 字符串。</param>
        /// <param name="content">请求的内容。</param>
        /// <returns>返回请求的响应。</returns>
        /// <exception cref="ArgumentNullException">如果 requestUri 或 content 为 null 或空字符串，则抛出此异常。</exception>
        public static HttpResponseMessage Put([StringSyntax("Uri")] string? requestUri, HttpContent content)
        {
            if (string.IsNullOrEmpty(requestUri))
                throw new ArgumentNullException(nameof(requestUri));

            return Put(new Uri(requestUri), content);
        }

        /// <summary>
        /// 异步使用 PUT 方法发送 HTTP 请求，URI 作为字符串参数。
        /// </summary>
        /// <param name="_httpClient">INetWork_httpClient 接口的实例。</param>
        /// <param name="requestUri">请求的 URI 字符串。</param>
        /// <param name="content">请求的内容。</param>
        /// <returns>返回请求的响应。</returns>
        /// <exception cref="ArgumentNullException">如果 requestUri 或 content 为 null 或空字符串，则抛出此异常。</exception>
        public static async Task<HttpResponseMessage> PutAsync([StringSyntax("Uri")] string? requestUri, HttpContent content)
        {
            if (string.IsNullOrEmpty(requestUri))
                throw new ArgumentNullException(nameof(requestUri));

            return await _httpClient.PutAsync(new Uri(requestUri), content);
        }

        /// <summary>
        /// 使用 PUT 方法发送 HTTP 请求。
        /// </summary>
        /// <param name="_httpClient">INetWork_httpClient 接口的实例。</param>
        /// <param name="uri">请求的 URI。</param>
        /// <param name="content">请求的内容。</param>
        /// <returns>返回请求的响应。</returns>
        /// <exception cref="ArgumentNullException">如果 _httpClient、uri 或 content 为 null，则抛出此异常。</exception>
        public static HttpResponseMessage Put([NotNull] Uri uri, HttpContent content)
        {
            if (_httpClient is null)
                throw new ArgumentNullException(nameof(_httpClient));

            if (content is null)
                throw new ArgumentNullException(nameof(content));

            var request = _requestPooled.Get();
            try
            {
                request.Method = HttpMethod.Put;
                request.RequestUri = uri;
                request.Content = content;

                var response = _httpClient.Send(request);
                return response;
            }
            finally
            {
                _requestPooled.Release(request);
            }
        }

        /// <summary>
        /// 异步使用 PUT 方法发送 HTTP 请求。
        /// </summary>
        /// <param name="_httpClient">INetWork_httpClient 接口的实例。</param>
        /// <param name="uri">请求的 URI。</param>
        /// <param name="content">请求的内容。</param>
        /// <returns>返回请求的响应。</returns>
        /// <exception cref="ArgumentNullException">如果 _httpClient、uri 或 content 为 null，则抛出此异常。</exception>
        public static async Task<HttpResponseMessage> PutAsync([NotNull] Uri uri, HttpContent content)
        {
            if (_httpClient is null)
                throw new ArgumentNullException(nameof(_httpClient));

            if (content is null)
                throw new ArgumentNullException(nameof(content));

            var request = _requestPooled.Get();
            try
            {
                request.Method = HttpMethod.Put;
                request.RequestUri = uri;
                request.Content = content;

                var response = await _httpClient.SendAsync(request);
                return response;
            }
            finally
            {
                _requestPooled.Release(request);
            }
        }

        #endregion

        #region Delete

        /// <summary>
        /// 使用 DELETE 方法发送 HTTP 请求，URI 作为字符串参数。
        /// </summary>
        /// <param name="_httpClient">INetWork_httpClient 接口的实例。</param>
        /// <param name="requestUri">请求的 URI 字符串。</param>
        /// <returns>返回请求的响应。</returns>
        /// <exception cref="ArgumentNullException">如果 requestUri 为 null 或空字符串，则抛出此异常。</exception>
        public static HttpResponseMessage Delete([StringSyntax("Uri")] string? requestUri)
        {
            if (string.IsNullOrEmpty(requestUri))
                throw new ArgumentNullException(nameof(requestUri));

            return Delete(new Uri(requestUri));
        }

        /// <summary>
        /// 异步使用 DELETE 方法发送 HTTP 请求，URI 作为字符串参数。
        /// </summary>
        /// <param name="_httpClient">INetWork_httpClient 接口的实例。</param>
        /// <param name="requestUri">请求的 URI 字符串。</param>
        /// <returns>返回请求的响应。</returns>
        /// <exception cref="ArgumentNullException">如果 requestUri 为 null 或空字符串，则抛出此异常。</exception>
        public static async Task<HttpResponseMessage> DeleteAsync([StringSyntax("Uri")] string? requestUri)
        {
            if (string.IsNullOrEmpty(requestUri))
                throw new ArgumentNullException(nameof(requestUri));

            return await _httpClient.DeleteAsync(new Uri(requestUri));
        }

        /// <summary>
        /// 使用 DELETE 方法发送 HTTP 请求。
        /// </summary>
        /// <param name="_httpClient">INetWork_httpClient 接口的实例。</param>
        /// <param name="uri">请求的 URI。</param>
        /// <returns>返回请求的响应。</returns>
        /// <exception cref="ArgumentNullException">如果 _httpClient 或 uri 为 null，则抛出此异常。</exception>
        public static HttpResponseMessage Delete([NotNull] Uri uri)
        {
            if (_httpClient is null)
                throw new ArgumentNullException(nameof(_httpClient));

            var request = _requestPooled.Get();
            try
            {
                request.Method = HttpMethod.Delete;
                request.RequestUri = uri;

                var response = _httpClient.Send(request);
                return response;
            }
            finally
            {
                _requestPooled.Release(request);
            }
        }

        /// <summary>
        /// 异步使用 DELETE 方法发送 HTTP 请求。
        /// </summary>
        /// <param name="_httpClient">INetWork_httpClient 接口的实例。</param>
        /// <param name="uri">请求的 URI。</param>
        /// <returns>返回请求的响应。</returns>
        /// <exception cref="ArgumentNullException">如果 _httpClient 或 uri 为 null，则抛出此异常。</exception>
        public static async Task<HttpResponseMessage> DeleteAsync([NotNull] Uri uri)
        {
            if (_httpClient is null)
                throw new ArgumentNullException(nameof(_httpClient));

            var request = _requestPooled.Get();
            try
            {
                request.Method = HttpMethod.Delete;
                request.RequestUri = uri;

                var response = await _httpClient.SendAsync(request);
                return response;
            }
            finally
            {
                _requestPooled.Release(request);
            }
        }

        #endregion
    }
}
