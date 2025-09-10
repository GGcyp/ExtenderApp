using System.Windows;
using System.Windows.Controls;
using ExtenderApp.Common;
using ExtenderApp.Views.Expansions;
using Microsoft.Win32;


namespace ExtenderApp.Mod.PPR
{
    /// <summary>
    /// PPRMainView.xaml 的交互逻辑
    /// </summary>
    public partial class PPRMainView : Window
    {
        private readonly PPRViewModel ;

        public PPRMainView(PPRViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
             = viewModel;
            Init();
        }

        private void Init()
        {
            //entityDetailsSlider.SetValue(0, 10, 10, 1);
            pprDataGrid.SizeChanged += PprDataGrid_SizeChanged;
        }

        private void PprDataGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var pageSize = (int)((PPRDataGrid)sender).ActualHeight / 25;
            .Inventories.UpdatePageSize(pageSize);
            .Inventories.Refresh();
        }

        private void OnEntityNodeSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (e.NewValue is not PPRDto dto) return;


            .OnSelectedPPRDtoChanged(dto);

            int allcount = .Inventories.GetAllEntitiesCount();
            entityDetailsSlider.SetValue(0, allcount, allcount, 10);
        }

        private void entityDetailsSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            //viewModel.Inventories.PageIndex = viewModel.Inventories.GetAllEntitiesCount() - (int)entityDetailsSlider.Value;

            .Inventories.Refresh(.Inventories.GetAllEntitiesCount() - (int)entityDetailsSlider.Value);
        }

        private void MessageShow(string message)
        {
            MessageBox.Show(message);
        }

        private void OpenFileClick(object sender, RoutedEventArgs e)
        {
            fileButton.IsChecked = false;

            if(.Root is not null)
            {
                MessageBox.Show("已经打开预算工程文件,不能重复打开");
                return;
            }

            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = .Filer;
            openFileDialog.Title = "打开预算工程文件";
            //if ((bool)openFileDialog.ShowDialog()!)
            //{
            //    .Read(openFileDialog.FileName);
            //}
        }

        private void AddFileClick(object sender, RoutedEventArgs e)
        {
            fileButton.IsChecked = false;
            if (.Root is null)
            {
                MessageBox.Show("请先加载一个预算工程");
                return;
            }
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = .Filer;
            openFileDialog.Title = "打开预算工程文件";
            if ((bool)openFileDialog.ShowDialog()!)
            {
                FrequencyView frequencyView = new FrequencyView(frequency =>
                {
                    if (string.IsNullOrEmpty(frequency)) return;

                    if (!int.TryParse(frequency, out int index)) return;
                    .AddPeriodQuantityEntity(openFileDialog.FileName, index);
                });
                frequencyView.ShowDialog();
            }
        }

        private void AddFolderClick(object sender, RoutedEventArgs e)
        {
            fileButton.IsChecked = false;
            if (.Root is null)
            {
                MessageBox.Show("请先加载一个预算工程");
                return;
            }
            var openFileDialog = new OpenFolderDialog();
            openFileDialog.Title = "打开预算工程文件";
            if ((bool)openFileDialog.ShowDialog()!)
            {
                FrequencyView frequencyView = new FrequencyView(frequency =>
                {
                    if (string.IsNullOrEmpty(frequency)) return;

                    if (!int.TryParse(frequency, out int index)) return;
                    .AddPeriodQuantityEntities(openFileDialog.FolderName, index);
                });
                frequencyView.ShowDialog();
            }
            //.Write("E:\\工程文件\\海口市秀英区西秀中心小学重建项目\\工程进度记录\\进度文件.xml");
        }
    }
}
