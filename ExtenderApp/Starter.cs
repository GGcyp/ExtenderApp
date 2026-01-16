

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
                app.LogEorrer(ex);
            }
        }
    }
}
