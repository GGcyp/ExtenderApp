using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using ExtenderApp.Abstract;
using ExtenderApp.ViewModels;
using ExtenderApp.Views;
using ExtenderApp.Views.Command;


namespace ExtenderApp.Media
{
    public class MediaMainViewModel : ExtenderAppViewModel<MediaMainView>
    {
        private readonly VideoModel _videoModel;
        private readonly IJsonParser _jsonParser;
        private readonly HashSet<string> _medaiPathHash;

        public ObservableCollection<VideoInfo> Videos { get; set; }
        public VideoInfo CurrentVideo { get; }
        public GridLength VideoListWidth { get; set; }
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

        public double SpeedRatio { get; set; }
        public double JumpTime { get; set; }

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
        }

        /// <summary>
        /// 初始化数据
        /// </summary>
        private void InitData()
        {
            if (!GetData(out ObservableCollection<VideoInfo> videos))
            {
                Videos = new();
            }
            else
            {
                Videos = videos;
            }

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
            SetData(Videos);
        }

        #region 视频列表

        public void OpenVideo(VideoInfo videoInfo)
        {
            _videoModel.CurrentVideoInfo = videoInfo;
            OpenVideo();
        }

        #endregion

        #region 媒体设置

        private void Play()
        {
            _videoModel.PlayAction.Invoke();
        }

        private void Pause()
        {
            _videoModel.PauseAction.Invoke();
        }

        private void Stop()
        {
            _videoModel.StopAction.Invoke();
        }

        private void FastForward()
        {
            _videoModel.FastForwardAction.Invoke(JumpTime);
        }

        private void OpenVideo()
        {
            _videoModel.OpenVideoAction.Invoke();
        }

        private bool CanPlay()
        {
            return _videoModel.CurrentVideoInfo != null;
        }

        #endregion
    }
}
