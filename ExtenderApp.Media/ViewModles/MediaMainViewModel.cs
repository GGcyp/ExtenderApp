using ExtenderApp.Abstract;
using ExtenderApp.Common;
using ExtenderApp.Data;
using ExtenderApp.Media.Models;
using ExtenderApp.ViewModels;
using ExtenderApp.Views.Commands;


namespace ExtenderApp.Media.ViewModels
{
    public class MediaMainViewModel : ExtenderAppViewModel<MediaMainView, MediaModel>
    {
        #region 内部属性

        /// <summary>
        /// 播放速度比率
        /// </summary>
        public double SpeedRatio { get; set; }

        /// <summary>
        /// 跳跃时间
        /// </summary>
        public double JumpTime { get; set; }


        /// <summary>
        /// 获取或设置媒体打开时执行的动作。
        /// </summary>
        private Action mediaOpened { get; set; }

        /// <summary>
        /// 窗口宽度
        /// </summary>
        public double WindowWidth { get; set; }

        /// <summary>
        /// 判断当前是否正在播放的布尔值。
        /// </summary>
        private bool isPlaying;

        /// <summary>
        /// 一个只读字段，存储一个ScheduledTask对象。
        /// </summary>
        private readonly ScheduledTask _task;

        #endregion

        #region 绑定属性

        /// <summary>
        /// 当前时间
        /// </summary>
        public TimeSpan CurrentTime { get; set; }

        /// <summary>
        /// 总时间
        /// </summary>
        public TimeSpan TotalTime { get; set; }

        #endregion

        #region 按钮

        /// <summary>
        /// 播放命令。
        /// </summary>
        public NoValueCommand PlayCommand { get; private set; }

        /// <summary>
        /// 暂停命令。
        /// </summary>
        public NoValueCommand PauseCommand { get; private set; }

        /// <summary>
        /// 停止命令。
        /// </summary>
        public NoValueCommand StopCommand { get; private set; }

        /// <summary>
        /// 快进命令。
        /// </summary>
        public NoValueCommand FastForwardCommand { get; private set; }

        #endregion

        public MediaMainViewModel(IServiceStore serviceStore) : base(serviceStore)
        {
            Model.CurrentVideoView = NavigateTo<VideoView>();
            Model.CurrentVideoListView = NavigateTo<VideoListView>();

            _task = new ScheduledTask();
        }
    }
}
