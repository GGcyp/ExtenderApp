using ExtenderApp.Abstract;
using ExtenderApp.Data;

namespace ExtenderApp.Services
{
    /// <summary>
    /// 临时服务扩展类
    /// </summary>
    public static class TemporarilyServiceExpansions
    {
        /// <summary>
        /// 获取临时数据
        /// </summary>
        /// <param name="service">临时服务接口实例</param>
        /// <param name="targetInfo">目标视图信息</param>
        /// <param name="view">视图信息</param>
        /// <returns>返回临时数据对象</returns>
        public static object? GetTemporarily(this ITemporarilyService service, ViewInfo targetInfo, ViewInfo view)
        {
            return service.GetTemporarily(targetInfo.ViewHashCode, view.ViewHashCode);
        }

        /// <summary>
        /// 获取特定类型的临时数据
        /// </summary>
        /// <typeparam name="T">要获取的临时数据的类型，必须为class类型</typeparam>
        /// <param name="service">临时服务接口实例</param>
        /// <param name="targetInfo">目标视图信息</param>
        /// <param name="view">视图信息</param>
        /// <returns>返回特定类型的临时数据对象，可能为null</returns>
        public static T? GetTemporarily<T>(this ITemporarilyService service, ViewInfo targetInfo, ViewInfo view) where T : class
        {
            return service.GetTemporarily(targetInfo.ViewHashCode, view.ViewHashCode) as T;
        }

        /// <summary>
        /// 添加临时数据
        /// </summary>
        /// <param name="service">临时服务接口实例</param>
        /// <param name="info">视图信息</param>
        /// <param name="value">要添加的数据对象</param>
        public static void AddTemporarily(this ITemporarilyService service, ViewInfo info, object value)
        {
            service.AddTemporarily(info.ViewHashCode, value);
        }

        /// <summary>
        /// 移除临时数据
        /// </summary>
        /// <param name="service">临时服务接口实例</param>
        /// <param name="info">视图信息</param>
        public static void RemoveTemporarily(this ITemporarilyService service, ViewInfo info)
        {
            service.RemoveTemporarily(info.ViewHashCode);
        }

        /// <summary>
        /// 移除关系
        /// </summary>
        /// <param name="service">临时服务接口实例</param>
        /// <param name="info">视图信息</param>
        public static void RemoveRelationship(this ITemporarilyService service, ViewInfo info)
        {
            service.RemoveRelationship(info.ViewHashCode);
        }
    }
}
