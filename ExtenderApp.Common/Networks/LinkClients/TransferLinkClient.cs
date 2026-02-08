using System.Buffers;
using ExtenderApp.Abstract;
using ExtenderApp.Data;

namespace ExtenderApp.Common.Networks.LinkClients
{
    /// <summary>
    /// 支持插件与格式化器的链接客户端发送器基类。
    /// </summary>
    /// <typeparam name="TLinker">指定类型连接器</typeparam>
    public class TransferLinkClient<TLinker> : LinkClient<TLinker, ITransferLinkClient>
        where TLinker : ILinker
    {
        /// <summary>
        /// 保护对接收相关状态的并发访问锁：用于在 StartReceive/StopReceive 中同步检查与切换 <see cref="_receiveCts"/> 与 <see cref="_receiveTask"/>。 注意：StopReceive 中可能在锁外等待任务完成或在持有锁的情况下启动取消，但应尽量保持锁粒度短以避免阻塞接收路径。
        /// </summary>
        private readonly object _receiveLock = new();

        /// <summary>
        /// 用于控制接收循环的取消令牌源。 在 StartReceive 创建，在 StopReceive 取消并释放；为 null 表示接收循环未在运行或已被释放。
        /// </summary>
        private CancellationTokenSource? _receiveCts;

        /// <summary>
        /// 表示当前正在运行的接收循环任务（由 StartReceive 启动）。 可能为 null（未启动或已完成/已释放）。对其检查/赋值需在 <see cref="_receiveLock"/> 保护下进行以避免竞态。
        /// </summary>
        private Task? _receiveTask;

        private readonly ManualResetEventSlim _waitEvent;


        public TransferLinkClient(TLinker linker) : base(linker)
        {
            _receiveLock = new();
            _waitEvent = new(true);
            if (Connected)
            {
                StartReceive();
            }
        }
    }
}