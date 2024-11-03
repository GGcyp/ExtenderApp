using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppHost.Extensions.Hosting
{
    public interface IHostEnvironment
    {
        /// <summary>
        /// 获取或设置应用程序的名称。
        /// </summary>
        string ApplicationName { get; set; }
        /// <summary>
        /// 获取或设置应用程序的内容根路径。
        /// </summary>
        string ContentRootPath { get; set; }
        /// <summary>
        /// 获取或设置应用程序的环境名称。
        /// </summary>
        string EnvironmentName { get; set; }
    }
}
