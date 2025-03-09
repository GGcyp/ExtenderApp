

namespace ExtenderApp.Common
{
    public static partial class Utility
    {
        //网络包头部工具

        /// <summary>
        /// 网络包头部长度
        /// </summary>
        public const int HEAD_LENGTH = 4;

        /// <summary>
        /// 网络包头部代码
        /// </summary>
        public const int HEAD_CODE = 0x0B;

        /// <summary>
        /// 在字节序列中查找头部信息。
        /// </summary>
        /// <param name="data">要查找的字节序列。</param>
        /// <param name="startIndex">找到头部信息的起始索引，如果没有找到则返回0。</param>
        /// <param name="count">要查找的字节数，默认值为-1，表示查找整个字节序列。</param>
        /// <returns>如果找到头部信息，则返回true，否则返回false。</returns>
        public static bool FindHead(ReadOnlySpan<byte> data, out int startIndex, int count = -1)
        {
            count = count == -1 ? data.Length : count;
            if (count < HEAD_LENGTH)
            {
                startIndex = 0;
                return false;
            }

            if (count == HEAD_LENGTH)
            {
                startIndex = 0;
                return true;
            }

            for (int i = 0; i < count - 4; i++)
            {
                bool hasHead = true;
                for (int j = 0; j < HEAD_LENGTH; j++)
                {
                    int code = GetHeadCode(j + 1);
                    if (data[i + j] != code)
                    {
                        hasHead = false;
                    }
                }

                if (hasHead)
                {
                    startIndex = i;
                    return true;
                }
            }

            startIndex = 0;
            return false;
        }

        /// <summary>
        /// 将头部信息写入字节数组
        /// </summary>
        /// <param name="bytes">要写入的字节数组</param>
        /// <param name="startIndex">开始写入的索引位置，默认为0</param>
        public static void WriteSendHead(Span<byte> bytes, int startIndex = 0)
        {
            for (int i = startIndex; i < HEAD_LENGTH; i++)
            {
                bytes[i] = (byte)GetHeadCode(i + 1);
            }
        }

        /// <summary>
        /// 获取发送头的代码
        /// </summary>
        /// <param name="index">索引</param>
        /// <returns>发送头的代码</returns>
        public static int GetHeadCode(int index)
        {
            return index * HEAD_CODE;
        }
    }
}
