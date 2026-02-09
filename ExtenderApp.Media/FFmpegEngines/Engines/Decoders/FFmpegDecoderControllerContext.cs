using ExtenderApp.Common;
using ExtenderApp.Contracts;

namespace ExtenderApp.FFmpegEngines.Decoders
{
    /// <summary>
    /// FFmpeg 解码器控制器上下文。
    /// <para>
    /// 该类型的职责是为“控制器/解码器/引擎”之间提供一个共享的运行上下文，主要包含：
    /// <list type="bullet">
    /// <item><description>共享的 <see cref="FFmpegEngine"/> 实例（封装 FFmpeg 原生 API）。</description></item>
    /// <item><description>共享的 <see cref="FFmpegContext"/>（格式上下文、解码器上下文集合等原生资源的聚合）。</description></item>
    /// <item><description>代际（generation）机制：用于 Seek/重建会话时快速丢弃旧包/旧帧，避免输出跳转前的数据。</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// 代际机制说明：
    /// <list type="bullet">
    /// <item><description>发生 Seek（或任何需要“丢弃旧数据”的操作）时，调用 <see cref="IncrementGeneration"/>。</description></item>
    /// <item><description>生产者在写入 packet/frame 时将当前代际写入其 <c>Generation</c> 字段。</description></item>
    /// <item><description>消费者在读取时使用 <see cref="GetCurrentGeneration"/> 对比，不一致则视为旧代数据并丢弃/回收资源。</description></item>
    /// </list>
    /// </para>
    /// </summary>
    public class FFmpegDecoderControllerContext : DisposableObject
    {
        /// <summary>
        /// 共享的 FFmpeg 引擎实例。
        /// <para>
        /// 用于执行 send/receive、seek、flush、资源池回收等底层调用。
        /// 通常由控制器创建并在其生命周期内保持有效。
        /// </para>
        /// </summary>
        public FFmpegEngine Engine { get; }

        /// <summary>
        /// 当前代际号。
        /// <para>
        /// 每次 Seek（或任何需要强制丢弃旧包/旧帧的操作）时递增。
        /// 生产者写入 <c>FFmpegPacket.Generation</c>/<c>FFmpegFrame.Generation</c>，消费者读取后与 <see cref="GetCurrentGeneration"/> 比对，
        /// 不一致则直接丢弃并回收资源。
        /// </para>
        /// <para>
        /// 并发与可见性：
        /// <list type="bullet">
        /// <item><description><c>volatile</c>：保证跨线程读取时可见性（读不会被缓存）。</description></item>
        /// <item><description><see cref="Interlocked.Increment(ref int)"/>：保证递增操作原子性。</description></item>
        /// </list>
        /// </para>
        /// </summary>
        private volatile int generation;

        private volatile int completed;

        /// <summary>
        /// 当前播放会话的 FFmpeg 上下文。
        /// <para>
        /// 通常包含：
        /// <list type="bullet">
        /// <item><description>格式上下文（<c>AVFormatContext</c>）</description></item>
        /// <item><description>字典/选项（<c>AVDictionary</c>）</description></item>
        /// <item><description>解码器上下文集合（多个流对应的 <c>AVCodecContext</c>）</description></item>
        /// </list>
        /// </para>
        /// <para>
        /// 重要：该字段包含原生资源；使用期间必须确保不会在其他线程仍在访问时被释放。
        /// </para>
        /// </summary>
        public FFmpegContext Context;

        /// <summary>
        /// 媒体文件的元数据信息。
        /// </summary>
        public FFmpegInfo Info => Context.Info;

        /// <summary>
        /// 当前播放会话的取消令牌源。
        /// </summary>
        public CancellationTokenSource AllSource { get; }

        /// <summary>
        /// 构造控制器上下文。
        /// </summary>
        /// <param name="engine">FFmpeg 引擎（共享）。</param>
        /// <param name="context">FFmpeg 会话上下文（包含原生指针资源）。</param>
        public FFmpegDecoderControllerContext(FFmpegEngine engine, FFmpegContext context)
        {
            Engine = engine;
            Context = context;
            AllSource = new();

            generation = 0;
        }

        /// <summary>
        /// 获取当前代际号。
        /// <para>
        /// 解码循环/渲染循环中调用以判定当前读取/写入的数据是否仍属于“有效播放会话”。
        /// </para>
        /// </summary>
        /// <returns>当前的代际编号（int）。</returns>
        public int GetCurrentGeneration()
        {
            return generation;
        }

        /// <summary>
        /// 递增代际号。
        /// <para>
        /// 通常在 Seek/重建解码会话前调用，使旧包/旧帧在消费者侧被快速识别并丢弃。
        /// </para>
        /// <para>
        /// 注意：代际递增本身只负责“逻辑丢弃”，不等价于 flush/free；
        /// 如果同时对 <see cref="Context"/> 中的原生指针做 flush/free，需要额外的同步保证（避免与解码线程并发访问）。
        /// </para>
        /// </summary>
        internal void IncrementGeneration()
        {
            Interlocked.Increment(ref generation);
        }

        /// <summary>
        /// 判断当前播放会话是否已被标记为完成。
        /// <para>返回基于内部原子标志的状态：非零表示已完成。</para>
        /// </summary>
        /// <returns>若会话已完成返回 <c>true</c>；否则返回 <c>false</c>。</returns>
        public bool GetIsCompleted()
        {
            return completed != 0;
        }

        /// <summary>
        /// 设置或清除“已完成”标志。
        /// <para>写入操作为简单的整型赋值（0/1），用于在多线程场景下通知会话完成状态。</para>
        /// </summary>
        /// <param name="value">若为 <c>true</c> 则标记为已完成；若为 <c>false</c> 则清除完成标志。</param>
        public void SetIsCompleted(bool value)
        {
            completed = value ? 1 : 0;
        }

        /// <summary>
        /// 释放托管资源：取消并释放与会话生命周期相关的取消令牌源。
        /// </summary>
        protected override void DisposeManagedResources()
        {
            AllSource.Cancel();
            AllSource.DisposeSafe();
        }

        /// <summary>
        /// 释放非托管资源。
        /// <para>
        /// 将 <see cref="Context"/> 中聚合的 FFmpeg 原生资源统一交由 <see cref="Engine"/> 释放。
        /// </para>
        /// </summary>
        protected override void DisposeUnmanagedResources()
        {
            Context.DisposeSafe();
        }
    }
}