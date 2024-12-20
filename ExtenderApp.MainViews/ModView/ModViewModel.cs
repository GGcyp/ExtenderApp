using ExtenderApp.Abstract;
using ExtenderApp.Data;
using ExtenderApp.Mod;
using ExtenderApp.ViewModels;

namespace ExtenderApp.MainViews
{
    public class ModViewModle : ExtenderAppViewModel
    {
        public ModStore ModStore { get; }
        private readonly IModLoader _modLoad;
        private readonly MainModel _mainModel;

        public ModViewModle(ModStore mods, IModLoader modLoad, MainModel mainModel, IServiceStore serviceStore) : base(serviceStore)
        {
            ModStore = mods;
            _modLoad = modLoad;
            _mainModel = mainModel;
        }

        public void OpenMod(ModDetails modDetails)
        {
            _modLoad.Load(modDetails);
            _mainModel.ToRunAction?.Invoke(modDetails.StartupType);
        }
    }
}
