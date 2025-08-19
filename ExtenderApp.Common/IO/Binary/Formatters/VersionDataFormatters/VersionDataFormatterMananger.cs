using ExtenderApp.Abstract;
using ExtenderApp.Data;

namespace ExtenderApp.Common.IO.Binaries.Formatters
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
    internal class VersionDataFormatterMananger<T> : ResolverFormatter<VersionData<T>>
    {
        private readonly IBinaryFormatter<Version> _version;

        public List<IVersionDataFormatter<T>> Formatters { get; }

        public override int DefaultLength => Formatters.Last().DefaultLength;

        public VersionDataFormatterMananger(IBinaryFormatterResolver resolver) : base(resolver)
        {
            Formatters = new();
            _version = resolver.GetFormatter<Version>();
        }

        public IVersionDataFormatter<T>? GetFormatter(Version version)
        {
            return Formatters.FirstOrDefault(f => f.FormatterVersion == version);
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

        public override VersionData<T> Deserialize(ref ExtenderBinaryReader reader)
        {
            var version = _version.Deserialize(ref reader);
            if (version == null)
            {
                throw new InvalidOperationException($"读取到的版本信息为空，无法确定使用哪个格式化器进行反序列化。序列化类型为：{typeof(T).Name}");
            }

            var formatter = GetFormatter(version);
            if (formatter == null)
            {
                throw new InvalidOperationException($"未找到对应版本的格式化器：{version} ：{typeof(T).Name}");
            }
            var data = formatter.Deserialize(ref reader);
            return new VersionData<T>(version, data);
        }

        public override void Serialize(ref ExtenderBinaryWriter writer, VersionData<T> value)
        {
            if (Formatters.Count == 0)
            {
                throw new InvalidOperationException($"没有可用的格式化器，请先添加格式化器。 {typeof(T).Name}");
            }

            if (value.IsEmpty)
            {
                _version.Serialize(ref writer, null);
                return;
            }

            var version = value.DataVersion;
            _version.Serialize(ref writer, version);

            var formatter = GetFormatter(version);
            if (formatter == null)
            {
                throw new InvalidOperationException($"未找到对应版本的格式化器：{version} ：{typeof(T).Name}");
            }
            formatter.Serialize(ref writer, value.Data);
        }

        public override long GetLength(VersionData<T> value)
        {
            if (value.IsEmpty)
            {
                return _version.DefaultLength;
            }
            var formatter = GetFormatter(value.DataVersion);
            if (formatter == null)
            {
                throw new InvalidOperationException($"未找到对应版本的格式化器：{value.DataVersion} ：{typeof(T).Name}");
            }
            return _version.DefaultLength + formatter.GetLength(value.Data);
        }
    }
}
