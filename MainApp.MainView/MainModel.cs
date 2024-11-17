using MainApp.Abstract;
using MainApp.Mod;

namespace MainApp.MainView
{
    public class MainModel : IModel
    {
        private readonly ModStore _store;

        public MainModel(ModStore store)
        {
            _store = store;
        }

        public IModelConverterExecutor Converter => throw new NotImplementedException();

        public void AddDataSource(object? data)
        {
            throw new NotImplementedException();
        }

        public object? GetDataSource()
        {
            throw new NotImplementedException();
        }
    }
}
