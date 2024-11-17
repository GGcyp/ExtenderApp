using System.Xml;
using System.Xml.Linq;
using MainApp.Abstract;
using MainApp.Common;
using MainApp.Models;
using MainApp.Models.Converters;

namespace MainApp.Mod.PPR
{
    /// <summary>
    /// 专门处理PPRHead到XML的类
    /// </summary>
    internal class PPRXmlReaderPolicy : XmlReaderPolicy<PPRXmlReadData, object>
    {
        protected override PPRXmlReadData CreateReadData() => new PPRXmlReadData();

        protected override object CreateWriteData() => null;

        #region Read

        protected override void ReadStart(XmlReader reader, PPRXmlReadData readData)
        {
            //暂时只有自己的XML和广联达xml来读取，先这样
            reader.MoveToContent();
            var isPPRXml = reader.Name == PPRXMLROOT;
            readData.NodeStart = isPPRXml ? PPRNodeStart : GLDNodeStart;
            readData.NodeValue = isPPRXml ? PPRNodeValue : GLDNodeValue;
            readData.NodeEnd = isPPRXml ? PPRNodeEnd : GLDNodeEnd;

            //如果不是PPR自己的xml格式，就使用GLD的
            if (!isPPRXml)
            {
                string name = reader.Name;
                NodeStart(name, readData);
                if (reader.HasAttributes)
                {
                    for (int i = 0; i < reader.AttributeCount; i++)
                    {
                        reader.MoveToAttribute(i);
                        NodeValue(reader.Name, reader.Value, readData);
                    }
                }
            }
        }

        protected override void NodeStart(string name, PPRXmlReadData readData)
        {
            readData.NodeStart?.Invoke(name, readData);
        }

        protected override void NodeValue(string name, string value, PPRXmlReadData readData)
        {
            readData.NodeValue?.Invoke(name, value, readData);
        }

        protected override void NodeEnd(string name, PPRXmlReadData readData)
        {
            readData.NodeEnd?.Invoke(name, readData);
        }

        protected override void ReadEnd(
            IModel model,
            FileInfoData infoData,
            PPRXmlReadData readData
        )
        {
            model.AddDataSource(readData.Root);
        }

        #region GLD

        private const string GLDSegment = "标段";
        private const string GLDSingleItemProject = "单项工程";
        private const string GLDUnitEngineering = "单位工程";
        private const string GLDSubdivision = "分部分项";
        private const string GLDDivision = "分部";
        private const string GLDBillOfMaterials = "清单";

        private const string GLDProectName = "项目名称";
        private const string GLDProId = "编码";
        private const string GLDProName = "名称";
        private const string GLDProjectCharacteristics = "项目特征";
        private const string GLDUnit = "单位";
        private const string GLDQuantity = "数量";
        private const string GLDUnitPrice = "单价";
        private const string GLDTotalPrice = "合价";

