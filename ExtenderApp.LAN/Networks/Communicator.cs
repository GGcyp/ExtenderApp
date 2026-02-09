using System.Net.NetworkInformation;
using ExtenderApp.Contracts;
using PacketDotNet;
using SharpPcap;
using System;
using Microsoft.Extensions.Logging;

namespace ExtenderApp.LAN
{
    /// <summary>
    /// 提供一个通用的、抽象的基类，用于处理特定类型数据包的发送和接收。
    /// 此类封装了 SharpPcap 的设备初始化、数据包捕获和资源释放的通用逻辑。
    /// </summary>
    /// <typeparam name="T">要处理的特定数据包类型，必须继承自 PacketDotNet.Packet。</typeparam>
    public abstract class Communicator<T> : DisposableObject
        where T : Packet
    {
        /// <summary>
        /// 用于记录日志的记录器实例。
        /// </summary>
        private readonly ILogger _logger;

        /// <summary>
        /// 用于数据包捕获和发送的底层网络设备。
        /// </summary>
        protected ILiveDevice _device;

        private EthernetPacket ethernetPacket;

        public virtual PhysicalAddress SourceHardwareAddress
        {
            get => ethernetPacket.SourceHardwareAddress;
            set => ethernetPacket.SourceHardwareAddress = value;
        }

        public virtual PhysicalAddress DestinationHardwareAddress
        {
            get => ethernetPacket.DestinationHardwareAddress;
            set => ethernetPacket.DestinationHardwareAddress = value;
        }

        /// <summary>
        /// 获取当前网络设备的物理（MAC）地址。
        /// </summary>
        protected PhysicalAddress? LocalMacAddress => _device.MacAddress;

        /// <summary>
        /// 获取一个由派生类定义的捕获过滤器字符串（BPF 语法）。
        /// </summary>
        protected abstract string Filter { get; }

        protected abstract EthernetType EthernetType { get; }

        protected T CommunicatorPacket { get; }

        /// <summary>
        /// 初始化 Communicator 的新实例。
        /// </summary>
        /// <param name="device">要用于通信的网络设备。</param>
        /// <param name="logger">用于记录日志的记录器实例。</param>
        protected Communicator(ILiveDevice device, ILogger logger)
        {
            _device = device ?? throw new ArgumentNullException(nameof(device));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _device.Open(DeviceModes.Promiscuous);
            _device.Filter = Filter;
            _device.OnPacketArrival += OnPacketArrival;
            _device.StartCapture();
            ethernetPacket = CreateEthernetPacket();
        }

        protected virtual EthernetPacket CreateEthernetPacket()
        {
            return new EthernetPacket(LocalMacAddress, PhysicalAddress.None, EthernetType);
        }

        /// <summary>
        /// SharpPcap 设备捕获到数据包时的内部事件处理程序。
        /// </summary>
        private void OnPacketArrival(object sender, PacketCapture e)
        {
            try
            {
                var packet = Packet.ParsePacket(e.Device.LinkType, e.Data.ToArray());
                var specificPacket = packet.Extract<T>();
                if (specificPacket != null)
                {
                    PacketArrival(specificPacket);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "FromException parsing packet");
            }
        }

        protected void SendPacket()
        {
            SendPacket(CommunicatorPacket);
        }

        /// <summary>
        /// 通过网络设备发送一个数据包。
        /// </summary>
        /// <param name="packet">要发送的数据包。</param>
        public void SendPacket(T packet)
        {
            ThrowIfDisposed();
            ethernetPacket.PayloadPacket = packet;
            _device.SendPacket(ethernetPacket);
        }

        #region 子类生成

        /// <summary>
        /// 当派生类需要处理已到达的特定类型数据包时调用。
        /// </summary>
        /// <param name="packet">已捕获并解析的特定类型的数据包。</param>
        protected abstract void PacketArrival(T packet);

        protected abstract T CreateCommunicatorPacket();

        #endregion 子类生成

        protected override void DisposeManagedResources()
        {
            // 停止捕获并注销事件
            if (_device != null)
            {
                _device.OnPacketArrival -= OnPacketArrival;
                if (_device.Started)
                {
                    _device.StopCapture();
                }
                _device.Close();
            }
        }
    }
}