using System.Collections.ObjectModel;
using System.Windows.Media.Imaging;
using ExtenderApp.Abstract;
using ExtenderApp.Media.Audios;
using ExtenderApp.Media.FFmpegEngines;
using ExtenderApp.Models;

namespace ExtenderApp.Media.Models
{
    /// <summary>
    /// 媒体数据类
    /// </summary>
    public class MediaModel : ExtenderAppModel
    {
        /// <summary>
        /// 视频信息集合
        /// </summary>
        public ObservableCollection<VideoInfo>? VideoInfos { get; set; }

        public MediaPlayer? MPlayer { get; set; }
        public AudioPlayer? APlayer { get; set; }
        public WriteableBitmap Bitmap { get; set; }
        public IView? CurrentVideoView { get; set; }

        public IView? CurrentVideoListView { get; set; }

        private float volume;

        /// <summary>
        /// 音量
        /// </summary>
        public float Volume
        {
            get => volume;
            set
            {
                volume = value;
                if (APlayer != null)
                {
                    APlayer.Volume = volume;
                }
            }
        }

        /// <summary>
        /// 是否记录观看时间
        /// </summary>
        public bool RecordWatchingTime { get; set; }

        /// <summary>
        /// 视频文件不存在则删除
        /// </summary>
        public bool VideoNotExist { get; set; }

        public MediaModel()
        {
            //VideoInfos = new ObservableCollection<VideoInfo>();
            //MediaSettings = new MediaSettings();
            //Bitmap = new WriteableBitmap(1920, 1080, 96, 96, PixelFormats.Bgr24, null);
        }

        public void SetPlayer(MediaPlayer player)
        {
            if (player == null)
                throw new ArgumentNullException(nameof(player));

            APlayer = new AudioPlayer(player.Info, 0);
            Bitmap = new(player.Info.Width, player.Info.Height, 96, 96, System.Windows.Media.PixelFormats.Bgr24, null);
        }
    }
}