        private void GLDNodeStart(string name, PPRXmlReadData readData)
        {
            var rootPPREntityNode = readData.Root;
            var unitEngineeringPPREntityNode = readData.TempUnitEngineeringPPREntityNode;
            var subdivisionPPREntityNode = readData.TempSubdivisionPPREntityNode;

            var tempPPREntityNode = readData.TempPPREntityNode;
            var tempPPREntity = tempPPREntityNode?.Entity;
            var tempPPRInventoryEntity = readData.TempPPRInventoryEntity;
            PPREntityNode tempNode;

            switch (name)
            {
                case GLDSegment:
                    rootPPREntityNode = new PPREntityNode();
                    tempPPREntityNode = rootPPREntityNode;
                    break;

                case GLDSingleItemProject:
                    unitEngineeringPPREntityNode = new PPREntityNode();
                    tempPPREntityNode = unitEngineeringPPREntityNode;
                    rootPPREntityNode.Add(unitEngineeringPPREntityNode);
                    break;

                case GLDUnitEngineering:
                    tempNode = new PPREntityNode();
                    unitEngineeringPPREntityNode?.Add(tempNode);
                    subdivisionPPREntityNode = tempNode;
                    tempPPREntityNode = tempNode;
                    break;

                case GLDDivision:
                    //适应多节点
                    tempNode = new PPREntityNode();
                    if (tempPPREntityNode is null)
                        tempPPREntityNode = subdivisionPPREntityNode;
                    tempPPREntityNode?.Add(tempNode);
                    tempPPREntityNode = tempNode;
                    break;

                //清单数据
                case GLDBillOfMaterials:
                    //如果单位工程下面没有再分分部
                    if (tempPPREntity is null)
                    {
                        tempPPREntityNode = subdivisionPPREntityNode;
                        tempPPREntity = tempPPREntityNode.Entity;
                    }

                    if (tempPPREntity?.DataList == null)
                        tempPPREntity!.DataList = new List<PPRInventoryEntity>();
                    tempPPRInventoryEntity = new PPRInventoryEntity();
                    tempPPREntity.DataList.Add(tempPPRInventoryEntity);
                    break;

                //case nameof(PPRPeriodQuantityEntity):
                //    if (tempPPRInventoryEntity?.PeriodQuantityList == null)
                //        tempPPRInventoryEntity!.PeriodQuantityList =
                //            new List<PPRPeriodQuantityEntity>();
                //    tempPPRPeriodQuantityEntity = new PPRPeriodQuantityEntity();
                //    tempPPRInventoryEntity.PeriodQuantityList.Add(tempPPRPeriodQuantityEntity);
                //    break;

                default:
                    tempPPREntityNode = null;
                    break;
            }

            readData.Root = rootPPREntityNode;
            readData.TempPPREntityNode = tempPPREntityNode;
            readData.TempPPRInventoryEntity = tempPPRInventoryEntity;
            readData.TempUnitEngineeringPPREntityNode = unitEngineeringPPREntityNode;
            readData.TempSubdivisionPPREntityNode = subdivisionPPREntityNode;
        }

        private void GLDNodeValue(string name, string value, PPRXmlReadData readData)
        {
            var root = readData.Root;
            var tempPPREntityNode = readData.TempPPREntityNode;
            var tempPPREntity = tempPPREntityNode?.Entity;
            var tempPPRInventoryEntity = readData.TempPPRInventoryEntity;

            if (tempPPREntity is null)
                return;

            switch (name)
            {
                //Root
                case GLDProectName:
                    tempPPREntity.ProjectName = value;
                    break;

                //PPREntity
                case GLDProName:
                    if (tempPPRInventoryEntity is null)
                    {
                        tempPPREntity.ProjectName = value;
                    }
                    else
                    {
                        tempPPRInventoryEntity.InventoryProjectName = value;
                    }
                    break;

                //PPRInventoryEntity
                //double
                case GLDQuantity:
                    tempPPRInventoryEntity.BillOfQuantitiesQuantity = double.Parse(value);
                    break;
                case GLDUnitPrice:
                    tempPPRInventoryEntity.UnitPrice = double.Parse(value);
                    break;
                //string
                case GLDProId:
                    tempPPRInventoryEntity.ProjectID = value;
                    break;
                //case GLDProName:
                //    tempPPRInventoryEntity.InventoryProjectName = value;
                //    break;
                case GLDProjectCharacteristics:
                    tempPPRInventoryEntity.ProjectFeatureDescription = value;
                    break;
                case GLDUnit:
                    tempPPRInventoryEntity.Unit = value;
                    break;
            }

            readData.Root = root;
            readData.TempPPREntityNode = tempPPREntityNode;
            readData.TempPPRInventoryEntity = tempPPRInventoryEntity;
        }

        private void GLDNodeEnd(string name, PPRXmlReadData readData)
        {
            var root = readData.Root;
            var tempPPREntityNode = readData.TempPPREntityNode;
            var tempPPREntity = tempPPREntityNode?.Entity;
            var tempPPRInventoryEntity = readData.TempPPRInventoryEntity;
            var subdivisionEntityNode = readData.TempSubdivisionPPREntityNode;

            switch (name)
            {
                //清单数据
                case GLDBillOfMaterials:
                    tempPPREntity?.DataList.Add(tempPPRInventoryEntity);
                    break;
                case GLDDivision:
                    if (tempPPREntityNode != null)
                    {
                        var parent = tempPPREntityNode.ParentNode;
                        tempPPREntityNode = parent != subdivisionEntityNode ? parent : null;
                    }
                    break;
            }

            readData.Root = root;
            readData.TempPPREntityNode = tempPPREntityNode;
            readData.TempPPRInventoryEntity = null;
        }

