namespace ExtenderApp.Contracts
{
    public class TreeNode<T>
    {
        /// <summary>
        /// 无限深度
        /// </summary>
        public const int InfiniteDeep = -1;
        /// <summary>
        /// 默认失败值
        /// </summary>
        public const int DefaultFailureValue = -1;

        /// <summary>
        /// 只在叶子节点保存数据
        /// </summary>
        private bool m_LeafDataOnly;

        /// <summary>
        /// 子集都被装满了
        /// </summary>
        private bool m_AllChildFilled;

        private long m_NodeID;
        public long NodeID => m_NodeID;

        private int m_NodeCount;
        public int nodesCount => m_NodeCount;

        private int m_NodesMinSize;
        public int NodesMinSize
        {
            get => m_NodesMinSize;
            //set
            //{
            //    m_NodesMinSize = value;
            //    LoopAllNodes(node => node.m_NodesMinSize = value);
            //}
        }

        private int m_NodesMaxSize;
        public int NodesMaxSize
        {
            get => m_NodesMaxSize;
            //set
            //{
            //    m_NodesMaxSize = value;
            //    LoopAllNodes(node => node.m_NodesMaxSize = value);
            //}
        }

        private int m_NodesDeep;
        public int NodeDeep => m_NodesDeep;

        private int m_NodesMaxDeep;
        public int NodeMaxDeep
        {
            get => m_NodesMaxDeep;
            //set
            //{
            //    m_NodesMaxDeep = value;
            //    LoopAllNodes(node => node.m_NodesMaxDeep = value);
            //}
        }

        private TreeNode<T> m_ParentNode;

        private TreeNode<T>[] m_Nodes;
        public TreeNode<T> this[int index]
        {
            get
            {
                if (!hasChildNodes || CheckNodeIndexOut(index))
                {
                    throw new IndexOutOfRangeException();
                }
                return m_Nodes[index];
            }
            set
            {
                if (!hasChildNodes || CheckNodeIndexOut(index))
                {
                    throw new IndexOutOfRangeException();
                }
                m_Nodes[index] = value;
            }
        }
        public bool hasChildNodes => m_Nodes != null;

        private T m_Value;
        public T value
        {
            get => m_Value;
            set => m_Value = value;
        }

        private bool CheckNodeIndexOut(int index) => index < 0 && index > m_NodeCount - 1;

        public TreeNode() : this(true, null, 1, 10, 4, 10)
        {
            //Root节点
        }

        public TreeNode(bool leafDataOnly, TreeNode<T> root, int deep, int maxDeep, int minSize, int maxSize)
        {
            m_LeafDataOnly = leafDataOnly;
            m_NodeCount = 0;
            m_Value = default(T);
            //m_AllChildFilled = deep == InfiniteDeep ? false : deep <= m_NodesMaxDeep;
            m_AllChildFilled = false;

            m_NodesMinSize = minSize;
            m_NodesMaxSize = maxSize;
            m_NodesDeep = deep;
            m_NodesMaxDeep = maxDeep;

            if (root != null)
            {
                m_NodeID = -1;
                m_ParentNode = root;
                m_NodeID = m_ParentNode.AddNode(this, out m_ParentNode);
            }
            else
            {
                m_NodeID = 0;
            }

            InternalInit();
        }

        public TreeNode(TreeNode<T> parent)
        {
            if (parent.m_AllChildFilled) return;
            m_NodeID = DefaultFailureValue;
            m_NodeID = parent.AddNode(this, out parent);
            if (m_NodeID == DefaultFailureValue) return;

            m_LeafDataOnly = parent.m_LeafDataOnly;
            m_NodeCount = 0;
            m_Value = default(T);
            //m_AllChildFilled = parent.NodeDeep == InfiniteDeep ? false : parent.NodeDeep + 1 <= m_NodesMaxDeep;
            m_AllChildFilled = false;

            m_NodesMinSize = parent.NodesMinSize;
            m_NodesMaxSize = parent.NodesMaxSize;
            m_NodesDeep = parent.NodeDeep + 1;
            m_NodesMaxDeep = parent.m_NodesMaxDeep;

            m_ParentNode = parent;

            InternalInit();
        }

