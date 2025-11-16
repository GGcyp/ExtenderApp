using System.Net;
using System.Net.Sockets;
using System.Runtime.Versioning;
using ExtenderApp.Abstract;
using ExtenderApp.Data;

namespace ExtenderApp.Common.Networks.Linkers.CustomizeLinkers
{
    /// <summary>
    /// 自定义链接器实现。
    /// </summary>
    /// <typeparam name="T">需要实现自定一的基础连接器</typeparam>
    internal class CustomizeLinker<T> : Linker, ICustomizeLinker
        where T : ILinker
    {
        private readonly T _linker;
        private readonly Socket _socket;

        public override bool Connected => _linker.Connected;

        public override EndPoint? LocalEndPoint => _linker.LocalEndPoint;

        public override EndPoint? RemoteEndPoint => _linker.LocalEndPoint;

        public override ProtocolType ProtocolType => _linker.ProtocolType;

        public override SocketType SocketType => _linker.SocketType;

        public override AddressFamily AddressFamily => _linker.AddressFamily;

        public CustomizeLinker(T linker)
        {
            _linker = linker;
            _socket = linker.GetSocket() ?? throw new ArgumentNullException(nameof(linker));
        }

        public int GetRawSocketOption(int optionLevel, int optionName, Span<byte> optionValue)
        {
            return _socket.GetRawSocketOption(optionLevel, optionName, optionValue);
        }

        public object? GetSocketOption(SocketOptionLevel optionLevel, SocketOptionName optionName)
        {
            return _socket.GetSocketOption(optionLevel, optionName);
        }

        public void GetSocketOption(SocketOptionLevel optionLevel, SocketOptionName optionName, byte[] optionValue)
        {
            _socket.GetSocketOption(optionLevel, optionName, optionValue);
        }

        public byte[] GetSocketOption(SocketOptionLevel optionLevel, SocketOptionName optionName, int optionLength)
        {
            return _socket.GetSocketOption(optionLevel, optionName, optionLength);
        }

        public int IOControl(int ioControlCode, byte[]? optionInValue, byte[]? optionOutValue)
        {
            return _socket.IOControl(ioControlCode, optionInValue, optionOutValue);
        }

        public int IOControl(IOControlCode ioControlCode, byte[]? optionInValue, byte[]? optionOutValue)
        {
            return _socket.IOControl(ioControlCode, optionInValue, optionOutValue);
        }

        [SupportedOSPlatform("windows")]
        public void SetIPProtectionLevel(IPProtectionLevel level)
        {
            _socket.SetIPProtectionLevel(level);
        }

        public void SetRawSocketOption(int optionLevel, int optionName, ReadOnlySpan<byte> optionValue)
        {
            _socket.SetRawSocketOption(optionLevel, optionName, optionValue);
        }

        public void SetSocketOption(SocketOptionLevel optionLevel, SocketOptionName optionName, int optionValue)
        {
            _socket.SetSocketOption(optionLevel, optionName, optionValue);
        }

        public void SetSocketOption(SocketOptionLevel optionLevel, SocketOptionName optionName, byte[] optionValue)
        {
            _socket.SetSocketOption(optionLevel, optionName, optionValue);
        }

        public void SetSocketOption(SocketOptionLevel optionLevel, SocketOptionName optionName, bool optionValue)
        {
            _socket.SetSocketOption(optionLevel, optionName, optionValue);
        }

        public void SetSocketOption(SocketOptionLevel optionLevel, SocketOptionName optionName, object optionValue)
        {
            _socket.SetSocketOption(optionLevel, optionName, optionValue);
        }

        protected override ValueTask ExecuteConnectAsync(EndPoint remoteEndPoint, CancellationToken token)
        {
            return _linker.ConnectAsync(remoteEndPoint, token);
        }

        protected override ValueTask ExecuteDisconnectAsync(CancellationToken token)
        {
            return _linker.DisconnectAsync(token);
        }

        protected override ValueTask<SocketOperationResult> ExecuteReceiveAsync(Memory<byte> memory, CancellationToken token)
        {
            return _linker.ReceiveAsync(memory, token);
        }

        protected override ValueTask<SocketOperationResult> ExecuteSendAsync(Memory<byte> memory, CancellationToken token)
        {
            return _linker.SendAsync(memory, token);
        }
    }
}