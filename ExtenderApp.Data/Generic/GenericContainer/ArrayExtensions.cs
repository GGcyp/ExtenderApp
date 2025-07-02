using System.Buffers;


namespace ExtenderApp.Data
{
    /// <summary>
    /// 字节数组扩展类
    /// </summary>
    public static class ArrayExtensions
    {
        /// <summary>
        /// 将字节数组返回给 <see cref="ArrayPool{byte}"/>
        /// </summary>
        /// <param name="bytes">要返回的字节数组</param>
        public static void Return(this byte[] bytes)
        {
            ArrayPool<byte>.Shared.Return(bytes);
        }
    }
}