        #endregion

        #region PPRXml

        private const string PPRXMLROOT = "Root";

        private void PPRNodeStart(string name, PPRXmlReadData readData)
        {
            var tempPPREntityNode = readData.TempPPREntityNode;
            var tempPPREntity = tempPPREntityNode?.Entity;
            var tempPPRInventoryEntity = readData.TempPPRInventoryEntity;
            var tempPPRPeriodQuantityEntity = readData.TempPPRPeriodQuantityEntity;
            var root = readData.Root;

            switch (name)
            {
                case nameof(PPREntity):
                    var tempNode = new PPREntityNode();
                    if (root is null)
                        root = tempNode;
                    else
                        tempPPREntityNode.Add(tempNode);
                    tempPPREntityNode = tempNode;
                    break;

                //单项数据
                case nameof(PPRInventoryEntity):
                    if (tempPPREntity?.DataList == null)
                        tempPPREntity.DataList = new List<PPRInventoryEntity>();
                    tempPPRInventoryEntity = new PPRInventoryEntity();
                    tempPPREntity.DataList.Add(tempPPRInventoryEntity);
                    break;

                case nameof(PPRPeriodQuantityEntity):
                    if (tempPPRInventoryEntity?.PeriodQuantityList == null)
                        tempPPRInventoryEntity.PeriodQuantityList =
                            new List<PPRPeriodQuantityEntity>();
                    tempPPRPeriodQuantityEntity = new PPRPeriodQuantityEntity();
                    tempPPRInventoryEntity.PeriodQuantityList.Add(tempPPRPeriodQuantityEntity);
                    break;
            }

            readData.TempPPREntityNode = tempPPREntityNode;
            readData.TempPPRInventoryEntity = tempPPRInventoryEntity;
            readData.TempPPRPeriodQuantityEntity = tempPPRPeriodQuantityEntity;
            readData.Root = root;
        }

        private void PPRNodeValue(string name, string value, PPRXmlReadData readData)
        {
            var root = readData.Root;
            var tempPPREntityNode = readData.TempPPREntityNode;
            var tempPPREntity = tempPPREntityNode.Entity;
            var tempPPRInventoryEntity = readData.TempPPRInventoryEntity;
            var tempPPRPeriodQuantityEntity = readData.TempPPRPeriodQuantityEntity;

            switch (name)
            {
                //PPREntity
                case nameof(tempPPREntity.ProjectName):
                    tempPPREntity.ProjectName = value;
                    break;

                //case nameof(tempPPREntity.ProjectAmount):
                //    tempPPREntity.ProjectAmount = double.Parse(value);
                //    break;

                //PPRInventoryEntity
                //double
                case nameof(tempPPRInventoryEntity.BillOfQuantitiesQuantity):
                    tempPPRInventoryEntity.BillOfQuantitiesQuantity = double.Parse(value);
                    break;
                case nameof(tempPPRInventoryEntity.UnitPrice):
                    tempPPRInventoryEntity.UnitPrice = double.Parse(value);
                    break;
                //string
                case nameof(tempPPRInventoryEntity.ProjectID):
                    tempPPRInventoryEntity.ProjectID = value;
                    break;
                case nameof(tempPPRInventoryEntity.ProjectRemark):
                    tempPPRInventoryEntity.ProjectRemark = value;
                    break;
                case nameof(tempPPRInventoryEntity.InventoryProjectName):
                    tempPPRInventoryEntity.InventoryProjectName = value;
                    break;
                case nameof(tempPPRInventoryEntity.ProjectFeatureDescription):
                    tempPPRInventoryEntity.ProjectFeatureDescription = value;
                    break;
                case nameof(tempPPRInventoryEntity.Unit):
                    tempPPRInventoryEntity.Unit = value;
                    break;

                //PPRPeriodQuantityEntity
                case nameof(tempPPRPeriodQuantityEntity.Time):
                    tempPPRPeriodQuantityEntity.Time = value;
                    break;
                case nameof(tempPPRPeriodQuantityEntity.FrequencyRemark):
                    tempPPRPeriodQuantityEntity.FrequencyRemark = value;
                    break;
                case nameof(tempPPRPeriodQuantityEntity.Frequency):
                    tempPPRPeriodQuantityEntity.Frequency = int.Parse(value);
                    break;
                case nameof(tempPPRPeriodQuantityEntity.FrequencyAmount):
                    tempPPRPeriodQuantityEntity.FrequencyAmount = double.Parse(value);
                    break;
                case nameof(tempPPRPeriodQuantityEntity.FrequencyQuantity):
                    tempPPRPeriodQuantityEntity.FrequencyQuantity = double.Parse(value);
                    break;
                case nameof(tempPPRPeriodQuantityEntity.FrequencyReportedQuantity):
                    tempPPRPeriodQuantityEntity.FrequencyReportedQuantity = double.Parse(value);
                    break;
            }

            readData.Root = root;
            readData.TempPPREntityNode = tempPPREntityNode;
            readData.TempPPRInventoryEntity = tempPPRInventoryEntity;
            readData.TempPPRPeriodQuantityEntity = tempPPRPeriodQuantityEntity;
        }

