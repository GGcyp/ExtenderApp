using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExtenderApp.Common
{
    public static partial class Utility
    {
        private const int prime = 31;

        /// <summary>
        /// 获取输入字符串的简单一致性哈希值。
        /// </summary>
        /// <param name="input">要计算哈希值的输入字符串。</param>
        /// <returns>返回计算出的哈希值。</returns>
        public static int GetSimpleConsistentHash(string input)
        {
            int hash = 0;
            foreach (char c in input)
            {
                hash = (hash * prime) + c;
                hash &= 0x7FFFFFFF; // 保持正值
            }
            return hash;
        }
    }
}
