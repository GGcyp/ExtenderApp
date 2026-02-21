using System.Diagnostics;
using ExtenderApp.Common.Threads;

namespace ExtenderApp.Test.Tests
{
    /// <summary>
    /// <see cref="AwaitableEventArgs"/> 测试集合。
    /// </summary>
    internal static class AwaitableEventArgsTests
    {
        /// <summary>
        /// 运行全部测试用例。
        /// </summary>
        public static void RunAll()
        {
            Debug.Print("-- AwaitableEventArgs 测试开始 --");
            Task.Run(async () =>
            {
                await TestActionResult();
                await TestFuncResult();
                await TestException();
                await TestDelayedActionResult();
                await TestDelayedFuncResult();
                await TestCancelableAction();
                await Test1();
                await Test3();
                await Test4();
                await TestMultipleAwaitGeneric();
                await TestMultipleAwaitGeneric2();
                Debug.Print("-- AwaitableEventArgs 测试结束 --");
            });
        }

        private static async ValueTask TestActionResult()
        {
            bool executed = false;
            await AwaitableEventArgs.FromResult(() => executed = true);

            Debug.Print(executed ? "[通过] TestActionResult" : "[失败] TestActionResult");
        }

        private static async ValueTask TestFuncResult()
        {
            int result = await AwaitableEventArgs.FromResult(() => 42);

            Debug.Print(result == 42 ? "[通过] TestFuncResult" : "[失败] TestFuncResult");
        }

        private static async ValueTask TestException()
        {
            try
            {
                await AwaitableEventArgs.FromException(new InvalidOperationException("test"));
                Debug.Print("[失败] TestException");
            }
            catch (InvalidOperationException)
            {
                Debug.Print("[通过] TestException");
            }
            catch (Exception ex)
            {
                Debug.Print($"[失败] TestException 异常类型 {ex.GetType().Name}");
            }
        }

        private static async ValueTask TestDelayedActionResult()
        {
            const int delayMilliseconds = 200;
            var stopwatch = Stopwatch.StartNew();
            await AwaitableEventArgs.FromResult(() => Task.Delay(delayMilliseconds).GetAwaiter().GetResult());
            stopwatch.Stop();

            Debug.Print(stopwatch.ElapsedMilliseconds >= delayMilliseconds
                ? "[通过] TestDelayedActionResult"
                : $"[失败] TestDelayedActionResult 用时 {stopwatch.ElapsedMilliseconds}ms");
        }

        private static async ValueTask TestDelayedFuncResult()
        {
            const int delayMilliseconds = 150;
            var stopwatch = Stopwatch.StartNew();
            int result = await AwaitableEventArgs.FromResult(() =>
            {
                Task.Delay(delayMilliseconds);
                return 7;
            });
            stopwatch.Stop();

            Debug.Print(result == 7 && stopwatch.ElapsedMilliseconds >= delayMilliseconds
                ? "[通过] TestDelayedFuncResult"
                : $"[失败] TestDelayedFuncResult 结果 {result}, 用时 {stopwatch.ElapsedMilliseconds}ms");
        }

        private static async ValueTask TestCancelableAction()
        {
            using var cts = new CancellationTokenSource();
            cts.Cancel();
            var awaitable = AwaitableEventArgs.GetAwaitable().SetResult(token => token.ThrowIfCancellationRequested(), cts.Token);

            try
            {
                await awaitable;
                Debug.Print("[失败] TestCancelableAction");
            }
            catch (OperationCanceledException)
            {
                Debug.Print("[通过] TestCancelableAction");
            }
            catch (Exception ex)
            {
                Debug.Print($"[失败] TestCancelableAction 异常类型 {ex.GetType().Name}");
            }
        }

        private static async ValueTask Test1()
        {
            var stopwatch = Stopwatch.StartNew();
            var args1 = AwaitableEventArgs<int>.GetAwaitable();
            Test2(args1).ConfigureAwait(false);
            Debug.Print("开始等待");
            await Task.Delay(1000);
            stopwatch.Stop();
            Debug.Print($"结束等待 {stopwatch.ElapsedMilliseconds}ms");
            args1.SetResult(123);

            var args2 = AwaitableEventArgs<int>.GetAwaitable();
            Debug.Print((args1 == args2).ToString());
            Test2(args2).ConfigureAwait(false);
            await Task.Delay(1000);
            args2.SetResult(456);
        }

        private static async ValueTask Test2(ValueTask<int> args)
        {
            int result = await args;
            Debug.Print($"接受到的数据为: {result}");
        }

        private static async Task Test3()
        {
            var args = AwaitableEventArgs.GetAwaitable();
            for (int i = 0; i < 5; i++)
            {
                Temp(args, i);
            }

            await Task.Delay(1000);
            args.SetResult();

            async ValueTask Temp(AwaitableEventArgs args, int index)
            {
                await args;
                Debug.Print($"Task {index} received signal");
            }
        }

        private static async Task Test4()
        {
            var args = AwaitableEventArgs.GetAwaitable();
            for (int i = 0; i < 5; i++)
            {
                Temp(args, i);
                if (i == 2)
                {
                    args.SetResult();
                }
            }

            await Task.Delay(1000);

            async ValueTask Temp(AwaitableEventArgs args, int index)
            {
                await args;
                Debug.Print($"Task {index} received signal");
            }
        }

        private static async Task TestMultipleAwaitGeneric()
        {
            var args = AwaitableEventArgs<int>.GetAwaitable();

            for (int i = 0; i < 5; i++)
            {
                Temp(args, i);
            }

            await Task.Delay(1000);
            args.SetResult(99);

            async ValueTask Temp(AwaitableEventArgs<int> args, int index)
            {
                int result = await args;
                Debug.Print($"Task {index} received result {result}");
            }
        }

        private static async Task TestMultipleAwaitGeneric2()
        {
            var args = AwaitableEventArgs<int>.GetAwaitable();

            for (int i = 0; i < 5; i++)
            {
                Temp(args, i);
                if (i == 2)
                {
                    args.SetResult(99);
                }
            }

            await Task.Delay(1000);

            async ValueTask Temp(AwaitableEventArgs<int> args, int index)
            {
                int result = await args;
                Debug.Print($"Task {index} received result {result}");
            }
        }
    }
}