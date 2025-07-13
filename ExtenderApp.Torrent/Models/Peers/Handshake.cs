using System.Runtime.Serialization;
using System.Text;
using ExtenderApp.Data;

namespace ExtenderApp.Torrent
{
    /// <summary>
    /// BitTorrent 握手消息
    /// </summary>
    public static class Handshake
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
        /// <summary>
        /// 握手消息的总长度，包含协议标识、保留字节、InfoHash和PeerId等信息。
        /// </summary>
        public const int HandshakeLength = 68;

        /// <summary>
        /// 将握手协议信息编码到ExtenderBinaryWriter中。
        /// </summary>
        /// <param name="writer">ExtenderBinaryWriter对象，用于写入编码后的握手协议信息。</param>
        /// <param name="infoHash">InfoHash对象，表示种子的唯一标识符。</param>
        /// <param name="id">PeerId对象，表示对端的PeerId。</param>
        /// <exception cref="ArgumentException">如果infoHash或id为空，则抛出此异常。</exception>
        public static void Encode(ref ExtenderBinaryWriter writer, InfoHash infoHash, PeerId id)
        {
            if (infoHash.IsEmpty)
                throw new ArgumentException("InfoHash不能为空", nameof(infoHash));
            if (id.IsEmpty)
                throw new ArgumentException("PeerId不能为空", nameof(id));

            var span = writer.GetSpan(1);
            span[0] = (byte)ProtocolBytes.Length;
            writer.Advance(1);
            writer.Write(ProtocolBytes);
            writer.Write(ReservedBytes);
            infoHash.CopyTo(ref writer);
            id.CopyTo(ref writer);
        }

        /// <summary>
        /// 从给定的字节缓冲区中解码握手协议信息。
        /// </summary>
        /// <param name="buffer">包含握手协议信息的字节缓冲区。</param>
        /// <param name="infoHash">输出参数，解码后的InfoHash对象。</param>
        /// <param name="id">输出参数，解码后的PeerId对象。</param>
        /// <exception cref="InvalidDataContractException">如果握手消息长度不足、不支持的协议版本或协议标识不匹配，则抛出此异常。</exception>
        public static void Decode(ReadOnlySpan<byte> buffer, out InfoHash infoHash, out PeerId id)
        {
            if (buffer.Length != HandshakeLength)
                throw new InvalidDataContractException($"握手消息长度应为{HandshakeLength}字节，实际为{buffer.Length}");

            int pstrlen = buffer[0];
            if (pstrlen != ProtocolBytes.Length)
                throw new InvalidDataContractException("不支持的协议版本");

            var pstr = buffer.Slice(1, pstrlen);
            if (!pstr.SequenceEqual(ProtocolBytes))
                throw new InvalidDataContractException("协议标识不匹配");

            infoHash = InfoHash.SHA1InfoHash(buffer.Slice(28, 20).ToArray());
            id = new PeerId(buffer.Slice(48, 20).ToArray());
        }
    }
}
