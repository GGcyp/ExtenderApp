using AppHost.Extensions.DependencyInjection;
using ExtenderApp.Abstract;
using ExtenderApp.Data;

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
    public class PipelineBuilder<T> : IPipelineBuilder<T> where T : PipelineContext
    {
        private readonly IServiceProvider? _serviceProvider;

        /// <summary>
        /// 维护已组合的管道入口委托。为 null 表示尚未注册任何中间件/委托。
        /// </summary>
        private PipelineDelegate<T>? middleware;

        public PipelineBuilder()
        {
        }

        /// <summary>
        /// 使用指定的 <see cref="IServiceProvider"/> 初始化管道构建器。
        /// </summary>
        /// <param name="serviceProvider">用于解析中间件实例的服务提供程序。</param>
        public PipelineBuilder(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        /// <summary>
        /// 从容器解析并注册一个中间件。
        /// </summary>
        /// <typeparam name="TMiddleware">中间件类型，必须实现 <see cref="IMiddleware{T}"/>。</typeparam>
        /// <returns>返回当前构建器以支持链式调用。</returns>
        /// <exception cref="InvalidOperationException">当容器中未注册该中间件时抛出。</exception>
        public IPipelineBuilder<T> Use<TMiddleware>()
            where TMiddleware : IMiddleware<T>
        {
            ArgumentNullException.ThrowIfNull(_serviceProvider);

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
        /// <c>Task InvokeAsync(TLinkClient context, Func&lt;Task&gt; next)</c>。
        /// </remarks>
        public IPipelineBuilder<T> Use(IMiddleware<T> middleware)
        {
            ArgumentNullException.ThrowIfNull(middleware);

            // 方法组转换为 PipelineDelegate<TLinkClient>
            return Use(middleware.InvokeAsync);
        }

        /// <summary>
        /// 直接注册一个管道委托。
        /// </summary>
        /// <param name="pipelineDelegate">形如 <c>(context, next) =&gt; ...</c> 的管道委托。</param>
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

        /// <summary>
        /// 构建并返回最终可执行的管道委托。
        /// </summary>
        /// <returns>可执行的 <see cref="PipelineExecute{T}"/>。</returns>
        /// <remarks>
        /// 若未注册任何步骤，将返回空操作委托。
        /// </remarks>
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
        private PipelineDelegate<T> Run(PipelineDelegate<T> lastPipelineDelegate)
        {
            return CreateDelegate(lastPipelineDelegate, (context, next) => Task.CompletedTask);
        }

        /// <summary>
        /// 将两个委托组合为一个链式委托：先执行 <paramref name="lastPipelineDelegate"/>，其 next 指向 <paramref name="nextPipelineDelegate"/>。
        /// </summary>
        private PipelineDelegate<T> CreateDelegate(PipelineDelegate<T> lastPipelineDelegate, PipelineDelegate<T> nextPipelineDelegate)
        {
            return (context, next) =>
            {
                // outer(context, next: () => inner(context, next))
                return lastPipelineDelegate(context, () => nextPipelineDelegate(context, next));
            };
        }
    }

    /// <summary>
    /// 支持“输入方向/输出方向”分离的管道构建器。
    /// </summary>
    /// <typeparam name="TInput">输入方向上下文类型。</typeparam>
    /// <typeparam name="TOutput">输出方向上下文类型。</typeparam>
    public class PipelineBuilder<TInput, TOutput> : IPipelineBuilder<TInput, TOutput>
        where TInput : PipelineContext
        where TOutput : PipelineContext
    {
        private readonly IServiceProvider? _serviceProvider;

        /// <summary>
        /// 维护已组合的管道入口委托。为 null 表示尚未注册任何中间件/委托。
        /// </summary>
        private PipelineInputDelegate<TInput>? inputMiddleware;

        /// <summary>
        /// 维护已组合的管道出口委托。为 null 表示尚未注册任何中间件/委托。
        /// </summary>
        private PipelineOutputDelegate<TOutput>? outputMiddleware;

        /// <summary>
        /// 使用指定的 <see cref="IServiceProvider"/> 初始化管道构建器。
        /// </summary>
        /// <param name="serviceProvider">用于解析中间件实例的服务提供程序。</param>
        public PipelineBuilder(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public PipelineBuilder()
        {
        }

        /// <summary>
        /// 从容器解析并注册一个中间件。
        /// </summary>
        /// <typeparam name="TMiddleware">中间件类型，必须实现 <see cref="IMiddleware{TInput, TOutput}"/>。</typeparam>
        /// <returns>返回当前构建器以支持链式调用。</returns>
        /// <exception cref="InvalidOperationException">当容器中未注册该中间件时抛出。</exception>
        public IPipelineBuilder<TInput, TOutput> Use<TMiddleware>()
            where TMiddleware : IMiddleware<TInput, TOutput>
        {
            ArgumentNullException.ThrowIfNull(_serviceProvider);

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
        /// 要求中间件的 <c>InputInvokeAsync</c> 与 <c>OutputInvokeAsync</c> 分别匹配
        /// <see cref="PipelineInputDelegate{T}"/> 与 <see cref="PipelineOutputDelegate{T}"/> 的签名。
        /// </remarks>
        public IPipelineBuilder<TInput, TOutput> Use(IMiddleware<TInput, TOutput> middleware)
        {
            ArgumentNullException.ThrowIfNull(middleware);

            Use(middleware.InputInvokeAsync);
            Use(middleware.OutputInvokeAsync);
            return this;
        }

        /// <summary>
        /// 为“输入方向”直接注册一个管道委托。
        /// </summary>
        /// <param name="pipelineDelegate">输入方向的委托。</param>
        /// <returns>返回输入方向的构建器以支持链式调用。</returns>
        /// <exception cref="ArgumentNullException">当 <paramref name="pipelineDelegate"/> 为 null 时抛出。</exception>
        public IPipelineBuilder<TInput, TOutput> Use(PipelineInputDelegate<TInput> pipelineDelegate)
        {
            ArgumentNullException.ThrowIfNull(pipelineDelegate);

            inputMiddleware = inputMiddleware is null
                ? pipelineDelegate
                : CreateDelegate(inputMiddleware, pipelineDelegate);

            return this;
        }

        /// <summary>
        /// 为“输出方向”直接注册一个管道委托。
        /// </summary>
        /// <param name="pipelineDelegate">输出方向的委托。</param>
        /// <returns>返回输出方向的构建器以支持链式调用。</returns>
        /// <exception cref="NotImplementedException">当前未实现输出方向的注册。</exception>
        public IPipelineBuilder<TInput, TOutput> Use(PipelineOutputDelegate<TOutput> pipelineDelegate)
        {
            ArgumentNullException.ThrowIfNull(pipelineDelegate);

            outputMiddleware = outputMiddleware is null
                ? pipelineDelegate
                : CreateDelegate(outputMiddleware, pipelineDelegate);

            return this;
        }

        /// <summary>
        /// 构建输入方向的可执行委托。
        /// </summary>
        /// <returns>输入方向的 <see cref="PipelineExecute{T}"/>。</returns>
        public PipelineExecute<TInput> BuildInput()
        {
            if (inputMiddleware == null)
                return (context) => Task.CompletedTask;

            inputMiddleware = Run(inputMiddleware);
            return (context) => inputMiddleware(context, () => Task.CompletedTask);
        }

        /// <summary>
        /// 构建输出方向的可执行委托。
        /// </summary>
        /// <returns>输出方向的 <see cref="PipelineExecute{T}"/>。</returns>
        public PipelineExecute<TOutput> BuildOutput()
        {
            if (outputMiddleware == null)
                return (context) => Task.CompletedTask;

            outputMiddleware = Run(outputMiddleware);
            return (context) => outputMiddleware(context, () => Task.CompletedTask);
        }

        /// <summary>
        /// 在“输入方向”的链尾追加一个终止委托（不再继续调用 next）。
        /// </summary>
        /// <param name="lastPipelineDelegate">当前已组合的最后一个输入方向委托。</param>
        /// <returns>封口后的输入方向委托。</returns>
        /// <remarks>
        /// 用于 <see cref="BuildInput"/> 之前对链进行收尾，确保最终对 <c>next</c> 的调用为无操作，以稳定执行语义。
        /// </remarks>
        private PipelineInputDelegate<TInput> Run(PipelineInputDelegate<TInput> lastPipelineDelegate)
        {
            return CreateDelegate(lastPipelineDelegate, (context, next) => Task.CompletedTask);
        }

        /// <summary>
        /// 在“输出方向”的链尾追加一个终止委托（不再继续调用 next）。
        /// </summary>
        /// <param name="lastPipelineDelegate">当前已组合的最后一个输出方向委托。</param>
        /// <returns>封口后的输出方向委托。</returns>
        /// <remarks>
        /// 用于 <see cref="BuildOutput"/> 之前对链进行收尾，确保最终对 <c>next</c> 的调用为无操作，以稳定执行语义。
        /// </remarks>
        private PipelineOutputDelegate<TOutput> Run(PipelineOutputDelegate<TOutput> lastPipelineDelegate)
        {
            return CreateDelegate(lastPipelineDelegate, (context, next) => Task.CompletedTask);
        }

        /// <summary>
        /// 将两个“输入方向”委托组合为一个链式委托：先执行 <paramref name="lastPipelineDelegate"/>，
        /// 其 <c>next</c> 指向 <paramref name="nextPipelineDelegate"/>。
        /// </summary>
        /// <param name="lastPipelineDelegate">外层（先执行）的输入方向委托。</param>
        /// <param name="nextPipelineDelegate">内层（后执行）的输入方向委托。</param>
        /// <returns>组合后的输入方向委托。</returns>
        private PipelineInputDelegate<TInput> CreateDelegate(PipelineInputDelegate<TInput> lastPipelineDelegate, PipelineInputDelegate<TInput> nextPipelineDelegate)
        {
            return (context, next) =>
            {
                return lastPipelineDelegate(context, () => nextPipelineDelegate(context, next));
            };
        }

        /// <summary>
        /// 将两个“输出方向”委托组合为一个链式委托：先执行 <paramref name="lastPipelineDelegate"/>，
        /// 其 <c>next</c> 指向 <paramref name="nextPipelineDelegate"/>。
        /// </summary>
        /// <param name="lastPipelineDelegate">外层（先执行）的输出方向委托。</param>
        /// <param name="nextPipelineDelegate">内层（后执行）的输出方向委托。</param>
        /// <returns>组合后的输出方向委托。</returns>
        private PipelineOutputDelegate<TOutput> CreateDelegate(PipelineOutputDelegate<TOutput> lastPipelineDelegate, PipelineOutputDelegate<TOutput> nextPipelineDelegate)
        {
            return (context, next) =>
            {
                return lastPipelineDelegate(context, () => nextPipelineDelegate(context, next));
            };
        }
    }
}