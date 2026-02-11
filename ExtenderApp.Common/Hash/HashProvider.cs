using System.Text;
using ExtenderApp.Abstract;

namespace ExtenderApp.Common.Hash
{
    /// <summary>
    /// 哈希提供程序类
    /// </summary>
    internal class HashProvider : IHashProvider
    {
        /// <summary>
        /// 文件操作提供者
        /// </summary>
        private readonly IFileOperateProvider _fileOperateProvider;

        /// <summary>
        /// 编码格式
        /// </summary>
        private readonly Encoding _encoding;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="poolStore">对象池存储</param>
        public HashProvider(IFileOperateProvider fileStore)
        {
            _fileOperateProvider = fileStore;
            _encoding = Encoding.UTF8; // 默认使用UTF8编码
        }
    }
}