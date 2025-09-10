using ExtenderApp.Abstract;
using ExtenderApp.Common;
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
            InitVideoCommand();
            InitData();

            Model.CurrentVideoView = NavigateTo<VideoView>();
            Model.CurrentVideoListView = NavigateTo<VideoListView>();

            _task = new ScheduledTask();
        }

        /// <summary>
        /// 初始化视频命令
        /// </summary>
        private void InitVideoCommand()
        {
            Model.SelectedVideoAction = SelectedVideo;
        }

        private void InitData()
        {
            JumpTime = 10;
            CurrentTime = TimeSpan.Zero;
            TotalTime = TimeSpan.Zero;
        }

        private void CheckVideoPath()
        {
            var intList = new List<int>();
            var list = Model.VideoInfos;
            for (int i = 0; i < list.Count; i++)
            {
                if (!list[i].VideoFileInfo.Exists)
                    intList.Add(i);
            }

            for (int i = 0; i < intList.Count; i++)
            {
                list.RemoveAt(i);
            }
        }

        /// <summary>
        /// 添加视频路径
        /// </summary>
        /// <param name="videoPath">视频文件的路径</param>
        public void AddVideoPath(string videoPath)
        {
            //不可以重复加载相同地址的视频
            var videoInfo = Model.AddVideoPathAction.Invoke(videoPath);
            if (videoInfo == null)
                return;

            Model.CurrentVideoInfo = videoInfo;

            SaveModel();
        }

        public override void Close()
        {
            base.Close();


            if (Model.CurrentVideoInfo is null)
                return;
            Model.CurrentVideoInfo = null;
            SaveModel();
        }

        #region 更新

        public void UpdateVoideoTime(TimeSpan newTimeSpan)
        {
            CurrentTime = newTimeSpan;
        }

        public void UpdateVolume()
        {
            SaveModel();
        }

        #endregion


        /// <summary>
        /// 打开视频。
        /// </summary>
        /// <param name="videoInfo">视频信息对象。</param>
        /// <remarks>
        /// 如果传入的视频信息为空，或者当前已打开的视频信息与传入的视频信息相同，则不进行任何操作。
        /// 否则，更新当前视频信息，并调用OpenVideo和Play方法。
        /// </remarks>
        public void SelectedVideo(VideoInfo videoInfo)
        {
            if (videoInfo is null || Model.CurrentVideoInfo == videoInfo)
                return;

            Model.CurrentVideoInfo = videoInfo;
        }
    }
}
