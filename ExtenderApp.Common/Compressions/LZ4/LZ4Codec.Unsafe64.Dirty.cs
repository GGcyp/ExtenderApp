
namespace ExtenderApp.Common.Serializations.Binary.LZ4
{
    internal partial class LZ4Codec
    {
        /// <summary>
        /// 使用LZ4算法对源数据进行压缩。
        /// </summary>
        /// <param name="hash_table">哈希表指针，用于存储哈希值。</param>
        /// <param name="src">源数据指针。</param>
        /// <param name="dst">目标数据指针。</param>
        /// <param name="src_len">源数据长度。</param>
        /// <param name="dst_maxlen">目标数据最大长度。</param>
        /// <returns>压缩后的数据长度，如果输出超出限制则返回0。</returns>
        private static unsafe int LZ4_compressCtx_64(
                   uint* hash_table,
                   byte* src,
                   byte* dst,
                   int src_len,
                   int dst_maxlen)
        {
            unchecked
            {
                byte* _p;

                // 固定指针指向DEBRUIJN_TABLE_64数组的首元素
                fixed (int* debruijn64 = &DEBRUIJN_TABLE_64[0])
                {
                    // r93
                    var src_p = src;
                    var src_base = src_p;
                    var src_anchor = src_p;
                    var src_end = src_p + src_len;
                    var src_mflimit = src_end - MFLIMIT;

                    var dst_p = dst;
                    var dst_end = dst_p + dst_maxlen;

                    var src_LASTLITERALS = src_end - LASTLITERALS;
                    var src_LASTLITERALS_1 = src_LASTLITERALS - 1;

                    var src_LASTLITERALS_3 = src_LASTLITERALS - 3;
                    var src_LASTLITERALS_STEPSIZE_1 = src_LASTLITERALS - (STEPSIZE_64 - 1);
                    var dst_LASTLITERALS_1 = dst_end - (1 + LASTLITERALS);
                    var dst_LASTLITERALS_3 = dst_end - (2 + 1 + LASTLITERALS);

                    int length;
                    uint h, h_fwd;

                    // 初始化
                    if (src_len < MINLENGTH)
                    {
                        goto _last_literals;
                    }

                    // 处理第一个字节
                    hash_table[(*(uint*)src_p * 2654435761u) >> HASH_ADJUST] = (uint)(src_p - src_base);
                    src_p++;
                    h_fwd = (*(uint*)src_p * 2654435761u) >> HASH_ADJUST;

                    // 主循环
                    while (true)
                    {
                        var findMatchAttempts = (1 << SKIPSTRENGTH) + 3;
                        var src_p_fwd = src_p;
                        byte* src_ref;
                        byte* dst_token;

                        // 寻找匹配项
                        do
                        {
                            h = h_fwd;
                            var step = findMatchAttempts++ >> SKIPSTRENGTH;
                            src_p = src_p_fwd;
                            src_p_fwd = src_p + step;

                            if (src_p_fwd > src_mflimit)
                            {
                                goto _last_literals;
                            }

                            h_fwd = (*(uint*)src_p_fwd * 2654435761u) >> HASH_ADJUST;
                            src_ref = src_base + hash_table[h];
                            hash_table[h] = (uint)(src_p - src_base);
                        }
                        while ((src_ref < src_p - MAX_DISTANCE) || ((*(uint*)src_ref) != (*(uint*)src_p)));

                        // 匹配项对齐
                        while ((src_p > src_anchor) && (src_ref > src) && (src_p[-1] == src_ref[-1]))
                        {
                            src_p--;
                            src_ref--;
                        }

                        // 编码文字长度
                        length = (int)(src_p - src_anchor);
                        dst_token = dst_p++;

                        if (dst_p + length + (length >> 8) > dst_LASTLITERALS_3)
                        {
                            return 0; // 检查输出限制
                        }

                        if (length >= RUN_MASK)
                        {
                            var len = length - RUN_MASK;
                            *dst_token = RUN_MASK << ML_BITS;
                            if (len > 254)
                            {
                                do
                                {
                                    *dst_p++ = 255;
                                    len -= 255;
                                }
                                while (len > 254);
                                *dst_p++ = (byte)len;
                                BlockCopy64(src_anchor, dst_p, length);
                                dst_p += length;
                                goto _next_match;
                            }

                            *dst_p++ = (byte)len;
                        }
                        else
                        {
                            *dst_token = (byte)(length << ML_BITS);
                        }

                        // 复制文字
                        _p = dst_p + length;
                        {
                            do
                            {
                                *(ulong*)dst_p = *(ulong*)src_anchor;
                                dst_p += 8;
                                src_anchor += 8;
                            }
                            while (dst_p < _p);
                        }

                        dst_p = _p;

                    _next_match:

                        // 编码偏移量
                        *(ushort*)dst_p = (ushort)(src_p - src_ref);
                        dst_p += 2;

                        // 开始计数
                        src_p += MINMATCH;
                        src_ref += MINMATCH; // MinMatch已经验证
                        src_anchor = src_p;

                        while (src_p < src_LASTLITERALS_STEPSIZE_1)
                        {
                            var diff = (*(long*)src_ref) ^ (*(long*)src_p);
                            if (diff == 0)
                            {
                                src_p += STEPSIZE_64;
                                src_ref += STEPSIZE_64;
                                continue;
                            }

                            src_p += debruijn64[((ulong)(diff & -diff) * 0x0218A392CDABBD3FL) >> 58];
                            goto _endCount;
                        }

                        if ((src_p < src_LASTLITERALS_3) && ((*(uint*)src_ref) == (*(uint*)src_p)))
                        {
                            src_p += 4;
                            src_ref += 4;
                        }

                        if ((src_p < src_LASTLITERALS_1) && ((*(ushort*)src_ref) == (*(ushort*)src_p)))
                        {
                            src_p += 2;
                            src_ref += 2;
                        }

                        if ((src_p < src_LASTLITERALS) && (*src_ref == *src_p))
                        {
                            src_p++;
                        }

                    _endCount:

                        // 编码匹配长度
                        length = (int)(src_p - src_anchor);

                        if (dst_p + (length >> 8) > dst_LASTLITERALS_1)
                        {
                            return 0; // 检查输出限制
                        }

                        if (length >= ML_MASK)
                        {
                            *dst_token += ML_MASK;
                            length -= ML_MASK;
                            for (; length > 509; length -= 510)
                            {
                                *dst_p++ = 255;
                                *dst_p++ = 255;
                            }

                            if (length > 254)
                            {
                                length -= 255;
                                *dst_p++ = 255;
                            }

                            *dst_p++ = (byte)length;
                        }
                        else
                        {
                            *dst_token += (byte)length;
                        }

                        // 测试块结束
                        if (src_p > src_mflimit)
                        {
                            src_anchor = src_p;
                            break;
                        }

                        // 填充哈希表
                        hash_table[(*(uint*)(src_p - 2) * 2654435761u) >> HASH_ADJUST] = (uint)(src_p - 2 - src_base);

                        // 测试下一个位置
                        h = (*(uint*)src_p * 2654435761u) >> HASH_ADJUST;
                        src_ref = src_base + hash_table[h];
                        hash_table[h] = (uint)(src_p - src_base);

                        if ((src_ref > src_p - (MAX_DISTANCE + 1)) && ((*(uint*)src_ref) == (*(uint*)src_p)))
                        {
                            dst_token = dst_p++;
                            *dst_token = 0;
                            goto _next_match;
                        }

                        // 准备下一个循环
                        src_anchor = src_p++;
                        h_fwd = (*(uint*)src_p * 2654435761u) >> HASH_ADJUST;
                    }

                _last_literals:

                    // 编码最后一个文字
                    var lastRun = (int)(src_end - src_anchor);
                    if (dst_p + lastRun + 1 + ((lastRun + 255 - RUN_MASK) / 255) > dst_end)
                    {
                        return 0;
                    }

                    if (lastRun >= RUN_MASK)
                    {
                        *dst_p++ = RUN_MASK << ML_BITS;
                        lastRun -= RUN_MASK;
                        for (; lastRun > 254; lastRun -= 255)
                        {
                            *dst_p++ = 255;
                        }

                        *dst_p++ = (byte)lastRun;
                    }
                    else
                    {
                        *dst_p++ = (byte)(lastRun << ML_BITS);
                    }

                    BlockCopy64(src_anchor, dst_p, (int)(src_end - src_anchor));
                    dst_p += src_end - src_anchor;

                    // 结束
                    return (int)(dst_p - dst);
                }
            }
        }