        private void PPRNodeEnd(string name, PPRXmlReadData readData)
        {
            var root = readData.Root;
            var tempPPREntityNode = readData.TempPPREntityNode;

            switch (name)
            {
                //case m_Project:
                //    MessageLogger.Print(name);
                //    if (head != null && root != null)
                //    {
                //        root.AddNode(head);
                //    }
                //    head = null;
                //    break;
                //case m_DataElement:
                //    head.AddValue(data);
                //    break;
                //case m_ParentProjectName:
                //    //没有找到父类就添加到根节点下
                //    if (tempPPREntityNode.ParentNode == null)
                //        root.Add(tempPPREntityNode);
                //    break;
                case nameof(PPREntity):
                    tempPPREntityNode = tempPPREntityNode.ParentNode;
                    break;
            }

            readData.Root = root;
            readData.TempPPREntityNode = tempPPREntityNode;
        }

        #endregion

        #endregion

        #region Write

        protected override void ProXmlWrite(
            IModel model,
            FileInfoData infoData,
            XmlElement rootXmlElement,
            XmlDocument xmlDoc,
            object writeData
        )
        {
            XmlElementNodeHandler handler = new XmlElementNodeHandler(xmlDoc);

            //根结点的工程名称
            XmlElement parentXmlElement = handler.AddDataXmlElement(
                rootXmlElement,
                nameof(PPREntity)
            );

            var root = model.GetDataSource() as PPREntityNode;

            ArgumentNullException.ThrowIfNull(root, nameof(root));
            ArgumentNullException.ThrowIfNull(root.Entity, nameof(root.Entity));


            handler.AddXmlElementMessage(
                parentXmlElement,
                nameof(root.Entity.ProjectName),
                root.Entity.ProjectName
            );
            root.LoopChildNodes(PPRToXml, parentXmlElement, handler);
        }

        /// <summary>
        /// 将PPRHead和里面的数据一起转化成XML
        /// </summary>
        /// <param name="rootXmlElement"></param>
        /// <param name="entity"></param>
        private void PPRToXml(
            PPREntityNode node,
            XmlElement rootXmlElement,
            XmlElementNodeHandler handler
        )
        {
            PPREntity entity = node.Entity;
            if (entity == null)
                return;
            //存储工程头
            XmlElement parentXmlElement = handler.AddDataXmlElement(
                rootXmlElement,
                nameof(PPREntity)
            );

            handler.AddXmlElementMessage(
                parentXmlElement,
                nameof(entity.ProjectName),
                entity.ProjectName
            );

            //存储工程数据
            if (entity.DataList == null)
            {
                if (!node.HasChildNodes)
                    return;

                for (int i = 0; i < node.Count; i++)
                {
                    PPRToXml(node[i], parentXmlElement, handler);
                }

                return;
            }

            AddMessageToXmlElement(entity.DataList, parentXmlElement, handler);
        }

