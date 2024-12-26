using AppHost.Builder;
using ExtenderApp.Common.File;

namespace ExtenderApp.Common
{
    /// <summary>
    /// 通用层启动类，继承自Startup基类
    /// </summary>
    public class CommonStartup : Startup
    {
        public override void Start(IHostApplicationBuilder builder)
        {
            builder.Services.AddFile();
        }
    }
}
