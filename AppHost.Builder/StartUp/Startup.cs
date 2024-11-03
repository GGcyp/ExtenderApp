

using AppHost.Extensions.DependencyInjection;

namespace AppHost.Builder
{
    public abstract class Startup
    {
        private static Type m_StartupType;
        public static Type Type
        {
            get
            {
                if(m_StartupType == null)
                {
                    m_StartupType = typeof(Startup);
                }
                return m_StartupType;
            }
        }
        public static string StartMethodName => nameof(Start);

        public virtual void Start(IHostApplicationBuilder builder)
        {
            AddService(builder.Services);
        }

        protected virtual void AddService(IServiceCollection services)
        {

        }
    }
}
