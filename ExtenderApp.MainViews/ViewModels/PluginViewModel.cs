using ExtenderApp.Abstract;
using ExtenderApp.Data;
using ExtenderApp.MainViews.Models;
using ExtenderApp.ViewModels;
using ExtenderApp.Views.Commands;
using ExtenderApp.Views.CutsceneViews;

namespace ExtenderApp.MainViews.ViewModels
{
    public class PluginViewModle : ExtenderAppViewModel<PluginView, MainModel>
    {
        public RelayCommand<PluginDetails> OpenPluginCommand { get; set; }

        public PluginViewModle(MainModel model, IServiceStore serviceStore) : base(model, serviceStore)
        {
            OpenPluginCommand = new(OpenPlugin);
        }

        public void OpenPlugin(PluginDetails details)
        {
            var cutscene = NavigateTo<CutsceneView>();
            Model.CurrentCutsceneView = cutscene;
            cutscene.Start();
            Task.Run(async () =>
            {
                await LoadPluginAsync(details);
                _serviceStore.DispatcherService.Invoke(() =>
                {
                    Model.CurrentMainView = NavigateTo<MainView_Run>();
                    Model.CurrentView = NavigateTo(details);
                    cutscene.End(() =>
                    {
                        Model.CurrentCutsceneView = null;
                    });
                });
                await Task.Delay(300);
            });
        }
    }
}
