namespace ExtenderApp.Common.IO.Binaries.LZ4
{
    /// <summary>
    /// 哈希表池类，用于提供线程安全的哈希表数组池。
    /// </summary>
    internal partial class LZ4Codec
    {
        /// <summary>
        /// 线程静态的 ushort 数组池。
        /// </summary>
        [ThreadStatic]
        private static ushort[]? ushortPool;

        /// <summary>
        /// 线程静态的 uint 数组池。
        /// </summary>
        [ThreadStatic]
        private static uint[]? uintPool;

        /// <summary>
        /// 线程静态的 int 数组池。
        /// </summary>
        [ThreadStatic]
        private static int[]? intPool;

        /// <summary>
        /// 获取 ushort 类型的哈希表数组池。
        /// 如果池为空，则创建一个新的 ushort 数组并返回；否则清空现有数组并返回。
        /// </summary>
        /// <returns>返回 ushort 类型的哈希表数组。</returns>
        public static ushort[] GetUShortHashTablePool()
        {
            if (ushortPool == null)
            {
                ushortPool = new ushort[HASH64K_TABLESIZE];
            }
            else
            {
                Array.Clear(ushortPool, 0, ushortPool.Length);
            }

            return ushortPool;
        }

        /// <summary>
        /// 获取 uint 类型的哈希表数组池。
        /// 如果池为空，则创建一个新的 uint 数组并返回；否则清空现有数组并返回。
        /// </summary>
        /// <returns>返回 uint 类型的哈希表数组。</returns>
        public static uint[] GetUIntHashTablePool()
        {
            if (uintPool == null)
            {
                uintPool = new uint[HASH_TABLESIZE];
            }
            else
            {
                Array.Clear(uintPool, 0, uintPool.Length);
            }

            return uintPool;
        }

        /// <summary>
        /// 获取 int 类型的哈希表数组池。
        /// 如果池为空，则创建一个新的 int 数组并返回；否则清空现有数组并返回。
        /// </summary>
        /// <returns>返回 int 类型的哈希表数组。</returns>
        public static int[] GetIntHashTablePool()
        {
            if (intPool == null)
            {
                intPool = new int[HASH_TABLESIZE];
            }
            else
            {
                Array.Clear(intPool, 0, intPool.Length);
            }

            return intPool;
        }
    }
}
