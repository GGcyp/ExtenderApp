namespace AppHost.Builder
{
    /// <summary>
    /// 主机生成层，程序选项设置
    /// </summary>
    public class ApplicationOptions
    {
        /// <summary>
        /// 命令行数据
        /// </summary>
        public string[]? Args { get; init; }

        /// <summary>
        /// 环境名称
        /// </summary>
        public string? EnvironmentName { get; init; }

        /// <summary>
        /// 程序名称
        /// </summary>
        public string? ApplicationName { get; init; }

        /// <summary>
        /// 内容根目录
        /// </summary>
        public string? ContentRootPath { get; init; }

        /// <summary>
        /// 程序根目录
        /// </summary>
        public string? AppRootPath { get; init; }
    }
}
