﻿using AppHost.Builder;
using AppHost.Extensions.DependencyInjection;
using ExtenderApp.Common.Buffers;
using ExtenderApp.Common.Caches;
using ExtenderApp.Common.Hash;
using ExtenderApp.Common.Networks;
using ExtenderApp.Common.Pipelines;

namespace ExtenderApp.Common
{
    /// <summary>
    /// 通用层启动类，继承自Startup基类
    /// </summary>
    public class CommonStartup : Startup
    {
        public override void AddService(IServiceCollection services)
        {
            services.AddIO();
            services.AddBufferFactory();
            services.AddObjectPool();
            services.AddNetwork();
            services.AddHash();
            services.AddCache();
            services.AddPipeline();
        }
    }
}
