namespace ExtenderApp.Media.FFmpegEngines
{
    /// <summary>
    /// FFmpeg 编解码调度接口。
    /// 用于定义编解码对象的调度行为，支持泛型参数，便于扩展不同类型的编解码任务。
    /// 实现类可根据具体业务场景（如解码队列、异步调度等）自定义调度逻辑。
    /// </summary>
    /// <typeparam name="T">调度的编解码对象类型。</typeparam>
    public interface IFFmpegCodecScheduling<T>
    {
        /// <summary>
        /// 调度一个编解码任务或对象。
        /// </summary>
        /// <param name="item">待调度的编解码对象。</param>
        void Schedule(T item);
    }
}
