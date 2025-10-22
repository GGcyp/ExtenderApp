using ExtenderApp.Abstract;
using ExtenderApp.Data;

namespace ExtenderApp.Common.Pipelines
{
    /// <summary>
    /// 单向管道中间件抽象基类。
    /// </summary>
    /// <typeparam name="T">管道上下文类型，必须实现 <see cref="IPipelineContext"/>。</typeparam>
    /// <remarks>
    /// 继承此类并实现 <see cref="InvokeAsync(T, Func{Task})"/> 以参与管道处理。
    /// 典型用法：前置逻辑 → <c>await next()</c> → 后置逻辑；如需终止后续中间件，可不调用 <paramref name="next"/>。
    /// </remarks>
    /// <seealso cref="IMiddleware{T}"/>
    /// <seealso cref="IPipelineContext"/>
    public abstract class MiddlewareBase<T> : DisposableObject, IMiddleware<T> 
        where T : PipelineContext
    {
        /// <summary>
        /// 处理上下文并按需调用后续中间件。
        /// </summary>
        /// <param name="context">当前管道上下文。</param>
        /// <param name="next">下一个中间件的委托。可选择不调用以终止后续流程。</param>
        /// <returns>表示异步操作的任务。</returns>
        public abstract Task InvokeAsync(T context, Func<Task> next);
    }

    /// <summary>
    /// 双向（输入/输出）中间件基础实现。
    /// </summary>
    /// <typeparam name="TInput">输入方向上下文类型，必须实现 <see cref="IPipelineContext"/>。</typeparam>
    /// <typeparam name="Toutput">输出方向上下文类型，必须实现 <see cref="IPipelineContext"/>。</typeparam>
    /// <remarks>
    /// 提供输入/输出方向的默认占位实现：不执行业务逻辑，直接完成。
    /// 派生类可按需重写 <see cref="InputInvokeAsync(TInput, Func{Task})"/> 或 <see cref="OutputInvokeAsync(Toutput, Func{Task})"/> 以加入实际处理。
    /// </remarks>
    /// <seealso cref="IMiddleware{TInput, Toutput}"/>
    /// <seealso cref="IPipelineContext"/>
    public class MiddlewareBase<TInput, Toutput> : DisposableObject, IMiddleware<TInput, Toutput>
        where TInput : PipelineContext
        where Toutput : PipelineContext
    {

        /// <summary>
        /// 输入方向的默认实现：不执行业务逻辑，直接返回已完成任务。
        /// </summary>
        /// <param name="context">输入方向上下文。</param>
        /// <param name="next">下一个输入中间件委托。</param>
        /// <returns>已完成的任务。</returns>
        /// <remarks>
        /// 基类实现仅将 <see cref="NeedInputInvoke"/> 置为 <c>false</c> 并返回。派生类应重写以实现实际处理，
        /// 并在需要继续后续中间件时调用且等待 <paramref name="next"/>。
        /// </remarks>
        public async Task InputInvokeAsync(TInput context, Func<Task> next)
        {
            bool canNext = await InputInvokeAsync(context);
            if (!canNext)
            {
                return;
            }
            await next();
        }

        /// <summary>
        /// 输出方向的默认实现：不执行业务逻辑，直接返回已完成任务。
        /// </summary>
        /// <param name="context">输出方向上下文。</param>
        /// <param name="next">下一个输出中间件委托。</param>
        /// <returns>已完成的任务。</returns>
        /// <remarks>
        /// 基类实现仅将 <see cref="NeedOutputInvoke"/> 置为 <c>false</c> 并返回。派生类应重写以实现实际处理，
        /// 并在需要继续后续中间件时调用且等待 <paramref name="next"/>。
        /// </remarks>
        public async Task OutputInvokeAsync(Toutput context, Func<Task> next)
        {
            bool canNext = await OutputInvokeAsync(context);
            if (!canNext)
            {
                return;
            }
            await next();
        }

        protected virtual Task<bool> InputInvokeAsync(TInput context)
        {
            return Task.FromResult(true);
        }

        protected virtual Task<bool> OutputInvokeAsync(Toutput context)
        {
            return Task.FromResult(true);
        }

        protected Task<bool> Ok()
        {
            return Task.FromResult(true);
        }
        protected Task<bool> NotOk()
        {
            return Task.FromResult(false);
        }
    }
}
