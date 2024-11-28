using ExtenderApp.Data;

namespace ExtenderApp.Abstract
{
    public interface IModLoader
    {
        void Load(ModDetails details);
        void Unload(ModDetails details);
    }
}
