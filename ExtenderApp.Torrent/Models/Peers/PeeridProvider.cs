using System.Security.Cryptography;
using System.Text;

namespace ExtenderApp.Torrent
{
    public class PeeridProvider
    {
        private readonly TorrentSetting _torrentSetting;

        public PeeridProvider(TorrentSetting torrentSetting)
        {
            _torrentSetting = torrentSetting ?? throw new ArgumentNullException(nameof(torrentSetting));
            Encoding encoding = Encoding.ASCII;
            encoding.GetString(encoding.GetBytes(torrentSetting.ClientPrefix));
            //RandomNumberGenerator.Create()
        }

        //public string GeneratePeerId()
        //{
        //    // 生成12字节的随机部分
        //    byte[] randomBytes = new byte[12];
        //    using (var rng = RandomNumberGenerator.Create())
        //    {
        //        rng.GetBytes(randomBytes);
        //    }

        //    // 转换为Base32编码（保持ASCII可打印字符）
        //    string randomPart = ConvertToBase32(randomBytes);

        //    // 拼接完整Peer ID
        //    return ClientPrefix + randomPart.Substring(0, 12);
        //}
    }
}
