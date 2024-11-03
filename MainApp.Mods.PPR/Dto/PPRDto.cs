using System.Collections;
using MainApp.Common.ObjectPool;
using MainApp.ViewModels;
using PropertyChanged;

namespace MainApp.Mods.PPR
{
    /// <summary>
    /// PPREntity的数据交互类
    /// </summary>
    [AddINotifyPropertyChangedInterface]
    public class PPRDto : BaseDto<PPREntity>,IEnumerable<PPRDto>
    {
        private static readonly ObjectPool<PPRDto> _pool = ObjectPool.Create<PPRDto>();

        /// <summary>
        /// 当前工程名称
        /// </summary>
        public string ProName
        {
            get => Entity.ProjectName;
            set => Entity.ProjectName = value;
        }

        /// <summary>
        /// 当前工程金额
        /// </summary>
        public double ProjectAmount
        {
            get => Entity.ProjectAmount;
            set => Entity.ProjectAmount = value;
        }

        /// <summary>
        /// 当前工程清单列表
        /// </summary>
        public List<PPRInventoryEntity>? Inventories
        {
            get => Entity?.DataList;
            set => Entity.DataList = value;
        }

        public PPREntityNode? Node { get; set; }
        public override PPREntity? Entity => Node?.Entity;

        private List<PPRDto>? pprDtoChilds;
        /// <summary>
        /// 当前工程包含的工程
        /// </summary>
        public List<PPRDto>? PPRDtoChilds
        {
            get
            {
                if(pprDtoChilds == null)
                {
                    UpdatePPRDtos();
                }
                return pprDtoChilds;
            }
        }

        public void UpdatePPRDtos()
        {
            if (Node is null) return;

            if (Node.HasChildNodes)
            {
                if (pprDtoChilds is null) pprDtoChilds = new List<PPRDto>(Node.Count);
                else
                {
                    for(int i= 0; i < pprDtoChilds.Count; i++)
                    {
                        _pool.Release(pprDtoChilds[i]);
                    }
                    pprDtoChilds.Clear();
                }

                Node.LoopChildNodes(p =>
                {
                    var dto = _pool.Get();
                    dto.UpdateEntity(p);
                    pprDtoChilds.Add(dto);
                });
            }
        }

        public override void UpdateEntity(object? obj)
        {
            if(obj is null)
            {
                if (PPRDtoChilds is not null)
                {
                    for(int i = 0; i < PPRDtoChilds.Count; i++)
                    {
                        PPRDtoChilds[i].UpdateEntity(null);
                    }
                }
                UpdateEntity(null);
                _pool.Release(this);
                return;
            }

            if (obj is not PPREntityNode node) return;
            Node = node;
            //UpdateEntity(node.Entity);
        }

        public IEnumerator<PPRDto> GetEnumerator()
        {
            return PPRDtoChilds?.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
