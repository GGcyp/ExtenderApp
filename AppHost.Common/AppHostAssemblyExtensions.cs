using System.Reflection;

namespace AppHost.Common
{
    /// <summary>
    /// 程序集操作扩展
    /// </summary>
    public static class AppHostAssemblyExtensions
    {
        private const string _AssmeblySearchPattern = "*.dll";

        public static List<Assembly> LoadAssemblyForFolder(string folderPath)
        {
            List<string> paths = new List<string>();
            FilePathHandle.GetAllFiles(folderPath, ref paths, _AssmeblySearchPattern);
            
            List<Assembly> assmblies = new List<Assembly>(paths.Count);
            foreach (var path in paths)
            {
                var assmbly = Assembly.LoadFrom(path);
                assmblies.Add(assmbly);
            }
            return assmblies;
        }
    }
}
