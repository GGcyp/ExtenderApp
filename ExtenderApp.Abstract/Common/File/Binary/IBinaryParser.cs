
namespace ExtenderApp.Abstract
{
    /// <summary>
    /// 二进制文件解析器接口，继承自文件解析器接口。
    /// </summary>
    public interface IBinaryParser : IFileParser
    {
        T? Deserialize<T>(byte[] bytes);
        byte[] Serialize<T>(T value);
    }
}
