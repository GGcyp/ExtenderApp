using System.Reflection;
using System.Threading.Tasks;
using AppHost.Extensions.DependencyInjection;
using ExtenderApp.Abstract;
using ExtenderApp.Data;

namespace ExtenderApp.Common.Networks
{
    /// <summary>
    /// 提供与链接操作相关的扩展方法。
    /// </summary>
    internal static class LinkerExtensions
    {
        private static readonly MethodInfo createListenerLinkerMethodInfo = typeof(IListenerLinkerFactory).GetMethod("CreateListenerLinker");

        /// <summary>
        /// 向服务集合中添加链接操作相关服务。
        /// </summary>
        /// <param name="services">服务集合。</param>
        public static IServiceCollection AddLinker(this IServiceCollection services)
        {
            services.AddTcpLinker();
            services.AddScoped<ILinkerFactory, LinkerFactory>();
            services.AddScoped<IListenerLinkerFactory, ListenerLinkerFactory>();

            services.AddTransient(typeof(IListenerLinker<>), (p, o) =>
            {
                var types = o as Type[];
                if (types is null)
                    return null;

                Type elementType = types[0];
                if (!elementType.IsAssignableTo(typeof(ILinker)))
                    return null;

                var listenerMethodInfo = createListenerLinkerMethodInfo.MakeGenericMethod(elementType);

                var factory = p.GetRequiredService<IListenerLinkerFactory>();

                var listenerLinker = listenerMethodInfo.Invoke(factory, null);

                return listenerLinker;
            });

            return services;
        }

        public static int Send(this ILinker linker, ReadOnlyMemory<byte> memory)
        {
            return linker.Send(memory.Span);
        }

        public static int Send(this ILinker linker, ref ByteBlock block)
        {
            int len = linker.Send(block);
            block.ReadAdvance(len);
            return len;
        }

        public static int Send(this ILinker linker, ref ByteBuffer buffer)
        {
            ByteBlock block = new((int)buffer.Remaining);
            buffer.TryCopyTo(ref block);
            int len = linker.Send(block);
            buffer.ReadAdvance(len);
            block.Dispose();
            return len;
        }

        public static async ValueTask<int> SendAsync(this ILinker linker, ReadOnlySpan<byte> span, CancellationToken token)
        {
            ByteBlock block = new(span);
            int len = await linker.SendAsync(block);
            block.Dispose();
            return len;
        }

        public static async Task<int> SendAsync(this ILinker linker, ref ByteBlock block, CancellationToken token)
        {
            int len = await linker.SendAsync(block);
            block.ReadAdvance(len);
            return len;
        }

        public static void Send(this ILinker linker, ref ByteBuffer buffer, CancellationToken token)
        {
            ByteBlock block = new((int)buffer.Remaining);
            buffer.TryCopyTo(ref block);
            int len = linker.Send(block);
            buffer.ReadAdvance(len);
            block.Dispose();
        }
    }
}
