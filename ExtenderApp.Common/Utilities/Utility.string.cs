

using System.Buffers;
using System;

namespace ExtenderApp.Common
{
    public static partial class Utility
    {
        private const int PRIME = 31;

        /// <summary>
        /// 获取给定类型T的简单一致性哈希值。
        /// </summary>
        /// <typeparam name="T">要计算一致性哈希值的类型。</typeparam>
        /// <returns>返回给定类型T的简单一致性哈希值。</returns>
        public static int GetSimpleConsistentHash<T>()
        {
            Type type = typeof(T);
            string typeName = type.FullName ?? type.Name;
            return GetSimpleConsistentHash(typeName);
        }

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
                hash = (hash * PRIME) + c;
                hash &= 0x7FFFFFFF; // 保持正值
            }
            return hash;
        }
    }
}
