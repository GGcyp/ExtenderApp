using System.Collections.ObjectModel;
using AppHost.Builder;
using AppHost.Extensions.DependencyInjection;
using AppHost.Extensions.Hosting;
using ExtenderApp.Abstract;

namespace ExtenderApp.Views
{
    internal class ViewStartup : Startup
    {
        public override void AddService(IServiceCollection services)
        {
            services.AddHosted<MainViewHostedService>();
            services.AddSingleton<IDispatcherService>(new Dispatcher_WPF());

            services.Configuration<IBinaryFormatterStore>(s => s.AddFormatter(typeof(ObservableCollection<>), typeof(ObservableCollectionFormatter<>)));
        }
    }
}
