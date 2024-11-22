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
            AddFileAccess(builder);
        }

        /// <summary>
        /// 添加文件访问相关服务的私有方法
        /// </summary>
        /// <param name="builder">主机应用程序构建器</param>
        private void AddFileAccess(IHostApplicationBuilder builder)
        {
            //builder.Services.AddAccessProvider();
            //builder.Services.AddFileParserStore();
            builder.Services.AddFileParser();
        }
    }
}
