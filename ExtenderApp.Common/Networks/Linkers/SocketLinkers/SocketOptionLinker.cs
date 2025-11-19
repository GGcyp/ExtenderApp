using System.Net.Sockets;
using System.Runtime.Versioning;
using ExtenderApp.Abstract;

namespace ExtenderApp.Common.Networks
{
    /// <summary>
    /// 可设置和获取套接字选项的套接字链路器基类。
    /// </summary>
    public abstract class SocketOptionLinker : SocketLinker, ILinkOption
    {
        protected SocketOptionLinker(Socket socket) : base(socket)
        {
        }

        public int GetRawSocketOption(int optionLevel, int optionName, Span<byte> optionValue)
        {
            SendSlim.Wait();
            ReceiveSlim.Wait();
            try
            {
                return Socket.GetRawSocketOption(optionLevel, optionName, optionValue);
            }
            finally
            {
                SendSlim.Release();
                ReceiveSlim.Release();
            }
        }

        public object? GetSocketOption(SocketOptionLevel optionLevel, SocketOptionName optionName)
        {
            SendSlim.Wait();
            ReceiveSlim.Wait();
            try
            {
                return Socket.GetSocketOption(optionLevel, optionName);
            }
            finally
            {
                SendSlim.Release();
                ReceiveSlim.Release();
            }
        }

        public void GetSocketOption(SocketOptionLevel optionLevel, SocketOptionName optionName, byte[] optionValue)
        {
            SendSlim.Wait();
            ReceiveSlim.Wait();
            try
            {
                Socket.GetSocketOption(optionLevel, optionName, optionValue);
            }
            finally
            {
                SendSlim.Release();
                ReceiveSlim.Release();
            }
        }

        public byte[] GetSocketOption(SocketOptionLevel optionLevel, SocketOptionName optionName, int optionLength)
        {
            SendSlim.Wait();
            ReceiveSlim.Wait();
            try
            {
                return Socket.GetSocketOption(optionLevel, optionName, optionLength);
            }
            finally
            {
                SendSlim.Release();
                ReceiveSlim.Release();
            }
        }

        public int IOControl(int ioControlCode, byte[]? optionInValue, byte[]? optionOutValue)
        {
            SendSlim.Wait();
            ReceiveSlim.Wait();
            try
            {
                return Socket.IOControl(ioControlCode, optionInValue, optionOutValue);
            }
            finally
            {
                SendSlim.Release();
                ReceiveSlim.Release();
            }
        }

        public int IOControl(IOControlCode ioControlCode, byte[]? optionInValue, byte[]? optionOutValue)
        {
            SendSlim.Wait();
            ReceiveSlim.Wait();
            try
            {
                return Socket.IOControl((int)ioControlCode, optionInValue, optionOutValue);
            }
            finally
            {
                SendSlim.Release();
                ReceiveSlim.Release();
            }
        }

        [SupportedOSPlatform("windows")]
        public void SetIPProtectionLevel(IPProtectionLevel level)
        {
            SendSlim.Wait();
            ReceiveSlim.Wait();
            try
            {
                Socket.SetIPProtectionLevel(level);
            }
            finally
            {
                SendSlim.Release();
                ReceiveSlim.Release();
            }
        }

        public void SetRawSocketOption(int optionLevel, int optionName, ReadOnlySpan<byte> optionValue)
        {
            SendSlim.Wait();
            ReceiveSlim.Wait();
            try
            {
                Socket.SetRawSocketOption(optionLevel, optionName, optionValue);
            }
            finally
            {
                SendSlim.Release();
                ReceiveSlim.Release();
            }
        }

        public void SetSocketOption(SocketOptionLevel optionLevel, SocketOptionName optionName, int optionValue)
        {
            SendSlim.Wait();
            ReceiveSlim.Wait();
            try
            {
                Socket.SetSocketOption(optionLevel, optionName, optionValue);
            }
            finally
            {
                SendSlim.Release();
                ReceiveSlim.Release();
            }
        }

        public void SetSocketOption(SocketOptionLevel optionLevel, SocketOptionName optionName, byte[] optionValue)
        {
            SendSlim.Wait();
            ReceiveSlim.Wait();
            try
            {
                Socket.SetSocketOption(optionLevel, optionName, optionValue);
            }
            finally
            {
                SendSlim.Release();
                ReceiveSlim.Release();
            }
        }

        public void SetSocketOption(SocketOptionLevel optionLevel, SocketOptionName optionName, bool optionValue)
        {
            SendSlim.Wait();
            ReceiveSlim.Wait();
            try
            {
                Socket.SetSocketOption(optionLevel, optionName, optionValue);
            }
            finally
            {
                SendSlim.Release();
                ReceiveSlim.Release();
            }
        }

        public void SetSocketOption(SocketOptionLevel optionLevel, SocketOptionName optionName, object optionValue)
        {
            SendSlim.Wait();
            ReceiveSlim.Wait();
            try
            {
                Socket.SetSocketOption(optionLevel, optionName, optionValue);
            }
            finally
            {
                SendSlim.Release();
                ReceiveSlim.Release();
            }
        }
    }
}