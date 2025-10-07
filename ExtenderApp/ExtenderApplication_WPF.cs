using System.Diagnostics;
using System.Windows;
using AppHost;
using AppHost.Builder;
using AppHost.Extensions.DependencyInjection;
using ExtenderApp.Abstract;

namespace ExtenderApp
{
    internal class ExtenderApplication_WPF : Application
    {
        private readonly AppHostBuilder _builder;
        private AppHostApplication application;
        private ILogingService logingService;

        public ExtenderApplication_WPF()
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            DebugMessage($"开始启动 : {DateTime.Now}");
            _builder = AppHostApplication.CreateBuilder();
            DebugMessage($"启动成功 : {DateTime.Now}");

            DebugMessage($"开始生成服务 : {DateTime.Now}");
            _builder.LoadAssembliesForFolder("pack");
            _builder.FindStarupForFolder("lib");
            application = _builder.Builde();
            DebugMessage($"生成服务成功 : {DateTime.Now}");

            logingService = application.ServiceProvider.GetRequiredService<ILogingService>();
            application.Run();
            sw.Stop();
            logingService?.Print(new Data.LogInfo()
            {
                LogLevel = Data.LogLevel.INFO,
                Message = $"启动成功 本次启动用时 ：{sw.ElapsedMilliseconds} 毫秒",
                Source = nameof(ExtenderApplication_WPF),
                Time = DateTime.Now,
                ThreadId = Thread.CurrentThread.ManagedThreadId
            });
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            _builder.MainThreadContext.InitMainThreadContext();
            //Resources.MergedDictionaries.Add(new() { Source = new("pack://application:,,,/ExtenderApp.Views;component/Themes/Global/DarkTheme.xaml") });
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
            application.StopAsync().ConfigureAwait(false);
        }

        public void Eorrer(Exception ex)
        {
            var logingService = application.ServiceProvider.GetService<ILogingService>();
            logingService?.Print(new Data.LogInfo()
            {
                LogLevel = Data.LogLevel.ERROR,
                Message = "程序出现问题了！",
                Source = nameof(ExtenderApplication_WPF),
                Time = DateTime.Now,
                ThreadId = Thread.CurrentThread.ManagedThreadId,
                Exception = ex
            });
        }

#if DEBUG
        private static void DebugMessage(string message)
        {
            Debug.Print(message);
        }
#endif
    }
}
