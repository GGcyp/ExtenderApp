

namespace ExtenderApp.Data
{
    /// <summary>
    /// ���л�ǰͳһ׼��/������������Ӧͷ�ĸ���������
    /// ���й� Host / Content-Length / ��ѡĬ�� Content-Type �Ĺ����е���������ɢ�ڸ�����
    /// </summary>
    public static class HttpHeaderHelpers
    {
        /// <summary>
        /// �����л�����֮ǰȷ��ͷ���ѱ����룺
        /// - ���û�� Host ��� requestUri �� Host��
        /// - ��� body �ǿ���û�� Content-Length ���� Content-Length��
        /// - ��� body �ǿ���û�� Content-Type ���ṩ�� defaultContentType��������Ĭ�� Content-Type��
        /// </summary>
        /// <param name="headers">��Ϣͷ����</param>
        /// <param name="requestUri">���� Uri������Ϊ null��</param>
        /// <param name="body">��Ϣ��</param>
        /// <param name="defaultContentType">��ѡĬ�� Content-Type����Ϊ null �����ã�</param>
        public static void EnsureRequestHeaders(this HttpHeader headers, Uri? requestUri, in ByteBlock body, string? defaultContentType = null)
        {
            if (headers is null) throw new ArgumentNullException(nameof(headers));

            // Host��ֻ�������� RequestUri ��δ���� Host ʱ����
            if (requestUri != null && !headers.ContainsHeader(HttpHeaders.Host))
            {
                var host = requestUri.Host;
                if (!requestUri.IsDefaultPort)
                    host += ":" + requestUri.Port;
                headers.SetValue(HttpHeaders.Host, host);
            }

            // Content-Length������ body ��δ����ʱ����
            if (body.Length > 0 && !headers.ContainsHeader(HttpHeaders.ContentLength))
            {
                headers.SetValue(HttpHeaders.ContentLength, body.Length.ToString());
            }

            // Ĭ�� Content-Type������ body ������δ��ʽ����ʱ��Ч��
            if (!string.IsNullOrEmpty(defaultContentType) && body.Length > 0 && !headers.ContainsHeader(HttpHeaders.ContentType))
            {
                headers.SetValue(HttpHeaders.ContentType, defaultContentType);
            }
        }

        /// <summary>
        /// �����л���Ӧ֮ǰȷ��ͷ���ѱ����루ʾ����Content-Length��Date �ȣ���
        /// </summary>
        /// <param name="headers">��Ӧͷ����</param>
        /// <param name="body">��Ӧ��</param>
        /// <param name="defaultContentType">��ѡĬ�� Content-Type</param>
        public static void EnsureResponseHeaders(this HttpHeader headers, in ByteBlock body, string? defaultContentType = null)
        {
            if (headers is null) throw new ArgumentNullException(nameof(headers));

            if (body.Length > 0 && !headers.ContainsHeader(HttpHeaders.ContentLength))
                headers.SetValue(HttpHeaders.ContentLength, body.Length.ToString());

            if (!string.IsNullOrEmpty(defaultContentType) && body.Length > 0 && !headers.ContainsHeader(HttpHeaders.ContentType))
                headers.SetValue(HttpHeaders.ContentType, defaultContentType);

            if (!headers.ContainsHeader(HttpHeaders.Date))
                headers.SetValue(HttpHeaders.Date, DateTime.UtcNow.ToString("r"));
        }
    }
}