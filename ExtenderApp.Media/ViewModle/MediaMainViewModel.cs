using ExtenderApp.Abstract;
using ExtenderApp.Data;
using ExtenderApp.ViewModels;
using ExtenderApp.Views;


namespace ExtenderApp.Media
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
        /// 扩展的取消令牌，用于管理当前时间的更新。
        /// </summary>
        private ExtenderCancellationToken currentTimeToken;

        #endregion

        #region 绑定属性

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
                Model.SetVolume.Invoke(value);
            }
        }

        /// <summary>
        /// 视频当前播放位置
        /// </summary>
        public TimeSpan Position
        {
            get
            {
                if (Model.CurrentVideoInfo is null)
                    return new(0);
                return Model.GetPosition.Invoke();
            }
            set
            {
                if (Model.CurrentVideoInfo is null)
                    return;
                Model.SetPosition.Invoke(value);
            }
        }

        /// <summary>
        /// 当前时间
        /// </summary>
        public TimeSpan CurrentTime { get; set; }

        /// <summary>
        /// 总时间
        /// </summary>
        public TimeSpan TotalTime { get; set; }


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
        /// 视频文件不存在则删除
        /// </summary>
        public bool VideoNotExist
        {
            get => Model.VideoNotExist;
            set => Model.VideoNotExist = value;
        }

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
        }

        /// <summary>
        /// 初始化视频命令
        /// </summary>
        private void InitVideoCommand()
        {
            PlayCommand = new(Play, CanPlay);
            PauseCommand = new(Pause, CanPlay);
            StopCommand = new(Stop, CanPlay);
            FastForwardCommand = new(FastForward, CanPlay);

            Model.MediaOpened = MediaOpened;

            Model.SelectedVideoAction = SelectedVideo;
        }

        private void InitData()
        {
            JumpTime = 10;
            CurrentTime = TimeSpan.Zero;
            TotalTime = TimeSpan.Zero;
            Volume = Model.Volume;
            RecordWatchingTime = true;

            if (VideoNotExist)
                CheckVideoPath();
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

        public override void InjectView(MediaMainView view)
        {
            base.InjectView(view);
            View.ShowVideoView(NavigateTo<VideoView>());
            View.ShowVideoList(NavigateTo<VideoListView>());

            Model.SetVolume.Invoke(Model.Volume);
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
            OpenVideo();
            Play();
            mediaOpened = DefaultMediaOpened;

            SaveModel();
        }

        public override void Close()
        {
            base.Close();


            if (Model.CurrentVideoInfo is null)
                return;
            Stop();
            Model.CurrentVideoInfo = null;
            SaveModel();
        }

        #region 更新

        public void UpdateVoideoTime(TimeSpan newTimeSpan)
        {
            CurrentTime = newTimeSpan;
            Position = newTimeSpan;

            if (RecordWatchingTime)
                Model.CurrentVideoInfo.VideoWatchedPosition = newTimeSpan;
        }

        public void UpdateVolume()
        {
            SaveModel();
        }

        #endregion

        #region 视频列表

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
            if (isPlaying) return;

            if (RecordWatchingTime)
                Position = Model.CurrentVideoInfo.VideoWatchedPosition;
            Model.PlayAction.Invoke();
            isPlaying = true;

            currentTimeToken.Resume();
        }

        /// <summary>
        /// 暂停视频
        /// </summary>
        private void Pause()
        {
            if (!isPlaying) return;

            Model.PauseAction.Invoke();
            if (RecordWatchingTime)
                Model.CurrentVideoInfo.VideoWatchedPosition = Position;

            isPlaying = false;
            currentTimeToken.Pause();
            SaveModel();
        }

        /// <summary>
        /// 停止播放视频
        /// </summary>
        private void Stop()
        {
            if (!isPlaying) return;

            if (RecordWatchingTime)
                Model.CurrentVideoInfo.VideoWatchedPosition = Position;

            Model.StopAction.Invoke();
            Model.CurrentVideoInfo = null;
            isPlaying = false;
            currentTimeToken.Stop();
            SaveModel();
        }

        /// <summary>
        /// 快进视频
        /// </summary>
        /// <param name="JumpTime">快进时间</param>
        private void FastForward()
        {
            Model.FastForwardAction.Invoke(JumpTime);
            CurrentTime = Position;
        }

        /// <summary>
        /// 打开视频
        /// </summary>
        private void OpenVideo()
        {
            Model.OpenVideoAction.Invoke();

            mediaOpened = () =>
            {
                TotalTime = Model.NaturalDurationFunc.Invoke().TimeSpan;
                CurrentTime = Model.RecordWatchingTime ? Model.CurrentVideoInfo.VideoWatchedPosition : new TimeSpan(0, 0, 0);
                currentTimeToken = StartCycle(o =>
                {
                    DispatcherInvoke(() =>
                    {
                        CurrentTime = Position;
                        if (RecordWatchingTime)
                            Model.CurrentVideoInfo.VideoWatchedPosition = Position;
                    });
                }, TimeSpan.FromSeconds(1));
            };
        }

        /// <summary>
        /// 判断是否可以播放视频
        /// </summary>
        /// <returns>如果可以播放，则返回true；否则返回false</returns>
        private bool CanPlay()
        {
            return Model.CurrentVideoInfo != null;
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
            Position = RecordWatchingTime ? Model.CurrentVideoInfo.VideoWatchedPosition : TimeSpan.Zero;
        }

        #endregion
    }
}
