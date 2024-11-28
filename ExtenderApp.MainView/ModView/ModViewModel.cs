using ExtenderApp.Abstract;
using ExtenderApp.Data;
using ExtenderApp.Mod;
using ExtenderApp.ViewModels;

namespace ExtenderApp.MainView
{
    public class ModViewModle : BaseViewModel
    {
        public ModStore ModStore { get; }
        private readonly IModLoader _modLoad;

        public ModViewModle(ModStore mods, IModLoader modLoad, IServiceStore serviceStore) : base(serviceStore)
        {
            ModStore = mods;
            _modLoad = modLoad;
        }

        public void OpenMod(ModDetails modDetails)
        {
            _modLoad.Load(modDetails);
            NavigateTo(modDetails.StartupType);
        }
    }
}
