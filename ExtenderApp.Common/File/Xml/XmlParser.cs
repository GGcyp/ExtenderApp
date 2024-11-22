using System.Text;
using System.Xml;

namespace ExtenderApp.Common.File
{
    /// <summary>
    /// xml文件解析器，默认使用流式读取XML
    /// </summary>
    internal class XmlParser
    {
        ////可选择类型
        ////XMLTextReader------提供以快速、单向、无缓冲的方式存取XML数据。（单向意味着你只能从前往后读取XML文件，而不能逆向读取）

        ////XMLValidatingReader------与XMLTextReader类一起使用，提供验证DTD、XDR和XSD架构的能力。

        ////XMLDocument------遵循W3C文档对象模型规范的一级和二级标准，实现XML数据随机的、有缓存的存取。
        ////一级水平包含了DOM的最基本的部分，而二级水平增加多种改进，包括增加了对名称空间和级连状图表(CSS)的支持。

        ////XMLTextWriter------生成遵循 W3C XML 1.0 规范的XML文件。

        //private const string YES = "yes";
        //private const string NO = "no";

        //private readonly string _version = "1.0";
        //private readonly string _encoding = "UTF-8";

        //public XmlParser() : base(null)
        //{
        //}

        //public override FileExtensionType ExtensionType => FileExtensionType.Xml;

        //protected override void Read(FileInfoData infoData, Action<object> processAction)
        //{
        //    using (XmlReader reader = XmlReader.Create(infoData.Info.FullName))
        //    {
        //        // 移动到根元素
        //        processAction?.Invoke(reader);
        //    }
        //}

        //protected override void Write(FileInfoData infoData, Action<object> processAction)
        //{
        //    if (processAction == null)
        //    {
        //        throw new ArgumentNullException($"写入文件的Xml不能为空：{infoData.FileName}");
        //    }

        //    // 创建 XmlDocument 对象
        //    var xmlDoc = new XmlDocument();
        //    // 创建 XML 声明
        //    XmlDeclaration xmlDeclaration = xmlDoc.CreateXmlDeclaration(_version, _encoding, null);
        //    xmlDoc.AppendChild(xmlDeclaration);
        //    // 创建根节点
        //    XmlElement rootElement = xmlDoc.CreateElement("Root");
        //    xmlDoc.AppendChild(rootElement);

        //    processAction?.Invoke(Tuple.Create(rootElement, xmlDoc));

        //    // 保存 XML 文件
        //    //文档一些设置
        //    XmlWriterSettings settings = new XmlWriterSettings();
        //    settings.Encoding = Encoding.UTF8;
        //    //是否使元素缩进默认值为 false (无缩进)
        //    settings.Indent = true;
        //    //指定在缩进时要使用的字符串。 默认值为两个空格。
        //    settings.IndentChars = "    ";
        //    //如果自己未指定路径,xml文件默认创建在bin目录下的debug目录里
        //    using (XmlWriter writer = XmlWriter.Create(infoData.Path, settings))
        //    {
        //        xmlDoc.Save(writer);
        //    }
        //}
    }
}
