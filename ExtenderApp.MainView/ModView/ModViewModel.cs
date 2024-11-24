using ExtenderApp.Abstract;
using ExtenderApp.Mod;
using ExtenderApp.ViewModels;

namespace ExtenderApp.MainView
{
    public class ModViewModle : BaseViewModel
    {
        public ModStore ModStore { get; }

        public ModViewModle(ModStore mods, IServiceStore serviceStore) : base(serviceStore)
        {
            ModStore = mods;
        }

        public void OpenMod(ModDetails modDetails)
        {

        }
    }
}
