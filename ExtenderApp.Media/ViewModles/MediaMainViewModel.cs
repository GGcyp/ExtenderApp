using ExtenderApp.Abstract;
using ExtenderApp.Data;
using ExtenderApp.FFmpegEngines;
using ExtenderApp.FFmpegEngines.Medias;
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
                Model.OpenMedia(openFileDialog.FileName);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("未选择文件或选择的文件无法打开！" + ex.Message);
                return;
            }
            SubscribeMessage<KeyDownEvent>(OnKeyDown);
            SubscribeMessage<KeyUpEvent>(OnKeyUp);
        }

        private void OnKeyUp(object? sender, KeyUpEvent e)
        {
            Key key = e.Key;
            bool isRepeat = e.IsRepeat;

            switch (key)
            {
                case Key.Left:
                    OnReverseOrForward(false);
                    break;

                case Key.Right:
                    if(Model.Rate != 1.0d)
                    {
                        Model.Rate = 1.0d;
                    }
                    else
                    {
                        OnReverseOrForward(true);
                    }
                    break;
            }
        }

        private void OnKeyDown(object? sender, KeyDownEvent e)
        {
            Key key = e.Key;
            bool isRepeat = e.IsRepeat;

            switch (key)
            {
                case Key.Right:
                    if (isRepeat)
                    {
                        Model.Rate = 2.0d;
                    }
                    break;

                case Key.Space:
                    if (!isRepeat)
                        OnMediaStateChange();
                    break;

                case Key.Up:
                    Model.UpdateVolume(true);
                    break;

                case Key.Down:
                    Model.UpdateVolume(false);
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