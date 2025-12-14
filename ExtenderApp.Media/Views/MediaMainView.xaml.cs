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
using ExtenderApp.Media.ViewModels;
using ExtenderApp.Views;
using ExtenderApp.Views.Animation;

namespace ExtenderApp.Media
{
    /// <summary>
    /// MedaiMainView.xaml 的交互逻辑
    /// </summary>
    public partial class MediaMainView : ExtenderAppView
    {
        public MediaMainView(MediaMainViewModel viewModel) : base(viewModel)
        {
            InitializeComponent();

            //拖拽视屏文件
            AllowDrop = true;
            Drop += MediaMainView_Drop;
            DragEnter += MediaMainView_DragEnter;
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
                //foreach (string filePath in filePaths)
                //{
                //    ViewModel<MediaMainViewModel>().AddVideoPath(filePath);

                //}
            }
            e.Handled = true;
        }

        #region MediaSlider滑块位置

        private void mediaSlider_DragStarted(object sender, DragStartedEventArgs e)
        {
            ViewModel<MediaMainViewModel>()!.Model.IsSeeking = true;
            e.Handled = true;
        }

        private void mediaSlider_DragDelta(object sender, DragDeltaEventArgs e)
        {
            var slider = sender as Slider;
            double value = slider?.Value ?? 0;
            var viewModel = ViewModel<MediaMainViewModel>()!;
            viewModel.Model.Position = TimeSpan.FromSeconds(value);
            e.Handled = true;
        }

        private void mediaSlider_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            var slider = sender as Slider;
            double value = slider?.Value ?? 0;
            var viewModel = ViewModel<MediaMainViewModel>()!;
            viewModel.Seek(TimeSpan.FromSeconds(value));
            viewModel.Model.IsSeeking = false;
            e.Handled = true;
        }

        private void mediaSlider_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            var slider = sender as Slider;
            //Float64 Value = slider?.Item1 ?? 0;

            // 获取鼠标在Slider上的位置
            var pos = e.GetPosition(slider);
            double percent = Math.Max(0, Math.Min(1, pos.X / slider.ActualWidth));
            double newValue = slider.Minimum + percent * (slider.Maximum - slider.Minimum);

            //slider.Item1 = newValue;

            var viewModel = ViewModel<MediaMainViewModel>()!;
            viewModel.Seek(TimeSpan.FromSeconds(newValue));
            e.Handled = true;
        }

        #endregion MediaSlider滑块位置
    }
}