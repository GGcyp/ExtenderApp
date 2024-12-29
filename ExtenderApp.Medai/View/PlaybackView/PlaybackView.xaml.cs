using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ExtenderApp.Views;

namespace ExtenderApp.Medai
{
    /// <summary>
    /// PlaybackView.xaml 的交互逻辑
    /// </summary>
    public partial class PlaybackView : ExtenderAppView
    {
        public PlaybackView(VideoModel model)
        {
            InitializeComponent();
            model.PlayAction = Play;
            model.StopAction = Stop;
            model.PauseAction = Pause;
            model.FastForwardAction = FastForward;
            model.OpenVideoAction = OpenVideo;


            model.GetPosition = GetPosition;
            model.SetPosition = SetPosition;
            model.NaturalTimeSpanFunc = NaturalTimeSpan;
            model.SpeedRatioAction = SpeedRatio;
        }

        private TimeSpan GetPosition() => mediaElement.Position;
        private void SetPosition(TimeSpan span) => mediaElement.Position = span;

        private TimeSpan NaturalTimeSpan() => mediaElement.NaturalDuration.TimeSpan;
        private void SpeedRatio(double speed) => mediaElement.SpeedRatio = speed;

        private void Play() => mediaElement.Play();

        private void Pause() => mediaElement.Pause();

        private void Stop() => mediaElement.Stop();

        private void FastForward(double value)
        {
            if (mediaElement.Position < mediaElement.NaturalDuration.TimeSpan)
            {
                mediaElement.Position += TimeSpan.FromSeconds(value);
            }
        }

        private void OpenVideo(string uri) => mediaElement.Source = new Uri(uri);
    }
}
