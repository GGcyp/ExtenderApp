using System.Diagnostics;
using AppHost;
using AppHost.Builder;
using AppHost.Extensions.DependencyInjection;
using ExtenderApp.Abstract;

namespace ExtenderApp
{
    internal class Starter
    {
        private static AppHostApplication application;
        private static App app;

        [STAThread]
        public static void Main(string[] args)
        {
            Debug.Close();
            Stopwatch sw = new Stopwatch();
            sw.Start();
            Debug.Print($"开始启动 : {DateTime.Now}");
            app = new App();

            var builder = AppHostApplication.CreateBuilder();

            //builder.FindStarupForFolder(AppSetting.AppBinFolderName);
            builder.FindStarupForFolder("bin");
            Debug.Print($"启动成功 : {DateTime.Now}");

            Debug.Print($"开始生成服务 : {DateTime.Now}");
            application = builder.Builde();
            Debug.Print($"生成服务成功 : {DateTime.Now}");

            ILogingService? logingService = application.Service.GetService<ILogingService>();

            try
            {
                sw.Stop();
                logingService?.Print(new Data.LogInfo()
                {
                    LogLevel = Data.LogLevel.INFO,
                    Message = $"启动成功 本次启动用时 ：{sw.ElapsedMilliseconds} 毫秒",
                    Source = nameof(Starter),
                    Time = DateTime.Now,
                    ThreadId = Thread.CurrentThread.ManagedThreadId
                });
                application.Run();
                app.Run();
            }
            catch (Exception ex)
            {
                Debug.Print(ex.Message);
                throw new Exception(ex.Message);
            }
        }
    }
}
