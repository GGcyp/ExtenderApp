using ExtenderApp.Data;

namespace ExtenderApp.Torrent
{
    /// <summary>
    /// 定义了跟踪器的接口，用于管理跟踪器的基本功能和属性。
    /// </summary>
    public interface ITracker
    {
        /// <summary>
        /// 获取一个值，该值指示跟踪器是否支持Scrape请求。
        /// </summary>
        /// <returns>如果跟踪器支持Scrape请求，则为True；否则为False。</returns>
        bool CanScrape { get; }

        /// <summary>
        /// 获取在最近一次宣告请求后跟踪器的状态。
        /// </summary>
        /// <returns>跟踪器的状态。</returns>
        LinkState Status { get; }

        /// <summary>
        /// 获取跟踪器的统一资源标识符（URI）。
        /// </summary>
        /// <returns>跟踪器的URI。</returns>
        Uri Uri { get; }
    }
}
