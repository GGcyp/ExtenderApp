

using System.Buffers;
using System;

namespace ExtenderApp.Common
{
    public static partial class Utility
    {
        private const int PRIME = 31;

        public static void GetSimpleConsistentHash<T>(out int hash)
        {
            Type type = typeof(T);
            string typeName = type.FullName ?? type.Name;
            GetSimpleConsistentHash(typeName, out hash);
        }


        public static void GetSimpleConsistentHash(string input, out int hash)
        {
            hash = 0;
            if (string.IsNullOrEmpty(input))
                return;

            hash = 0;
            foreach (char c in input)
            {
                hash = (hash * PRIME) + c;
                hash &= 0x7FFFFFFF; // 保持正值
            }
        }

        public static void GetSimpleConsistentHash<T>(out long hash)
        {
            Type type = typeof(T);
            string typeName = type.FullName ?? type.Name;
            GetSimpleConsistentHash(typeName, out hash);
        }


        public static void GetSimpleConsistentHash(string input, out long hash)
        {
            hash = 0;
            if (string.IsNullOrEmpty(input))
                return;

            hash = 0;
            foreach (char c in input)
            {
                hash = (hash * PRIME) + c;
                hash &= 0x7FFFFFFF; // 保持正值
            }
        }
    }
}
