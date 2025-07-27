using System.Buffers;
using System.Buffers.Binary;
using System.IO;
using ExtenderApp.Data;

namespace ExtenderApp.Common.Torrent
{
    /// <summary>
    /// BTMessageEncoder 类用于对 BTMessage 对象进行编码和解码。
    /// </summary>
    public class BTMessageEncoder
    {
        /// <summary>
        /// 整数类型的长度（字节数）。
        /// </summary>
        private readonly int _intLength;

        public BTMessageEncoder()
        {
            _intLength = sizeof(int);
        }

        /// <summary>
        /// 将 BTMessage 对象编码为二进制数据。
        /// </summary>
        /// <param name="writer">用于写入二进制数据的 ExtenderBinaryWriter 对象。</param>
        /// <param name="message">要编码的 BTMessage 对象。</param>
        public void Encode(ref ExtenderBinaryWriter writer, BTMessage message)
        {
            WriteInt(ref writer, message.LengthPrefix);

            var span = writer.GetSpan(1);
            span[0] = (byte)message.Id;
            writer.Advance(1);

            if (message.PieceIndex != -1)
                WriteInt(ref writer, message.PieceIndex);
            if (message.Begin != -1)
                WriteInt(ref writer, message.Begin);
            if (message.Length > 0)
                WriteInt(ref writer, message.Length);
            if (message.Data != null)
                writer.Write(message.Data);
        }

        /// <summary>
        /// 从二进制数据中解码出 BTMessage 对象。
        /// </summary>
        /// <param name="reader">用于读取二进制数据的 ExtenderBinaryReader 对象。</param>
        /// <returns>解码后的 BTMessage 对象。</returns>
        public BTMessage Decode(ref ExtenderBinaryReader reader)
        {
            if (reader.Remaining < 4)
                throw new InvalidDataException("消息长度不足");
            int length = BinaryPrimitives.ReadInt32BigEndian(reader.UnreadSpan);
            reader.Advance(_intLength);

            if (length == 0)
                return new BTMessage(BTMessageType.KeepAlive);
            if (reader.Remaining < 1)
                throw new InvalidDataException("消息ID缺失");

            byte messageId = reader.UnreadSpan[0];
            reader.Advance(1);

            switch ((BTMessageType)messageId)
            {
                case BTMessageType.Choke:
                case BTMessageType.Unchoke:
                case BTMessageType.Interested:
                case BTMessageType.NotInterested:
                    return new BTMessage((BTMessageType)messageId);

                case BTMessageType.Have:
                    return HaveMessage(reader.UnreadSpan);

                case BTMessageType.BitField:
                    return BitFieldMessage(reader.UnreadSpan, length - 1);
                case BTMessageType.Request:
                    return RequestMessage(reader.UnreadSpan);
                case BTMessageType.Piece:
                    return PieceMessage(reader.UnreadSpan);
                case BTMessageType.Cancel:
                    return CancelMessage(reader.UnreadSpan);
                case BTMessageType.Port:
                    return PortMessage(reader.UnreadSpan);
                default:
                    return UnknownMessage(reader.UnreadSpan);
            }
        }

        /// <summary>
        /// 将整数写入 ExtenderBinaryWriter 对象。
        /// </summary>
        /// <param name="writer">用于写入二进制数据的 ExtenderBinaryWriter 对象。</param>
        /// <param name="value">要写入的整数。</param>
        private void WriteInt(ref ExtenderBinaryWriter writer, int value)
        {
            var span = writer.GetSpan(4);
            BinaryPrimitives.WriteInt32BigEndian(span, value);
            writer.Advance(4);
        }

        /// <summary>
        /// 从二进制数据中解码出 Have 类型的 BTMessage 对象。
        /// </summary>
        /// <param name="bytes">包含二进制数据的字节数组。</param>
        /// <returns>解码后的 Have 类型的 BTMessage 对象。</returns>
        private BTMessage HaveMessage(ReadOnlySpan<byte> bytes)
        {
            int pieceIndex = BinaryPrimitives.ReadInt32BigEndian(bytes);


            int lengthPrefix = 1 + _intLength;
            return new BTMessage(BTMessageType.Have, lengthPrefix, pieceIndex);
        }

