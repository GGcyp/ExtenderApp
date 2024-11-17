using MainApp.Common.Data;

namespace MainApp.Mod.PPR
{
    /// <summary>
    /// 存储PPREntity的节点
    /// </summary>
    public class PPREntityNode : Node<PPREntityNode>
    {
        public PPREntity Entity { get; }

        public PPREntityNode() : this(new PPREntity())
        {

        }

        public PPREntityNode(PPREntity entity)
        {
            Entity = entity;
        }

        /// <summary>
        /// 将指定的PPREntity添加到具有指定父项目名称的节点中。
        /// </summary>
        /// <param name="parentProName">父项目名称。</param>
        /// <param name="entity">要添加的PPREntity对象。</param>
        public void Add(string parentProName, PPREntity entity)
        {
            Get(parentProName)?.Add(new PPREntityNode(entity));
        }

        /// <summary>
        /// 将指定的PPREntity添加到具有指定父项目名称的节点中。
        /// </summary>
        /// <param name="parentProName">父项目名称。</param>
        /// <param name="node">要添加的PPREntityNode对象。</param>
        public void Add(string parentProName, PPREntityNode node)
        {
            PPREntityNode root = this;
            if (parentProName != Entity?.ProjectName)
            {
                root = Get(parentProName);
                if (root is null) return;
            }
            root.Add(node);
        }

        /// <summary>
        /// 将指定的PPREntity添加到当前集合中。
        /// </summary>
        /// <param name="entity">要添加的PPREntity对象。</param>
        public void Add(PPREntity? entity)
        {
            ArgumentNullException.ThrowIfNull(entity, nameof(entity));

            Add(new PPREntityNode(entity));
        }

        /// <summary>
        /// 将指定的PPRInventoryEntity添加到具有指定父项目名称的节点的值列表中。
        /// </summary>
        /// <param name="parentProName">父项目名称。</param>
        /// <param name="data">要添加的PPRInventoryEntity对象。</param>
        public void AddPPRInventoryEntity(string parentProName, PPRInventoryEntity data)
        {
            Get(parentProName)?.AddPPRInventoryEntity(data);
        }

        /// <summary>
        /// 将指定的PPRInventoryEntity添加到当前实体的值列表中。
        /// 如果当前实体为空，则返回false。
        /// </summary>
        /// <param name="data">要添加的PPRInventoryEntity对象。</param>
        /// <returns>如果添加成功，则返回true；否则返回false。</returns>
        public bool AddPPRInventoryEntity(PPRInventoryEntity data)
        {
            if (Entity == null) return false;

            if (Entity.DataList == null) Entity.DataList = new List<PPRInventoryEntity>();
            Entity.DataList.Add(data);
            return true;
        }

        /// <summary>
        /// 向PPREntityNode中添加PPRPeriodQuantityEntity实体
        /// </summary>
        /// <param name="node">要添加的PPREntityNode实体</param>
        /// <exception cref="ArgumentNullException">如果node为null，则抛出异常</exception>
        public void AddPPRPeriodQuantityEntity(PPREntityNode node)
        {
            ArgumentNullException.ThrowIfNull(node, nameof(node));

            if (node.HasChildNodes)
            {
                for(int i = 0; i < node.Count; i++)
                {
                    var n = Get(node[i].Entity.ProjectName);
                    if (n is null) continue;
                    n.AddPPRPeriodQuantityEntity(node[i]);
                }
                return;
            }

            if (node.Entity is null || node.Entity.DataList is null) return;

            var list = node.Entity.DataList;
            for(int i = 0; i < list.Count; i++)
            {
                AddPPRPeriodQuantityEntity(list[i], -1);
            }
        }

        /// <summary>
        /// 向PPRInventoryEntity中添加PPRPeriodQuantityEntity实体
        /// </summary>
        /// <param name="entity">要添加的PPRInventoryEntity实体</param>
        /// <param name="frequency">频率</param>
        /// <exception cref="ArgumentNullException">如果Entity.DataList或inventoryEntity为null，则抛出异常</exception>
        public void AddPPRPeriodQuantityEntity(PPRInventoryEntity entity, int frequency)
        {
            if(Entity.DataList is null) return;

            var inventoryEntity = GetPPRInventoryEntity(entity.ProjectID);
            ArgumentNullException.ThrowIfNull(inventoryEntity, nameof(inventoryEntity));

            if (inventoryEntity.PeriodQuantityList is null) inventoryEntity.PeriodQuantityList = new List<PPRPeriodQuantityEntity>();

            inventoryEntity.PeriodQuantityList.Add(new PPRPeriodQuantityEntity()
            {
                Frequency = frequency,
                FrequencyReportedQuantity = entity.BillOfQuantitiesQuantity,
                FrequencyAmount = inventoryEntity.UnitPrice * entity.BillOfQuantitiesQuantity,
                Time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
            });
        }

        /// <summary>
        /// 获取具有指定项目名称的PPREntityNode对象。
        /// </summary>
        /// <param name="proName">项目名称。</param>
        /// <returns>与指定项目名称匹配的PPREntityNode对象；如果未找到匹配项或项目名称为空/null，则返回null。</returns>
        public PPREntityNode? Get(string proName)
        {
            if (string.IsNullOrEmpty(proName)) return null;

            if (Entity.ProjectName == proName) return this;

            return Get(n => n.Entity.ProjectName == proName);
        }

        public PPRInventoryEntity? GetPPRInventoryEntity(string proId)
        {
            if (string.IsNullOrEmpty(proId)) return null;

            PPRInventoryEntity entity = null;

            if (Entity.DataList is not null)
            {
                entity = Entity.DataList?.Find(e => e.ProjectID == proId);
                if (entity is not null) return entity;
            }

            if (HasChildNodes)
            {
                for (int i = 0; i < Count; i++)
                {
                    entity = this[i].GetPPRInventoryEntity(proId);
                    if (entity is not null) return entity;
                }
            }

            return entity;
        }

        /// <summary>
        /// 从集合中移除具有指定项目名称的PPREntityNode对象，并返回该对象。
        /// </summary>
        /// <param name="proName">项目名称。</param>
        /// <returns>被移除的PPREntityNode对象；如果未找到匹配项，则返回null。</returns>
        public PPREntityNode? Remove(string proName)
        {
            var node = Get(proName);
            if (node == null) return null;
            Remove(node);
            return node;
        }
    }
}
