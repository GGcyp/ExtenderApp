using ExtenderApp.Abstract;
using ExtenderApp.FFmpegEngines;
using ExtenderApp.Media.Models;
using ExtenderApp.ViewModels;
using ExtenderApp.Views.Commands;
using Microsoft.Win32;


namespace ExtenderApp.Media.ViewModels
{
    public class MediaMainViewModel : ExtenderAppViewModel<MediaMainView, MediaModel>
    {
        private readonly MediaEngine _engine;

        #region 按钮

        /// <summary>
        /// 播放命令。
        /// </summary>
        public NoValueCommand MediaStateChangeCommand { get; private set; }

        /// <summary>
        /// 快进命令。
        /// </summary>
        public NoValueCommand FastForwardCommand { get; private set; }

        #endregion

        public MediaMainViewModel(IServiceStore serviceStore, MediaEngine engine) : base(serviceStore)
        {
            _engine = engine;

            Model.CurrentVideoView = NavigateTo<VideoView>();
            Model.CurrentVideoListView = NavigateTo<VideoListView>();

            MediaStateChangeCommand = new NoValueCommand(OnMediaStateChange);
            FastForwardCommand = new NoValueCommand(OnFastForward);

            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Title = "选择视频文件",
                Filter = "视频文件|*.mp4;*.mkv;*.avi;*.mov;*.wmv;*.flv;*.webm;*.mpg;*.mpeg;*.3gp|所有文件|*.*",
                Multiselect = true
            };
            openFileDialog.ShowDialog();
            Model.SelectedVideoInfo = new MediaInfo(new Uri(openFileDialog.FileName));
            var player = _engine.OpenMedia(Model.SelectedVideoInfo.MediaUri);
            Model.SetPlayer(player);
        }

        private void OnFastForward()
        {
            Model.Seek(Model.Position + TimeSpan.FromSeconds(10));
        }

        private void OnMediaStateChange()
        {
            if (Model.SelectedVideoInfo == null || Model.MPlayer == null)
            {
                return;
            }

            if (Model.MPlayer.State == PlayerState.Paused || Model.MPlayer.State == PlayerState.Initializing)
            {
                Model.Play();
            }
            else if (Model.MPlayer.State == PlayerState.Playing)
            {
                Model.Pause();
            }
        }
    }
}
