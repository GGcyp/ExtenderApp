using ExtenderApp.Data;

namespace ExtenderApp.Abstract
{
    /// <summary>
    /// 通用管道处理委托（类似 ASP.NET Core 的 RequestDelegate）。
    /// </summary>
    /// <typeparam name="T">管道上下文类型，必须实现 <see cref="IPipelineContext"/>。</typeparam>
    /// <param name="context">当前处理的管道上下文。</param>
    /// <param name="next">
    /// 下一个中间件的委托。调用并等待该委托以继续执行后续步骤；
    /// 若不调用（或不等待）则终止后续流程。
    /// </param>
    /// <returns>表示异步操作的任务。</returns>
    /// <seealso cref="IMiddleware{T}"/>
    public delegate Task PipelineDelegate<T>(T context, Func<Task> next) where T : PipelineContext;

    /// <summary>
    /// 输入方向的管道委托。
    /// </summary>
    /// <typeparam name="T">输入方向的上下文类型，必须实现 <see cref="IPipelineContext"/>。</typeparam>
    /// <param name="context">输入上下文。</param>
    /// <param name="next">下一个输入中间件委托。</param>
    public delegate Task PipelineInputDelegate<T>(T context, Func<Task> next) where T : PipelineContext;

    /// <summary>
    /// 输出方向的管道委托。
    /// </summary>
    /// <typeparam name="T">输出方向的上下文类型，必须实现 <see cref="IPipelineContext"/>。</typeparam>
    /// <param name="context">输出上下文。</param>
    /// <param name="next">下一个输出中间件委托。</param>
    public delegate Task PipelineOutputDelegate<T>(T context, Func<Task> next) where T : PipelineContext;

    /// <summary>
    /// 已构建管道的执行委托（无 next）。
    /// </summary>
    /// <typeparam name="T">上下文类型，必须实现 <see cref="IPipelineContext"/>。</typeparam>
    /// <param name="context">要执行的上下文。</param>
    public delegate Task PipelineExecute<T>(T context) where T : PipelineContext;

    /// <summary>
    /// 管道构建器接口：用于按顺序注册中间件或委托，并构建可执行的管道。
    /// </summary>
    /// <typeparam name="T">管道上下文类型，必须实现 <see cref="IPipelineContext"/>。</typeparam>
    /// <remarks>
    /// 执行顺序遵循注册顺序：先注册的作为外层，后注册的作为内层（洋葱模型）。
    /// 构建器用于配置阶段，通常不保证线程安全。
    /// </remarks>
    public interface IPipelineBuilder<T> where T : PipelineContext
    {
        /// <summary>
        /// 从依赖注入容器解析并注册一个中间件。
        /// </summary>
        /// <typeparam name="TMiddleware">中间件类型，必须实现 <see cref="IMiddleware{T}"/>。</typeparam>
        /// <returns>返回当前构建器以支持链式调用。</returns>
        /// <exception cref="InvalidOperationException">当容器中未注册 <typeparamref name="TMiddleware"/> 时抛出。</exception>
        IPipelineBuilder<T> Use<TMiddleware>() where TMiddleware : IMiddleware<T>;

        /// <summary>
        /// 注册一个已实例化的中间件。
        /// </summary>
        /// <param name="middleware">中间件实例。</param>
        /// <returns>返回当前构建器以支持链式调用。</returns>
        /// <exception cref="ArgumentNullException">当 <paramref name="middleware"/> 为 null 时抛出。</exception>
        IPipelineBuilder<T> Use(IMiddleware<T> middleware);

        /// <summary>
        /// 直接注册一个管道委托。
        /// </summary>
        /// <param name="pipelineDelegate">形如 <c>(context, next) =&gt; ...</c> 的委托。</param>
        /// <returns>返回当前构建器以支持链式调用。</returns>
        /// <exception cref="ArgumentNullException">当 <paramref name="pipelineDelegate"/> 为 null 时抛出。</exception>
        IPipelineBuilder<T> Use(PipelineDelegate<T> pipelineDelegate);

        /// <summary>
        /// 构建并返回最终可执行的管道委托。
        /// </summary>
        /// <returns>可执行的 <see cref="PipelineExecute{T}"/>。</returns>
        /// <remarks>
        /// 未注册任何步骤时的行为（如返回空操作委托）由具体实现决定。
        /// 是否可重复调用也由实现决定，通常建议在配置完成后仅调用一次。
        /// </remarks>
        PipelineExecute<T> Build();
    }


    /// <summary>
    /// 支持入/出方向分离的管道构建器接口。
    /// </summary>
    /// <typeparam name="TInput">输入方向上下文类型，必须实现 <see cref="IPipelineContext"/>。</typeparam>
    /// <typeparam name="TOutput">输出方向上下文类型，必须实现 <see cref="IPipelineContext"/>。</typeparam>
    public interface IPipelineBuilder<TInput, TOutput>
        where TInput : PipelineContext
        where TOutput : PipelineContext
    {
        /// <summary>
        /// 从依赖注入容器解析并注册一个中间件。
        /// </summary>
        /// <typeparam name="TMiddleware">中间件类型，必须实现 <see cref="IMiddleware{TInput, TOutput}"/>。</typeparam>
        /// <returns>返回当前构建器以支持链式调用。</returns>
        /// <exception cref="InvalidOperationException">当容器中未注册 <typeparamref name="TMiddleware"/> 时抛出。</exception>
        IPipelineBuilder<TInput, TOutput> Use<TMiddleware>() where TMiddleware : IMiddleware<TInput, TOutput>;

        /// <summary>
        /// 注册一个已实例化的中间件。
        /// </summary>
        /// <param name="middleware">中间件实例。</param>
        /// <returns>返回当前构建器以支持链式调用。</returns>
        /// <exception cref="ArgumentNullException">当 <paramref name="middleware"/> 为 null 时抛出。</exception>
        IPipelineBuilder<TInput, TOutput> Use(IMiddleware<TInput, TOutput> middleware);

        /// <summary>
        /// 为“输入方向”直接注册一个管道委托。
        /// </summary>
        /// <param name="pipelineDelegate">形如 <c>(context, next) =&gt; ...</c> 的输入委托。</param>
        /// <returns>返回当前构建器以支持链式调用。</returns>
        /// <exception cref="ArgumentNullException">当 <paramref name="pipelineDelegate"/> 为 null 时抛出。</exception>
        IPipelineBuilder<TInput, TOutput> Use(PipelineInputDelegate<TInput> pipelineDelegate);

        /// <summary>
        /// 为“输出方向”直接注册一个管道委托。
        /// </summary>
        /// <param name="pipelineDelegate">形如 <c>(context, next) =&gt; ...</c> 的输出委托。</param>
        /// <returns>返回当前构建器以支持链式调用。</returns>
        /// <exception cref="ArgumentNullException">当 <paramref name="pipelineDelegate"/> 为 null 时抛出。</exception>
        IPipelineBuilder<TInput, TOutput> Use(PipelineOutputDelegate<TOutput> pipelineDelegate);

        /// <summary>
        /// 构建输入方向的可执行委托。
        /// </summary>
        /// <returns>输入方向的 <see cref="PipelineExecute{T}"/>。</returns>
        PipelineExecute<TInput> BuildInput();

        /// <summary>
        /// 构建输出方向的可执行委托。
        /// </summary>
        /// <returns>输出方向的 <see cref="PipelineExecute{T}"/>。</returns>
        PipelineExecute<TOutput> BuildOutput();
    }
}
