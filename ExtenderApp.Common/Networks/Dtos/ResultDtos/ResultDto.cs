namespace ExtenderApp.Common.Networks
{
    /// <summary>
    /// 结果数据传输对象
    /// </summary>
    public struct ResultDto
    {
        /// <summary>
        /// 状态码
        /// </summary>
        public int StateCode { get; }

        /// <summary>
        /// 消息内容
        /// </summary>
        public string? Message { get; }

        /// <summary>
        /// 是否成功
        /// </summary>
        public bool IsSuccess => StateCode == 200;

        /// <summary>
        /// 初始化 ResultDto 类的新实例。
        /// </summary>
        /// <param name="stateCode">状态码。</param>
        /// <param name="message">消息内容。</param>
        public ResultDto(int stateCode, string? message)
        {
            StateCode = stateCode;
            Message = message;
        }

        /// <summary>
        /// 创建一个表示成功的 ResultDto 实例。
        /// </summary>
        /// <param name="message">可选的消息内容。</param>
        /// <returns>表示成功的 ResultDto 实例。</returns>
        public static ResultDto Ok(string? message = null) => new ResultDto(200, message);

        /// <summary>
        /// 创建一个表示错误的 ResultDto 实例。
        /// </summary>
        /// <param name="message">可选的消息内容。</param>
        /// <returns>表示错误的 ResultDto 实例。</returns>
        public static ResultDto Eorror(string? message = null) => new ResultDto(500, message);

        /// <summary>
        /// 创建一个表示未找到的 ResultDto 实例。
        /// </summary>
        /// <param name="message">可选的消息内容。</param>
        /// <returns>表示未找到的 ResultDto 实例。</returns>
        public static ResultDto NotFound(string? message = null) => new ResultDto(404, message);

        /// <summary>
        /// 创建一个表示错误的请求 ResultDto 实例。
        /// </summary>
        /// <param name="message">可选的消息内容。</param>
        /// <returns>表示错误的请求 ResultDto 实例。</returns>
        public static ResultDto BadRequest(string? message = null) => new ResultDto(400, message);
    }
}
