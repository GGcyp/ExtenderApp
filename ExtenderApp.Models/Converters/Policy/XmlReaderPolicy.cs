using System.Xml;
using ExtenderApp.Abstract;
using ExtenderApp.Data;

namespace ExtenderApp.Models.Converters
{
    /// <summary>
    /// 用于将XML文件与数据模型之间进行转换的抽象基类。
    /// </summary>
    /// <typeparam name="TReadData">读取XML时使用的数据类型。</typeparam>
    /// <typeparam name="TWriteData">写入XML时使用的数据类型。</typeparam>
    public abstract class XmlReaderPolicy<TReadData, TWriteData> : BaseModelConvertPolicy<TReadData, TWriteData> where TReadData : class,new() where TWriteData : class,new()
    {
        public override FileExtensionType ExtensionType => FileExtensionType.Xml;

        protected XmlNodeType CureentNodeType { get; set; }

        #region ConvertToModel

        protected override void ConvertToModel(IModel model, FileInfoData infoData, TReadData readData, object fileData)
        {
            if (!(fileData is XmlReader xmlReader)) throw new ArgumentNullException(nameof(XmlReader));
            ReadStart(xmlReader, readData);
            ReadRun(xmlReader, readData);
            ReadEnd(model, infoData, readData);
        }

        protected virtual void ReadStart(XmlReader reader, TReadData readData)
        {
            // 移动到根元素
            //reader.ReadToFollowing("Root");

            //reader.MoveToContent();
        }

        /// <summary>
        /// 从XmlReader对象中读取XML数据。
        /// </summary>
        /// <param name="reader">包含XML数据的XmlReader对象。</param>
        /// <param name="readData">读取到的数据对象。</param>
        /// <exception cref="ArgumentNullException">如果reader参数为null。</exception>
        private void ReadRun(XmlReader reader, TReadData readData)
        {
            if (reader == null) throw new ArgumentNullException(nameof(reader));

            string name = string.Empty;
            while (reader.Read())
            {
                CureentNodeType = reader.NodeType;
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        name = reader.Name;
                        NodeStart(name, readData);
                        if (reader.HasAttributes)
                        {
                            for (int i = 0; i < reader.AttributeCount; i++)
                            {
                                reader.MoveToAttribute(i);
                                NodeValue(reader.Name, reader.Value, readData);
                            }
                        }
                        break;
                    case XmlNodeType.Text:
                    case XmlNodeType.CDATA:
                        NodeValue(name, reader.Value, readData);
                        break;
                    case XmlNodeType.EndElement:
                        NodeEnd(reader.Name, readData);
                        break;
                }
            }
        }

        /// <summary>
        /// XML读取指针移动到开始位置时调用。
        /// </summary>
        /// <param name="name">元素名称。</param>
        /// <param name="readData">读取到的数据对象。</param>
        protected abstract void NodeStart(string name, TReadData readData);

        /// <summary>
        /// XML读取指针移动到数据位置时调用。
        /// </summary>
        /// <param name="name">元素名称。</param>
        /// <param name="value">元素值。</param>
        /// <param name="readData">读取到的数据对象。</param>
        protected abstract void NodeValue(string name, string value, TReadData readData);

        /// <summary>
        /// XML读取指针移动到结束位置时调用。
        /// </summary>
        /// <param name="name">元素名称。</param>
        /// <param name="readData">读取到的数据对象。</param>
        protected abstract void NodeEnd(string name, TReadData readData);

        /// <summary>
        /// 所有读取操作完成后调用。
        /// </summary>
        /// <param name="model">数据模型。</param>
        /// <param name="infoData">文件信息数据。</param>
        /// <param name="readData">读取到的数据对象。</param>
        protected abstract void ReadEnd(IModel model, FileInfoData infoData, TReadData readData);

        #endregion

        #region ConvertToFile

        /// <summary>
        /// 将数据转换为XML文件。
        /// </summary>
        /// <param name="model">数据模型。</param>
        /// <param name="infoData">文件信息数据。</param>
        /// <param name="writeData">待写入的数据对象。</param>
        /// <param name="fileData">XML文件数据。</param>
        protected override void ConvertToFile(IModel model, FileInfoData infoData, TWriteData writeData, object fileData)
        {
            var temp = fileData as Tuple<XmlElement, XmlDocument>;
            if (temp == null) throw new ArgumentNullException(nameof(fileData));

            XmlElement xmlElement = temp.Item1;
            XmlDocument xmlDoc = temp.Item2 ;
            if (xmlElement == null || xmlDoc == null) throw new ArgumentNullException(nameof(XmlReaderPolicy<TReadData, TWriteData>));

            ProXmlWrite(model, infoData, xmlElement, xmlDoc, writeData);
        }

        /// <summary>
        /// 子类实现XML写入逻辑。
        /// </summary>
        /// <param name="model">数据模型。</param>
        /// <param name="infoData">文件信息数据。</param>
        /// <param name="xmlElement">XML元素。</param>
        /// <param name="xmlDoc">XML文档。</param>
        /// <param name="writeData">待写入的数据对象。</param>
        protected abstract void ProXmlWrite(IModel model, FileInfoData infoData, XmlElement xmlElement, XmlDocument xmlDoc, TWriteData writeData);

        #endregion
    }

    public struct XmlElementNodeHandler
    {
        XmlDocument xmlDoc;

        public XmlElementNodeHandler(XmlDocument xmlDoc)
        {
            this.xmlDoc = xmlDoc;
        }

        /// <summary>
        /// 创建新的XML节点，加入父节点中，并添加数据。
        /// </summary>
        /// <param name="parentXmlElement">父XML元素。</param>
        /// <param name="nodeName">节点名称。</param>
        /// <param name="value">节点值，默认为null。</param>
        public void AddXmlElementMessage(XmlElement parentXmlElement, string nodeName, string value = null)
        {
            XmlElement xmlElement = AddDataXmlElement(parentXmlElement, nodeName);
            xmlElement.InnerText = value;
        }

        /// <summary>
        /// 创建新的XML节点，并将其加入指定的父节点中。
        /// </summary>
        /// <param name="parentXmlElement">父节点XmlElement。</param>
        /// <param name="nodeName">新节点的名称。</param>
        /// <returns>新创建的XmlElement节点。</returns>
        public XmlElement AddDataXmlElement(XmlElement parentXmlElement, string nodeName)
        {
            XmlElement xmlElement = xmlDoc.CreateElement(nodeName);
            parentXmlElement.AppendChild(xmlElement);
            return xmlElement;
        }

        /// <summary>
        /// 创建新的XML节点，并将其加入指定的父节点中。
        /// (是添加大数据的CData节点)
        /// </summary>
        /// <param name="parentXmlElement">父节点XmlElement。</param>
        /// <param name="nodeName">新节点的名称。</param>
        /// <returns>新创建的XmlElement节点。</returns>
        public void AddXmlCDataSection(XmlElement parentXmlElement, string nodeName, string value = null)
        {
            XmlElement xmlElement = AddDataXmlElement(parentXmlElement, nodeName);
            XmlCDataSection cdataSection = xmlDoc.CreateCDataSection(value);
            xmlElement.AppendChild(cdataSection);
        }
    }
}
