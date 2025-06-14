namespace ExtenderApp.Data
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

        internal MultiExtensionType multiType;

        /// <summary>
        /// 是否为空
        /// </summary>
        public bool IsEmpty => singleType.IsEmpty && multiType.IsEmpty;

        /// <summary>
        /// 是否是唯一后缀名
        /// </summary>
        public bool IsSingExtension => multiType.IsEmpty;

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
            if (IsEmpty && type.IsEmpty)
            {
                return true;
            }
            else if ((IsEmpty && !type.IsEmpty) || (!IsEmpty && type.IsEmpty))
            {
                return true;
            }

            //两个都为唯一后缀名时
            if (IsSingExtension && type.IsSingExtension)
            {
                return type.Extension.Equals(Extension);
            }
            else if (IsSingExtension && type.IsSingExtension)
            {
                //其中一个是
                SingleExtensionType single = IsSingExtension ? singleType : type.singleType;
                MultiExtensionType multi = IsSingExtension ? type.multiType : multiType;
                return multi.Contains(single);
            }

            ////都是多数后缀时
            return multiType.Contains(type.multiType);
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
            if (left.multiType.IsEmpty)
            {
                left.multiType = new MultiExtensionType(left.singleType);
                left.singleType = SingleExtensionType.Empty;
            }
            left.multiType.AddExtension(right);
            return left;
        }

        public static FileExtensionType operator -(FileExtensionType left, FileExtensionType right)
        {
            if (left.multiType.IsEmpty)
                return left;

            left.multiType.RemoveExtension(right);
            if (right.multiType.extensionTypes.Count == 0)
            {
                left.multiType = new MultiExtensionType();
            }
            else if(right.multiType.extensionTypes.Count==1)
            {
                SingleExtensionType single = left.multiType.extensionTypes[0];
                left.multiType = new MultiExtensionType();
                left.singleType = single;
            }

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
