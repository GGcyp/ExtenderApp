
using System.Buffers;

namespace ExtenderApp.Torrent
{
    /// <summary>
    /// 表示一个BT消息的结构体，实现了IDisposable接口。
    /// </summary>
    public struct BTMessage : IDisposable
    {
        /// <summary>
        /// 获取消息长度前缀。
        /// </summary>
        public int LengthPrefix { get; }

        /// <summary>
        /// 获取消息类型ID。
        /// </summary>
        public BTMessageType Id { get; }

        #region Cancel

        /// <summary>
        /// 获取取消操作的片段索引。
        /// </summary>
        public int PieceIndex { get; }

        /// <summary>
        /// 获取取消操作的开始位置。
        /// </summary>
        public int Begin { get; }

        /// <summary>
        /// 获取取消操作的长度。
        /// </summary>
        public int Length { get; }

        #endregion

        #region Port

        /// <summary>
        /// 获取端口号。
        /// </summary>
        public ushort Port { get; }

        #endregion

        #region Create

        /// <summary>
        /// 创建 Have 消息
        /// </summary>
        public static BTMessage CreateHave(int pieceIndex)
            => new BTMessage(BTMessageType.Have, lengthPrefix: 5, pieceIndex: pieceIndex);

        /// <summary>
        /// 创建 BitField 消息
        /// </summary>
        public static BTMessage CreateBitField(int length, byte[] data)
            => new BTMessage(BTMessageType.BitField, lengthPrefix: 5 + length, length: length, data: data);

        /// <summary>
        /// 创建 Request 消息
        /// </summary>
        public static BTMessage CreateRequest(int pieceIndex, int begin, int length)
            => new BTMessage(BTMessageType.Request, lengthPrefix: 13, pieceIndex: pieceIndex, begin: begin, length: length);

        /// <summary>
        /// 创建 Piece 消息
        /// </summary>
        public static BTMessage CreatePiece(int pieceIndex, int begin, int length, byte[] data)
            => new BTMessage(BTMessageType.Piece, lengthPrefix: 9 + length, pieceIndex: pieceIndex, begin: begin, length: length, data: data);

        /// <summary>
        /// 创建 Cancel 消息
        /// </summary>
        public static BTMessage CreateCancel(int pieceIndex, int begin, int length)
            => new BTMessage(BTMessageType.Cancel, lengthPrefix: 13, pieceIndex: pieceIndex, begin: begin, length: length);

        /// <summary>
        /// 创建 Port 消息
        /// </summary>
        public static BTMessage CreatePort(ushort port)
            => new BTMessage(BTMessageType.Port, lengthPrefix: 3, port: port);

        /// <summary>
        /// 创建 Unknown 消息
        /// </summary>
        public static BTMessage CreateUnknown(int length, byte[] data)
            => new BTMessage(BTMessageType.Unknown, lengthPrefix: 1 + length, length: length, data: data);

        /// <summary>
        /// 创建 KeepAlive 消息
        /// </summary>
        public static BTMessage CreateKeepAlive()
            => new BTMessage(BTMessageType.KeepAlive, lengthPrefix: 0);

        #endregion

        /// <summary>
        /// 获取消息数据。
        /// </summary>
        public byte[]? Data { get; }

        /// <summary>
        /// 初始化BTMessage实例。
        /// </summary>
        /// <param name="messageId">消息类型ID。</param>
        /// <param name="lengthPrefix">消息长度前缀，默认为1。</param>
        /// <param name="pieceIndex">取消操作的片段索引，默认为-1。</param>
        /// <param name="begin">取消操作的开始位置，默认为-1。</param>
        /// <param name="length">取消操作的长度，默认为0。</param>
        /// <param name="data">消息数据，默认为null。</param>
        /// <param name="port">端口号，默认为0。</param>
        public BTMessage(BTMessageType messageId, int lengthPrefix = 1, int pieceIndex = -1, int begin = -1, int length = 0, byte[]? data = null, ushort port = 0)
        {
            LengthPrefix = lengthPrefix;
            Id = messageId;
            Data = data;
            PieceIndex = pieceIndex;
            Begin = begin;
            Length = length;
            Port = port;
        }

        /// <summary>
        /// 释放由<see cref="Data"/>占用的资源。
        /// </summary>
        public void Dispose()
        {
            if (Data == null) return;
            ArrayPool<byte>.Shared.Return(Data);
        }
    }
}
