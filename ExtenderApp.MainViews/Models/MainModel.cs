using System.ComponentModel;
using ExtenderApp.Abstract;
using ExtenderApp.Data;
using ExtenderApp.Services;


namespace ExtenderApp.MainViews.Models
{
    public class MainModel : INotifyPropertyChanged
    {
        public IView? CurrentMainView { get; set; }

        public IView? CurrentCutsceneView { get; set; }

        public IView? CurrentView { get; set; }

        public Action? ToHomeAction { get; set; }
        public Action? ToRunAction { get; set; }
        public PluginDetails? CurrentModDetails { get; set; }
        public PluginStore? PluginStore { get; set; }

        public event PropertyChangedEventHandler? PropertyChanged;

        public MainModel(PluginStore store)
        {
            PluginStore = store;
        }
    }
}
