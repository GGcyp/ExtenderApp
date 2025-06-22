using ExtenderApp.Data;

namespace ExtenderApp.Torrent
{
    internal class MagnetlinkFactory
    {
        public MagnetLink FromUri(Uri uri)
        {
            InfoHash infoHash = InfoHash.Empty;
            string? name = null;
            List<string>? trackers = null;
            List<string>? dhtNodes = null;
            long? size = null;

            if (uri.Scheme != "magnet")
                throw new FormatException("磁力链接的开头不是 'magnet:'.");

            string[] parameters = uri.Query.Substring(1).Split('&');
            for (int i = 0; i < parameters.Length; i++)
            {
                string[] keyval = parameters[i].Split('=');
                if (keyval.Length != 2)
                {
                    // Skip anything we don't understand. Urls could theoretically contain many
                    // unknown parameters.
                    continue;
                }
                switch (keyval[0].Substring(0, 2))
                {
                    case "xt"://exact topic，精确主题.指定资源的唯一标识，最常见的是通过哈希值定位资源。
                        infoHash = GetTorrentHashValue(keyval[1], infoHash);
                        break;
                    case "tr"://tracker，追踪服务器地址
                        if (trackers == null)
                        {
                            trackers = new();
                        }
                        trackers.Add(keyval[1].UrlDecode());
                        break;
                    case "as"://Acceptable Source，可接受来源,限制允许的连接来源（安全筛选）不参与数据传输，仅控制连接策略
                        if (dhtNodes == null)
                        {
                            dhtNodes = new();
                        }
                        dhtNodes.Add(keyval[1].UrlDecode());
                        break;
                    case "ws"://Web Seed，Web种子地址 提供额外的下载路径（数据来源）直接参与文件下载（HTTP/HTTPS 传输）
                        if (dhtNodes == null)
                        {
                            dhtNodes = new();
                        }
                        dhtNodes.Add(keyval[1].UrlDecode());
                        break;
                    case "dn"://display name，显示名称
                        name = keyval[1].UrlDecode();
                        break;
                    case "xl"://exact length，精确长度
                        size = long.Parse(keyval[1]);
                        break;
                    //case "xs":// eXact Source - P2P link.
                    //case "kt"://keyword topic
                    //case "mt"://manifest topic
                    //case "fs"://File Source，文件来源
                    //case "up"://Uploader，上传者
                    // Unused
                    //break;
                    default:
                        // Unknown/unsupported
                        break;
                }
            }

            if (infoHash == null)
                throw new FormatException("The magnet link did not contain a valid 'xt' parameter referencing the infohash");

            return new MagnetLink(infoHash, name, trackers, dhtNodes);
        }

        private InfoHash GetTorrentHashValue(string value, InfoHash torrentHashValue)
        {
            string val = value.Substring(9);
            switch (value.Substring(4, 9))
            {
                case "sha1:"://base32 hash
                case "btih:":
                    if (!torrentHashValue.sha1.IsEmpty)
                        throw new FormatException("不允许有两个sha1磁力哈希值");

                    if (val.Length == 32)
                        torrentHashValue = new InfoHash(HashValue.FromBase64String(val), torrentHashValue.sha256);
                    else if (val.Length == 40)
                        torrentHashValue = new InfoHash(HashValue.FromHexString(val), torrentHashValue.sha256);
                    else
                        throw new FormatException("sha1哈希值不是base64或者16进制格式");
                    break;

                case "btmh:":
                    if (!torrentHashValue.sha256.IsEmpty)
                        throw new FormatException("不允许有两个sha256磁力哈希值");

                    if (val.Length != 64)
                        throw new FormatException("无效的 SHA256 十六进制字符串，长度必须为 64 个字符。");

                    byte[] sha256Bytes = new byte[val.Length / 2];
                    for (int j = 0; j < sha256Bytes.Length; j++)
                    {
                        sha256Bytes[j] = Convert.ToByte(val.Substring(j * 2, 2), 16);
                    }

                    if (sha256Bytes[0] != 0x12 || sha256Bytes[1] != 0x20)
                        throw new ArgumentException("目前V2多哈希中仅支持sha-256哈希。");

                    torrentHashValue = new InfoHash(torrentHashValue.sha1, new HashValue(sha256Bytes));
                    break;
            }

            return torrentHashValue;
        }
    }
}
