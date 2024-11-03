using MainApp.Common.Data;

namespace MainApp.Common.File
{
    /// <summary>
    /// 存储多个文件拓展名，只能用来比较
    /// </summary>
    internal struct MultiExtensionType
    {
        /// <summary>
        /// 多个文件拓展hash表
        /// </summary>
        private ValueList<SingleExtensionType> singleExtensionTypes;

        /// <summary>
        /// 是否为空
        /// </summary>
        public bool IsEmpty => singleExtensionTypes.IsEmpty;

        /// <summary>
        /// 筛选器，用于指定允许或禁止的扩展名列表
        /// </summary>
        public string Filter
        {
            get
            {
                if (singleExtensionTypes.IsEmpty) return string.Empty;
                string filter = singleExtensionTypes[0].Filter;
                for(int i = 1; i < singleExtensionTypes.Count; i++)
                {
                    filter = string.Format("{0};{1}", filter, singleExtensionTypes[i].Filter);
                }
                return filter;
            }
        }

        /// <summary>
        /// 添加文件扩展名
        /// </summary>
        /// <param name="extension">要添加的文件扩展名类型</param>
        /// <returns>添加是否成功，成功返回true，失败返回false</returns>
        public bool AddExtension(FileExtensionType extension)
        {
            if (extension.IsEmpty) return false;

            return AddExtension(extension.singleType);
        }

        /// <summary>
        /// 添加后缀名
        /// </summary>
        /// <param name="extension">要添加的后缀名类型</param>
        /// <returns>如果添加成功返回true，否则返回false</returns>
        public bool AddExtension(SingleExtensionType extension)
        {
            if (extension.IsEmpty) return false;
            if (!Contains(extension))
            {
                singleExtensionTypes.Add(extension);
                return true;
            }
            return false;
        }

        /// <summary>
        /// 添加后缀名
        /// </summary>
        /// <param name="extension">要添加的后缀名字符串</param>
        /// <returns>如果添加成功返回true，否则返回false</returns>
        public bool AddExtension(string extension)
        {
            if (string.IsNullOrEmpty(extension)) return false;

            if (!Contains(extension))
            {
                singleExtensionTypes.Add(new SingleExtensionType(extension));
                return true;
            }
            return false;
        }

        /// <summary>
        /// 从文件名中移除指定的文件扩展名
        /// </summary>
        /// <param name="extension">要移除的文件扩展名类型</param>
        /// <returns>如果成功移除扩展名则返回true，否则返回false</returns>
        public bool RemoveExtension(FileExtensionType extension)
        {
            if (extension.IsEmpty) return false;

            return RemoveExtension(extension.singleType);
        }

        /// <summary>
        /// 使用<see cref="SingleExtensionType"/>类型的参数来删除指定的后缀名。
        /// </summary>
        /// <param name="extension">指定要删除后缀名的<see cref="SingleExtensionType"/>类型参数。</param>
        /// <returns>如果成功删除后缀名，则返回true；否则返回false。</returns>
        public bool RemoveExtension(SingleExtensionType extension)
        {
            if(extension.IsEmpty) return false;

            return RemoveExtension(extension.Extension);
        }

        /// <summary>
        /// 使用字符串类型的参数来删除指定的后缀名。
        /// </summary>
        /// <param name="extension">指定要删除的后缀名字符串。</param>
        /// <returns>如果成功删除后缀名，则返回true；否则返回false。</returns>
        public bool RemoveExtension(string extension)
        {
            if (string.IsNullOrEmpty(extension)) return false;

            if (singleExtensionTypes.IsEmpty) return false;

            return !singleExtensionTypes.Remove(s => s.Extension == extension).IsEmpty;
        }

        /// <summary>
        /// 判断集合中是否包含指定的文件扩展名类型
        /// </summary>
        /// <param name="extension">要检查的文件扩展名类型</param>
        /// <returns>如果集合中包含指定的文件扩展名类型，则返回true；否则返回false</returns>
        public bool Contains(FileExtensionType extension)
        {
            if (extension.IsEmpty) return false;

            return Contains(extension.singleType);
        }

        /// <summary>
        /// 判断给定的 <see cref="SingleExtensionType"/> 是否存在于集合中。
        /// </summary>
        /// <param name="extension">要判断的 <see cref="SingleExtensionType"/> 对象。</param>
        /// <returns>如果 <paramref name="extension"/> 的扩展名存在于集合中，则返回 true；否则返回 false。</returns>
        public bool Contains(SingleExtensionType extension)
        {
            if (extension.IsEmpty) return false;

            return Contains(extension.Extension);
        }

        /// <summary>
        /// 判断给定的扩展名是否存在于集合中。
        /// </summary>
        /// <param name="extension">要判断的扩展名。</param>
        /// <returns>如果 <paramref name="extension"/> 存在于集合中，则返回 true；否则返回 false。</returns>
        public bool Contains(string extension)
        {
            if(string.IsNullOrEmpty(extension)) return false;

            if(singleExtensionTypes.IsEmpty) return false;

            return singleExtensionTypes.Contains(s => s.Extension == extension);
        }

        /// <summary>
        /// 清除单扩展类型并返回是否成功
        /// </summary>
        /// <param name="single">清除后的单扩展类型值，如果清除失败则为空</param>
        /// <returns>如果成功清除单扩展类型，则返回true；否则返回false</returns>
        public bool Clear(out SingleExtensionType single)
        {
            single = SingleExtensionType.Empty;
            if (singleExtensionTypes.Count != 1)
            {
                return false;
            }

            single = singleExtensionTypes.First();
            return true;
        }
    }
}
