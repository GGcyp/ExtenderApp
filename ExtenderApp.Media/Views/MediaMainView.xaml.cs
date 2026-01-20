using System.Windows;
using System.Windows.Media;
using ExtenderApp.Media.ViewModles;
using ExtenderApp.Views;

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
                //    GetViewModel<MediaMainViewModel>().AddVideoPath(filePath);

                //}
            }
            e.Handled = true;
        }
    }
}