namespace ExtenderApp
{
    public static partial class AppSetting
    {
        // 添加第一个筛选器
        public static string AllFilter = "所有文件 (*.*)|*.*";
        public static string TxtFilter = "文本文件 (*.txt)|*.txt";
        public static string ImageFilter = "图像文件 (*.jpg;*.png)|*.jpg;*.png";
        public static string ExcelFilter = "Excel (*.xlsx;*.xls)|*.xlsx;*.xls";
        public static string XmlFilter = "Xml (*.xml)|*.xml";
        public static string JsonFilter = "Json (*.json)|*.json";

        //默认(暂时
        public static string DefaultFileName = "新建文件";

        //文件夹名字
        public static string AppBinFolderName = "bin";
        public static string AppConfigFolderName = "config";
        public static string AppSaveFolderName = "save";
        public static string AppModsFolderName = "mods";
        public static string AppPackFolderName = "pack";
        public static string AppLogFolderName = "log";
    }
}
