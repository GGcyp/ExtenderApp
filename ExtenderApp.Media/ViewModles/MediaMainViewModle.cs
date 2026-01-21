using ExtenderApp.Abstract;
using ExtenderApp.Data;
using ExtenderApp.Media.Models;
using ExtenderApp.ViewModels;
using Microsoft.Win32;

namespace ExtenderApp.Media.ViewModles
{
    public class MediaMainViewModel : ExtenderAppViewModel<MediaModel>
    {
        public IViewModel? VideoListViewModel { get; set; }

        public IViewModel? VideoViewModel { get; set; }
        public IViewModel? VideoControlViewModel { get; set; }
        public MediaMainViewModel(IServiceStore serviceStore) : base(serviceStore)
        {
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
                    Model.ReverseOrForward(false);
                    break;

                case Key.Right:
                    if (Model.SpeedRatio != 1.0d)
                    {
                        Model.SpeedRatio = 1.0d;
                    }
                    else
                    {
                        Model.ReverseOrForward(true);
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
                        Model.SpeedRatio = 2.0d;
                    }
                    break;

                case Key.Space:
                    if (!isRepeat)
                        Model.ChangeMediaState();
                    break;

                case Key.Up:
                    Model.UpdateVolume(true);
                    break;

                case Key.Down:
                    Model.UpdateVolume(false);
                    break;
            }
        }
    }
}