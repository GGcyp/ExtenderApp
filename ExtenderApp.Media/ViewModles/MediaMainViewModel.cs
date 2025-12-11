using ExtenderApp.Abstract;
using ExtenderApp.Data;
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

        public NoValueCommand StopCommand { get; private set; }

        /// <summary>
        /// 快进命令。
        /// </summary>
        public NoValueCommand FastForwardCommand { get; private set; }

        /// <summary>
        /// 快退命令。
        /// </summary>
        public NoValueCommand RewindCommand { get; private set; }

        #endregion 按钮

        public MediaMainViewModel(IServiceStore serviceStore, MediaEngine engine) : base(serviceStore)
        {
            _engine = engine;

            Model.CurrentVideoListView = NavigateTo<VideoListView>();

            MediaStateChangeCommand = new(OnMediaStateChange);
            FastForwardCommand = new(OnFastForward);

            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Title = "选择视频文件",
                Filter = "视频文件|*.mp4;*.mkv;*.avi;*.mov;*.wmv;*.flv;*.webm;*.mpg;*.mpeg;*.3gp|所有文件|*.*",
                Multiselect = true
            };
            openFileDialog.ShowDialog();
            try
            {
                Model.SelectedVideoInfo = new MediaInfo(new Uri(openFileDialog.FileName));
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("未选择文件或选择的文件无法打开！" + ex.Message);
                return;
            }
            var player = _engine.OpenMedia(Model.SelectedVideoInfo.MediaUri);
            Model.SetPlayer(player);
            SubscribeMessage<KeyDownEvent>(OnKeyDown);
        }

        private void OnKeyDown(object? sender, KeyDownEvent e)
        {
            if (e.Key == Key.Space && !e.IsRepeat)
            {
                OnMediaStateChange();
            }
        }

        public void OnFastForward()
        {
            Model.Seek(Model.Position + TimeSpan.FromSeconds(Model.JumpTime));
        }

        public void OnReverseOrForward(bool isForward)
        {
            TimeSpan jumpTime = TimeSpan.FromSeconds(Model.JumpTime);
            TimeSpan targetTime = isForward ? Model.Position + jumpTime : Model.Position - jumpTime;
            Model.Seek(targetTime);
        }

        internal void OnRate(double rate)
        {
            Model.Rate = rate;
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

        internal void Seek(TimeSpan timeSpan)
        {
            Model.Seek(timeSpan);
            //Model.Play();
        }
    }
}