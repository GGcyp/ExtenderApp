using ExtenderApp.Abstract;
using ExtenderApp.MainViews.Models;
using ExtenderApp.ViewModels;
using ExtenderApp.Views.Commands;
using ExtenderApp.Views.CutsceneViews;

namespace ExtenderApp.MainViews.ViewModels
{
    public class MainView_RunViewModel : ExtenderAppViewModel<MainView_Run, MainModel>
    {
        public NoValueCommand ToMainViewCommand { get; set; }

        public MainView_RunViewModel(MainModel model, IServiceStore serviceStore) : base(model, serviceStore)
        {
            ToMainViewCommand = new(ToMainView);
        }

        private void ToMainView()
        {
            var cutscene = NavigateTo<CutsceneView>();
            Model.CurrentCutsceneView = cutscene;
            cutscene.Start();
            Task.Run(async () =>
            {
                await Task.Delay(500);
                _serviceStore.DispatcherService.Invoke(() =>
                {
                    Model.CurrentMainView = NavigateTo<MainView>();
                    cutscene.End(() =>
                    {
                        Model.CurrentCutsceneView = null;
                    });
                });
            });
        }
    }
}
