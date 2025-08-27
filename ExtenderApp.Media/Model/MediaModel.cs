using System.Collections.ObjectModel;
using System.Windows;
using ExtenderApp.Models;

namespace ExtenderApp.Media
{
    /// <summary>
    /// 媒体数据类
    /// </summary>
    public class MediaModel : ExtenderAppModel
    {
        /// <summary>
        /// 视频信息集合
        /// </summary>
        public ObservableCollection<VideoInfo> VideoInfos { get; set; }

        /// <summary>
        /// 音量
        /// </summary>
        public double Volume { get; set; }

        /// <summary>
        /// 是否记录观看时间
        /// </summary>
        public bool RecordWatchingTime { get; set; }

        /// <summary>
        /// 视频文件不存在则删除
        /// </summary>
        public bool VideoNotExist { get; set; }

        public MediaModel()
        {
            VideoInfos = new ObservableCollection<VideoInfo>();
            Volume = 0;
        }

        #region 视频列表操作相关属性和方法

        /// <summary>
        /// 获取或设置添加视频路径的操作。
        /// </summary>
        /// <value>
        /// 一个将字符串（视频路径）作为输入并返回 VideoInfo 对象的委托。
        /// </value>
        public Func<string, VideoInfo> AddVideoPathAction { get; set; }

        /// <summary>
        /// 获取或设置选中的视频信息对应的操作。
        /// </summary>
        public Action<VideoInfo> SelectedVideoAction { get; set; }

        #endregion

        #region 视频操作相关属性和方法

        /// <summary>
        /// 获取或设置当前视频信息。
        /// </summary>
        public VideoInfo CurrentVideoInfo { get; set; }

        /// <summary>
        /// 播放视频。
        /// </summary>
        public Action PlayAction { get; set; }

        /// <summary>
        /// 停止视频。
        /// </summary>
        public Action StopAction { get; set; }

        /// <summary>
        /// 暂停视频。
        /// </summary>
        public Action PauseAction { get; set; }

        /// <summary>
        /// 设置快进视频，参数表示跳过的时间。
        /// </summary>
        public Action<double> FastForwardAction { get; set; }

        /// <summary>
        /// 打开视频。
        /// </summary>
        public Action OpenVideoAction { get; set; }

        /// <summary>
        /// 获取或设置用于设置速度比例的动作。
        /// </summary>
        public Action<double> SpeedRatioAction { get; set; }

        /// <summary>
        /// 获取或设置用于获取自然时间间隔的函数。
        /// </summary>
        public Func<Duration> NaturalDurationFunc { get; set; }

        /// <summary>
        /// 获取或设置用于设置位置的动作。
        /// </summary>
        public Action<TimeSpan> SetPosition { get; set; }

        /// <summary>
        /// 获取或设置用于获取当前位置的时间间隔的函数。
        /// </summary>
        public Func<TimeSpan> GetPosition { get; set; }

        /// <summary>
        /// 设置音量的动作委托。
        /// </summary>
        public Action<double> SetVolume { get; set; }

        /// <summary>
        /// 获取音量的函数委托。
        /// </summary>
        public Func<double> GetVolume { get; set; }

        /// <summary>
        /// 媒体打开视频时触发的事件
        /// </summary>
        public Action MediaOpened { get; set; }

        #endregion
    }
}
