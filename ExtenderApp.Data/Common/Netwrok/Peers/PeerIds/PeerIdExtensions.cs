using System.Buffers;
using System.Text;

namespace ExtenderApp.Data
{
    /// <summary>
    /// PeerId 的扩展方法类
    /// </summary>
    public static class PeerIdExtensions
    {
        ///// <summary>
        ///// 将 PeerId 对象复制到 ExtenderBinaryWriter 中
        ///// </summary>
        ///// <param name="peerId">要复制的 PeerId 对象</param>
        ///// <param name="writer">目标 ExtenderBinaryWriter 对象</param>
        ///// <exception cref="ArgumentException">如果 PeerId 为空，则抛出此异常</exception>
        //public static void CopyTo(this PeerId peerId, ref ExtenderBinaryWriter writer)
        //{
        //    if (peerId.IsEmpty)
        //    {
        //        throw new ArgumentException("对等节点ID不能为空", nameof(peerId));
        //    }

        //    //Encoding encoding = Encoding.ASCII;
        //    //var length = encoding.GetByteCount(peerId.Id);
        //    //var length = peerId.Id.Length;
        //    //var bytes = ArrayPool<byte>.Shared.Rent(length);
        //    //encoding.GetBytes(peerId.Id, bytes);

        //    //writer.Write(bytes.AsSpan().Slice(0, length));
        //    //ArrayPool<byte>.Shared.Return(bytes);
        //    writer.Write(peerId.Id.AsSpan());
        //}
    }
}
