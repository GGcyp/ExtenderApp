using System.Security.Cryptography;
using System.Text;
using ExtenderApp.Abstract;

using ExtenderApp.Contracts;

namespace ExtenderApp.Common.Hash
{
    /// <summary>
    /// 哈希提供程序类
    /// </summary>
    internal class HashProvider : IHashProvider
    {
        /// <summary>
        /// 哈希池策略类
        /// </summary>
        /// <typeparam name="T">哈希算法类型</typeparam>
        private class HashPoolPolicy<T> : FactoryPooledObjectPolicy<T> where T : HashAlgorithm
        {
            /// <summary>
            /// 构造函数
            /// </summary>
            /// <param name="factory">工厂方法</param>
            public HashPoolPolicy(Func<T> factory) : base(factory)
            {
            }

            /// <summary>
            /// 释放对象
            /// </summary>
            /// <param name="obj">待释放对象</param>
            /// <returns>是否成功释放对象</returns>
            public override bool Release(T obj)
            {
                obj.Initialize();
                return true;
            }
        }

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