

namespace ExtenderApp.Common
{
    public static partial class Utility
    {
        /// <summary>
        /// 将兆字节转换为字节
        /// </summary>
        /// <param name="megabytes">要转换的兆字节数</param>
        /// <returns>转换后的字节数</returns>
        public static long MegabytesToBytes(double megabytes)
        {
            // 1 兆字节等于 1024 * 1024 字节
            return (long)(megabytes * 1024 * 1024);
        }

        /// <summary>
        /// 将千兆字节转换为字节
        /// </summary>
        /// <param name="gigabytes">要转换的千兆字节数</param>
        /// <returns>转换后的字节数</returns>
        public static long GigabytesToBytes(double gigabytes)
        {
            // 1GB 等于 1024 * 1024 * 1024 字节
            return (long)(gigabytes * 1024 * 1024 * 1024);
        }

        /// <summary>
        /// 将千字节转换为字节
        /// </summary>
        /// <param name="kilobytes">要转换的千字节数</param>
        /// <returns>转换后的字节数</returns>
        public static long KilobytesToBytes(double kilobytes)
        {
            // 1KB 等于 1024 字节
            return (long)(kilobytes * 1024);
        }

        /// <summary>
        /// 将字节转换为兆字节
        /// </summary>
        /// <param name="bytes">要转换的字节数</param>
        /// <returns>转换后的兆字节数</returns>
        public static double BytesToMegabytes(long bytes)
        {
            // 1 兆字节等于 1024 * 1024 字节
            return (double)bytes / (1024 * 1024);
        }

        /// <summary>
        /// 将字节转换为千兆字节
        /// </summary>
        /// <param name="bytes">要转换的字节数</param>
        /// <returns>转换后的千兆字节数</returns>
        public static double BytesToGigabytes(long bytes)
        {
            // 1 千兆字节等于 1024 * 1024 * 1024 字节
            return (double)bytes / (1024 * 1024 * 1024);
        }

        /// <summary>
        /// 将字节转换为千字节
        /// </summary>
        /// <param name="bytes">要转换的字节数</param>
        /// <returns>转换后的千字节数</returns>
        public static double BytesToKilobytes(long bytes)
        {
            // 1 千字节等于 1024 字节
            return (double)bytes / 1024;
        }
    }
}
