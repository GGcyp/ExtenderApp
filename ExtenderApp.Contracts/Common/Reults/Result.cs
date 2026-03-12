using System.Runtime.ExceptionServices;

namespace ExtenderApp.Contracts
{
    /// <summary>
    /// 表示一个操作的结果，不包含返回值。
    /// </summary>
    public readonly struct Result : IEquatable<Result>
    {
        public const string DefaultSuccessMessage = "OK";
        public const string DefaultFailureMessage = "Failure";
        public const string DefaultMessage = "No message provided";

        /// <summary>
        /// 获取与结果相关的可选消息。
        /// </summary>
        public string? Message { get; }

        /// <summary>
        /// 获取与失败结果相关的异常（如有）。
        /// </summary>
        public Exception? Exception { get; }

        /// <summary>
        /// 获取一个值，该值指示操作是否成功。
        /// </summary>
        public bool IsSuccess { get; }

        /// <summary>
        /// 获取一个值，该值指示结果是否包含异常。 只有当结果表示失败且包含异常时才为 true。
        /// </summary>
        public bool HasException => Exception != null;

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
            return string.Format("{0}，返回信息{1}", IsSuccess ? DefaultSuccessMessage : DefaultFailureMessage, Message ?? (IsSuccess ? DefaultSuccessMessage : DefaultMessage));
        }

        /// <inheritdoc/>
        public bool Equals(Result other)
        {
            return IsSuccess == other.IsSuccess &&
                   Message == other.Message &&
                   EqualityComparer<Exception?>.Default.Equals(Exception, other.Exception);
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

        /// <summary>
        /// 如果操作失败且包含异常，则重新抛出该异常。 注意：此方法会重置堆栈跟踪，使其始于当前调用点。
        /// </summary>
        public void ThrowExceptionIfError()
        {
            if (!IsSuccess && Exception != null)
            {
                throw Exception;
            }
        }

        /// <summary>
        /// 如果操作失败且包含异常，则重新抛出该异常，并保留原始的堆栈跟踪信息。
        /// </summary>
        public void ThrowExceptionWithOriginalStackTraceIfError()
        {
            if (!IsSuccess && Exception != null)
            {
                ExceptionDispatchInfo.Capture(Exception).Throw();
            }
        }

        public static bool operator ==(Result left, Result right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Result left, Result right)
        {
            return !(left == right);
        }

        #region Static Members

        /// <summary>
        /// 创建一个表示成功的 <see cref="Result"/>。
        /// </summary>
        public static Result Success(string? message = DefaultSuccessMessage) => new(true, message);

        /// <summary>
        /// 创建一个表示不成功的 <see cref="Result"/>。
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public static Result Failure(string? message = DefaultFailureMessage) => new(false, message);

        /// <summary>
        /// 创建一个表示异常的 <see cref="Result"/>。
        /// </summary>
        /// <param name="exception">捕获到的异常。</param>
        public static Result FromException(Exception exception) => new(false, exception.Message, exception);

        /// <summary>
        /// 创建一个表示异常的 <see cref="Result"/>。
        /// </summary>
        /// <param name="exception">捕获到的异常。</param>
        /// <param name="message">与异常相关的自定义消息。</param>
        public static Result FromException(Exception exception, string message) => new(false, message, exception);

        /// <summary>
        /// 创建一个表示成功的 <see cref="Result{T}"/>。
        /// </summary>
        public static Result<T> Success<T>(T data, string? message = DefaultSuccessMessage) => new(true, data, message);

        /// <summary>
        /// 创建一个表示不成功的 <see cref="Result{T}"/>。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data">操作返回的数据。</param>
        /// <param name="message"></param>
        /// <returns></returns>
        public static Result<T> Failure<T>(T data, string? message = DefaultFailureMessage) => new(false, data, message);

        /// <summary>
        /// 创建一个表示不成功的 <see cref="Result{T}"/>。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="message"></param>
        /// <returns></returns>
        public static Result<T> Failure<T>(string? message = DefaultFailureMessage) => new(false, default(T), message);

        /// <summary>
        /// 创建一个表示异常的 <see cref="Result{T}"/>。
        /// </summary>
        public static Result<T> FromException<T>(Exception exception) => new(false, default, exception.Message, exception);

        /// <summary>
        /// 创建一个表示异常的 <see cref="Result{T}"/>。
        /// </summary>
        public static Result<T> FromException<T>(Exception exception, string message) => new(false, default, message, exception);

        /// <summary>
        /// 创建一个表示异常的 <see cref="Result{TValue}"/>，其中异常类型由类型参数 <typeparamref name="TException"/> 指定，并使用默认构造函数实例化该异常。
        /// </summary>
        /// <typeparam name="TValue"></typeparam>
        /// <typeparam name="TException"></typeparam>
        /// <returns></returns>
        /// <exception cref="TException"></exception>
        public static Result FromException<TValue, TException>() where TException : Exception, new()
        {
            try
            {
                throw new TException();
            }
            catch (Exception)
            {
                return FromException<TValue>(new TException());
            }
        }

        /// <summary>
        /// 返回两个结果的逻辑与（AND）组合。 只有当两个结果都成功时，才返回成功；如果两个结果都失败，则返回一个包含两个失败消息的失败结果；如果只有一个结果失败，则返回该失败结果。
        /// </summary>
        /// <param name="result1">第一个结果。</param>
        /// <param name="result2">第二个结果。</param>
        /// <returns>逻辑与组合的结果。</returns>
        public static Result And(Result result1, Result result2)
        {
            if (result1.IsSuccess && result2.IsSuccess)
            {
                return Success();
            }
            else if (!result1.IsSuccess && !result2.IsSuccess)
            {
                return Failure($"{result1.Message}；{result2.Message}");
            }
            else if (!result1.IsSuccess)
            {
                return Failure(result1.Message ?? DefaultFailureMessage);
            }
            else
            {
                return Failure(result2.Message ?? DefaultFailureMessage);
            }
        }

        #endregion Static Members

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

        public static implicit operator Exception?(Result result)
            => result.Exception;

        public static implicit operator ValueTask<Result>(Result result)
            => ValueTask.FromResult(result);
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
        /// 获取一个值，该值指示结果是否包含异常。 只有当结果表示失败且包含异常时才为 true。
        /// </summary>
        public bool HasException => Exception != null;

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
        public T? Value { get; }

        public Result(bool isSuccess) : this(isSuccess, default, null, null)
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
            Value = data;
            Message = message;
            Exception = exception;
        }

