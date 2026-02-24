using System;
using System.Linq;
using System.Net;
using BenchmarkDotNet.Attributes;
using ExtenderApp.Abstract;
using ExtenderApp.Buffer;
using ExtenderApp.Common.Networks.LinkClients;
using ExtenderApp.Contracts;

[MemoryDiagnoser]
public class LinkClientPipelineBenchmarks
{
    private ILinkClientPipeline pipeline;
    private ILinkClientHandler dummy;

    [GlobalSetup]
    public void Setup()
    {
        // 通过引用程序集并反射创建内部类型实例（如果类型为 internal）
        var asm = AppDomain.CurrentDomain
            .GetAssemblies()
            .FirstOrDefault(a => a.GetName().Name.Equals("ExtenderApp.Common", StringComparison.OrdinalIgnoreCase))
            ?? throw new InvalidOperationException("ExtenderApp.Common not loaded");

        var type = asm.GetType("ExtenderApp.Common.Networks.LinkClients.LinkClientPipeline", throwOnError: true);
        pipeline = (ILinkClientPipeline)Activator.CreateInstance(type)!;

        // 简单实现的 ILinkClientHandler，用于测试
        dummy = new DummyHandler();
    }

    [Benchmark]
    public void AddLast_Remove()
    {
        pipeline.AddLast("h", dummy);
        pipeline.Remove("h");
    }

    [Benchmark]
    public void AddMany_Enumerate()
    {
        for (int i = 0; i < 100; i++)
            pipeline.AddLast("h" + i, dummy);
        var cnt = pipeline.Cast<ILinkClientHandler>().Count();
        // 清理
        for (int i = 0; i < 100; i++)
            pipeline.Remove("h" + i);
    }

    private class DummyHandler : LinkClientHandler
    {
    }
}