

using ExtenderApp.Data;

namespace ExtenderApp.Abstract
{
    public interface IClient : ILinker
    {
        Task SendAsync<T>(T data);
        void SetClientPipeline(IPipelineBuilder<LinkerClientContext, LinkerClientContext> builder);
        void SetClientPluginManager(IClientPluginManager pluginManager);
    }
}
