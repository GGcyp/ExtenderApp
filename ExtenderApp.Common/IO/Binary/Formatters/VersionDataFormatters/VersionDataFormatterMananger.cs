using ExtenderApp.Abstract;
using ExtenderApp.Data;

namespace ExtenderApp.Common.IO.Binary.Formatters
{
    /// <summary>
    /// 版本化数据格式化器管理器，用于管理不同版本的 <see cref="VersionData{T}"/> 序列化/反序列化逻辑
    /// </summary>
    /// <typeparam name="T">需要版本化管理的数据类型</typeparam>
    /// <remarks>
    /// 1. 继承自 <see cref="ResolverFormatter{VersionData{T}}"/>，提供基础的格式化器解析功能
    /// 2. 实现 <see cref="IVersionFormatterMananger"/> 接口，管理多个版本的格式化器
    /// 3. 内部维护格式化器列表，按版本号排序以确保使用正确的格式化器
    /// </remarks>
    internal class VersionDataFormatterMananger<T> : ResolverFormatter<VersionData<T>>, IVersionDataFormatterMananger<T>
    {
        private readonly IBinaryFormatter<Version> _version;

        public List<IVersionDataFormatter<T>> Formatters { get; }

        public override int DefaultLength => Formatters.Last().DefaultLength;

        public VersionDataFormatterMananger(IBinaryFormatterResolver resolver) : base(resolver)
        {
            Formatters = new();
            _version = resolver.GetFormatter<Version>();
        }

        public IVersionDataFormatter<T> GetFormatter(Version version)
        {
            IVersionDataFormatter<T>? formatter = Formatters.FirstOrDefault(f => f.FormatterVersion == version);
            if (formatter == null)
            {
                throw new InvalidOperationException($"未找到对应版本的格式化器，版本号：{version} ，类型名：{typeof(T).FullName}");
            }
            return formatter;
        }

        private IVersionDataFormatter<T> LastFormatter()
        {
            if (Formatters.Count <= 0)
            {
                throw new ArgumentOutOfRangeException("还未添加格式化器", typeof(T).FullName);
            }

            return Formatters[Formatters.Count - 1];
        }

        public void AddFormatter(object formatter)
        {
            if (formatter == null)
            {
                throw new ArgumentNullException(nameof(formatter), "格式化器不能为空");
            }
            if (formatter is not IVersionDataFormatter<T> localFormatter)
            {
                throw new ArgumentException("格式化器必须实现IVersionFormatter<T>接口", nameof(formatter));
            }
            if (Formatters.Any(f => f.FormatterVersion == localFormatter.FormatterVersion))
            {
                throw new InvalidOperationException($"不能重复添加相同版本的格式化器：{localFormatter.FormatterVersion} ：{typeof(T).Name}");
            }

            // 找到第一个版本号比当前大的位置，插入到它前面（保持升序）
            int index = Formatters.FindIndex(f => f.FormatterVersion > localFormatter.FormatterVersion);
            if (index == -1)
            {
                // 如果没有更大的版本号，直接添加到末尾
                Formatters.Add(localFormatter);
            }
            else
            {
                // 否则插入到该位置，确保大的版本号在最后
                Formatters.Insert(index, localFormatter);
            }
        }

        public override VersionData<T> Deserialize(ref ByteBuffer block)
        {
            var version = _version.Deserialize(ref block);
            if (version == null)
            {
                //throw new InvalidOperationException($"读取到的版本信息为空，无法确定使用哪个格式化器进行反序列化。序列化类型为：{typeof(T).Name}");
                return new VersionData<T>(version, default);
            }

            var formatter = GetFormatter(version);
            var data = formatter.Deserialize(ref block);
            return new VersionData<T>(version, data);
        }

        public override void Serialize(ref ByteBuffer block, VersionData<T> value)
        {
            if (Formatters.Count == 0)
            {
                throw new InvalidOperationException($"没有可用的格式化器，请先添加格式化器。 {typeof(T).FullName}");
            }

            if (value.IsEmpty)
            {
                WriteNil(ref block);
                return;
            }

            var version = value.DataVersion;
            if (version == null)
            {
                throw new ArgumentNullException($"格式化器版本不能为空 {typeof(T).FullName}");
            }
            _version.Serialize(ref block, version);

            var formatter = GetFormatter(version);
            formatter.Serialize(ref block, value.Data);
        }

        public override long GetLength(VersionData<T> value)
        {
            if (value.IsEmpty)
            {
                return _version.DefaultLength;
            }
            var formatter = GetFormatter(value.DataVersion);
            return _version.GetLength(value.DataVersion) + formatter.GetLength(value.Data);
        }

        public void Serialize(ref ByteBuffer block, T value, Version version)
        {
            if (Formatters.Count == 0)
            {
                throw new InvalidOperationException($"没有可用的格式化器，请先添加格式化器。 {typeof(T).FullName}");
            }

            if (version == null)
            {
                throw new ArgumentNullException($"格式化器版本不能为空 {typeof(T).FullName}");
            }

            if (EqualityComparer<T>.Default.Equals(value, default))
            {
                WriteNil(ref block);
                return;
            }

            IVersionDataFormatter<T> formatter = GetFormatter(version);
            _version.Serialize(ref block, version);
            formatter.Serialize(ref block, value);
        }

        public T Deserialize(ref ByteBuffer block, Version version)
        {
            if (version == null)
            {
                throw new ArgumentNullException("格式化器版本不能为空", nameof(version));
            }

            var binaryVersion = _version.Deserialize(ref block);
            if (binaryVersion == null)
            {
                throw new InvalidOperationException($"读取到的版本信息为空，无法确定使用哪个格式化器进行反序列化。序列化类型为：{typeof(T).Name}");

            }

            if (binaryVersion != version)
            {
                throw new InvalidCastException($"文件版本和需要版本不匹配,文件版本：{binaryVersion},需要版本：{version},{typeof(T).FullName}");
            }

            var formatter = GetFormatter(version);
            return formatter.Deserialize(ref block);
        }

        public long GetLength(T value, Version version)
        {
            if (version == null)
            {
                throw new ArgumentNullException("输入格式化版本不能为空");
            }
            var formatter = GetFormatter(version);
            return _version.GetLength(version) + formatter.GetLength(value);
        }

        public void Serialize(ref ByteBuffer buffer, T value)
        {
            IVersionDataFormatter<T> formatter = LastFormatter();

            _version.Serialize(ref buffer, formatter.FormatterVersion);
            formatter.Serialize(ref buffer, value);
        }

        T IBinaryFormatter<T>.Deserialize(ref ByteBuffer block)
        {
            var version = _version.Deserialize(ref block);
            IVersionDataFormatter<T> formatter = version == null ? LastFormatter() : GetFormatter(version);
            return formatter.Deserialize(ref block);
        }

        public long GetLength(T value)
        {
            var formatter = LastFormatter();
            return _version.GetLength(formatter.FormatterVersion) + formatter.GetLength(value);
        }
    }
}