        protected virtual void InternalInit()
        {

        }

        #region 增删改查

        private void CreateOrExpansionNodes()
        {
            if (NodeDeep + 1 > m_NodesMaxSize && m_NodesMaxSize != InfiniteDeep) return;

            if (!hasChildNodes)
            {
                m_Nodes = new TreeNode<T>[m_NodesMinSize];
                return;
            }

            var array = m_Nodes;
            if (array.Length >= m_NodesMaxSize) return;


            int expandQuantity = array.Length * 2 > m_NodesMaxSize ? m_NodesMaxSize : array.Length * 2;
            m_Nodes = new TreeNode<T>[expandQuantity];
            array.CopyTo(m_Nodes, 0);
            return;
        }

        /// <summary>
        /// 添加节点，返回ID
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public long AddNode(TreeNode<T> node)
        {
            return AddNode(node, out var parentNode);
        }

        /// <summary>
        /// 添加节点，返回ID
        /// </summary>
        /// <param name="node"></param>
        /// <param name="parentNode"></param>
        /// <returns></returns>
        public long AddNode(TreeNode<T> node, out TreeNode<T> parentNode)
        {
            parentNode = null;
            if (m_AllChildFilled)
            {
                return DefaultFailureValue;
            }
            CreateOrExpansionNodes();
            return InternalAddNode(node, out parentNode);
        }

        /// <summary>
        /// 添加节点，返回ID
        /// </summary>
        /// <param name="node"></param>
        /// <param name="parentID"></param>
        /// <returns></returns>
        public long AddNode(TreeNode<T> node, long parentID)
        {
            var parentNode = GetNode(parentID);
            if (parentNode == null) return DefaultFailureValue;
            return parentNode.AddNode(node);
        }

        /// <summary>
        /// 添加节点，返回ID
        /// </summary>
        /// <param name="node"></param>
        /// <param name="parentNode"></param>
        /// <returns></returns>
        protected virtual long InternalAddNode(TreeNode<T> node, out TreeNode<T> parentNode)
        {
            parentNode = null;
            long resultID = DefaultFailureValue;
            if (!hasChildNodes) return resultID;
            if (node.NodeID != resultID)
            {
                if (GetNode(node.NodeID) != null) return resultID;
            }

            if (m_NodeCount + 1 > m_NodesMaxSize)
            {
                //从后往前添加node
                for (int i = m_NodeCount - 1; i >= 0; i--)
                {
                    var tempNode = m_Nodes[i];
                    resultID = tempNode.AddNode(node, out parentNode);

                    if (resultID != -1) break;
                }

                //后面都满了
                if (resultID == DefaultFailureValue)
                {
                    m_AllChildFilled = true;
                    return resultID;
                }
            }
            else
            {
                parentNode = this;
                m_Nodes[m_NodeCount] = node;
                resultID = GetChildNodeId(m_NodeCount);
                m_NodeCount++;
                //MessageLogger.Print(m_NodeCount);
                m_AllChildFilled = NodeDeep + 1 == NodeMaxDeep && m_NodeCount == m_NodesMaxSize;
            }

            if (!m_LeafDataOnly || value == null) return resultID;
            node.value = m_Value;
            this.m_Value = default(T);
            return resultID;
        }

        /// <summary>
        /// 删除自己
        /// </summary>
        public bool RemoveNode()
        {
            if (m_ParentNode == null) return false;
            return m_ParentNode.RemoveNode(NodeID) != null;
        }

        public bool RemoveNode(TreeNode<T> node)
        {
            if (m_ParentNode == null) return false;
            return m_ParentNode.RemoveNode(node.NodeID) != null;
        }

        public TreeNode<T> RemoveNode(long id)
        {
            return InternalRemoveNode(id, out T value);
        }

        public TreeNode<T> RemoveNode(long id, out T value)
        {
            return InternalRemoveNode(id, out value);
        }

