using System.Buffers;
using System.Linq.Expressions;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using ExtenderApp.Abstract;
using ExtenderApp.Common.IO;
using ExtenderApp.Common.ObjectPools;
using ExtenderApp.Data;

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
        /// 对象池存储
        /// </summary>
        private readonly ObjectPoolStore _poolStore;

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
        public HashProvider(ObjectPoolStore poolStore, IFileOperateProvider fileStore)
        {
            _poolStore = poolStore;
            _fileOperateProvider = fileStore;
            _encoding = Encoding.UTF8; // 默认使用UTF8编码
        }

        #region ComputeHash

        public HashValue ComputeHash<T>(string text) where T : HashAlgorithm
        {
            if (string.IsNullOrEmpty(text))
                throw new ArgumentNullException(nameof(text), "文本不能为空。");
            var bytes = _encoding.GetBytes(text);
            return ComputeHash<T>(bytes, 0, bytes.Length);
        }

        /// <summary>
        /// 计算哈希值
        /// </summary>
        /// <typeparam name="T">哈希算法类型</typeparam>
        /// <param name="bytes">字节数组</param>
        /// <returns>哈希值</returns>
        public HashValue ComputeHash<T>(byte[] bytes) where T : HashAlgorithm
        {
            return ComputeHash<T>(bytes, 0, bytes.Length);
        }

        /// <summary>
        /// 计算指定字节数组的哈希值。
        /// </summary>
        /// <typeparam name="T">哈希算法的类型。</typeparam>
        /// <param name="bytes">要计算哈希值的字节数组。</param>
        /// <param name="offset">开始计算哈希值的字节数组的起始位置。</param>
        /// <param name="count">要计算哈希值的字节数。</param>
        /// <returns>计算得到的哈希值。</returns>
        public HashValue ComputeHash<T>(byte[] bytes, int offset, int count) where T : HashAlgorithm
        {
            var pool = GetPool<T>();
            var hashAlgorithm = pool.Get();
            var reslut = hashAlgorithm.ComputeHash(bytes, offset, count);
            pool.Release(hashAlgorithm);

            return new HashValue(reslut);
        }

        /// <summary>
        /// 计算指定流的哈希值。
        /// </summary>
        /// <typeparam name="T">哈希算法的类型。</typeparam>
        /// <param name="stream">要计算哈希值的流。</param>
        /// <returns>计算得到的哈希值。</returns>
        public HashValue ComputeHash<T>(Stream stream) where T : HashAlgorithm
        {
            var pool = GetPool<T>();
            var hashAlgorithm = pool.Get();
            var reslut = hashAlgorithm.ComputeHash(stream);
            pool.Release(hashAlgorithm);

            return new HashValue(reslut);
        }

        public HashValue ComputeHash<T>(FileOperateInfo info) where T : HashAlgorithm
        {
            var pool = GetPool<T>();
            var hashAlgorithm = pool.Get();
            var fileOperate = _fileOperateProvider.GetOperate(info);

            byte[] bytes = fileOperate.Read();
            var reslut = hashAlgorithm.ComputeHash(bytes);

            ArrayPool<byte>.Shared.Return(bytes);
            pool.Release(hashAlgorithm);
            return new HashValue(reslut);
        }

        #endregion

        #region ComputeHashAsync

        public async Task<HashValue> ComputeHashAsync<T>(Stream stream) where T : HashAlgorithm
        {
            var pool = GetPool<T>();
            var hashAlgorithm = pool.Get();
            var reslut = await hashAlgorithm.ComputeHashAsync(stream);
            pool.Release(reslut);

            return new HashValue(reslut);
        }

        //public async Task<HashValue> ComputeHashAsync<TLinkClient>(FileOperateInfo fileOperate) where TLinkClient : HashAlgorithm
        //{
        //    var pool = GetPool<TLinkClient>();
        //    var hashAlgorithm = pool.Get();
        //    var fileConcurrent = _fileOperateProvider.GetOperate(fileOperate);

        //    byte[] bytes = await fileConcurrent.ReadForArrayPoolAsync(out var length);
        //    byte[] reslut = await hashAlgorithm.ComputeHashAsync(new MemoryStream(bytes));
        //    pool.Release(hashAlgorithm);

        //    return new HashValue(reslut);
        //}

        #endregion

        /// <summary>
        /// 获取对象池
        /// </summary>
        /// <typeparam name="T">哈希算法类型</typeparam>
        /// <returns>对象池</returns>
        private ObjectPool<T> GetPool<T>() where T : HashAlgorithm
        {
            if (_poolStore.TryGetValue(typeof(T), out var pool))
            {
                return (ObjectPool<T>)pool;
            }

            // 使用反射获取 MethodInfo
            MethodInfo methodInfo = typeof(T).GetMethod("Create", BindingFlags.Static | BindingFlags.Public, null, Type.EmptyTypes, null);

            // 验证方法是否存在且正确
            if (methodInfo == null || methodInfo.ReturnType != typeof(T) || !methodInfo.IsStatic)
            {
                throw new InvalidOperationException($"“类型 {typeof(T).FullName} 没有一个公开的静态 'Create()' 方法。");
            }

            // 构建 Expression 树
            MethodCallExpression callExpr = Expression.Call(methodInfo);

            var lambdaExpr = Expression.Lambda<Func<T>>(callExpr);
            Func<T> func = lambdaExpr.Compile();

            return _poolStore.GetPool(new HashPoolPolicy<T>(func));
        }
    }
}
