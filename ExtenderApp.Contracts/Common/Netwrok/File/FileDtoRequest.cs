namespace ExtenderApp.Contracts
{
    /// <summary>
    /// 表示一个文件传输请求，可以是推送（上传）或拉取（下载）操作。
    /// </summary>
    public struct FileDtoRequest
    {
        /// <summary>
        /// 获取与此请求关联的文件数据传输对象（DTO）集合。
        /// </summary>
        public ValueOrList<FileDto> FileDtos { get; }

        /// <summary>
        /// 获取一个值，该值指示这是否是一个推送（上传）请求。
        /// </summary>
        /// <value>如果为推送请求，则为 <c>true</c>；如果为拉取请求，则为 <c>false</c>。</value>
        public bool IsPush { get; }

        /// <summary>
        /// 初始化 <see cref="FileDtoRequest"/> 结构的新实例。
        /// </summary>
        /// <param name="dtos">文件DTO的集合。</param>
        /// <param name="isPush">指示请求是推送还是拉取。</param>
        public FileDtoRequest(ValueOrList<FileDto> dtos, bool isPush)
        {
            FileDtos = dtos;
            IsPush = isPush;
        }

        /// <summary>
        /// 初始化 <see cref="FileDtoRequest"/> 结构的新实例。
        /// </summary>
        /// <param name="dtos">文件DTO的可枚举集合。</param>
        /// <param name="isPush">指示请求是推送还是拉取。</param>
        public FileDtoRequest(IEnumerable<FileDto> dtos, bool isPush) : this(isPush)
        {
            FileDtos.AddRange(dtos);
        }

        /// <summary>
        /// 初始化 <see cref="FileDtoRequest"/> 结构的新实例。
        /// </summary>
        /// <param name="dto">单个文件DTO。</param>
        /// <param name="isPush">指示请求是推送还是拉取。</param>
        public FileDtoRequest(FileDto dto, bool isPush) : this(isPush)
        {
            FileDtos.Add(dto);
        }

        /// <summary>
        /// 初始化 <see cref="FileDtoRequest"/> 结构的新实例。
        /// </summary>
        /// <param name="isPush">指示请求是推送还是拉取。</param>
        public FileDtoRequest(bool isPush)
        {
            IsPush = isPush;
            FileDtos = new();
        }

        /// <summary>
        /// 创建一个用于单个文件的推送请求。
        /// </summary>
        /// <param name="dto">文件DTO。</param>
        /// <returns>一个新的推送文件请求。</returns>
        public static FileDtoRequest Push(FileDto dto) => new(dto, true);

        /// <summary>
        /// 创建一个用于多个文件的推送请求。
        /// </summary>
        /// <param name="dtos">文件DTO的可枚举集合。</param>
        /// <returns>一个新的推送文件请求。</returns>
        public static FileDtoRequest Push(IEnumerable<FileDto> dtos) => new(dtos, true);

        /// <summary>
        /// 创建一个用于多个文件的推送请求。
        /// </summary>
        /// <param name="dtos">文件DTO的集合。</param>
        /// <returns>一个新的推送文件请求。</returns>
        public static FileDtoRequest Push(ValueOrList<FileDto> dtos) => new(dtos, true);

        /// <summary>
        /// 创建一个用于单个文件的拉取请求。
        /// </summary>
        /// <param name="dto">文件DTO。</param>
        /// <returns>一个新的拉取文件请求。</returns>
        public static FileDtoRequest Pull(FileDto dto) => new(dto, false);

        /// <summary>
        /// 创建一个用于多个文件的拉取请求。
        /// </summary>
        /// <param name="dtos">文件DTO的可枚举集合。</param>
        /// <returns>一个新的拉取文件请求。</returns>
        public static FileDtoRequest Pull(IEnumerable<FileDto> dtos) => new(dtos, false);

        /// <summary>
        /// 创建一个用于多个文件的拉取请求。
        /// </summary>
        /// <param name="dtos">文件DTO的集合。</param>
        /// <returns>一个新的拉取文件请求。</returns>
        public static FileDtoRequest Pull(ValueOrList<FileDto> dtos) => new(dtos, false);
    }
}