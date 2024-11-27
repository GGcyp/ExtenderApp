using System.Runtime.Loader;


namespace ExtenderApp.Mod
{
    internal class ModLoader
    {
        private void Load(ModDetails details)
        {

            var loadContext = new AssemblyLoadContext(details.Title, true);
            using (var stream = new FileStream(details.StartupDll, FileMode.Open, FileAccess.Read))
            {
                loadContext.LoadFromStream(stream);
                foreach(var item in loadContext.Assemblies)
                {

                }
            }
        }

        public void Unload()
        {
            //if (_plugin == null) return false;
            //_loadContext.Unload();
            //_loadContext = null;
            //_plugin = null;
            //return true;
        }

    }
}
