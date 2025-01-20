using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ExtenderApp.Abstract;
using ExtenderApp.Common;
using ExtenderApp.Data;
using ExtenderApp.Views;
using ExtenderApp.Views.Animation;

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
            Drop += MediaMainView_Drop;
            DragEnter += MediaMainView_DragEnter;

            // 监听窗口大小变化事件
            SizeChanged += MediaMainView_SizeChanged;

            //监听视频进度条更改
            mediaSlider.ThumbDragCompleted += MediaSlider_DragCompleted;
            volumeSlider.ThumbDragCompleted += VolumeSlider_ValueChanged;
        }

        private void MediaMainView_DragEnter(object sender, DragEventArgs e)
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

        private void MediaMainView_Drop(object sender, DragEventArgs e)
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

        private void MediaMainView_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            _viewModel.WindowWidth = e.NewSize.Width;
        }

        private void VolumeSlider_ValueChanged()
        {
            _viewModel.UpdateVolume();
        }

        private void MediaSlider_DragCompleted()
        {
            // 更新 ViewModel 中的 CurrentTime 属性
            _viewModel.UpdateVoideoTime(TimeSpan.FromSeconds(mediaSlider.Value));
        }

        #region 基础操作

        public void ShowVideoView(IView view)
        {
            playbackViewControl.Content = view;
        }

        public void ShowVideoList(IView view)
        {
            videoListControl.Content = view;
        }

        public override void Exit(ViewInfo newViewInfo)
        {
            _viewModel.Close();
        }

        #endregion

        #region 动画控制

        private void Grid_Click(object sender, RoutedEventArgs e)
        {
            double width = videoListGrid.Width.Value == 0 ? 200 : 0;

            AnimateGridWidth(width);
        }

        private void AnimateGridWidth(double targetWidth)
        {
            var animation = new GridLengthAnimation
            {
                From = videoListGrid.Width,
                To = new GridLength(targetWidth),
                Duration = TimeSpan.FromSeconds(0.5),
                EasingFunction = new SineEase { EasingMode = EasingMode.EaseOut }
            };

            videoListGrid.BeginAnimation(ColumnDefinition.WidthProperty, animation);
        }

        #endregion
    }
}
