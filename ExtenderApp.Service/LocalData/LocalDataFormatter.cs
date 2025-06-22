using ExtenderApp.Abstract;
using ExtenderApp.Common.IO.Binaries.Formatter;
using ExtenderApp.Data;

namespace ExtenderApp.Services
{
    /// <summary>
    /// 本地数据格式化器类，用于序列化和反序列化本地数据
    /// </summary>
    /// <typeparam name="T">数据类型</typeparam>
    internal class LocalDataFormatter<T> : ResolverFormatter<LocalData<T>>
    {
        /// <summary>
        /// 用于序列化和反序列化数据类型为T的二进制格式化器
        /// </summary>
        private IBinaryFormatter<T> _binaryFormatter;

        /// <summary>
        /// 用于序列化和反序列化数据类型为Version的二进制格式化器
        /// </summary>
        private IBinaryFormatter<Version> _versionFormatter;

        /// <summary>
        /// 获取默认的本地数据对象
        /// </summary>
        /// <returns>返回默认的本地数据对象</returns>
        public override LocalData<T> Default => new LocalData<T>(_binaryFormatter.Default, _versionFormatter.Default);

        /// <summary>
        /// 获取二进制格式化器中的对象数量
        /// </summary>
        /// <returns>返回二进制格式化器中的对象数量</returns>
        public override int Length => _binaryFormatter.Length + _versionFormatter.Length;

        /// <summary>
        /// 初始化本地数据格式化器对象
        /// </summary>
        /// <param name="resolver">二进制格式化器解析器</param>
        public LocalDataFormatter(IBinaryFormatterResolver resolver) : base(resolver)
        {
            _binaryFormatter = GetFormatter<T>();
            _versionFormatter = GetFormatter<Version>();
        }

        /// <summary>
        /// 从扩展二进制读取器中反序列化本地数据对象
        /// </summary>
        /// <param name="reader">扩展二进制读取器</param>
        /// <returns>返回反序列化的本地数据对象</returns>
        public override LocalData<T> Deserialize(ref ExtenderBinaryReader reader)
        {
            var version = _versionFormatter.Deserialize(ref reader);
            var data = _binaryFormatter.Deserialize(ref reader);
            return new LocalData<T>(data, version);
        }

        /// <summary>
        /// 将本地数据对象序列化到扩展二进制写入器中
        /// </summary>
        /// <param name="writer">扩展二进制写入器</param>
        /// <param name="value">要序列化的本地数据对象</param>
        public override void Serialize(ref ExtenderBinaryWriter writer, LocalData<T> value)
        {
            _versionFormatter.Serialize(ref writer, value.Version);
            _binaryFormatter.Serialize(ref writer, value.Data);
        }

        /// <summary>
        /// 获取本地数据对象在二进制格式化器中的对象数量
        /// </summary>
        /// <param name="value">本地数据对象</param>
        /// <returns>返回本地数据对象在二进制格式化器中的对象数量</returns>
        public override long GetLength(LocalData<T> value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            return _versionFormatter.GetLength(value.Version) + _binaryFormatter.GetLength(value.Data);
        }
    }
}
