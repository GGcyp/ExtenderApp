using ExtenderApp.Contracts;

namespace ExtenderApp.Common
{
    /// <summary>
    /// 提供与 <see cref="Task"/> / <see cref="ValueTask"/> 相关的辅助扩展方法。 该类为部分类（partial），其他文件中可能包含更多扩展方法。
    /// </summary>
    public static partial class Utilitity
    {
        /// <summary>
        /// 尝试执行一个不带返回值的异步 <see cref="Task"/>，捕获异常并将结果封装为 <see cref="Result"/> 对象。
        /// </summary>
        /// <typeparam name="T">占位类型参数（当前方法签名中未使用）。建议移除此类型参数以消除混淆。</typeparam>
        /// <param name="task">要执行的异步任务。</param>
        /// <param name="continueOnCapturedContext">指示是否在捕获的上下文中继续执行后续代码。默认为 true。</param>
        /// <returns>如果任务成功完成，返回 <see cref="Result.Success"/>；如果抛出异常，返回包含异常信息的 <see cref="Result"/>。</returns>
        public static async Task<Result> TryCatchAsync<T>(this Task task, bool continueOnCapturedContext = true)
        {
            try
            {
                await task.ConfigureAwait(continueOnCapturedContext);
                return Result.Success();
            }
            catch (Exception ex)
            {
                return Result.FromException(ex);
            }
        }

        /// <summary>
        /// 尝试执行一个返回结果的异步 <see cref="Task{TResult}"/>，捕获异常并将结果或异常封装为 <see cref="Result{T}"/>。
        /// </summary>
        /// <typeparam name="T">异步任务返回的结果类型。</typeparam>
        /// <param name="task">要执行的异步任务。</param>
        /// <param name="continueOnCapturedContext">指示是否在捕获的上下文中继续执行后续代码。默认为 true。</param>
        /// <returns>任务成功时返回包含结果的 <see cref="Result{T}"/>；任务失败时返回包含异常信息的 <see cref="Result{T}"/>。</returns>
        public static async Task<Result<T>> TryCatchAsync<T>(this Task<T> task, bool continueOnCapturedContext = true)
        {
            try
            {
                var result = await task.ConfigureAwait(continueOnCapturedContext);
                return Result.Success(result);
            }
            catch (Exception ex)
            {
                return Result.FromException<T>(ex);
            }
        }

        /// <summary>
        /// 阻塞等待一个 <see cref="ValueTask"/> 完成（同步等待）。仅当任务尚未完成时才调用等待。
        /// </summary>
        /// <param name="task">要等待完成的 <see cref="ValueTask"/>。</param>
        /// <param name="continueOnCapturedContext">指示是否在捕获的上下文中继续执行后续代码。默认为 true。</param>
        public static void Await(this ValueTask task, bool continueOnCapturedContext = true)
        {
            if (!task.IsCompleted)
            {
                task.ConfigureAwait(continueOnCapturedContext).GetAwaiter().GetResult();
            }
        }

        /// <summary>
        /// 阻塞等待一个返回值的 <see cref="ValueTask{TResult}"/> 完成并返回其结果（同步等待）。仅当任务尚未完成时才调用等待。
        /// </summary>
        /// <typeparam name="T"><see cref="ValueTask{TResult}"/> 的结果类型。</typeparam>
        /// <param name="task">要等待完成的 <see cref="ValueTask{TResult}"/>。</param>
        /// <param name="continueOnCapturedContext">指示是否在捕获的上下文中继续执行后续代码。默认为 true。</param>
        /// <returns>任务完成时的结果。</returns>
        public static T Await<T>(this ValueTask<T> task, bool continueOnCapturedContext = true)
        {
            if (!task.IsCompleted)
            {
                task.ConfigureAwait(continueOnCapturedContext).GetAwaiter().GetResult();
            }
            return task.Result;
        }
    }
}