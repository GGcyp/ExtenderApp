using ExtenderApp.Data;
using FFmpeg.AutoGen;

namespace ExtenderApp.FFmpegEngines
{
    /// <summary>
    /// 带“代际（Generation）”信息的 FFmpeg 数据包封装。
    /// <para>
    /// 用于 Seek/跳转后的数据隔离：每次 Seek 会递增全局代际号，解复用线程在写入包时将当前代际附加到包上。
    /// 解码线程在消费包时比较 <see cref="Generation"/>，若代际不匹配则丢弃该包，避免 Seek 前后的旧包被错误解码。
    /// </para>
    /// <para>
    /// 说明：该类型为只读值类型，仅携带元数据与原生包指针，不负责资源释放；包的回收/释放应由调用方统一通过 <c>FFmpegEngine.Return</c> 处理。
    /// </para>
    /// </summary>
    public readonly struct FFmpegPacket
    {
        /// <summary>
        /// 数据包所属的解码代际号。
        /// <para>
        /// 通常由控制器在每次 Seek 时递增，全链路（读包 / 解码 / 输出）均以该值判断数据是否过期。
        /// </para>
        /// </summary>
        public long Generation { get; }

        /// <summary>
        /// 原生 FFmpeg 数据包指针。
        /// <para>
        /// 指向 <see cref="AVPacket"/>；其生命周期由调用方管理，消费完成后应归还到包池或释放。
        /// </para>
        /// </summary>
        public NativeIntPtr<AVPacket> PacketPtr { get; }

        /// <summary>
        /// 创建一个带代际信息的 FFmpeg 数据包封装。
        /// </summary>
        /// <param name="generation">数据包所属代际号。</param>
        /// <param name="packetPtr">原生 <see cref="AVPacket"/> 指针。</param>
        public FFmpegPacket(long generation, NativeIntPtr<AVPacket> packetPtr)
        {
            Generation = generation;
            PacketPtr = packetPtr;
        }
    }
}