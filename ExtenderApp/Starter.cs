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
            DebugMessage($"开始启动 : {DateTime.Now}");

            var builder = AppHostApplication.CreateBuilder();
            app = new App(builder.Context);

            //builder.FindStarupForFolder(AppSetting.AppBinFolderName);
            builder.LoadAssembliesForFolder("pack");
            builder.FindStarupForFolder("lib");
            DebugMessage($"启动成功 : {DateTime.Now}");

            DebugMessage($"开始生成服务 : {DateTime.Now}");
            application = builder.Builde();
            DebugMessage($"生成服务成功 : {DateTime.Now}");

            ILogingService? logingService = application.ServiceProvider.GetService<ILogingService>();
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
                logingService?.Print(new Data.LogInfo()
                {
                    LogLevel = Data.LogLevel.ERROR,
                    Message = "程序出现问题了！",
                    Source = nameof(Starter),
                    Time = DateTime.Now,
                    ThreadId = Thread.CurrentThread.ManagedThreadId,
                    Exception = ex
                });
            }
        }

#if DEBUG
        private static void DebugMessage(string message)
        {
            Debug.Print(message);
        }
#endif
    }
}
