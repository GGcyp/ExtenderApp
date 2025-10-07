using System.Diagnostics;
using AppHost;
using AppHost.Builder;
using AppHost.Extensions.DependencyInjection;
using ExtenderApp.Abstract;

namespace ExtenderApp
{
    internal class Starter
    {
        private static ExtenderApplication_WPF app;

        [STAThread]
        public static void Main(string[] args)
        {
            app = new ExtenderApplication_WPF();

            try
            {
                app.Run();
            }
            catch (Exception ex)
            {
                app.Eorrer(ex);
            }
        }
    }
}
