using ExtenderApp.Abstract;
using ExtenderApp.Data;
using ExtenderApp.FFmpegEngines;
using ExtenderApp.FFmpegEngines.Medias;
using ExtenderApp.Media.Models;
using ExtenderApp.Models;
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
                Model.OpenMedia(openFileDialog.FileName);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("未选择文件或选择的文件无法打开！" + ex.Message);
                return;
            }
            SubscribeMessage<KeyDownEvent>(OnKeyDown);
        }

        private void OnKeyDown(object? sender, KeyDownEvent e)
        {
            if (e.IsRepeat)
                return;

            switch (e.Key)
            {
                case Key.Left:
                    OnReverseOrForward(false);
                    break;

                case Key.Right:
                    OnReverseOrForward(true);
                    break;

                case Key.Space:
                    OnMediaStateChange();
                    break;

                case Key.Up:
                    Model.Volume += 0.05f;
                    break;

                case Key.Down:
                    Model.Volume -= 0.05f;
                    break;
            }
        }

        public void OnFastForward()
        {
            OnReverseOrForward(true);
        }

        public void OnReverseOrForward(bool isForward)
        {
            TimeSpan jumpTime = TimeSpan.FromSeconds(Model.JumpTime);
            TimeSpan targetTime = isForward ? Model.Position + jumpTime : Model.Position - jumpTime;
            Model.Seek(targetTime);
        }

        private void OnMediaStateChange()
        {
            switch (Model.State)
            {
                case PlayerState.Playing:
                    Model.Pause();
                    break;

                case PlayerState.Paused:
                case PlayerState.Initializing:
                    Model.Play();
                    break;
            }
        }

        internal void Seek(TimeSpan timeSpan)
        {
            Model.Seek(timeSpan);
            //Model.Play();
        }
    }
}