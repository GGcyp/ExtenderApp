using System.IO;
using MainApp.Abstract;
using MainApp.Common;
using MainApp.ViewModels;
using PropertyChanged;

namespace MainApp.Mod.PPR
{
    [AddINotifyPropertyChangedInterface]
    public class PPRViewModel : BaseViewModel<IPPRModel>
    {
        private readonly FileExtensionType PPRExtensionType = FileExtensionType.Xml + FileExtensionType.Xls + FileExtensionType.Xlsx;

        public string Filer => "预算文件工程|" + PPRExtensionType.Filter;

        public PPRDto Root { get; set; }

        public DtoList<PPRInventoryDto, PPRInventoryEntity> Inventories {  get; set; }

        public PPRTitles Titles { get; set; }

        public PPRViewModel(IPPRModel model, IDispatcher dispatcher) : base(model, dispatcher)
        {
            Inventories = new();
            Titles = new();
            Init();
        }

        //public PPRViewModel(IPPRModel model)
        //{
        //    this.model = model;
        //    Inventories = new();
        //    //Inventories.Filter = item =>
        //    //{
        //    //    if (string.IsNullOrEmpty(FilterText))
        //    //        return true;
        //    //    return item.InventoryProjectName.Contains(FilterText)
        //    //        || item.ProjectID.Contains(FilterText);
        //    //};
        //    Titles = new();
        //    Init();
        //}

        private void Init()
        {
            Titles = new();
            //Read("E:\\工程文件\\海口市秀英区西秀中心小学重建项目\\主要文件\\海口市秀英区西秀中心小学重建项目.xml");
            //Root = _model.Get(string.Empty);
        }

        public void OnSelectedPPRDtoChanged(PPRDto dto)
        {
            ArgumentNullException.ThrowIfNull(dto, nameof(dto));

            Inventories.Clear();
            AddDtoInventories(dto);
            Inventories.Refresh();
        }

        private void AddDtoInventories(PPRDto dto)
        {
            ArgumentNullException.ThrowIfNull(dto, nameof(dto));

            if (dto.Inventories is not null) Inventories.AddEntitiesList(dto.Inventories);

            if (dto.PPRDtoChilds is null) return;

            for (int i = 0; i < dto.PPRDtoChilds.Count; i++)
            {
                AddDtoInventories(dto.PPRDtoChilds[i]);
            }
        }

        protected override void OnReadend()
        {
            Root = _model.Get(string.Empty);
        }

        public void AddPeriodQuantityEntity(string path, int frequency)
        {
            Read(path);
            _model.UpdatePerPeriodQuantityEntityFrequency(frequency);
        }

        public void AddPeriodQuantityEntities(string path, int frequency)
        {
            GetAllFiles(path, s => Read(s),"*.xlsx");
            _model.UpdatePerPeriodQuantityEntityFrequency(frequency);
        }

        private void GetAllFiles(string folderPath, Action<string> action, string searchPattern = "*.*")
        {
            if (action is null) throw new ArgumentNullException(nameof(action));

            try
            {
                // 获取文件夹中直接的文件（不包括子文件夹）
                foreach (var file in Directory.GetFiles(folderPath, searchPattern))
                {
                    action.Invoke(file);
                }

                // 递归遍历子文件夹
                foreach (string subDir in Directory.GetDirectories(folderPath))
                {
                    GetAllFiles(subDir, action, searchPattern);// 递归调用
                }
            }
            catch (Exception ex)
            {
                // 异常处理，例如记录日志
                Console.WriteLine("An error occurred: " + ex.Message);
            }
        }
    }
}