        /// <summary>
        /// 如果操作失败且包含异常，则重新抛出该异常。 注意：此方法会重置堆栈跟踪，使其始于当前调用点。
        /// </summary>
        public void ThrowExceptionIfError()
        {
            if (!IsSuccess && Exception != null)
            {
                throw Exception;
            }
        }

        /// <summary>
        /// 如果操作失败且包含异常，则重新抛出该异常，并保留原始的堆栈跟踪信息。
        /// </summary>
        public void ThrowExceptionWithOriginalStackTraceIfError()
        {
            if (!IsSuccess && Exception != null)
            {
                ExceptionDispatchInfo.Capture(Exception).Throw();
            }
        }

        /// <inheritdoc/>
        public bool Equals(Result<T> other)
        {
            return IsSuccess == other.IsSuccess &&
                   EqualityComparer<T?>.Default.Equals(Value, other.Value) &&
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
            return HashCode.Combine(IsSuccess, Value, Message, Exception);
        }

        /// <summary>
        /// 返回当前结果的字符串表示形式。
        /// </summary>
        /// <returns>描述结果状态、消息和数据的字符串。</returns>
        public override string ToString()
        {
            return string.Format("{0}，返回信息{1}，数据：{2}", IsSuccess ? Result.DefaultSuccessMessage : Result.DefaultFailureMessage, Message ?? (IsSuccess ? Result.DefaultSuccessMessage : Result.DefaultMessage), Value?.ToString() ?? "null");
        }

        public static bool operator ==(Result<T> left, Result<T> right)
        {
            return left.Equals(right);
        }

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
            => result.Value;

        public static explicit operator Result<T>(T data)
            => Result.Success(data);

        public static implicit operator ValueTask<Result<T>>(Result<T> result)
            => ValueTask.FromResult(result);

        public static implicit operator Exception?(Result<T> result)
            => result.Exception;
    }
}