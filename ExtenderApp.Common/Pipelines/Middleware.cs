

using ExtenderApp.Abstract;

namespace ExtenderApp.Common.Pipelines
{
    public abstract class MiddlewareBase<T> : DisposableObject, IMiddleware<T> where T : IPipelineContext
    {
        public abstract Task InvokeAsync(T context, Func<Task> next);
    }
}