        protected virtual TreeNode<T> InternalRemoveNode(long id, out T value)
        {
            value = default(T);

            TreeNode<T> resultNode = null;
            if (!hasChildNodes) return resultNode;

            resultNode = GetNode(id);

            //在里面找不到或者已经被删除
            if (resultNode == null || resultNode.m_ParentNode == null) return resultNode;

            var nodeIndex = resultNode.m_ParentNode.GetNodeInChildNodeIndex(id);
            if (nodeIndex == DefaultFailureValue) return resultNode;
            var node = resultNode.m_ParentNode[nodeIndex];
            if (node.NodeID != id) throw new ArgumentNullException($"没有找到将要删除{id}的Node");
            resultNode = node;
            value = node.value;


            for (int i = 0; i < m_NodeCount - 1; i++)
            {
                m_Nodes[i] = m_Nodes[i + 1];
            }

            m_NodeCount--;
            return node;
        }

        /// <summary>
        /// 获取叶子节点的最后一个节点
        /// </summary>
        /// <returns></returns>
        public TreeNode<T> GetLeafTreeNode()
        {
            TreeNode<T> resultNode = this;

            while (resultNode.hasChildNodes)
            {
                var tempNode = resultNode.m_Nodes[resultNode.nodesCount - 1];
                if (tempNode == null) return resultNode;
                resultNode = tempNode;
            }
            return resultNode;
        }

        public TreeNode<T> GetNode(long id)
        {
            return InternalGetNode(id);
        }

        protected virtual TreeNode<T> InternalGetNode(long id)
        {
            if (NodeID == id) return this;
            if (!hasChildNodes) return null;

            int deep = GetIDForDeep(id, out long min);
            TreeNode<T> resulfNode = this;

            while (true)
            {
                deep -= 1;
                int index = resulfNode.GetNodeInChildNodeIndex(id, min, deep, out resulfNode);
                if (index < 0 || resulfNode == null || resulfNode.NodeID == id) return resulfNode;
            }
        }

        protected virtual TreeNode<T> GetNodeInChildNode(long id)
        {
            int index = GetNodeInChildNodeIndex(id);
            if (index == DefaultFailureValue) return null;
            return m_Nodes[index];
        }

        /// <summary>
        /// 获得这个的id在当节点的子节点中的ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        protected virtual long GetNodeInChildNodeID(long id)
        {
            int index = GetNodeInChildNodeIndex(id);
            if (index == DefaultFailureValue) return DefaultFailureValue;
            return m_Nodes[index].NodeID;
        }

        /// <summary>
        /// 获得这个的id在当节点的子节点中的Index
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        protected virtual int GetNodeInChildNodeIndex(long id)
        {
            return GetNodeInChildNodeIndex(id, DefaultFailureValue, DefaultFailureValue, out TreeNode<T> resulfNode);
        }

        /// <summary>
        /// 获得这个的id在当节点的子节点中的Index
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        protected virtual int GetNodeInChildNodeIndex(long id, long min, int deep, out TreeNode<T> resulfNode)
        {
            resulfNode = null;
            if (!hasChildNodes || id < NodeID) return DefaultFailureValue;
            //Debug.Print(deep.ToString());

            if (deep == DefaultFailureValue)
            {
                deep = GetIDForDeep(id, out min);
            }
            //没有找到所在层
            if (deep == DefaultFailureValue) return DefaultFailureValue - 1;

            long offset = (int)Math.Pow(m_NodesMaxSize, deep);
            int index = (int)(NodeID % m_NodesMaxSize);
            //index需要少1，注意nodeID为0的情况
            index = NodeID == 0 ? 0 : index == 0 ? m_NodesMaxSize - 1 : index - 1;
            min += index * (long)Math.Pow(m_NodesMaxSize, deep + 1);
            //Debug.Print("测算：" + min + "  :  " + offset + "  :  " + deep + "  :  " + (NodeID % m_NodesMaxSize) + "  :  " + index);

            for (int i = 0; i < nodesCount; i++)
            {
                resulfNode = m_Nodes[i];

                long offsetIndex = resulfNode.NodeID % m_NodesMaxSize;
                //为0时是最后的数
                offsetIndex = offsetIndex == 0 ? m_NodesMaxSize - 1 : offsetIndex - 1;
                offsetIndex *= offset;

                //Debug.Print(offsetIndex + "  :  " + offset + "  :  " + (min + offsetIndex) + "  :  " + (min + offsetIndex + offset - 1));
                //因为在offset为1时，会在没到达指定位置时触发，所以需要加上条件
                if ((min + offsetIndex <= id && min + offsetIndex + offset - 1 >= id) && (offset != 1 || m_Nodes[i].NodeID == id))
                {
                    return i;
                }
            }

            resulfNode = null;
            return DefaultFailureValue - 2;
        }

