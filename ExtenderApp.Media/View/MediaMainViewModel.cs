using System.Collections.ObjectModel;
using System.Windows;
using ExtenderApp.Abstract;
using ExtenderApp.Service;
using ExtenderApp.ViewModels;
using ExtenderApp.Views;


namespace ExtenderApp.Media
{
    public class MediaMainViewModel : ExtenderAppViewModel<MediaMainView, MediaModel>
    {
        private readonly VideoModel _videoModel;
        private readonly IJsonParser _jsonParser;
        private readonly HashSet<string> _medaiPathHash;

        /// <summary>
        /// 视频列表集合
        /// </summary>
        public ObservableCollection<VideoInfo> Videos => Model.VideoInfos;

        /// <summary>
        /// 当前视频信息
        /// </summary>
        public VideoInfo CurrentVideo { get; }

        /// <summary>
        /// 视频列表宽度
        /// </summary>
        public GridLength VideoListWidth { get; set; }

        /// <summary>
        /// 视频当前播放位置
        /// </summary>
        public TimeSpan Position
        {
            get
            {
                if (_videoModel.CurrentVideoInfo is null)
                    return new(0);
                return _videoModel.GetPosition.Invoke();
            }
            set
            {
                if (_videoModel.CurrentVideoInfo is null)
                    return;
                _videoModel.SetPosition.Invoke(value);
            }
        }

        /// <summary>
        /// 获取或设置音量。
        /// </summary>
        /// <value>音量值，范围为0到1。</value>
        public double Volume
        {
            get
            {
                return Model.Volume;
            }
            set
            {
                if (value == Model.Volume)
                {
                    return;
                }
                Model.Volume = value;
                _videoModel.SetVolume.Invoke(value);
            }
        }

        /// <summary>
        /// 播放速度比率
        /// </summary>
        public double SpeedRatio { get; set; }

        /// <summary>
        /// 跳跃时间
        /// </summary>
        public double JumpTime { get; set; }

        /// <summary>
        /// 获取或设置是否记录观看时间
        /// </summary>
        public bool RecordWatchingTime
        {
            get
            {
                return Model.RecordWatchingTime;
            }
            set
            {
                Model.RecordWatchingTime = value;
            }
        }

        /// <summary>
        /// 获取或设置媒体打开时执行的动作。
        /// </summary>
        private Action mediaOpened { get; set; }

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

        public MediaMainViewModel(VideoModel videoModel, IJsonParser parser, IServiceStore serviceStore) : base(serviceStore)
        {
            _videoModel = videoModel;
            _jsonParser = parser;
            _medaiPathHash = new();

            InitVideoCommand();
            InitData();
            VideoListWidth = new GridLength(200);
            JumpTime = 10;
            _serviceStore.ScheduledTaskService.StartCycle(o => Debug("sdad"), 1000);
        }

        /// <summary>
        /// 初始化视频命令
        /// </summary>
        private void InitVideoCommand()
        {
            if (_videoModel is null)
                throw new ArgumentNullException(nameof(VideoModel));

            PlayCommand = new(Play, CanPlay);
            PauseCommand = new(Pause, CanPlay);
            StopCommand = new(Stop, CanPlay);
            FastForwardCommand = new(FastForward, CanPlay);

            _videoModel.MediaOpened = MediaOpened;
        }

        /// <summary>
        /// 初始化数据
        /// </summary>
        private void InitData()
        {
            LoadModel();
            for (int i = 0; i < Videos.Count; i++)
            {
                _medaiPathHash.Add(Videos[i].VideoPath);
            }
        }

        public override void InjectView(MediaMainView view)
        {
            base.InjectView(view);
            View.ShowVideoView(NavigateTo<PlaybackView>());
        }

        /// <summary>
        /// 添加视频路径
        /// </summary>
        /// <param name="videoPath">视频文件的路径</param>
        public void AddVideoPath(string videoPath)
        {
            var videoInfo = new VideoInfo(videoPath);

            //不可以重复加载相同地址的视频
            if (_medaiPathHash.Contains(videoInfo.VideoPath))
                return;

            Videos.Add(videoInfo);
            _videoModel.CurrentVideoInfo = videoInfo;
            OpenVideo();
            Play();
            mediaOpened = DefaultMediaOpened;

            SaveLocalData(Videos);
        }

        public override void Close()
        {
            base.Close();


            if (_videoModel.CurrentVideoInfo is null)
                return;
            Stop();
        }

        #region 视频列表

        /// <summary>
        /// 打开视频。
        /// </summary>
        /// <param name="videoInfo">视频信息对象。</param>
        /// <remarks>
        /// 如果传入的视频信息为空，或者当前已打开的视频信息与传入的视频信息相同，则不进行任何操作。
        /// 否则，更新当前视频信息，并调用OpenVideo和Play方法。
        /// </remarks>
        public void OpenVideo(VideoInfo videoInfo)
        {
            if (videoInfo is null || _videoModel.CurrentVideoInfo == videoInfo)
                return;
            _videoModel.CurrentVideoInfo = videoInfo;
            OpenVideo();
            Play();
        }

        #endregion

        #region 媒体设置

        /// <summary>
        /// 播放视频
        /// </summary>
        private void Play()
        {
            _videoModel.PlayAction.Invoke();
            if (RecordWatchingTime)
                Position = _videoModel.CurrentVideoInfo.VideoWatchedDuration;
        }

        /// <summary>
        /// 暂停视频
        /// </summary>
        private void Pause()
        {
            if (RecordWatchingTime) _videoModel.CurrentVideoInfo.VideoWatchedDuration = Position;
            _videoModel.PauseAction.Invoke();
        }

        /// <summary>
        /// 停止播放视频
        /// </summary>
        private void Stop()
        {
            if (RecordWatchingTime) 
                _videoModel.CurrentVideoInfo.VideoWatchedDuration = Position;
            _videoModel.StopAction.Invoke();
        }

        /// <summary>
        /// 快进视频
        /// </summary>
        /// <param name="JumpTime">快进时间</param>
        private void FastForward()
        {
            _videoModel.FastForwardAction.Invoke(JumpTime);
        }

        /// <summary>
        /// 打开视频
        /// </summary>
        private void OpenVideo()
        {
            _videoModel.OpenVideoAction.Invoke();
        }

        /// <summary>
        /// 判断是否可以播放视频
        /// </summary>
        /// <returns>如果可以播放，则返回true；否则返回false</returns>
        private bool CanPlay()
        {
            return _videoModel.CurrentVideoInfo != null;
        }

        /// <summary>
        /// 当媒体文件打开时调用此方法。
        /// </summary>
        private void MediaOpened()
        {
            mediaOpened?.Invoke();
            mediaOpened = null;
        }

        /// <summary>
        /// 默认媒体文件打开时调用此方法。
        /// 暂停播放并将播放位置重置为0。
        /// </summary>
        private void DefaultMediaOpened()
        {
            Pause();
            Position = RecordWatchingTime ? _videoModel.CurrentVideoInfo.VideoWatchedDuration : TimeSpan.Zero;
        }

        #endregion
    }
}
