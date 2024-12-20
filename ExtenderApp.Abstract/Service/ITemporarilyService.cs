

namespace ExtenderApp.Abstract
{
    /// <summary>
    /// 临时存储服务接口
    /// </summary>
    public interface ITemporarilyService
    {
        /// <summary>
        /// 添加值到临时存储
        /// </summary>
        /// <param name="id">唯一标识符</param>
        /// <param name="value">要存储的值</param>
        void AddTemporarily(int id, object value);

        /// <summary>
        /// 根据值和视图ID获取临时存储中的值
        /// </summary>
        /// <param name="valueID">值的唯一标识符</param>
        /// <param name="viewID">视图ID</param>
        /// <returns>存储的值</returns>
        object? GetTemporarily(int valueID, int viewID);

        /// <summary>
        /// 移除与指定ID相关的关系
        /// </summary>
        /// <param name="id">唯一标识符</param>
        void RemoveRelationship(int id);

        /// <summary>
        /// 移除指定ID的值
        /// </summary>
        /// <param name="id">唯一标识符</param>
        void RemoveTemporarily(int id);
    }
}
