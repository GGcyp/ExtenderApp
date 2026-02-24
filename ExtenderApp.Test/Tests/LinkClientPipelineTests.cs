using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using ExtenderApp.Abstract;
using ExtenderApp.Contracts;
using ExtenderApp.Buffer;

namespace ExtenderApp.Test.Tests
{
    /// <summary>
    /// 针对内部类型 `ExtenderApp.Common.Networks.LinkClients.LinkClientPipeline` 的轻量级测试。
    /// 通过反射创建实例并验证增删改查与枚举行为。
    /// </summary>
    public static class LinkClientPipelineTests
    {
        public static void RunAll()
        {
            try
            {
                TestAddRemoveReplaceEnumerate();
                Debug.Print("LinkClientPipelineTests: All tests finished.");
            }
            catch (Exception ex)
            {
                Debug.Print($"LinkClientPipelineTests: Exception: {ex}");
            }
        }

        private static void TestAddRemoveReplaceEnumerate()
        {
            var pipeline = CreatePipeline();

            // 创建几个简单的处理器实例
            var h1 = new DummyHandler("h1");
            var h2 = new DummyHandler("h2");
            var h3 = new DummyHandler("h3");

            pipeline.AddLast("handler1", h1);
            pipeline.AddLast("handler2", h2);

            // 枚举应至少包含我们添加的两个处理器
            var count = pipeline.Cast<ILinkClientHandler>().Count();
            Debug.Print($"After AddLast count={count}");

            // 在 handler2 之前插入 h3
            pipeline.AddBefore("handler2", "handler1.5", h3);
            count = pipeline.Cast<ILinkClientHandler>().Count();
            Debug.Print($"After AddBefore count={count}");

            // 移除指定名称并验证返回实例
            var removed = pipeline.Remove("handler1");
            Debug.Print(ReferenceEquals(removed, h1) ? "Remove by name: OK" : "Remove by name: FAIL");

            // 使用 Remove(handler) 移除实例
            pipeline.Remove(h3);
            count = pipeline.Cast<ILinkClientHandler>().Count();
            Debug.Print($"After Remove(handler) count={count}");

            // Replace handler2 -> new handler
            var newHandler = new DummyHandler("h2-replaced");
            pipeline.Replace("handler2", "handler2-new", newHandler);
            var removed2 = pipeline.Remove("handler2-new");
            Debug.Print(ReferenceEquals(removed2, newHandler) ? "Replace by name: OK" : "Replace by name: FAIL");

            Debug.Print("TestAddRemoveReplaceEnumerate completed.");
        }

        private static ILinkClientPipeline CreatePipeline()
        {
            // 在运行时查找已加载的 ExtenderApp.Common 程序集并通过反射构造管道实例
            var asm = AppDomain.CurrentDomain.GetAssemblies()
                .FirstOrDefault(a => string.Equals(a.GetName().Name, "ExtenderApp.Common", StringComparison.OrdinalIgnoreCase));
            if (asm == null)
                throw new InvalidOperationException("Cannot find assembly 'ExtenderApp.Common' in current AppDomain.");

            var type = asm.GetType("ExtenderApp.Common.Networks.LinkClients.LinkClientPipeline", throwOnError: true);
            var instance = Activator.CreateInstance(type)!;
            return (ILinkClientPipeline)instance;
        }

        private sealed class DummyHandler : ILinkClientHandler
        {
            public string Id { get; }

            public DummyHandler(string id) => Id = id;

            public void Added(ILinkClientHandlerContext context)
            {
                Debug.Print($"Handler {Id} added to pipeline.");
            }

            public void Removed(ILinkClientHandlerContext context)
            {
                Debug.Print($"Handler {Id} removed from pipeline.");
            }

            public void Active(ILinkClientHandlerContext context)
            {
                Debug.Print($"Handler {Id} active.");
            }

            public void Inactive(ILinkClientHandlerContext context)
            {
                Debug.Print($"Handler {Id} inactive.");
            }

            public ValueTask<Result> ConnectAsync(ILinkClientHandlerContext context, EndPoint remoteAddress, EndPoint localAddress, CancellationToken token = default)
                => new ValueTask<Result>(Result.Success());

            public ValueTask<Result> DisconnectAsync(ILinkClientHandlerContext context, CancellationToken token = default)
                => new ValueTask<Result>(Result.Success());

            public ValueTask<Result> CloseAsync(ILinkClientHandlerContext context, CancellationToken token = default)
                => new ValueTask<Result>(Result.Success());

            public ValueTask<Result> BindAsync(ILinkClientHandlerContext context, EndPoint localAddress, CancellationToken token = default)
                => new ValueTask<Result>(Result.Success());

            public ValueTask ExceptionCaught(ILinkClientHandlerContext context, Exception exception)
                => ValueTask.CompletedTask;

            public ValueTask<Result<int>> InboundHandleAsync(ILinkClientHandlerContext context, ValueCache cache, CancellationToken token = default)
                => new ValueTask<Result<int>>(Result.Success(0));

            public ValueTask<Result> OutboundHandleAsync(ILinkClientHandlerContext context, ValueCache cache, CancellationToken token = default)
                => new ValueTask<Result>(Result.Success());

            public void Dispose()
            { }

            public override string ToString() => Id;
        }
    }
}