        /// <summary>
        /// 使用LZ4算法压缩数据。
        /// </summary>
        /// <param name="hash_table">哈希表指针，用于存储哈希值对应的指针位置。</param>
        /// <param name="src">源数据指针。</param>
        /// <param name="dst">目标数据指针。</param>
        /// <param name="src_len">源数据长度。</param>
        /// <param name="dst_maxlen">目标数据最大长度。</param>
        /// <returns>压缩后的数据长度。</returns>
        private static unsafe int LZ4_compress64kCtx_64(
            ushort* hash_table,
            byte* src,
            byte* dst,
            int src_len,
            int dst_maxlen)
        {
            unchecked
            {
                byte* _p;

                // 固定指向DEBRUIJN_TABLE_64数组的指针
                fixed (int* debruijn64 = &DEBRUIJN_TABLE_64[0])
                {
                    // r93
                    var src_p = src;                       // 源数据指针
                    var src_anchor = src_p;               // 源数据锚点指针
                    var src_base = src_p;                  // 源数据基准指针
                    var src_end = src_p + src_len;         // 源数据结束指针
                    var src_mflimit = src_end - MFLIMIT;   // 源数据最大匹配限制指针

                    var dst_p = dst;                       // 目标数据指针
                    var dst_end = dst_p + dst_maxlen;      // 目标数据结束指针

                    var src_LASTLITERALS = src_end - LASTLITERALS; // 源数据最后文字段指针
                    var src_LASTLITERALS_1 = src_LASTLITERALS - 1; // 源数据最后文字段指针减一

                    var src_LASTLITERALS_3 = src_LASTLITERALS - 3; // 源数据最后文字段指针减三

                    var src_LASTLITERALS_STEPSIZE_1 = src_LASTLITERALS - (STEPSIZE_64 - 1); // 源数据最后文字段指针减去步长减一
                    var dst_LASTLITERALS_1 = dst_end - (1 + LASTLITERALS); // 目标数据最后文字段指针减一
                    var dst_LASTLITERALS_3 = dst_end - (2 + 1 + LASTLITERALS); // 目标数据最后文字段指针减三

                    int len, length;

                    uint h, h_fwd;

                    // 初始化
                    if (src_len < MINLENGTH)
                    {
                        goto _last_literals;
                    }

                    // 第一个字节
                    src_p++;
                    h_fwd = (*(uint*)src_p * 2654435761u) >> HASH64K_ADJUST;

                    // 主循环
                    while (true)
                    {
                        var findMatchAttempts = (1 << SKIPSTRENGTH) + 3;// 匹配尝试次数
                        var src_p_fwd = src_p;
                        byte* src_ref;
                        byte* dst_token;

                        // 查找匹配
                        do
                        {
                            h = h_fwd;
                            var step = findMatchAttempts++ >> SKIPSTRENGTH;// 步长
                            src_p = src_p_fwd;
                            src_p_fwd = src_p + step;

                            if (src_p_fwd > src_mflimit)
                            {
                                goto _last_literals;
                            }

                            h_fwd = (*(uint*)src_p_fwd * 2654435761u) >> HASH64K_ADJUST;
                            src_ref = src_base + hash_table[h];// 匹配引用指针
                            hash_table[h] = (ushort)(src_p - src_base);// 更新哈希表
                        }
                        while ((*(uint*)src_ref) != (*(uint*)src_p));// 查找匹配

                        // 匹配追赶
                        while ((src_p > src_anchor) && (src_ref > src) && (src_p[-1] == src_ref[-1]))
                        {
                            src_p--;
                            src_ref--;
                        }

                        // 编码文字长度
                        length = (int)(src_p - src_anchor);
                        dst_token = dst_p++;

                        if (dst_p + length + (length >> 8) > dst_LASTLITERALS_3)
                        {
                            return 0; // 检查输出限制
                        }

                        if (length >= RUN_MASK)
                        {
                            len = length - RUN_MASK;
                            *dst_token = RUN_MASK << ML_BITS;
                            if (len > 254)
                            {
                                do
                                {
                                    *dst_p++ = 255;
                                    len -= 255;
                                }
                                while (len > 254);
                                *dst_p++ = (byte)len;
                                BlockCopy64(src_anchor, dst_p, length);// 复制64位块
                                dst_p += length;
                                goto _next_match;
                            }

                            *dst_p++ = (byte)len;
                        }
                        else
                        {
                            *dst_token = (byte)(length << ML_BITS);
                        }

                        // 复制文字
                        {
                            _p = dst_p + length;
                            {
                                do
                                {
                                    *(ulong*)dst_p = *(ulong*)src_anchor;
                                    dst_p += 8;
                                    src_anchor += 8;
                                }
                                while (dst_p < _p);
                            }

                            dst_p = _p;
                        }

                    _next_match:

                        // 编码偏移量
                        *(ushort*)dst_p = (ushort)(src_p - src_ref);
                        dst_p += 2;

                        // 开始计数
                        src_p += MINMATCH;
                        src_ref += MINMATCH;  // 最小匹配验证
                        src_anchor = src_p;

                        while (src_p < src_LASTLITERALS_STEPSIZE_1)
                        {
                            var diff = (*(long*)src_ref) ^ (*(long*)src_p);
                            if (diff == 0)
                            {
                                src_p += STEPSIZE_64;
                                src_ref += STEPSIZE_64;
                                continue;
                            }

                            src_p += debruijn64[((ulong)(diff & -diff) * 0x0218A392CDABBD3FL) >> 58];
                            goto _endCount;
                        }

                        if ((src_p < src_LASTLITERALS_3) && ((*(uint*)src_ref) == (*(uint*)src_p)))
                        {
                            src_p += 4;
                            src_ref += 4;
                        }

                        if ((src_p < src_LASTLITERALS_1) && ((*(ushort*)src_ref) == (*(ushort*)src_p)))
                        {
                            src_p += 2;
                            src_ref += 2;
                        }

                        if ((src_p < src_LASTLITERALS) && (*src_ref == *src_p))
                        {
                            src_p++;
                        }

                    _endCount:

                        // 编码匹配长度
                        len = (int)(src_p - src_anchor);

                        if (dst_p + (len >> 8) > dst_LASTLITERALS_1)
                        {
                            return 0; // 检查输出限制
                        }

                        if (len >= ML_MASK)
                        {
                            *dst_token += ML_MASK;
                            len -= ML_MASK;
                            for (; len > 509; len -= 510)
                            {
                                *dst_p++ = 255;
                                *dst_p++ = 255;
                            }

                            if (len > 254)
                            {
                                len -= 255;
                                *dst_p++ = 255;
                            }

                            *dst_p++ = (byte)len;
                        }
                        else
                        {
                            *dst_token += (byte)len;
                        }

                        // 测试块结束
                        if (src_p > src_mflimit)
                        {
                            src_anchor = src_p;
                            break;
                        }

                        // 填充哈希表
                        hash_table[(*(uint*)(src_p - 2) * 2654435761u) >> HASH64K_ADJUST] = (ushort)(src_p - 2 - src_base);

                        // 测试下一个位置
                        h = (*(uint*)src_p * 2654435761u) >> HASH64K_ADJUST;
                        src_ref = src_base + hash_table[h];
                        hash_table[h] = (ushort)(src_p - src_base);

                        if ((*(uint*)src_ref) == (*(uint*)src_p))
                        {
                            dst_token = dst_p++;
                            *dst_token = 0;
                            goto _next_match;
                        }

                        // 准备下一个循环
                        src_anchor = src_p++;
                        h_fwd = (*(uint*)src_p * 2654435761u) >> HASH64K_ADJUST;
                    }

                _last_literals:

                    // 编码最后文字段
                    var lastRun = (int)(src_end - src_anchor);
                    if (dst_p + lastRun + 1 + ((lastRun - RUN_MASK + 255) / 255) > dst_end)
                    {
                        return 0;
                    }

                    if (lastRun >= RUN_MASK)
                    {
                        *dst_p++ = RUN_MASK << ML_BITS;
                        lastRun -= RUN_MASK;
                        for (; lastRun > 254; lastRun -= 255)
                        {
                            *dst_p++ = 255;
                        }

                        *dst_p++ = (byte)lastRun;
                    }
                    else
                    {
                        *dst_p++ = (byte)(lastRun << ML_BITS);
                    }

                    BlockCopy64(src_anchor, dst_p, (int)(src_end - src_anchor));
                    dst_p += src_end - src_anchor;

                    // 结束
                    return (int)(dst_p - dst);
                }
            }
        }

