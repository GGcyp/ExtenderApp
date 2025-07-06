using System.Text;
using ExtenderApp.Data;

namespace ExtenderApp.Torrent
{
    /// <summary>
    /// BitTorrent 握手消息
    /// </summary>
    public struct Handshake
    {
        /// <summary>
        /// 表示BitTorrent协议的字符串。
        /// </summary>
        private const string ProtocolString = "BitTorrent protocol";
        /// <summary>
        /// 将ProtocolString转换为ASCII编码的字节数组。
        /// </summary>
        private static readonly byte[] ProtocolBytes = Encoding.ASCII.GetBytes(ProtocolString);
        /// <summary>
        /// 保留字节数组，长度为8。
        /// </summary>
        private static readonly byte[] ReservedBytes = new byte[8];

        public InfoHash Hash { get; }
        public PeerId PeerId { get; }

        public Handshake(InfoHash infoHash, PeerId id)
        {
            if (infoHash.IsEmpty)
                throw new ArgumentException("InfoHash不能为空", nameof(infoHash));
            if (id.IsEmpty)
                throw new ArgumentException("PeerId不能为空", nameof(id));

            Hash = infoHash;
            PeerId = id;
        }

        public void Encode(ExtenderBinaryWriter writer)
        {
            var span = writer.GetSpan(1);
            span[0] = (byte)ProtocolBytes.Length;
            writer.Advance(1);
            writer.Write(ProtocolBytes);
            writer.Write(ReservedBytes);
            Hash.CopyTo(ref writer);
            PeerId.CopyTo(ref writer);
        }

        //public static Handshake Decode(ReadOnlySpan<byte> buffer)
        //{
        //    if (buffer.Length < 68)
        //        throw new InvalidDataException("握手消息长度不足");

        //    int pstrlen = buffer[0];
        //    if (pstrlen != ProtocolBytes.Length)
        //        throw new InvalidDataException("不支持的协议版本");

        //    var pstr = buffer.Slice(1, pstrlen);
        //    if (!pstr.SequenceEqual(ProtocolBytes))
        //        throw new InvalidDataException("协议标识不匹配");

        //    var infoHash = buffer.Slice(28, 20).ToArray();
        //    var peerId = buffer.Slice(48, 20).ToArray();

        //    return new Handshake(infoHash, peerId);
        //}
    }
}
