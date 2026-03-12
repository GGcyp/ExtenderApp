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

        public override async ValueTask<Result> ActiveAsync(ILinkChannelHandlerContext context, CancellationToken token = default)
        {
            try
            {
                if (context.LinkChannel.Linker is not ITcpLinker tcpLinker)
                    throw new InvalidOperationException("当前连接不是TCP连接，无法使用SSL");

                tcpLinkerStream = tcpLinker.GetStream();
                sslStream = new(tcpLinkerStream, false);
                await sslStream.AuthenticateAsClientAsync(AuthenticationOptions, token).ConfigureAwait(false);
                return Result.Success("SSL 已连接");
            }
            catch (Exception ex)
            {
                return context.ExceptionCaught(ex);
            }
        }

        public override ValueTask<Result> CloseAsync(ILinkChannelHandlerContext context, CancellationToken token = default)
        {
            sslStream!.Close();
            return context.CloseAsync(token);
        }
    }
}