        private void AddMessageToXmlElement(
            List<PPRInventoryEntity> dataList,
            XmlElement parentXmlElement,
            XmlElementNodeHandler handler
        )
        {
            for (int i = 0; i < dataList.Count; i++)
            {
                var data = dataList[i];

                XmlElement xmlElement = handler.AddDataXmlElement(
                    parentXmlElement,
                    nameof(PPRInventoryEntity)
                );

                //不改动
                handler.AddXmlElementMessage(xmlElement, nameof(data.Unit), data.Unit);
                handler.AddXmlElementMessage(xmlElement, nameof(data.ProjectID), data.ProjectID);
                handler.AddXmlElementMessage(
                    xmlElement,
                    nameof(data.InventoryProjectName),
                    data.InventoryProjectName
                );
                handler.AddXmlElementMessage(
                    xmlElement,
                    nameof(data.UnitPrice),
                    data.UnitPrice.ToString()
                );
                handler.AddXmlElementMessage(
                    xmlElement,
                    nameof(data.BillOfQuantitiesQuantity),
                    data.BillOfQuantitiesQuantity.ToString()
                );
                handler.AddXmlCDataSection(
                    xmlElement,
                    nameof(data.ProjectFeatureDescription),
                    data.ProjectFeatureDescription
                );

                //有变动
                handler.AddXmlCDataSection(
                    xmlElement,
                    nameof(data.ProjectRemark),
                    data.ProjectRemark
                );

                //计算出来的
                //handler.AddXmlElementMessage(xmlElement, nameof(data.RemainingActualQuantity), data.RemainingActualQuantity.ToString());
                //handler.AddXmlElementMessage(xmlElement, nameof(data.CompletedQuantity), data.CompletedQuantity.ToString());
                //handler.AddXmlElementMessage(xmlElement, nameof(data.ReportedQuantity), data.ReportedQuantity.ToString());
                //handler.AddXmlElementMessage(xmlElement, nameof(data.RemainingBillQuantity), data.RemainingBillQuantity.ToString());

                if (data.PeriodQuantityList != null)
                {
                    for (int j = 0; j < data.PeriodQuantityList.Count; j++)
                    {
                        var qp = data.PeriodQuantityList[j];

                        XmlElement tempPQXmlElement = handler.AddDataXmlElement(
                            xmlElement,
                            nameof(PPRPeriodQuantityEntity)
                        );

                        handler.AddXmlElementMessage(
                            tempPQXmlElement,
                            nameof(qp.Time),
                            qp.Time.ToString()
                        );
                        handler.AddXmlElementMessage(
                            tempPQXmlElement,
                            nameof(qp.Frequency),
                            qp.Frequency.ToString()
                        );
                        handler.AddXmlElementMessage(
                            tempPQXmlElement,
                            nameof(qp.FrequencyRemark),
                            qp.FrequencyRemark
                        );
                        handler.AddXmlElementMessage(
                            tempPQXmlElement,
                            nameof(qp.FrequencyAmount),
                            qp.FrequencyAmount.ToString()
                        );
                        handler.AddXmlElementMessage(
                            tempPQXmlElement,
                            nameof(qp.FrequencyQuantity),
                            qp.FrequencyQuantity.ToString()
                        );
                        handler.AddXmlElementMessage(
                            tempPQXmlElement,
                            nameof(qp.FrequencyReportedQuantity),
                            qp.FrequencyReportedQuantity.ToString()
                        );
                    }
                }
            }
        }

        #endregion
    }

    internal class PPRXmlReadData
    {
        public PPREntityNode? Root { get; set; }
        public PPREntityNode? TempPPREntityNode { get; set; }
        public PPREntityNode? TempUnitEngineeringPPREntityNode { get; set; }
        public PPREntityNode? TempSubdivisionPPREntityNode { get; set; }
        public PPRInventoryEntity? TempPPRInventoryEntity { get; set; }
        public PPRPeriodQuantityEntity? TempPPRPeriodQuantityEntity { get; set; }

        public Action<string, PPRXmlReadData>? NodeStart { get; set; }

        public Action<string, string, PPRXmlReadData>? NodeValue { get; set; }

        public Action<string, PPRXmlReadData>? NodeEnd { get; set; }
    }
}
