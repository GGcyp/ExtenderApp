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

namespace ExtenderApp.Medai
{
    /// <summary>
    /// MedaiMainView.xaml 的交互逻辑
    /// </summary>
    public partial class MedaiMainView : ExtenderAppView
    {
        private readonly MedaiMainViewModel _viewModel;

        public MedaiMainView(MedaiMainViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            _viewModel.InjectView(this);
            DataContext = _viewModel;

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
                this.Background = Brushes.LightGray;
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

        private void VideoListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //if (videoListBox.SelectedItem != null)
            //{
            //    string selectedVideo = videoListBox.SelectedItem.ToString();
            //    mediaElement.Source = new Uri(selectedVideo);
            //    mediaElement.Play();
            //}
        }

        private void TextBlock_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            //var textBlock = sender as TextBlock;
            //if (textBlock != null)
            //{
            //    string selectedVideo = textBlock.Text;
            //    mediaElement.Source = new Uri(selectedVideo);
            //    mediaElement.Play();
            //}
        }

        private void HideListButton_Click(object sender, RoutedEventArgs e)
        {
            var border = (Border)FindName("videoListBorder");
            if (border != null)
            {
                border.Visibility = Visibility.Collapsed;
            }
        }

        private void ShowListButton_Click(object sender, RoutedEventArgs e)
        {
            var border = (Border)FindName("videoListBorder");
            if (border != null)
            {
                border.Visibility = Visibility.Visible;
            }
        }

        #region 显示控件

        public void ShowVideoView(IView view)
        {
            playbackViewControl.Content = view;
        }

        #endregion
    }
}
