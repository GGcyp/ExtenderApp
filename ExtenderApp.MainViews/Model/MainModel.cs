using ExtenderApp.Abstract;


namespace ExtenderApp.MainViews
{
    public class MainModel
    {
        public IMainWindow MainWindow { get; }
        public IMainView? CurrentMainView { get; set; }
        public Action? ToHomeAction { get; set; }
        public Action<Type>? ToRunAction { get; set; }

        public MainModel(IMainWindow mainWindow)
        {
            MainWindow = mainWindow;
        }
    }
}