        protected virtual int GetIDForDeep(long id, out long min)
        {
            //根结点ID为0
            //先确定ID在什么深度
            min = 1;
            long max = 0;

            for (int i = 1; i <= NodeMaxDeep; i++)
            {
                min += max;
                //一位Index的偏移量
                long offset = (long)Math.Pow(m_NodesMaxSize, i);
                max += offset;
                //Debug.Print("深度：" + min + "  :  " + offset + "  :  " + max);

                //找到深度
                if (min <= id && max >= id)
                {
                    return i;
                }
            }
            return DefaultFailureValue;
        }

        public long GetChildNodeId()
        {
            return GetChildNodeId(m_NodeCount);
        }

        /// <summary>
        /// 计算当前节点最后一个子节点的ID
        /// </summary>
        /// <returns></returns>
        public virtual long GetChildNodeId(int index)
        {
            if (index == DefaultFailureValue) return DefaultFailureValue;

            long min = 0;
            for (int i = 0; i < NodeDeep; i++)
            {
                min += (long)Math.Pow(m_NodesMaxSize, i);
            }

            long offsetIndex = 0;
            long tempOffset = NodeID % m_NodesMaxSize;
            if (NodeID != 0) offsetIndex = tempOffset == 0 ? m_NodesMaxSize : tempOffset - 1;
            long offset = offsetIndex * (long)Math.Pow(m_NodesMaxSize, NodeDeep - 1);

            return min + offset + index;
        }

        protected virtual void LoopChildNodes(Action<TreeNode<T>> action)
        {
            if (!hasChildNodes) return;

            for (int i = 0; i < nodesCount; i++)
            {
                var node = m_Nodes[i];
                action(node);
            }
        }

        protected virtual void LoopAllChildNodes(Action<TreeNode<T>> action)
        {
            if (!hasChildNodes) return;

            LoopChildNodes(action);

            for (int i = 0; i < nodesCount; i++)
            {
                var node = m_Nodes[i];
                node.LoopAllChildNodes(action);
            }
        }

        protected virtual void LoopChildNodes<T1>(Action<TreeNode<T>, T1> action, T1 value)
        {
            if (!hasChildNodes) return;

            for (int i = 0; i < nodesCount; i++)
            {
                var node = m_Nodes[i];
                action(node, value);
            }
        }

        protected virtual void LoopAllChildNodes<T1>(Action<TreeNode<T>, T1> action, T1 value)
        {
            if (!hasChildNodes) return;

            LoopChildNodes(action, value);

            for (int i = 0; i < nodesCount; i++)
            {
                var node = m_Nodes[i];
                node.LoopAllChildNodes(action, value);
            }
        }

        protected virtual void LoopChildNodes<T1, T2>(Action<TreeNode<T>, T1, T2> action, T1 value1, T2 value2)
        {
            if (!hasChildNodes) return;

            for (int i = 0; i < nodesCount; i++)
            {
                var node = m_Nodes[i];
                action(node, value1, value2);
            }
        }

        protected virtual void LoopAllChildNodes<T1, T2>(Action<TreeNode<T>, T1, T2> action, T1 value1, T2 value2)
        {
            if (!hasChildNodes) return;

            LoopChildNodes(action, value1, value2);

            for (int i = 0; i < nodesCount; i++)
            {
                var node = m_Nodes[i];
                node.LoopAllChildNodes(action, value1, value2);
            }
        }

        #endregion
    }
}
