using ExtenderApp.Data;

namespace ExtenderApp.Common.Torrent
{
    /// <summary>
    /// InfoHash 扩展类
    /// </summary>
    public static class InfoHashExtensions
    {
        /// <summary>
        /// 将 InfoHash 对象复制到 ExtenderBinaryWriter 对象中
        /// </summary>
        /// <param name="infoHash">要复制的 InfoHash 对象</param>
        /// <param name="writer">目标 ExtenderBinaryWriter 对象</param>
        /// <exception cref="ArgumentException">当 InfoHash 为空时抛出异常</exception>
        public static void CopyTo(this InfoHash infoHash, ref ExtenderBinaryWriter writer)
        {
            if (infoHash.IsEmpty)
                throw new ArgumentException("InfoHash不能为空", nameof(infoHash));

            var hash = infoHash.GetSha1orSha256();
            hash.CopyTo(ref writer);
        }
    }
}
