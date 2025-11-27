using ExtenderApp.Data;
using ExtenderApp.FFmpegEngines.Decoders;
using FFmpeg.AutoGen;

namespace ExtenderApp.FFmpegEngines
{
    /// <summary>
    /// 封装了与单个媒体文件或流相关的所有核心 FFmpeg 资源。
    /// 它包含格式上下文、解码器信息、媒体元数据和用于资源管理的引擎实例，是进行解码、跳转等操作的中心对象。
    /// </summary>
    public struct FFmpegContext : IDisposable
    {
        /// <summary>
        /// 关联的 FFmpeg 引擎实例，用于管理此上下文的生命周期（如资源分配与释放）。
        /// </summary>
        private readonly FFmpegEngine _engine;

        /// <summary>
        /// 指向 FFmpeg 的 <see cref="AVFormatContext"/> 的指针。这是处理媒体文件的核心结构，包含了文件的格式、流信息等。
        /// </summary>
        public NativeIntPtr<AVFormatContext> FormatContext;

        /// <summary>
        /// 指向 FFmpeg 的 <see cref="AVDictionary"/> 的指针，用于在打开媒体时传递配置选项。
        /// </summary>
        public NativeIntPtr<AVDictionary> Options;

        /// <summary>
        /// 从媒体文件中提取的元数据信息，如时长、分辨率、编解码器名称等。
        /// </summary>
        public FFmpegInfo Info;

        /// <summary>
        /// 包含此媒体文件中所有流（如视频、音频）的解码器上下文集合。
        /// </summary>
        public FFmpegDecoderContextCollection ContextCollection;

        /// <summary>
        /// 获取一个值，该值指示此上下文是否有效。
        /// 如果格式上下文指针为空，则认为此上下文无效。
        /// </summary>
        public bool IsEmpty => FormatContext.IsEmpty;

        /// <summary>
        /// 初始化一个新的 <see cref="FFmpegContext"/> 实例。
        /// </summary>
        /// <param name="engine">用于管理此上下文生命周期的 FFmpeg 引擎实例。</param>
        /// <param name="formatContext">媒体文件的格式上下文指针。</param>
        /// <param name="options">打开媒体时使用的选项字典指针。</param>
        /// <param name="info">从媒体中提取的元数据。</param>
        /// <param name="collection">媒体流的解码器上下文集合。</param>
        public FFmpegContext(FFmpegEngine engine, NativeIntPtr<AVFormatContext> formatContext, NativeIntPtr<AVDictionary> options, FFmpegInfo info, FFmpegDecoderContextCollection collection)
        {
            _engine = engine;
            FormatContext = formatContext;
            Options = options;
            Info = info;
            ContextCollection = collection;
        }

        /// <summary>
        /// 释放此上下文持有的所有非托管 FFmpeg 资源。
        /// 此方法会委托给 <see cref="FFmpegEngine"/> 来执行实际的清理工作。
        /// </summary>
        public void Dispose()
        {
            _engine.Free(ref this);
        }
    }
}
