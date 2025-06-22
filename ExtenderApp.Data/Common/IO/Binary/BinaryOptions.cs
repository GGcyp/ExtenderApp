using System.Text;
using ExtenderApp.Data;

namespace ExtenderApp.Data
{
    /// <summary>
    /// 二进制选项类
    /// </summary>
    public class BinaryOptions
    {
        /// <summary>
        /// 获取或设置二进制代码对象
        /// </summary>
        public BinaryCode BinaryCode { get; set; }

        /// <summary>
        /// 获取或设置二进制范围对象
        /// </summary>
        public BinaryRang BinaryRang { get; set; }

        /// <summary>
        /// 获取或设置日期时间常量对象
        /// </summary>
        public DateTimeConstants DateTimeConstants { get; set; }

        /// <summary>
        /// 获取或设置最大对象图深度
        /// </summary>
        public int MaximumObjectGraphDepth { get; private set; }

        /// <summary>
        /// 获取UTF-8编码对象。
        /// </summary>
        /// <value>
        /// 表示UTF-8编码的<see cref="Encoding"/>对象。
        /// </value>
        public Encoding BinaryEncoding { get; }

        /// <summary>
        /// 二进制选项类的构造函数
        /// </summary>
        public BinaryOptions()
        {
            BinaryCode = new BinaryCode();
            BinaryRang = new BinaryRang();
            DateTimeConstants = new DateTimeConstants();
            MaximumObjectGraphDepth = 500;
            BinaryEncoding = Encoding.UTF8; // 使用UTF-8编码
        }
    }
}
