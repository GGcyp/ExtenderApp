using ExtenderApp.Abstract;
using ExtenderApp.Data;


namespace ExtenderApp.MainViews
{
    public class MainModel
    {
        public IMainWindow MainWindow { get; }
        public IMainView CurrentMainView { get; set; }
        public Action ToHomeAction { get; set; }
        public Action ToRunAction { get; set; }
        public PluginDetails CurrentModDetails { get; set; }

        public MainModel(IMainWindow mainWindow)
        {
            MainWindow = mainWindow;
        }
    }
}
