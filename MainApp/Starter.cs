using System.Diagnostics;
using AppHost;
using AppHost.Builder;
using AppHost.Extensions.DependencyInjection;
using MainApp.Abstract;
using MainApp.Views;

namespace MainApp
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

            AddWPFServices(builder);

            //builder.FindStarupForFolder(AppSetting.AppBinFolderName);
            builder.FindStarupForFolder("bin");
            Debug.Print($"启动成功 : {DateTime.Now}");

            Debug.Print($"开始生成服务 : {DateTime.Now}");
            application = builder.Builde();
            Debug.Print($"生成服务成功 : {DateTime.Now}");

            try
            {
                sw.Stop();
                Debug.Print($"启动成功 : {DateTime.Now} : 本次启动用时 ：{sw.ElapsedMilliseconds} 毫秒");
                application.Run();
                app.Run();
            }
            catch (Exception ex)
            {
                Debug.Print(ex.Message);
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// 添加WPF的服务
        /// </summary>
        /// <param name="builder"></param>
        private static void AddWPFServices(IHostApplicationBuilder builder)
        {
            builder.Services.AddSingleton<IDispatcher>(new WPF_Dispatcher(app.Dispatcher));
        }
    }
}
