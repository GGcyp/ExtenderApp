
using System.Buffers;

namespace ExtenderApp.Torrent
{
    public struct BTMessage : IDisposable
    {
        public int LengthPrefix { get; }
        public BTMessageType Id { get; }

        #region Cancel

        public int PieceIndex { get; }
        public int Begin { get; }
        public int Length { get; }

        #endregion

        #region Port

        public ushort Port { get; }

        #endregion

        public byte[]? Data { get; }

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

        public void Dispose()
        {
            if (Data == null) return;
            ArrayPool<byte>.Shared.Return(Data);
        }
    }
}
