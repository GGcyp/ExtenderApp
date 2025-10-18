

namespace ExtenderApp.Data
{
    public readonly struct Result
    {
        public ResultCode Code { get; }

        public string? Message { get; }

        public Result(ResultCode code, string? message = null)
        {
            Code = code;
            Message = message;
        }
    }

    public readonly struct Result<T>
    {
        public ResultCode Code { get; }

        public string? Message { get; }

        public T? Data { get; }

        public Result(ResultCode code, string? message) : this(code, default, message)
        {
        }

        public Result(ResultCode code, T? data, string? message = null)
        {
            Code = code;
            Data = data;
            Message = message;
        }

        public static implicit operator Result(Result<T> result)
            => new Result(result.Code, result.Message);

        public static implicit operator Result<T>(Result result)
            => new Result(result.Code, result.Message);
    }
}
