using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExtenderApp.Data
{
    /// <summary>
    /// 二进制格式化器信息类
    /// </summary>
    public class BinaryFormatterInfo
    {
        /// <summary>
        /// 获取格式化器所在作用域
        /// </summary>
        public string FormatterScope { get; set; }

        /// <summary>
        /// 获取格式化器的类型
        /// </summary>
        public Type FormatterType { get; set; }

        public BinaryFormatterInfo(string formatterScope, Type formatterType)
        {
            FormatterScope = formatterScope;
            FormatterType = formatterType;
        }
    }
}
