using System.Buffers;
using System.Net;
using System.Net.Sockets;
using ExtenderApp.Data;


namespace ExtenderApp.Common.Networks.LinkOperates
{
    /// <summary>
    /// 表示一个并发操作的类，用于处理链路操作。
    /// </summary>
    /// <typeparam name="LinkOperateData">链路操作的数据类型。</typeparam>
    public class LinkerOperation : ConcurrentOperation<LinkOperateData>
    {
        /// <summary>
        /// 发送流量的回调函数。
        /// </summary>
        private Action<int>? sendTrafficCallback;

        /// <summary>
        /// 要发送的字节数组。
        /// </summary>
        private byte[] sendBytes;

        /// <summary>
        /// 发送字节数组的回调函数。
        /// </summary>
        private Action<byte[]>? sendBytesCallbcak;

        /// <summary>
        /// 只读字节序列。
        /// </summary>
        private ReadOnlySequence<byte> readOnlySequence;

        /// <summary>
        /// 字节序列池的租赁。
        /// </summary>
        private SequencePool<byte>.Rental rental;

        private EndPoint? endPoint;

        public LinkerOperation()
        {
            sendBytes = Array.Empty<byte>();
        }

        public void Set(byte[] bytes, int offset, int length, Action<int>? sendTrafficCallback, Action<byte[]>? sendBytesCallbcak = null, EndPoint? end = null)
        {
            readOnlySequence = new ReadOnlySequence<byte>(bytes, offset, length);
            this.sendTrafficCallback = sendTrafficCallback;
            this.sendBytesCallbcak = sendBytesCallbcak;
            sendBytes = bytes;
            this.endPoint = end;
        }

        public void Set(Memory<byte> memory, Action<int>? sendTrafficCallback, EndPoint? end = null)
        {
            readOnlySequence = new ReadOnlySequence<byte>(memory);
            this.sendTrafficCallback = sendTrafficCallback;
            endPoint = end;
        }

        public void Set(ReadOnlySequence<byte> readOnlyMemories, Action<int>? sendTrafficCallback, EndPoint? end = null)
        {
            this.readOnlySequence = readOnlyMemories;
            this.sendTrafficCallback = sendTrafficCallback;
            sendBytesCallbcak = null;
            sendBytes = Array.Empty<byte>();
            endPoint = end;
        }

        public void Set(ExtenderBinaryWriter writer, Action<int>? sendTrafficCallback, EndPoint? end = null)
        {
            rental = writer.Rental;
            readOnlySequence = rental.Value.AsReadOnlySequence;
            this.sendTrafficCallback = sendTrafficCallback;
            sendBytesCallbcak = null;
            sendBytes = Array.Empty<byte>();
            endPoint = end;
        }

        public override void Execute(LinkOperateData item)
        {
            var socket = item.Socket;
            var sendTrafficLength = 0;
            if (endPoint == null)
            {
                sendTrafficLength = Send(socket);
            }
            else
            {
                sendTrafficLength = SendTo(socket);
            }

            sendTrafficCallback?.Invoke(sendTrafficLength);
            sendBytesCallbcak?.Invoke(sendBytes);
            rental.Dispose();
            Release();
        }

        private int Send(Socket socket)
        {
            var sendTrafficLength = 0;
            foreach (ReadOnlyMemory<byte> meory in readOnlySequence)
            {
                sendTrafficLength += socket.Send(meory.Span);
            }
            return sendTrafficLength;
        }

        private int SendTo(Socket socket)
        {
            var sendTrafficLength = 0;
            foreach (ReadOnlyMemory<byte> meory in readOnlySequence)
            {
                sendTrafficLength += socket.SendTo(meory.Span, endPoint!);
            }
            return sendTrafficLength;
        }

        public override bool TryReset()
        {
            sendBytes = null;
            sendBytesCallbcak = null;
            sendTrafficCallback = null;
            rental = default;
            readOnlySequence = ReadOnlySequence<byte>.Empty;
            return true;
        }

        protected override void Dispose(bool disposing)
        {
            TryReset();
            base.Dispose(disposing);
        }
    }
}
