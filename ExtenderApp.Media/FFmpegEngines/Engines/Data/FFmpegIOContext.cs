using System.Runtime.InteropServices;
using ExtenderApp.Data;
using FFmpeg.AutoGen;

namespace ExtenderApp.FFmpegEngines
{
    /// <summary>
    /// FFmpeg 自定义 IO 上下文封装。
    /// <para>
    /// 用于在 <see cref="AVFormatContext"/> 中绑定自定义 <see cref="AVIOContext"/>（通常由 <c>avio_alloc_context</c> 创建），
    /// 使解复用器通过回调从托管侧提供的数据源（如 <see cref="Stream"/>）读取字节流。
    /// </para>
    /// <para>
    /// 该结构负责维护自定义 IO 所需的三个关键资源：
    /// <list type="bullet">
    /// <item><description><see cref="Context"/>：FFmpeg 侧的 <see cref="AVIOContext"/> 指针（需要在释放时 <c>avio_context_free</c>）。</description></item>
    /// <item><description><see cref="_buffer"/>：IO 缓冲区（通常要求使用 <c>av_malloc</c> 分配；此处由 <see cref="NativeByteMemory"/> 管理）。</description></item>
    /// <item><description><see cref="_handle"/>：用于将托管对象（如 <see cref="Stream"/>）通过 <c>opaque</c> 传入回调的句柄（必须显式 <c>Free</c>）。</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// 注意：<see cref="GCHandle"/> 不会随 GC 自动释放；若未调用 <see cref="Dispose"/>，可能导致句柄泄漏与对象无法回收。
    /// </para>
    /// </summary>
    public struct FFmpegIOContext : IDisposable
    {
        /// <summary>
        /// 关联的 <see cref="FFmpegEngine"/>，用于复用引擎内的统一释放逻辑（如 <see cref="FFmpegEngine.Free(ref NativeIntPtr{AVIOContext})"/>）。
        /// </summary>
        private readonly FFmpegEngine _engine;

        /// <summary>
        /// 托管句柄：通常用于将托管对象（例如 <see cref="Stream"/>）固定为一个可传递给 FFmpeg 的 <c>opaque</c> 指针。
        /// <para>
        /// 回调中一般通过 <c>GCHandle.FromIntPtr((nint)opaque)</c> 找回原始托管对象。
        /// </para>
        /// </summary>
        private readonly GCHandle _handle;

        /// <summary>
        /// AVIOContext 使用的缓冲区。
        /// <para>
        /// 对 FFmpeg 而言，该 buffer 属于 IO 层缓存，用于减少回调次数、提升吞吐量。
        /// </para>
        /// </summary>
        private readonly NativeByteMemory _buffer;

        /// <summary>
        /// FFmpeg 侧的 <see cref="AVIOContext"/> 指针封装。
        /// <para>
        /// 通常会被赋值给 <c>AVFormatContext->pb</c>。
        /// </para>
        /// </summary>
        public NativeIntPtr<AVIOContext> Context;

        /// <summary>
        /// 指示当前 IO 上下文是否为空（未绑定/已释放）。
        /// </summary>
        public bool IsEmpty => Context.IsEmpty;

        /// <summary>
        /// 构造一个自定义 IO 上下文实例。
        /// <para>
        /// 该构造函数只做“资源封装”，不负责分配 IOContext；外部通常先创建：
        /// <list type="bullet">
        /// <item><description><see cref="GCHandle"/>：用于通过 opaque 传递托管对象。</description></item>
        /// <item><description><see cref="NativeByteMemory"/>：作为 IO buffer。</description></item>
        /// <item><description><see cref="AVIOContext"/>：由 <c>avio_alloc_context</c> 创建。</description></item>
        /// </list>
        /// </para>
        /// </summary>
        /// <param name="engine">用于释放指针资源的引擎实例。</param>
        /// <param name="handle">用于回调 opaque 的托管句柄。</param>
        /// <param name="context">FFmpeg 侧的 AVIOContext 指针。</param>
        /// <param name="buffer">IO buffer。</param>
        public FFmpegIOContext(FFmpegEngine engine, GCHandle handle, NativeIntPtr<AVIOContext> context, NativeByteMemory buffer)
        {
            _engine = engine;
            _handle = handle;
            _buffer = buffer;
            Context = context;
        }

        /// <summary>
        /// 释放自定义 IO 的非托管资源。
        /// <para>
        /// 释放顺序：
        /// <list type="number">
        /// <item><description>释放 <see cref="Context"/>（内部会调用 FFmpeg 的 <c>avio_context_free</c>）。</description></item>
        /// <item><description>释放 <see cref="_buffer"/>。</description></item>
        /// <item><description>释放 <see cref="_handle"/>（解除对托管对象的引用，避免句柄泄漏）。</description></item>
        /// </list>
        /// </para>
        /// </summary>
        public void Dispose()
        {
            _engine.Free(ref Context);
            _buffer.Dispose();

            if (_handle.IsAllocated)
            {
                _handle.Free();
            }
        }
    }
}