using ExtenderApp.Abstract;
using ExtenderApp.MainViews.Models;
using ExtenderApp.ViewModels;
using ExtenderApp.Views.Commands;
using ExtenderApp.Views.CutsceneViews;

namespace ExtenderApp.MainViews.ViewModels
{
    public class PluginViewModle : ExtenderAppViewModel<PluginView, MainModel>
    {
        public NoValueCommand OpenPluginCommand { get; set; }

        public PluginViewModle(MainModel model, IServiceStore serviceStore) : base(model, serviceStore)
        {
            OpenPluginCommand = new NoValueCommand(OpenPlugin, () => Model.CurrentModDetails != null);
        }

        public void OpenPlugin()
        {
            var cutscene = NavigateTo<CutsceneView>();
            Model.CurrentCutsceneView = cutscene;
            cutscene.Start();
            Task.Run(async () =>
            {
                _serviceStore.ModService.LoadPlugin(Model.CurrentModDetails);
                await Task.Delay(500);
                _serviceStore.DispatcherService.Invoke(() =>
                {
                    Model.CurrentMainView = NavigateTo<MainView_Run>();
                    Model.CurrentView = NavigateTo(Model.CurrentModDetails);
                    cutscene.End(() =>
                    {
                        Model.CurrentCutsceneView = null;
                    });
                });
            });
        }
    }
}
