namespace ExtenderApp.Data
{
    /// <summary>
    /// 表示一个操作的结果，不包含返回值。
    /// </summary>
    public readonly struct Result : IEquatable<Result>
    {
        /// <summary>
        /// 获取一个值，该值指示操作是否成功。
        /// </summary>
        public bool IsSuccess { get; }

        /// <summary>
        /// 获取与结果相关的可选消息。
        /// </summary>
        public string? Message { get; }

        /// <summary>
        /// 获取与失败结果相关的异常（如有）。
        /// </summary>
        public Exception? Exception { get; }

        /// <summary>
        /// 初始化 <see cref="Result"/> 结构的新实例。
        /// </summary>
        /// <param name="isSuccess">操作是否成功。</param>
        /// <param name="message">与结果相关的可选消息。</param>
        /// <param name="exception">与结果相关的异常。</param>
        public Result(bool isSuccess, string? message = null, Exception? exception = null)
        {
            IsSuccess = isSuccess;
            Message = message;
            Exception = exception;
        }

        /// <summary>
        /// 返回当前结果的字符串表示形式。
        /// </summary>
        /// <returns>描述结果状态和消息的字符串。</returns>
        public override string ToString()
        {
            return string.Format("{0}，返回信息{1}", IsSuccess ? "OK" : "Error", Message ?? (IsSuccess ? "OK" : "未发现返回信息"));
        }

        /// <inheritdoc/>
        public override bool Equals(object? obj)
        {
            return obj is Result other && Equals(other);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return HashCode.Combine(IsSuccess, Message, Exception);
        }

        /// <inheritdoc/>
        public bool Equals(Result other)
        {
            return IsSuccess == other.IsSuccess &&
                   Message == other.Message &&
                   EqualityComparer<Exception?>.Default.Equals(Exception, other.Exception);
        }

        #region Static Members

        /// <summary>
        /// 创建一个表示成功的 <see cref="Result"/>。
        /// </summary>
        public static Result Success(string? message = "OK") => new(true, message);

        /// <summary>
        /// 创建一个表示不成功的 <see cref="Result"/>。
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public static Result Failure(string? message = "Failure") => new(false, message);

        /// <summary>
        /// 创建一个表示异常的 <see cref="Result"/>。
        /// </summary>
        /// <param name="exception">捕获到的异常。</param>
        public static Result Error(Exception exception) => new(false, exception.Message, exception);

        /// <summary>
        /// 创建一个表示成功的 <see cref="Result{T}"/>。
        /// </summary>
        public static Result<T> Success<T>(T data, string? message = "OK") => new(true, data, message);

        /// <summary>
        /// 创建一个表示不成功的 <see cref="Result{T}"/>。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="message"></param>
        /// <returns></returns>
        public static Result<T> Failure<T>(string? message = "Failure") => new(false, default(T), message);

        /// <summary>
        /// 创建一个表示异常的 <see cref="Result{T}"/>。
        /// </summary>
        public static Result<T> Error<T>(Exception exception) => new(false, default, exception.Message, exception);

        #endregion Static Members

        /// <summary>
        /// 比较两个 <see cref="Result"/> 实例是否相等。
        /// </summary>
        public static bool operator ==(Result left, Result right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// 比较两个 <see cref="Result"/> 实例是否不相等。
        /// </summary>
        public static bool operator !=(Result left, Result right)
        {
            return !(left == right);
        }

        /// <summary>
        /// 定义从 <see cref="Result"/> 到其成功状态 <see cref="bool"/> 的隐式转换。
        /// </summary>
        public static implicit operator bool(Result result)
            => result.IsSuccess;

        /// <summary>
        /// 定义从 <see cref="Result"/> 到其消息字符串的隐式转换。
        /// </summary>
        public static implicit operator string?(Result result)
            => result.Message;
    }

    /// <summary>
    /// 表示一个操作的结果，包含一个类型为 <typeparamref name="T"/> 的返回值。
    /// </summary>
    /// <typeparam name="T">返回值的类型。</typeparam>
    public readonly struct Result<T> : IEquatable<Result<T>>
    {
        /// <summary>
        /// 获取一个值，该值指示操作是否成功。
        /// </summary>
        public bool IsSuccess { get; }

        /// <summary>
        /// 获取与结果相关的可选消息。
        /// </summary>
        public string? Message { get; }

        /// <summary>
        /// 获取与失败结果相关的异常（如有）。
        /// </summary>
        public Exception? Exception { get; }

        /// <summary>
        /// 获取操作返回的数据。
        /// </summary>
        public T? Data { get; }

        /// <summary>
        /// 初始化一个不带数据的 <see cref="Result{T}"/> 新实例。
        /// </summary>
        /// <param name="isSuccess">操作是否成功。</param>
        /// <param name="message">与结果相关的可选消息。</param>
        public Result(bool isSuccess, string? message) : this(isSuccess, default, message, null)
        {
        }

        /// <summary>
        /// 初始化 <see cref="Result{T}"/> 结构的新实例。
        /// </summary>
        /// <param name="isSuccess">操作是否成功。</param>
        /// <param name="data">操作返回的数据。</param>
        /// <param name="message">与结果相关的可选消息。</param>
        /// <param name="exception">与结果相关的异常。</param>
        public Result(bool isSuccess, T? data, string? message = null, Exception? exception = null)
        {
            IsSuccess = isSuccess;
            Data = data;
            Message = message;
            Exception = exception;
        }

        /// <inheritdoc/>
        public bool Equals(Result<T> other)
        {
            return IsSuccess == other.IsSuccess &&
                   EqualityComparer<T?>.Default.Equals(Data, other.Data) &&
                   Message == other.Message &&
                   EqualityComparer<Exception?>.Default.Equals(Exception, other.Exception);
        }

        /// <inheritdoc/>
        public override bool Equals(object? obj)
        {
            return obj is Result<T> other && Equals(other);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return HashCode.Combine(IsSuccess, Data, Message, Exception);
        }

        /// <summary>
        /// 返回当前结果的字符串表示形式。
        /// </summary>
        /// <returns>描述结果状态、消息和数据的字符串。</returns>
        public override string ToString()
        {
            return string.Format("{0}，返回信息{1}，数据：{2}", IsSuccess ? "OK" : "Error", Message ?? (IsSuccess ? "OK" : "未发现返回信息"), Data?.ToString() ?? "null");
        }

        /// <summary>
        /// 比较两个 <see cref="Result{T}"/> 实例是否相等。
        /// </summary>
        public static bool operator ==(Result<T> left, Result<T> right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// 比较两个 <see cref="Result{T}"/> 实例是否不相等。
        /// </summary>
        public static bool operator !=(Result<T> left, Result<T> right)
        {
            return !(left == right);
        }

        /// <summary>
        /// 定义从 <see cref="Result{T}"/> 到其成功状态 <see cref="bool"/> 的隐式转换。
        /// </summary>
        public static implicit operator bool(Result<T> result)
            => result.IsSuccess;

        /// <summary>
        /// 定义从 <see cref="Result{T}"/> 到其消息字符串的隐式转换。
        /// </summary>
        public static implicit operator string?(Result<T> result)
            => result.Message;

        /// <summary>
        /// 定义从 <see cref="Result{T}"/> 到 <see cref="Result"/> 的隐式转换，会丢失数据。
        /// </summary>
        public static implicit operator Result(Result<T> result)
            => new Result(result.IsSuccess, result.Message, result.Exception);

        /// <summary>
        /// 定义从 <see cref="Result{T}"/> 到其数据类型 <typeparamref name="T"/> 的隐式转换。
        /// </summary>
        public static implicit operator T?(Result<T> result)
            => result.Data;
    }
}