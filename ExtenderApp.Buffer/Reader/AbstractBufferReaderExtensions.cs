namespace ExtenderApp.Buffer
{
    /// <summary>
    /// 为 <see cref="AbstractBufferReader{byte}"/> 提供的扩展方法集合。
    /// </summary>
    public static class AbstractBufferReaderExtensions
    {
        /// <summary>
        /// 从 reader 的未读序列中读取类型为 <typeparamref name="T"/> 的值，并按读取字节数前进 reader。 默认按 big-endian 解释字节序，可以通过 <paramref name="isBigEndian"/> 指定。
        /// </summary>
        /// <typeparam name="T">要读取的目标类型，必须为 <c>unmanaged</c>。</typeparam>
        /// <param name="reader">要从中读取的 <see cref="AbstractBufferReader{byte}"/>。</param>
        /// <param name="isBigEndian">指示未读序列中的字节序是否为大端：若平台为 little-endian 且本参数为 <c>true</c>，方法会对字节进行反转以恢复正确值。</param>
        /// <returns>解析得到的 <typeparamref name="T"/> 值。</returns>
        public static T Read<T>(this AbstractBufferReader<byte> reader, bool isBigEndian = true)
            where T : unmanaged
        {
            ArgumentNullException.ThrowIfNull(reader, nameof(reader));

            var result = reader.UnreadSequence.Read<T>(out int size, isBigEndian);
            reader.Advance(size);
            return result;
        }

        /// <summary>
        /// 尝试从 reader 的未读序列中读取类型为 <typeparamref name="T"/> 的值。 若序列中可用字节不足或发生错误，则返回 <c>false</c>，且 reader 不会前进。 若返回 <c>true</c>，reader 将按读取的字节数前进。
        /// </summary>
        /// <typeparam name="T">要读取的目标类型，必须为 <c>unmanaged</c>。</typeparam>
        /// <param name="reader">要从中读取的 <see cref="AbstractBufferReader{byte}"/>。</param>
        /// <param name="value">输出读取到的值，失败时为 default。</param>
        /// <param name="size">输出读取所用的字节数，失败时为 0。</param>
        /// <param name="isBigEndian">指示未读序列中的字节序是否为大端：若平台为 little-endian 且本参数为 <c>true</c>，方法会对字节进行反转以恢复正确值。</param>
        /// <returns>如果成功读取则返回 <c>true</c>，否则返回 <c>false</c>。</returns>
        public static bool TryRead<T>(this AbstractBufferReader<byte> reader, out T value, out int size, bool isBigEndian = true)
            where T : unmanaged
        {
            ArgumentNullException.ThrowIfNull(reader, nameof(reader));

            if (reader.UnreadSequence.TryRead(out value, out size, isBigEndian))
            {
                reader.Advance(size);
                return true;
            }
            value = default;
            size = 0;
            return false;
        }
    }
}