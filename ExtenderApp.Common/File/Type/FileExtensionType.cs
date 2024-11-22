using ExtenderApp.Common.File;

namespace ExtenderApp.Common
{
    /// <summary>
    /// 文件扩展名类型结构体
    /// </summary>
    public struct FileExtensionType
    {
        /// <summary>
        /// 文件扩展名
        /// </summary>
        public string Extension => singleType.Extension;

        internal SingleExtensionType singleType;

        private MultiExtensionType multiType;

        /// <summary>
        /// 是否为空
        /// </summary>
        public bool IsEmpty => singleType.IsEmpty && multiType.IsEmpty;

        /// <summary>
        /// 是否是唯一后缀名
        /// </summary>
        public bool IsSingExtension => !singleType.IsEmpty;

        /// <summary>
        /// 筛选器，用于指定允许或禁止的扩展名列表
        /// </summary>
        public string Filter => IsSingExtension ? singleType.Filter : multiType.Filter;

        /// <summary>
        /// 初始化文件扩展名类型结构体
        /// </summary>
        /// <param name="extension">文件扩展名</param>
        public FileExtensionType(string extension)
        {
            singleType = new SingleExtensionType(extension);
        }

        public bool Equals(FileExtensionType type)
        {
            //两个中的一个为空时，就直接返回否
            if (IsEmpty || type.IsEmpty) return false;

            //两个都为唯一后缀名时
            if (IsSingExtension && type.IsSingExtension)
            {
                return type.Extension.Equals(Extension);
            }

            //当其中有一个是多数后缀时
            SingleExtensionType single = IsSingExtension ? singleType : type.singleType;
            MultiExtensionType multi = IsSingExtension ? type.multiType : multiType;

            return multi.Contains(single);
        }

        public static bool operator ==(FileExtensionType left, FileExtensionType right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(FileExtensionType left, FileExtensionType right)
        {
            return !(left == right);
        }

        public static FileExtensionType operator +(FileExtensionType left, FileExtensionType right)
        {
            left.multiType.AddExtension(left);

            if (left.IsSingExtension) left.singleType = SingleExtensionType.Empty;

            left.multiType.AddExtension(right);

            return left;
        }

        public static FileExtensionType operator -(FileExtensionType left, FileExtensionType right)
        {
            left.multiType.RemoveExtension(right);

            left.multiType.Clear(out left.singleType);

            return left;
        }

        public override string ToString()
        {
            return IsSingExtension ? Extension : string.Empty;
        }

        #region 扩展

        /// <summary>
        /// 空的文件后缀
        /// </summary>
        public static FileExtensionType Empty => new FileExtensionType(string.Empty);

        const string _folder = "folder";
        /// <summary>
        /// 文件夹类型
        /// </summary>
        public static FileExtensionType Folder => new FileExtensionType(_folder);

        const string _xml = ".xml";
        /// <summary>
        /// 获取 XML 文件扩展名类型
        /// </summary>
        public static FileExtensionType Xml => new FileExtensionType(_xml);

        const string _xlsx = ".xlsx";
        /// <summary>
        /// 获取 Excel 文件扩展名类型
        /// </summary>
        public static FileExtensionType Xlsx => new FileExtensionType(_xlsx);

        const string _xls = ".xls";
        /// <summary>
        /// 获取 Excel 文件扩展名类型
        /// </summary>
        public static FileExtensionType Xls => new FileExtensionType(_xls);

        const string _txt = ".txt";
        /// <summary>
        /// 获取 文本 文件扩展名类型
        /// </summary>
        public static FileExtensionType Txt => new FileExtensionType(_txt);

        const string _json = ".json";
        /// <summary>
        /// 获取 JSON 文件扩展名类型
        /// </summary>
        public static FileExtensionType Json => new FileExtensionType(_json);

        #endregion
    }
}
