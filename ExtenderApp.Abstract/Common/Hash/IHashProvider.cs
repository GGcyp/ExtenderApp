using System.Security.Cryptography;
using ExtenderApp.Data;

namespace ExtenderApp.Abstract
{
    /// <summary>
    /// 散列值提供程序接口
    /// </summary>
    public interface IHashProvider
    {
        ///// <summary>
        ///// 计算指定字节数组的散列值
        ///// </summary>
        ///// <typeparam name="T">散列算法类型</typeparam>
        ///// <param name="bytes">要计算散列值的字节数组</param>
        ///// <returns>计算得到的散列值</returns>
        //HashValue ComputeHash<T>(byte[] bytes) where T : HashAlgorithm;

        ///// <summary>
        ///// 计算指定字节数组子范围的散列值
        ///// </summary>
        ///// <typeparam name="T">散列算法类型</typeparam>
        ///// <param name="bytes">要计算散列值的字节数组</param>
        ///// <param name="offset">子范围的起始位置</param>
        ///// <param name="count">子范围的长度</param>
        ///// <returns>计算得到的散列值</returns>
        //HashValue ComputeHash<T>(byte[] bytes, int offset, int count) where T : HashAlgorithm;

        ///// <summary>
        ///// 计算指定流的散列值
        ///// </summary>
        ///// <typeparam name="T">散列算法类型</typeparam>
        ///// <param name="stream">要计算散列值的流</param>
        ///// <returns>计算得到的散列值</returns>
        //HashValue ComputeHash<T>(Stream stream) where T : HashAlgorithm;

        ///// <summary>
        ///// 计算文件的哈希值。
        ///// </summary>
        ///// <typeparam name="T">哈希算法类型，必须继承自HashAlgorithm。</typeparam>
        ///// <param name="info">文件操作信息。</param>
        ///// <returns>计算得到的哈希值。</returns>
        //HashValue ComputeHash<T>(FileOperateInfo info) where T : HashAlgorithm;

        ///// <summary>
        ///// 计算给定文本的哈希值。
        ///// </summary>
        ///// <typeparam name="T">用于计算哈希值的哈希算法类型，必须继承自HashAlgorithm类。</typeparam>
        ///// <param name="text">要计算哈希值的文本。</param>
        ///// <returns>计算出的哈希值。</returns>
        //HashValue ComputeHash<T>(string text) where T : HashAlgorithm;

        ///// <summary>
        ///// 异步计算给定流的哈希值。
        ///// </summary>
        ///// <typeparam name="T">指定哈希算法的类型。</typeparam>
        ///// <param name="stream">要计算哈希值的流。</param>
        ///// <returns>返回计算出的哈希值。</returns>
        //Task<HashValue> ComputeHashAsync<T>(Stream stream) where T : HashAlgorithm;

        ///// <summary>
        ///// 异步计算给定文件的哈希值。
        ///// </summary>
        ///// <typeparam name="T">指定哈希算法的类型。</typeparam>
        ///// <param name="info">包含文件操作信息的对象。</param>
        ///// <returns>返回计算出的哈希值。</returns>
        //Task<HashValue> ComputeHashAsync<T>(FileOperateInfo info) where T : HashAlgorithm;
    }
}
