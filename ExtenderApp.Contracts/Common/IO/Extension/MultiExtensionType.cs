using System.Text;

namespace ExtenderApp.Contracts
{
    /// <summary>
    /// 存储多个文件拓展名，只能用来比较
    /// </summary>
    internal struct MultiExtensionType
    {
        private readonly static StringBuilder _builder = new StringBuilder();

        /// <summary>
        /// 多个文件拓展hash表
        /// </summary>
        internal readonly List<SingleExtensionType>? extensionTypes;

        /// <summary>
        /// 是否为空
        /// </summary>
        public bool IsEmpty => extensionTypes == null;

        /// <summary>
        /// 筛选器，用于指定允许或禁止的扩展名列表
        /// </summary>
        public string Filter
        {
            get
            {
                if (extensionTypes == null)
                    return string.Empty;

                lock (_builder)
                {
                    _builder.Append("|");
                    for (int i = 0; i < extensionTypes.Count; i++)
                    {
                        _builder.Append(extensionTypes[i].Filter);
                        _builder.Append(";");
                    }
                    string result = _builder.ToString();
                    _builder.Clear();
                    return result;
                }
            }
        }

        public MultiExtensionType(SingleExtensionType single)
        {
            extensionTypes = single.IsEmpty ? null : new();
            AddExtension(single);
        }

        #region Add

        /// <summary>
        /// 向集合中添加扩展名
        /// </summary>
        /// <param name="extension">要添加的扩展名类型</param>
        /// <returns>添加成功返回true，否则返回false</returns>
        public bool AddExtension(FileExtensionType extension)
        {
            if (extension.IsEmpty)
                return false;

            return AddExtension(extension.singleType);
        }

        /// <summary>
        /// 向集合中添加单个扩展名
        /// </summary>
        /// <param name="extension">要添加的单个扩展名类型</param>
        /// <returns>添加成功返回true，否则返回false</returns>
        public bool AddExtension(SingleExtensionType extension)
        {
            if (extension.IsEmpty || IsEmpty)
                throw new ArgumentNullException(nameof(extension));

            if (!Contains(extension))
            {
                extensionTypes.Add(extension);
                return true;
            }
            return false;
        }

        /// <summary>
        /// 向集合中添加字符串形式的扩展名
        /// </summary>
        /// <param name="extension">要添加的字符串形式的扩展名</param>
        /// <returns>添加成功返回true，否则返回false</returns>
        public bool AddExtension(string extension)
        {
            if (string.IsNullOrEmpty(extension))
                return false;

            if (IsEmpty)
                throw new ArgumentNullException(nameof(extension));

            if (!Contains(extension))
            {
                extensionTypes.Add(new SingleExtensionType(extension));
                return true;
            }
            return false;
        }

        #endregion

        #region Remove

        /// <summary>
        /// 移除文件扩展名
        /// </summary>
        /// <param name="extension">要移除的文件扩展名类型</param>
        /// <returns>如果成功移除扩展名，则返回true；否则返回false</returns>
        public bool RemoveExtension(FileExtensionType extension)
        {
            if (extension.IsEmpty)
                return false;

            if (extension.IsSingExtension)
            {

                return RemoveExtension(extension.singleType);
            }
            else
            {
                return RemoveExtension(extension.multiType);
            }

        }