        /// <summary>
        /// 使用LZ4算法解压数据。
        /// </summary>
        /// <param name="src">指向压缩数据的指针。</param>
        /// <param name="dst">指向解压后数据存储位置的指针。</param>
        /// <param name="dst_len">解压后数据的长度。</param>
        /// <returns>解压后的数据长度，如果发生错误则返回负值。</returns>
        private static unsafe int LZ4_uncompress_64(
            byte* src,
            byte* dst,
            int dst_len)
        {
            unchecked
            {
                // 固定解码表指针
                fixed (int* dec32table = &DECODER_TABLE_32[0])
                fixed (int* dec64table = &DECODER_TABLE_64[0])
                {
                    // r93
                    var src_p = src;
                    byte* dst_ref;

                    var dst_p = dst;
                    var dst_end = dst_p + dst_len;
                    byte* dst_cpy;

                    var dst_LASTLITERALS = dst_end - LASTLITERALS;
                    var dst_COPYLENGTH = dst_end - COPYLENGTH;
                    var dst_COPYLENGTH_STEPSIZE_4 = dst_end - COPYLENGTH - (STEPSIZE_64 - 4);

                    byte token;

                    // 主循环
                    while (true)
                    {
                        int length;

                        // 获取运行长度
                        token = *src_p++;
                        if ((length = token >> ML_BITS) == RUN_MASK)
                        {
                            int len;
                            for (; (len = *src_p++) == 255; length += 255)
                            {
                                //检查长度是否超出限制
                            }

                            length += len;
                        }

                        // 复制字面量
                        dst_cpy = dst_p + length;

                        if (dst_cpy > dst_COPYLENGTH)
                        {
                            if (dst_cpy != dst_end)
                            {
                                // 错误：没有足够的空间容纳另一个匹配（最小4）和5个字面量
                                goto _output_error;
                            }

                            BlockCopy64(src_p, dst_p, length);
                            src_p += length;
                            break; // EOF
                        }

                        do
                        {
                            *(ulong*)dst_p = *(ulong*)src_p;
                            dst_p += 8;
                            src_p += 8;
                        }
                        while (dst_p < dst_cpy);
                        src_p -= dst_p - dst_cpy;
                        dst_p = dst_cpy;

                        // 获取偏移量
                        dst_ref = dst_cpy - (*(ushort*)src_p);
                        src_p += 2;
                        if (dst_ref < dst)
                        {
                            // 错误：偏移量在目标缓冲区之外
                            goto _output_error;
                        }

                        // 获取匹配长度
                        if ((length = token & ML_MASK) == ML_MASK)
                        {
                            for (; *src_p == 255; length += 255)
                            {
                                src_p++;
                            }

                            length += *src_p++;
                        }

                        // 复制重复序列
                        if ((dst_p - dst_ref) < STEPSIZE_64)
                        {
                            var dec64 = dec64table[dst_p - dst_ref];

                            dst_p[0] = dst_ref[0];
                            dst_p[1] = dst_ref[1];
                            dst_p[2] = dst_ref[2];
                            dst_p[3] = dst_ref[3];
                            dst_p += 4;
                            dst_ref += 4;
                            dst_ref -= dec32table[dst_p - dst_ref];
                            *(uint*)dst_p = *(uint*)dst_ref;
                            dst_p += STEPSIZE_64 - 4;
                            dst_ref -= dec64;
                        }
                        else
                        {
                            *(ulong*)dst_p = *(ulong*)dst_ref;
                            dst_p += 8;
                            dst_ref += 8;
                        }

                        dst_cpy = dst_p + length - (STEPSIZE_64 - 4);

                        if (dst_cpy > dst_COPYLENGTH_STEPSIZE_4)
                        {
                            if (dst_cpy > dst_LASTLITERALS)
                            {
                                // 错误：最后5个字节必须是字面量
                                goto _output_error;
                            }

                            while (dst_p < dst_COPYLENGTH)
                            {
                                *(ulong*)dst_p = *(ulong*)dst_ref;
                                dst_p += 8;
                                dst_ref += 8;
                            }

                            while (dst_p < dst_cpy)
                            {
                                *dst_p++ = *dst_ref++;
                            }

                            dst_p = dst_cpy;// 修正
                            continue;
                        }

                        {
                            do
                            {
                                *(ulong*)dst_p = *(ulong*)dst_ref;
                                dst_p += 8;
                                dst_ref += 8;
                            }
                            while (dst_p < dst_cpy);
                        }

                        // 修正
                        dst_p = dst_cpy;
                    }

                    // 解码结束
                    return (int)(src_p - src);

                // 写入溢出错误检测
                _output_error:
                    return (int)-(src_p - src);
                }
            }
        }

        /// <summary>
        /// 使用不安全代码块复制内存中的数据。
        /// </summary>
        /// <param name="src">源字节指针。</param>
        /// <param name="dst">目标字节指针。</param>
        /// <param name="len">要复制的字节数。</param>
        private static unsafe void BlockCopy64(byte* src, byte* dst, int len)
        {
            while (len >= 8)
            {
                *(ulong*)dst = *(ulong*)src;
                dst += 8;
                src += 8;
                len -= 8;
            }

            if (len >= 4)
            {
                *(uint*)dst = *(uint*)src;
                dst += 4;
                src += 4;
                len -= 4;
            }

            if (len >= 2)
            {
                *(ushort*)dst = *(ushort*)src;
                dst += 2;
                src += 2;
                len -= 2;
            }

            if (len >= 1)
            {
                *dst = *src; /* d++; s++; l--; */
            }
        }
    }
}
