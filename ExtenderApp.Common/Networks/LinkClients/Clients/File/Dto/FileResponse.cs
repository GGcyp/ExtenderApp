namespace ExtenderApp.Contracts
{
    /// <summary>
    /// 表示对文件传输请求的响应。
    /// </summary>
    public readonly struct FileResponse
    {
        /// <summary>
        /// 获取与此响应关联的文件数据传输对象。
        /// </summary>
        public ValueOrList<FileDto>? Dtos { get; }

        /// <summary>
        /// 获取一个值，该值指示请求是否被接受。
        /// </summary>
        public bool IsAccepted { get; }

        /// <summary>
        /// 初始化 <see cref="FileResponse"/> 结构的新实例。
        /// </summary>
        /// <param name="isAccepted">一个值，指示请求是否被接受。</param>
        /// <param name="dtos">与此响应关联的文件数据传输对象。</param>
        public FileResponse(bool isAccepted, ValueOrList<FileDto>? dtos = null)
        {
            IsAccepted = isAccepted;
            Dtos = dtos;
        }
    }
}