        /// <summary>
        /// 从当前对象中移除指定的扩展类型。
        /// </summary>
        /// <param name="extension">要移除的扩展类型。</param>
        /// <returns>如果成功移除扩展类型，则返回true；否则返回false。</returns>
        public bool RemoveExtension(MultiExtensionType extension)
        {
            if (extension.IsEmpty)
                return false;

            for (int i = 0; i < extension.extensionTypes.Count; i++)
            {
                for (int j = 0; j < extensionTypes.Count; j++)
                {
                    if (extension.extensionTypes[i] == extensionTypes[i])
                    {
                        extensionTypes.RemoveAt(j);
                        break;
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// 移除单个文件扩展名
        /// </summary>
        /// <param name="extension">要移除的单个文件扩展名类型</param>
        /// <returns>如果成功移除扩展名，则返回true；否则返回false</returns>
        public bool RemoveExtension(SingleExtensionType extension)
        {
            if (extension.IsEmpty || IsEmpty)
                throw new ArgumentNullException(nameof(extension));

            return extensionTypes.Remove(extension);
        }

        /// <summary>
        /// 移除文件扩展名字符串
        /// </summary>
        /// <param name="extension">要移除的文件扩展名字符串</param>
        /// <returns>如果成功移除扩展名，则返回true；否则返回false</returns>
        public bool RemoveExtension(string extension)
        {
            if (string.IsNullOrEmpty(extension))
                return false;

            if (IsEmpty)
                throw new ArgumentNullException(nameof(extension));

            return extensionTypes.Remove(new SingleExtensionType(extension));
        }

        #endregion

        #region Contains

        /// <summary>
        /// 检查是否包含指定的文件扩展名类型。
        /// </summary>
        /// <param name="extension">要检查的文件扩展名类型。</param>
        /// <returns>如果包含指定的文件扩展名类型，则返回true；否则返回false。</returns>
        public bool Contains(FileExtensionType extension)
        {
            if (extension.IsEmpty)
                throw new ArgumentNullException(nameof(extension));

            return Contains(extension.singleType);
        }

        /// <summary>
        /// 检查当前对象是否包含指定的多扩展类型。
        /// </summary>
        /// <param name="extension">要检查的多扩展类型。</param>
        /// <returns>如果当前对象包含指定的多扩展类型，则返回 true；否则返回 false。</returns>
        /// <exception cref="ArgumentNullException">
        /// 如果指定的 <paramref name="extension"/> 参数为空，或者当前对象为空，则抛出此异常。
        /// </exception>
        public bool Contains(MultiExtensionType extension)
        {
            if (extension.IsEmpty || IsEmpty)
                throw new ArgumentNullException(nameof(extension));

            if (extension.extensionTypes.Count != extensionTypes.Count)
                return false;

            int count = 0;
            for (int i = 0; i < extensionTypes.Count; i++)
            {
                for (int j = 0; j < extension.extensionTypes.Count; j++)
                {
                    if (extensionTypes[i].Equals(extensionTypes[j]))
                    {
                        count++;
                        break;
                    }
                }
            }
            return count == extensionTypes.Count;
        }

        /// <summary>
        /// 检查是否包含指定的单一文件扩展名类型。
        /// </summary>
        /// <param name="extension">要检查的单一文件扩展名类型。</param>
        /// <returns>如果包含指定的单一文件扩展名类型，则返回true；否则返回false。</returns>
        public bool Contains(SingleExtensionType extension)
        {
            if (extension.IsEmpty && IsEmpty)
                throw new ArgumentNullException(nameof(extension));

            return extensionTypes.Contains(extension);
        }

        /// <summary>
        /// 检查是否包含指定的文件扩展名字符串。
        /// </summary>
        /// <param name="extension">要检查的文件扩展名字符串。</param>
        /// <returns>如果包含指定的文件扩展名字符串，则返回true；否则返回false。</returns>
        public bool Contains(string extension)
        {
            if (string.IsNullOrEmpty(extension) && IsEmpty)
                throw new ArgumentNullException(nameof(extension));

            return extensionTypes.Contains(new SingleExtensionType(extension));
        }

        #endregion

        /// <summary>
        /// 清空单扩展类型的集合。
        /// </summary>
        public void Clear()
        {
            extensionTypes.Clear();
        }

        public static bool operator ==(MultiExtensionType left, MultiExtensionType right)
        {
            return left.Contains(right);
        }

        public static bool operator !=(MultiExtensionType left, MultiExtensionType right)
        {
            return !(left == right);
        }

        public bool Equals(MultiExtensionType obj)
        {
            return this.Contains(obj);
        }
    }
}
