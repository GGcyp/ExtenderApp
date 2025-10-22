using ExtenderApp.Data;

namespace ExtenderApp.Abstract
{
    /// <summary>
    /// 泛型中间件接口：处理特定类型的管道上下文。
    /// </summary>
    /// <typeparam name="T">管道上下文类型，必须继承 <see cref="PipelineContext"/>。</typeparam>
    public interface IMiddleware<in T> where T : PipelineContext
    {
        /// <summary>
        /// 处理上下文数据。
        /// </summary>
        /// <param name="context">管道上下文。</param>
        /// <param name="next">下一个中间件的委托。若不调用（或不等待）则终止后续流程。</param>
        /// <returns>表示异步操作的任务。</returns>
        /// <remarks>
        /// 使用方式（环绕/拦截）：
        /// 1. 调用前执行前置逻辑；
        /// 2. 调用并等待 <paramref name="next"/> 以继续后续中间件；
        /// 3. 调用后执行后置逻辑（如清理、记录）。
        /// 若需中止后续中间件，可不调用 <paramref name="next"/> 或将 <c>context.IsTerminated</c> 设为 <c>true</c>。
        /// 异常处理建议：捕获异常后设置 <c>context.Error</c>，并按需中止流程。
        /// </remarks>
        /// <seealso cref="IPipelineContext"/>
        Task InvokeAsync(T context, Func<Task> next);
    }

    /// <summary>
    /// 同时支持“输入方向”和“输出方向”的中间件接口。
    /// </summary>
    /// <typeparam name="TInput">输入方向上下文类型，必须实现 <see cref="IPipelineContext"/>。</typeparam>
    /// <typeparam name="TOutput">输出方向上下文类型，必须实现 <see cref="IPipelineContext"/>。</typeparam>
    public interface IMiddleware<in TInput, in TOutput>
        where TInput : PipelineContext
        where TOutput : PipelineContext
    {
        /// <summary>
        /// 输入方向的中间件调用。
        /// </summary>
        /// <param name="context">输入方向上下文。</param>
        /// <param name="next">下一个输入中间件委托。</param>
        Task InputInvokeAsync(TInput context, Func<Task> next);

        /// <summary>
        /// 输出方向的中间件调用。
        /// </summary>
        /// <param name="context">输出方向上下文。</param>
        /// <param name="next">下一个输出中间件委托。</param>
        Task OutputInvokeAsync(TOutput context, Func<Task> next);
    }
}
