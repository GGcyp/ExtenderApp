using ExtenderApp.Abstract;

namespace ExtenderApp.Services
{
    /// <summary>
    /// 临时存储类，实现了ITemporarilyStore接口
    /// </summary>
    internal class TemporarilyService : ITemporarilyService
    {
        /// <summary>
        /// 临时字典，用于存储临时数据。
        /// </summary>
        private readonly Dictionary<int, object> _temporarilyDict;

        /// <summary>
        /// 视图数据关系字典，用于存储视图数据之间的关系。
        /// </summary>
        private readonly Dictionary<int, List<int>> _viewDataRelationshipDict;

        public TemporarilyService()
        {
            _temporarilyDict = new Dictionary<int, object>();
            _viewDataRelationshipDict = new Dictionary<int, List<int>>();
        }

        public void AddTemporarily(int id, object value)
        {
            if (_temporarilyDict.ContainsKey(id))
                throw new Exception($"cannt be repeat {value.GetType().Name}");

            _temporarilyDict.Add(id, value);
        }

        public void RemoveTemporarily(int id)
        {
            _temporarilyDict.Remove(id);
        }

        public void RemoveRelationship(int id)
        {
            _viewDataRelationshipDict.Remove(id, out var list);
            if (list is null) return;

            for (int i = 0; i < list.Count; i++)
            {
                _temporarilyDict.Remove(list[i]);
            }
        }

        public object? GetTemporarily(int valueID, int viewID)
        {
            if (!_temporarilyDict.TryGetValue(valueID, out var value))
                return null;

            if (!_viewDataRelationshipDict.TryGetValue(viewID, out var list))
            {
                list = new List<int>();
                _viewDataRelationshipDict.Add(viewID, list);
            }

            list.Add(valueID);
            return value;
        }
    }
}
