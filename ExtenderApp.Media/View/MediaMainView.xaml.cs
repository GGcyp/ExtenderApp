using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using ExtenderApp.Abstract;
using ExtenderApp.Data;
using ExtenderApp.Views;

namespace ExtenderApp.Media
{
    /// <summary>
    /// MedaiMainView.xaml 的交互逻辑
    /// </summary>
    public partial class MediaMainView : ExtenderAppView
    {
        private readonly MediaMainViewModel _viewModel;

        public MediaMainView(MediaMainViewModel viewModel)
        {
            _viewModel = viewModel;
            DataContext = _viewModel;
            InitializeComponent();
            _viewModel.InjectView(this);

            //拖拽视屏文件
            AllowDrop = true;
            Drop += MedaiMainView_Drop;
            DragEnter += MedaiMainView_DragEnter;
        }

        private void MedaiMainView_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effects = DragDropEffects.Copy;
                // 可以在这里添加一些视觉提示，例如改变窗口背景色等（以下是简单示例，可按需完善）
                Background = Brushes.Gray;
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }
            e.Handled = true;
        }

        private void MedaiMainView_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                // 使用GetData方法获取文件路径数组（因为可以一次拖拽多个文件，所以是数组形式）
                string[] filePaths = e.Data.GetData(DataFormats.FileDrop) as string[];
                foreach (string filePath in filePaths)
                {
                    _viewModel.AddVideoPath(filePath);
                }
            }
            e.Handled = true;
        }

        #region 基础操作
        public void ShowVideoView(IView view)
        {
            playbackViewControl.Content = view;
        }

        public override void Exit(ViewInfo newViewInfo)
        {
            _viewModel.StopCommand.Execute(null);
        }

        #endregion

        #region 视频列表

        private void VideoListClik(object sender, MouseButtonEventArgs e)
        {
            if (sender is TextBlock textBlock && textBlock.DataContext is VideoInfo videoInfo)
            {
                // 处理点击事件，例如播放视频
                _viewModel.OpenVideo(videoInfo);
            }
        }

        #endregion
    }
}
