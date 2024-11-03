namespace MainApp
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

        //文件夹
        private static string m_AppFolderPath;
        /// <summary>
        /// 获取当前程序文件夹路径
        /// </summary>
        public static string AppFolderPath
        {
            get
            {
                if (string.IsNullOrEmpty(m_AppFolderPath))
                {
                    m_AppFolderPath = Directory.GetCurrentDirectory();
                }
                return m_AppFolderPath;
            }
        }

        //文件夹名字
        public static string AppBinFolderName = "bin";
        public static string AppConfigFolderName = "save";
        public static string AppSaveFolderName = "config";
        public static string AppModelFolderName = "mods";
    }
}
