using AppHost.Extensions.DependencyInjection;
using ExtenderApp.Abstract;

namespace ExtenderApp.Common.Pipelines
{
    /// <summary>
    /// 支持入/出分离的管道构建器。
    /// 通过链式注册中间件（或委托）并最终构建一个可执行的 <see cref="PipelineDelegate{T}"/>。
    /// </summary>
    /// <typeparam name="T">管道上下文类型，必须实现 <see cref="IPipelineContext"/>。</typeparam>
    /// <remarks>
    /// 使用说明：
    /// 1. 通过 <see cref="Use{TMiddleware}"/> 或 <see cref="Use(IMiddleware{T})"/> 注册中间件；
    /// 2. 或直接使用 <see cref="Use(PipelineDelegate{T})"/> 注册委托；
    /// 3. 调用 <see cref="Build"/> 生成最终可执行的委托。
    /// 执行顺序：按注册顺序执行（先注册的外层先执行，再调用 next 进入后注册的内层）。
    /// 线程安全：构建器仅用于配置阶段，非线程安全，不建议并发调用 Use/Build。
    /// </remarks>
    public class PipelineBuilder<T> : IPipelineBuilder<T> where T : IPipelineContext
    {
        private readonly IServiceProvider _serviceProvider;

        /// <summary>
        /// 维护已组合的管道入口委托。为 null 表示尚未注册任何中间件/委托。
        /// </summary>
        private PipelineDelegate<T>? middleware;

        /// <summary>
        /// 使用指定的 <see cref="IServiceProvider"/> 初始化管道构建器。
        /// </summary>
        /// <param name="serviceProvider">用于解析中间件实例的服务提供程序。</param>
        public PipelineBuilder(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        /// <summary>
        /// 注册一个从容器解析的中间件。
        /// </summary>
        /// <typeparam name="TMiddleware">中间件类型，必须实现 <see cref="IMiddleware{T}"/>。</typeparam>
        /// <returns>返回当前构建器以支持链式调用。</returns>
        /// <exception cref="InvalidOperationException">当容器中未注册该中间件时抛出。</exception>
        public IPipelineBuilder<T> Use<TMiddleware>()
            where TMiddleware : IMiddleware<T>
        {
            // 注意：此处的局部变量名与字段同名，但作用域不同，不影响字段。
            var middleware = _serviceProvider.GetRequiredService<TMiddleware>();
            return Use(middleware);
        }

        /// <summary>
        /// 注册一个中间件实例。
        /// </summary>
        /// <param name="middleware">中间件实例。</param>
        /// <returns>返回当前构建器以支持链式调用。</returns>
        /// <exception cref="ArgumentNullException">当 <paramref name="middleware"/> 为 null 时抛出。</exception>
        /// <remarks>
        /// 要求中间件的 <c>InvokeAsync</c> 签名与 <see cref="PipelineDelegate{T}"/> 保持一致：
        /// <c>Task InvokeAsync(T context, Func&lt;Task&gt; next)</c>。
        /// </remarks>
        public IPipelineBuilder<T> Use(IMiddleware<T> middleware)
        {
            ArgumentNullException.ThrowIfNull(middleware);

            // 方法组转换为 PipelineDelegate<T>
            return Use(middleware.InvokeAsync);
        }

        /// <summary>
        /// 直接注册一个管道委托。
        /// </summary>
        /// <param name="pipelineDelegate">形如 <c>(ctx, next) =&gt; ...</c> 的管道委托。</param>
        /// <returns>返回当前构建器以支持链式调用。</returns>
        /// <exception cref="ArgumentNullException">当 <paramref name="pipelineDelegate"/> 为 null 时抛出。</exception>
        /// <remarks>
        /// 组合规则：
        /// - 第一次注册：设为入口委托；
        /// - 后续注册：将其作为“下一步”拼接在已有委托之后。
        /// </remarks>
        public IPipelineBuilder<T> Use(PipelineDelegate<T> pipelineDelegate)
        {
            ArgumentNullException.ThrowIfNull(pipelineDelegate);

            middleware = middleware is null
                ? pipelineDelegate
                : CreateDelegate(middleware, pipelineDelegate);

            return this;
        }

        public PipelineExecute<T> Build()
        {
            if (middleware == null)
                return (context) => Task.CompletedTask;

            middleware = Run(middleware);
            return (context) => middleware(context, () => Task.CompletedTask);
        }

        /// <summary>
        /// 在链尾追加一个“终止委托”（不再继续调用 next）。
        /// </summary>
        /// <param name="lastPipelineDelegate">当前已组合好的入口委托。</param>
        /// <returns>追加终止步骤后的入口委托。</returns>
        private PipelineDelegate<T> Run(PipelineDelegate<T> lastPipelineDelegate)
        {
            return CreateDelegate(lastPipelineDelegate, (context, next) => Task.CompletedTask);
        }

        /// <summary>
        /// 将两个委托组合为一个链式委托：先执行 <paramref name="lastPipelineDelegate"/>，其 next 指向 <paramref name="nextPipelineDelegate"/>。
        /// </summary>
        /// <param name="lastPipelineDelegate">上一个（外层）委托。</param>
        /// <param name="nextPipelineDelegate">下一个（内层）委托。</param>
        /// <returns>组合后的入口委托。</returns>
        private PipelineDelegate<T> CreateDelegate(PipelineDelegate<T> lastPipelineDelegate, PipelineDelegate<T> nextPipelineDelegate)
        {
            return (context, next) =>
            {
                // outer(context, next: () => inner(context, next))
                return lastPipelineDelegate(context, () => nextPipelineDelegate(context, next));
            };
        }
    }
}
