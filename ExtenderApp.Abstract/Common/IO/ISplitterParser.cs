using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExtenderApp.Data;

namespace ExtenderApp.Abstract
{
    /// <summary>
    /// 文件分割器接口
    /// 继承自文件解析器接口
    /// </summary>
    public interface ISplitterParser : IFileParser
    {
        /// <summary>
        /// 创建文件
        /// </summary>
        /// <param name="fileInfo">本地文件信息</param>
        /// <param name="info">文件分割器信息</param>
        void Creat(ExpectLocalFileInfo fileInfo, SplitterInfo info);

        /// <summary>
        /// 写入文件
        /// </summary>
        /// <param name="info">本地文件信息</param>
        /// <param name="bytes">要写入的数据</param>
        /// <param name="chunkIndex">块索引</param>
        void Write(ExpectLocalFileInfo info, byte[] bytes, uint chunkIndex);
    }
}
