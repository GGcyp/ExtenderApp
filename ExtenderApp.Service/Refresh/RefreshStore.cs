using ExtenderApp.Data;

namespace ExtenderApp.Service
{
    /// <summary>
    /// RefreshStore 类用于管理更新操作的存储。
    /// </summary>
    internal class RefreshStore
    {
        /// <summary>
        /// 存储更新操作的列表。
        /// </summary>
        private ValueList<Action> updateActions;

        /// <summary>
        /// 获取更新操作的数量。
        /// </summary>
        public int UpdateCount => updateActions.Count;

        /// <summary>
        /// 存储固定更新操作的列表。
        /// </summary>
        private ValueList<Action> fixupdateActions;

        /// <summary>
        /// 获取固定更新操作的数量。
        /// </summary>
        public int FixUpdateCount => fixupdateActions.Count;

        public Action ChangeEvent;

        /// <summary>
        /// 初始化 RefreshStore 类的新实例。
        /// </summary>
        public RefreshStore()
        {
            updateActions = new ValueList<Action>();
            fixupdateActions = new ValueList<Action>();
        }

        #region Update

        /// <summary>
        /// 向更新操作列表中添加一个新的更新操作。
        /// </summary>
        /// <param name="action">要添加的更新操作。</param>
        public void AddUpdate(Action action)
        {
            updateActions.Add(action);
            ChangeEvent?.Invoke();
        }

        /// <summary>
        /// 从更新操作列表中移除一个更新操作。
        /// </summary>
        /// <param name="action">要移除的更新操作。</param>
        public void RemoveUpdate(Action action)
        {
            updateActions.Remove(action);
            ChangeEvent?.Invoke();
        }

        /// <summary>
        /// 获取指定索引处的更新操作。
        /// </summary>
        /// <param name="index">要获取的更新操作的索引。</param>
        /// <returns>指定索引处的更新操作。</returns>
        public Action GetUpdateAction(int index)
        {
            return updateActions[index];
        }

        #endregion

        #region FixUpdate

        /// <summary>
        /// 向固定更新操作列表中添加一个新的固定更新操作。
        /// </summary>
        /// <param name="action">要添加的固定更新操作。</param>
        public void AddFixUpdate(Action action)
        {
            fixupdateActions.Add(action);
            ChangeEvent?.Invoke();
        }

        /// <summary>
        /// 从固定更新操作列表中移除一个固定更新操作。
        /// </summary>
        /// <param name="action">要移除的固定更新操作。</param>
        public void RemoveFixUpdate(Action action)
        {
            fixupdateActions.Remove(action);
            ChangeEvent?.Invoke();
        }

        /// <summary>
        /// 获取指定索引处的固定更新操作。
        /// </summary>
        /// <param name="index">要获取的固定更新操作的索引。</param>
        /// <returns>指定索引处的固定更新操作。</returns>
        public Action GetFixUpdateAction(int index)
        {
            return fixupdateActions[index];
        }

        #endregion
    }
}
