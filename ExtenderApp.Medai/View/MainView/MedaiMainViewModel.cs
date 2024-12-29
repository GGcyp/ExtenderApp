using System;
using System.Windows;
using System.Windows.Controls;
using ExtenderApp.Abstract;
using ExtenderApp.ViewModels;
using PropertyChanged;
using ExtenderApp.Views;
using System.Windows.Input;
using System.Reflection.Metadata;

namespace ExtenderApp.Medai
{
    public class MedaiMainViewModel : ExtenderAppViewModel<MedaiMainView>
    {
        private readonly VideoModel _videoModel;
        private readonly IJsonParser _jsonParser;

        public VideoInfo CurrentVideo { get; }

        public GridLength VideoListWidth { get; set; }

        #region 按钮

        public NoValueCommand PlayCommand { get; private set; }
        public NoValueCommand PauseCommand { get; private set; }
        public NoValueCommand StopCommand { get; private set; }
        public NoValueCommand FastForwardCommand { get; private set; }

        #endregion

        public MedaiMainViewModel(VideoModel videoModel, IJsonParser parser, IServiceStore serviceStore) : base(serviceStore)
        {
            _videoModel = videoModel;
            _jsonParser = parser;
            VideoListWidth = new GridLength(200);
            InitVideoCommand();
        }

        private void InitVideoCommand()
        {
            if (_videoModel is null)
                throw new ArgumentNullException(nameof(VideoModel));

            PlayCommand = new(Play, CanPlay);
            PauseCommand = new(Pause, CanPlay);
            StopCommand = new(Stop, CanPlay);
            FastForwardCommand = new(FastForward, CanPlay);
        }

        public override void InjectView(MedaiMainView view)
        {
            base.InjectView(view);
            View.ShowVideoView(NavigateTo<PlaybackView>());
        }

        public void AddVideoPath(string videoPath)
        {
            OpenVideo(videoPath);
        }

        #region VideoOperate

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
            _videoModel.FastForwardAction.Invoke(20);
        }

        private void OpenVideo(string uri)
        {
            _videoModel.OpenVideoAction.Invoke(uri);
        }

        private bool CanPlay()
        {
            return true;
        }

        #endregion
    }
}
