namespace ExtenderApp.Abstract
{
    /// <summary>
    /// 管道处理委托（类似 ASP.NET Core 的 RequestDelegate）。
    /// </summary>
    /// <typeparam name="T">管道上下文类型，必须实现 <see cref="IPipelineContext"/>。</typeparam>
    /// <param name="context">当前处理的管道上下文。</param>
    /// <param name="next">
    /// 下一个中间件的委托。调用并等待该委托以继续执行后续步骤；
    /// 若不调用（或不等待）则终止后续流程。
    /// </param>
    /// <returns>表示异步操作的任务。</returns>
    /// <seealso cref="IMiddleware{T}"/>
    public delegate Task PipelineDelegate<T>(T context, Func<Task> next) where T : IPipelineContext;

    public delegate Task PipelineExecute<T>(T context) where T : IPipelineContext;

    /// <summary>
    /// 管道构建器接口：用于按顺序注册中间件或委托，并构建可执行的管道。
    /// </summary>
    /// <typeparam name="T">管道上下文类型，必须实现 <see cref="IPipelineContext"/>。</typeparam>
    /// <remarks>
    /// 执行顺序遵循注册顺序：先注册的作为外层，后注册的作为内层（洋葱模型）。
    /// 构建器用于配置阶段，通常不保证线程安全。
    /// </remarks>
    public interface IPipelineBuilder<T> where T : IPipelineContext
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
        /// 构建并返回最终可执行的管道委托。
        /// </summary>
        /// <returns>可执行的 <see cref="PipelineExecute{T}"/>。</returns>
        /// <remarks>
        /// 未注册任何步骤时的行为（如返回空操作委托）由具体实现决定。
        /// 是否可重复调用也由实现决定，通常建议在配置完成后仅调用一次。
        /// </remarks>
        PipelineExecute<T> Build();

        /// <summary>
        /// 直接注册一个管道委托。
        /// </summary>
        /// <param name="pipelineDelegate">形如 <c>(context, next) =&gt; ...</c> 的委托。</param>
        /// <returns>返回当前构建器以支持链式调用。</returns>
        /// <exception cref="ArgumentNullException">当 <paramref name="pipelineDelegate"/> 为 null 时抛出。</exception>
        IPipelineBuilder<T> Use(PipelineDelegate<T> pipelineDelegate);
    }
}
