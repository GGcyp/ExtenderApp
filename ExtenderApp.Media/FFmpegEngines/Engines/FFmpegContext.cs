using ExtenderApp.Data;
using FFmpeg.AutoGen;

namespace ExtenderApp.FFmpegEngines
{
    /// <summary>
    /// 表示一个FFmpeg上下文结构。
    /// </summary>
    public struct FFmpegContext : IDisposable
    {
        /// <summary>
        /// FFmpeg引擎实例。
        /// </summary>
        private readonly FFmpegEngine _engine;

        /// <summary>
        /// FFmpeg格式上下文指针。
        /// </summary>
        public NativeIntPtr<AVFormatContext> FormatContext;

        /// <summary>
        /// FFmpeg选项字典指针。
        /// </summary>
        public NativeIntPtr<AVDictionary> Options;

        /// <summary>
        /// FFmpeg信息。
        /// </summary>
        public FFmpegInfo Info;

        /// <summary>
        /// FFmpeg决策上下文集合。
        /// </summary>
        public FFmpegDecoderContextCollection ContextCollection;

        /// <summary>
        /// 获取一个值，指示上下文是否为空。
        /// </summary>
        public bool IsEmpty => FormatContext.IsEmpty;

        /// <summary>
        /// 初始化一个新的FFmpeg上下文实例。
        /// </summary>
        /// <param name="engine">FFmpeg引擎实例。</param>
        /// <param name="formatContext">FFmpeg格式上下文指针。</param>
        /// <param name="options">FFmpeg选项字典指针。</param>
        /// <param name="info">FFmpeg信息。</param>
        /// <param name="collection">FFmpeg决策上下文集合。</param>
        public FFmpegContext(FFmpegEngine engine, NativeIntPtr<AVFormatContext> formatContext, NativeIntPtr<AVDictionary> options, FFmpegInfo info, FFmpegDecoderContextCollection collection)
        {
            _engine = engine;
            FormatContext = formatContext;
            Options = options;
            Info = info;
            ContextCollection = collection;
        }

        /// <summary>
        /// 释放FFmpeg上下文占用的资源。
        /// </summary>
        public void Dispose()
        {
            _engine.Free(ref this);
        }
    }
}
