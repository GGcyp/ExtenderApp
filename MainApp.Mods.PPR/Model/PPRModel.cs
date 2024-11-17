using MainApp.Models;
using MainApp.Models.Converters;
using MainApp.ViewModels;

namespace MainApp.Mod.PPR
{
    /// <summary>
    /// 工程支数据的读取和写入
    /// </summary>
    internal class PPRModel : BaseModel<PPRDto>, IPPRModel
    {
        private readonly DtoPool<PPRDto> _pool;

        private PPREntityNode Root { get; set; }

        public PPRModel(ModelConverterExecutor<IPPRModel> converter) : base(converter)
        {
            //FileInfoData infoData = new FileInfoData(
            //    "E:\\工程文件\\海口市秀英区西秀中心小学重建项目\\主要文件\\海口市秀英区西秀中心小学重建项目001[2023-5-4 12：08].xml"
            //);
            //converter.Execute(this, infoData);
            //infoData = new FileInfoData(
            //    "E:\\工程文件\\海口市秀英区西秀中心小学重建项目\\主要文件\\海口市秀英区西秀中心小学重建项目.xml",
            //    System.IO.FileAccess.Write
            //);
            //converter.Execute(this, infoData);

            //FileInfoData infoData = new FileInfoData(
            //    "E:\\工程文件\\海口市秀英区西秀中心小学重建项目\\主要文件\\海口市秀英区西秀中心小学重建项目.xml"
            //);
            //converter.Execute(this, infoData);
            _pool = new();
        }

        #region 业务逻辑

        public override void AddDataSource(object? data)
        {
            if (data is not PPREntityNode node) throw new ArgumentNullException(nameof(data));

            if (Root is null)
            {
                Root = node;
                return;
            }

            Root.AddPPRPeriodQuantityEntity(node);
        }

        public override object? GetDataSource()
        {
            return Root;
        }

        public override PPRDto? Get(object key)
        {
            PPRDto? result = null;
            switch (key)
            {
                case string proName:
                    result = Get(proName);
                    break;
            }
            return result;
        }

        public PPRDto? Get(string proName)
        {
            if (string.IsNullOrEmpty(proName)) return _pool.Get(Root);

            var node = Root.Get(proName);
            return _pool.Get(node);
        }

        public PPRDto? Remove(string proName)
        {
            var entity = Root.Remove(proName)?.Entity;
            return _pool.Get(entity);
        }

        public override void Add(PPRDto dto)
        {
            ArgumentNullException.ThrowIfNull(dto, nameof(dto));

            Root.Add(dto.Entity);
            _pool.Release(dto);
        }

        public override void Remove(PPRDto dto)
        {
            ArgumentNullException.ThrowIfNull(dto, nameof(dto));

            Root.Remove(dto.Entity?.ProjectName!);
            _pool.Release(dto);
        }

        public override void ForEach(Action<PPRDto> action)
        {
            ArgumentNullException.ThrowIfNull(action, nameof(action));

            var dto = _pool.Get();
            Root.LoopAllChildNodes(e =>
            {
                dto.UpdateEntity(e);
                action?.Invoke(dto);
            });
            _pool.Release(dto);
        }

        public override void Clear()
        {
            Root = null;
        }

        #endregion

        #region 专属方法

        public void ForEachPPREntityNode(Action<PPREntityNode> action)
        {
            action?.Invoke(Root);
            Root.LoopAllChildNodes((item, a) => a?.Invoke(item), action);
        }

        public void TransferPPREntity(string sourceProName, string targetProName)
        {
            //if (string.IsNullOrEmpty(sourceProName)) throw new ArgumentNullException($"PPRHead cannot be empty");

            //var sourcePPRHead = Get(sourceProName);
            //var targetPPRHead = Get(targetProName);

            //var sourceParentPPRHead = sourcePPRHead.ParentNode;
            //if (sourceParentPPRHead != null) sourceParentPPRHead.Remove(sourcePPRHead);
            //else Remove(sourceProName);

            ////加入根节点
            //if (targetPPRHead == null) Add(sourcePPRHead);
            //else targetPPRHead.Add(sourcePPRHead);
        }

        public void UpdatePerPeriodQuantityEntityFrequency(int frequency)
        {
            Root.LoopAllChildNodes(e =>
            {
                if (e.Entity is null || e.Entity.DataList is null) return;

                var list = e.Entity.DataList;
                for(int i = 0; i < list.Count; i++)
                {
                    var item = list[i];
                    var perList = item.PeriodQuantityList;
                    if (perList is null) continue;

                    for(int j = 0; j < perList.Count; j++)
                    {
                        var per = perList[j];
                        if (per.Frequency == -1)
                        {
                            per.Frequency = frequency;
                        }
                    }
                }
            });
        }

        #endregion
    }
}
