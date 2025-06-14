

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
        /// 获取简单的一致性哈希值
        /// </summary>
        /// <param name="input">输入字符串</param>
        /// <returns>一致性哈希值</returns>
        public static int GetSimpleConsistentHash(string input)
        {
            if (string.IsNullOrEmpty(input))
                return 0;

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