        /// <summary>
        /// 从二进制数据中解码出 BitField 类型的 BTMessage 对象。
        /// </summary>
        /// <param name="bytes">包含二进制数据的字节数组。</param>
        /// <param name="length">BitField 数据的长度。</param>
        /// <returns>解码后的 BitField 类型的 BTMessage 对象。</returns>
        private BTMessage BitFieldMessage(ReadOnlySpan<byte> bytes, int length)
        {
            if (bytes.Length < length)
                throw new InvalidDataException("BitField消息数据不足");

            var byteArray = ArrayPool<byte>.Shared.Rent(length);
            bytes.CopyTo(byteArray);
            int lengthPrefix = 1 + _intLength + length;
            return new BTMessage(BTMessageType.BitField, lengthPrefix, length: length, data: byteArray);
        }

        /// <summary>
        /// 从二进制数据中解码出 Request 类型的 BTMessage 对象。
        /// </summary>
        /// <param name="bytes">包含二进制数据的字节数组。</param>
        /// <returns>解码后的 Request 类型的 BTMessage 对象。</returns>
        private BTMessage RequestMessage(ReadOnlySpan<byte> bytes)
        {
            if (bytes.Length < 12)
                throw new InvalidDataException("Request消息数据不足");

            int pieceIndex = BinaryPrimitives.ReadInt32BigEndian(bytes);
            int begin = BinaryPrimitives.ReadInt32BigEndian(bytes.Slice(4));
            int length = BinaryPrimitives.ReadInt32BigEndian(bytes.Slice(8));
            int lengthPrefix = 1 + _intLength * 3;
            return new BTMessage(BTMessageType.Request, lengthPrefix, pieceIndex, begin, length);
        }

        /// <summary>
        /// 从二进制数据中解码出 Piece 类型的 BTMessage 对象。
        /// </summary>
        /// <param name="bytes">包含二进制数据的字节数组。</param>
        /// <returns>解码后的 Piece 类型的 BTMessage 对象。</returns>
        private BTMessage PieceMessage(ReadOnlySpan<byte> bytes)
        {
            if (bytes.Length < 8)
                throw new InvalidDataException("Piece消息头部数据不足");

            int pieceIndex = BinaryPrimitives.ReadInt32BigEndian(bytes);
            int begin = BinaryPrimitives.ReadInt32BigEndian(bytes.Slice(4));

            int length = bytes.Length - 8;
            var block = ArrayPool<byte>.Shared.Rent(length);
            bytes.Slice(8).CopyTo(block);

            int lengthPrefix = 1 + _intLength * 2 + length;
            return new BTMessage(BTMessageType.Piece, lengthPrefix, pieceIndex, begin, length, block);
        }

        /// <summary>
        /// 从二进制数据中解码出 Cancel 类型的 BTMessage 对象。
        /// </summary>
        /// <param name="bytes">包含二进制数据的字节数组。</param>
        /// <returns>解码后的 Cancel 类型的 BTMessage 对象。</returns>
        private BTMessage CancelMessage(ReadOnlySpan<byte> bytes)
        {
            if (bytes.Length < 12)
                throw new InvalidDataException("Cancel消息数据不足");

            int pieceIndex = BinaryPrimitives.ReadInt32BigEndian(bytes);
            int begin = BinaryPrimitives.ReadInt32BigEndian(bytes.Slice(4));
            int length = BinaryPrimitives.ReadInt32BigEndian(bytes.Slice(8));
            int lengthPrefix = 1 + _intLength * 3;

            return new BTMessage(BTMessageType.Cancel, lengthPrefix, pieceIndex, begin, length);
        }

        /// <summary>
        /// 从二进制数据中解码出 Port 类型的 BTMessage 对象。
        /// </summary>
        /// <param name="bytes">包含二进制数据的字节数组。</param>
        /// <returns>解码后的 Port 类型的 BTMessage 对象。</returns>
        private BTMessage PortMessage(ReadOnlySpan<byte> bytes)
        {
            if (bytes.Length < 2)
                throw new InvalidDataException("Port消息数据不足");

            ushort port = BinaryPrimitives.ReadUInt16BigEndian(bytes);
            int lengthPrefix = 1 + sizeof(ushort);


            return new BTMessage(BTMessageType.Port, lengthPrefix, port: port);
        }

        /// <summary>
        /// 从二进制数据中解码出 Unknown 类型的 BTMessage 对象。
        /// </summary>
        /// <param name="bytes">包含二进制数据的字节数组。</param>
        /// <returns>解码后的 Unknown 类型的 BTMessage 对象。</returns>
        private BTMessage UnknownMessage(ReadOnlySpan<byte> bytes)
        {
            int length = bytes.Length;
            var byteArray = ArrayPool<byte>.Shared.Rent(length);
            bytes.CopyTo(byteArray);
            int lengthPrefix = 1 + length;

            return new BTMessage(BTMessageType.Unknown, lengthPrefix, length: length, data: byteArray);
        }
    }
}
