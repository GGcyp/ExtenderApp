namespace ExtenderApp.Abstract
{
    public interface ILinkClientPlugunManager
    {
        ILinkClientPluginManager? PluginManager { get; }

        void SetClientPluginManager(ILinkClientPluginManager pluginManager);
    }
}