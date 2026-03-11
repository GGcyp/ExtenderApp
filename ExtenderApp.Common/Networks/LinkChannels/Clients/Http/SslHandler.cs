using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using ExtenderApp.Abstract;
using ExtenderApp.Contracts;

namespace ExtenderApp.Common.Networks.LinkChannels
{
    public class SslHandler : LinkChannelHandler
    {
        private static readonly SslClientAuthenticationOptions AuthenticationOptions = new SslClientAuthenticationOptions
        {
            EnabledSslProtocols = System.Security.Authentication.SslProtocols.Tls12 | System.Security.Authentication.SslProtocols.Tls13,
            CertificateRevocationCheckMode = X509RevocationMode.Online,
            EncryptionPolicy = EncryptionPolicy.RequireEncryption,
        };

        private SslStream? sslStream;
        private TcpLinkerStream? tcpLinkerStream;

        public override void Added(ILinkChannelHandlerContext context)
        {
            tcpLinkerStream = GetTcpLinkerStream(context);
        }

        public override async ValueTask<Result> ActiveAsync(ILinkChannelHandlerContext context, CancellationToken token = default)
        {
            try
            {
                tcpLinkerStream = GetTcpLinkerStream(context) ?? throw new InvalidOperationException("当前连接不是TCP连接，无法使用SSL");
                sslStream = new(tcpLinkerStream, false);
                await sslStream.AuthenticateAsClientAsync(AuthenticationOptions, token).ConfigureAwait(false);
                return Result.Success();
            }
            catch (Exception ex)
            {
                var result = context.ExceptionCaught(ex);

                return result ?
                    result :
                    Result.FromException(ex, "SSL authentication failed.");
            }
        }

        private static TcpLinkerStream? GetTcpLinkerStream(ILinkChannelHandlerContext context)
        {
            return default;
        }
    }
}