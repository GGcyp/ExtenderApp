
namespace ExtenderApp.Common.IO.Binary.LZ4
{
    internal partial class LZ4Codec
    {
        /// <summary>
        /// 使用LZ4算法压缩32位上下文的数据。
        /// </summary>
        /// <param name="hash_table">哈希表，用于存储源数据的哈希值及其对应的偏移量。</param>
        /// <param name="src">指向源数据的指针。</param>
        /// <param name="dst">指向目标数据的指针。</param>
        /// <param name="src_len">源数据的长度。</param>
        /// <param name="dst_maxlen">目标数据的最大长度。</param>
        /// <returns>返回压缩后数据的长度，如果返回0，则表示压缩失败。</returns>
        private static unsafe int LZ4_compressCtx_32(
            uint* hash_table,
            byte* src,
            byte* dst,
            int src_len,
            int dst_maxlen)
        {
            unchecked
            {
                byte* _p;

                fixed (int* debruijn32 = &DEBRUIJN_TABLE_32[0])
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

                    var src_LASTLITERALS_STEPSIZE_1 = src_LASTLITERALS - (STEPSIZE_32 - 1);
                    var dst_LASTLITERALS_1 = dst_end - (1 + LASTLITERALS);
                    var dst_LASTLITERALS_3 = dst_end - (2 + 1 + LASTLITERALS);

                    int length;

                    uint h, h_fwd;

                    // Init
                    if (src_len < MINLENGTH)
                    {
                        goto _last_literals;
                    }

                    // First Byte
                    hash_table[(*(uint*)src_p * 2654435761u) >> HASH_ADJUST] = (uint)(src_p - src_base);
                    src_p++;
                    h_fwd = (*(uint*)src_p * 2654435761u) >> HASH_ADJUST;

                    // Main Loop
                    while (true)
                    {
                        var findMatchAttempts = (1 << SKIPSTRENGTH) + 3;
                        var src_p_fwd = src_p;
                        byte* xxx_ref;
                        byte* xxx_token;

                        // Find a match
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
                            xxx_ref = src_base + hash_table[h];
                            hash_table[h] = (uint)(src_p - src_base);
                        }
                        while ((xxx_ref < src_p - MAX_DISTANCE) || ((*(uint*)xxx_ref) != (*(uint*)src_p)));

                        // Catch up
                        while ((src_p > src_anchor) && (xxx_ref > src) && (src_p[-1] == xxx_ref[-1]))
                        {
                            src_p--;
                            xxx_ref--;
                        }

                        // EncodeSequence Literal length
                        length = (int)(src_p - src_anchor);
                        xxx_token = dst_p++;

                        if (dst_p + length + (length >> 8) > dst_LASTLITERALS_3)
                        {
                            return 0; // Check output limit
                        }

                        if (length >= RUN_MASK)
                        {
                            var len = length - RUN_MASK;
                            *xxx_token = RUN_MASK << ML_BITS;
                            if (len > 254)
                            {
                                do
                                {
                                    *dst_p++ = 255;
                                    len -= 255;
                                }
                                while (len > 254);
                                *dst_p++ = (byte)len;
                                BlockCopy32(src_anchor, dst_p, length);
                                dst_p += length;
                                goto _next_match;
                            }

                            *dst_p++ = (byte)len;
                        }
                        else
                        {
                            *xxx_token = (byte)(length << ML_BITS);
                        }

                        // Copy Literals
                        _p = dst_p + length;
                        do
                        {
                            *(uint*)dst_p = *(uint*)src_anchor;
                            dst_p += 4;
                            src_anchor += 4;
                            *(uint*)dst_p = *(uint*)src_anchor;
                            dst_p += 4;
                            src_anchor += 4;
                        }
                        while (dst_p < _p);
                        dst_p = _p;

                    _next_match:

                        // EncodeSequence Offset
                        *(ushort*)dst_p = (ushort)(src_p - xxx_ref);
                        dst_p += 2;

                        // Start Counting
                        src_p += MINMATCH;
                        xxx_ref += MINMATCH; // MinMatch already verified
                        src_anchor = src_p;

                        while (src_p < src_LASTLITERALS_STEPSIZE_1)
                        {
                            var diff = (*(int*)xxx_ref) ^ (*(int*)src_p);
                            if (diff == 0)
                            {
                                src_p += STEPSIZE_32;
                                xxx_ref += STEPSIZE_32;
                                continue;
                            }

                            src_p += debruijn32[((uint)(diff & -diff) * 0x077CB531u) >> 27];
                            goto _endCount;
                        }

                        if ((src_p < src_LASTLITERALS_1) && ((*(ushort*)xxx_ref) == (*(ushort*)src_p)))
                        {
                            src_p += 2;
                            xxx_ref += 2;
                        }

                        if ((src_p < src_LASTLITERALS) && (*xxx_ref == *src_p))
                        {
                            src_p++;
                        }

                    _endCount:

                        // EncodeSequence MatchLength
                        length = (int)(src_p - src_anchor);

                        if (dst_p + (length >> 8) > dst_LASTLITERALS_1)
                        {
                            return 0; // Check output limit
                        }

                        if (length >= ML_MASK)
                        {
                            *xxx_token += ML_MASK;
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
                            *xxx_token += (byte)length;
                        }

                        // Test end of chunk
                        if (src_p > src_mflimit)
                        {
                            src_anchor = src_p;
                            break;
                        }

                        // Fill table
                        hash_table[(*(uint*)(src_p - 2) * 2654435761u) >> HASH_ADJUST] = (uint)(src_p - 2 - src_base);

                        // Test next position
                        h = (*(uint*)src_p * 2654435761u) >> HASH_ADJUST;
                        xxx_ref = src_base + hash_table[h];
                        hash_table[h] = (uint)(src_p - src_base);

                        if ((xxx_ref > src_p - (MAX_DISTANCE + 1)) && ((*(uint*)xxx_ref) == (*(uint*)src_p)))
                        {
                            xxx_token = dst_p++;
                            *xxx_token = 0;
                            goto _next_match;
                        }

                        // Prepare next loop
                        src_anchor = src_p++;
                        h_fwd = (*(uint*)src_p * 2654435761u) >> HASH_ADJUST;
                    }

                _last_literals:

                    // EncodeSequence Last Literals
                    {
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

                        BlockCopy32(src_anchor, dst_p, (int)(src_end - src_anchor));
                        dst_p += src_end - src_anchor;
                    }

                    // End
                    return (int)(dst_p - dst);
                }
            }
        }

        /// <summary>
        /// 使用LZ4算法压缩64KB上下文的数据。
        /// </summary>
        /// <param name="hash_table">哈希表，用于存储源数据的哈希值及其对应的偏移量。</param>
        /// <param name="src">指向源数据的指针。</param>
        /// <param name="dst">指向目标数据的指针。</param>
        /// <param name="src_len">源数据的长度。</param>
        /// <param name="dst_maxlen">目标数据的最大长度。</param>
        /// <returns>返回压缩后数据的长度，如果返回0，则表示压缩失败。</returns>
        private static unsafe int LZ4_compress64kCtx_32(
            ushort* hash_table,
            byte* src,
            byte* dst,
            int src_len,
            int dst_maxlen)
        {
            unchecked
            {
                byte* _p;
                fixed (int* debruijn32 = &DEBRUIJN_TABLE_32[0])
                {
                    // r93
                    var src_p = src;
                    var src_anchor = src_p;
                    var src_base = src_p;
                    var src_end = src_p + src_len;
                    var src_mflimit = src_end - MFLIMIT;

                    var dst_p = dst;
                    var dst_end = dst_p + dst_maxlen;

                    var src_LASTLITERALS = src_end - LASTLITERALS;
                    var src_LASTLITERALS_1 = src_LASTLITERALS - 1;

                    var src_LASTLITERALS_STEPSIZE_1 = src_LASTLITERALS - (STEPSIZE_32 - 1);
                    var dst_LASTLITERALS_1 = dst_end - (1 + LASTLITERALS);
                    var dst_LASTLITERALS_3 = dst_end - (2 + 1 + LASTLITERALS);

                    int len, length;

                    uint h, h_fwd;

                    // Init
                    if (src_len < MINLENGTH)
                    {
                        goto _last_literals;
                    }

                    // First Byte
                    src_p++;
                    h_fwd = (*(uint*)src_p * 2654435761u) >> HASH64K_ADJUST;

                    // Main Loop
                    while (true)
                    {
                        var findMatchAttempts = (1 << SKIPSTRENGTH) + 3;
                        var src_p_fwd = src_p;
                        byte* xxx_ref;
                        byte* xxx_token;

                        // Find a match
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

                            h_fwd = (*(uint*)src_p_fwd * 2654435761u) >> HASH64K_ADJUST;
                            xxx_ref = src_base + hash_table[h];
                            hash_table[h] = (ushort)(src_p - src_base);
                        }
                        while ((*(uint*)xxx_ref) != (*(uint*)src_p));

                        // Catch up
                        while ((src_p > src_anchor) && (xxx_ref > src) && (src_p[-1] == xxx_ref[-1]))
                        {
                            src_p--;
                            xxx_ref--;
                        }

                        // EncodeSequence Literal length
                        length = (int)(src_p - src_anchor);
                        xxx_token = dst_p++;

                        if (dst_p + length + (length >> 8) > dst_LASTLITERALS_3)
                        {
                            return 0; // Check output limit
                        }

                        if (length >= RUN_MASK)
                        {
                            len = length - RUN_MASK;
                            *xxx_token = RUN_MASK << ML_BITS;
                            if (len > 254)
                            {
                                do
                                {
                                    *dst_p++ = 255;
                                    len -= 255;
                                }
                                while (len > 254);
                                *dst_p++ = (byte)len;
                                BlockCopy32(src_anchor, dst_p, length);
                                dst_p += length;
                                goto _next_match;
                            }

                            *dst_p++ = (byte)len;
                        }
                        else
                        {
                            *xxx_token = (byte)(length << ML_BITS);
                        }

                        // Copy Literals
                        _p = dst_p + length;
                        do
                        {
                            *(uint*)dst_p = *(uint*)src_anchor;
                            dst_p += 4;
                            src_anchor += 4;
                            *(uint*)dst_p = *(uint*)src_anchor;
                            dst_p += 4;
                            src_anchor += 4;
                        }
                        while (dst_p < _p);
                        dst_p = _p;

                    _next_match:

                        // EncodeSequence Offset
                        *(ushort*)dst_p = (ushort)(src_p - xxx_ref);
                        dst_p += 2;

                        // Start Counting
                        src_p += MINMATCH;
                        xxx_ref += MINMATCH; // MinMatch verified
                        src_anchor = src_p;

                        while (src_p < src_LASTLITERALS_STEPSIZE_1)
                        {
                            var diff = (*(int*)xxx_ref) ^ (*(int*)src_p);
                            if (diff == 0)
                            {
                                src_p += STEPSIZE_32;
                                xxx_ref += STEPSIZE_32;
                                continue;
                            }

                            src_p += debruijn32[((uint)(diff & -diff) * 0x077CB531u) >> 27];
                            goto _endCount;
                        }

                        if ((src_p < src_LASTLITERALS_1) && ((*(ushort*)xxx_ref) == (*(ushort*)src_p)))
                        {
                            src_p += 2;
                            xxx_ref += 2;
                        }

                        if ((src_p < src_LASTLITERALS) && (*xxx_ref == *src_p))
                        {
                            src_p++;
                        }

                    _endCount:

                        // EncodeSequence MatchLength
                        len = (int)(src_p - src_anchor);

                        if (dst_p + (len >> 8) > dst_LASTLITERALS_1)
                        {
                            return 0; // Check output limit
                        }

                        if (len >= ML_MASK)
                        {
                            *xxx_token += ML_MASK;
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
                            *xxx_token += (byte)len;
                        }

                        // Test end of chunk
                        if (src_p > src_mflimit)
                        {
                            src_anchor = src_p;
                            break;
                        }

                        // Fill table
                        hash_table[(*(uint*)(src_p - 2) * 2654435761u) >> HASH64K_ADJUST] = (ushort)(src_p - 2 - src_base);

                        // Test next position
                        h = (*(uint*)src_p * 2654435761u) >> HASH64K_ADJUST;
                        xxx_ref = src_base + hash_table[h];
                        hash_table[h] = (ushort)(src_p - src_base);

                        if ((*(uint*)xxx_ref) == (*(uint*)src_p))
                        {
                            xxx_token = dst_p++;
                            *xxx_token = 0;
                            goto _next_match;
                        }

                        // Prepare next loop
                        src_anchor = src_p++;
                        h_fwd = (*(uint*)src_p * 2654435761u) >> HASH64K_ADJUST;
                    }

                _last_literals:

                    // EncodeSequence Last Literals
                    {
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

                        BlockCopy32(src_anchor, dst_p, (int)(src_end - src_anchor));
                        dst_p += src_end - src_anchor;
                    }

                    // End
                    return (int)(dst_p - dst);
                }
            }
        }


        /// <summary>
        /// 使用LZ4算法解压数据。
        /// </summary>
        /// <param name="src">指向源数据的指针。</param>
        /// <param name="dst">指向目标数据的指针。</param>
        /// <param name="dst_len">目标数据的长度。</param>
        /// <returns>解压后的字节数。如果返回负值，则表示发生错误。</returns>
        private static unsafe int LZ4_uncompress_32(
            byte* src,
            byte* dst,
            int dst_len)
        {
            unchecked
            {
                fixed (int* dec32table = &DECODER_TABLE_32[0])
                {
                    // r93
                    var src_p = src;
                    byte* xxx_ref;

                    var dst_p = dst;
                    var dst_end = dst_p + dst_len;
                    byte* dst_cpy;

                    var dst_LASTLITERALS = dst_end - LASTLITERALS;
                    var dst_COPYLENGTH = dst_end - COPYLENGTH;
                    var dst_COPYLENGTH_STEPSIZE_4 = dst_end - COPYLENGTH - (STEPSIZE_32 - 4);

                    uint xxx_token;

                    // Main Loop
                    while (true)
                    {
                        int length;

                        // get runlength
                        xxx_token = *src_p++;
                        if ((length = (int)(xxx_token >> ML_BITS)) == RUN_MASK)
                        {
                            int len;
                            for (; (len = *src_p++) == 255; length += 255)
                            {
                                /* do nothing */
                            }

                            length += len;
                        }

                        // copy literals
                        dst_cpy = dst_p + length;

                        if (dst_cpy > dst_COPYLENGTH)
                        {
                            if (dst_cpy != dst_end)
                            {
                                goto _output_error; // Error : not enough place for another match (min 4) + 5 literals
                            }

                            BlockCopy32(src_p, dst_p, length);
                            src_p += length;
                            break; // EOF
                        }

                        do
                        {
                            *(uint*)dst_p = *(uint*)src_p;
                            dst_p += 4;
                            src_p += 4;
                            *(uint*)dst_p = *(uint*)src_p;
                            dst_p += 4;
                            src_p += 4;
                        }
                        while (dst_p < dst_cpy);
                        src_p -= dst_p - dst_cpy;
                        dst_p = dst_cpy;

                        // get offset
                        xxx_ref = dst_cpy - (*(ushort*)src_p);
                        src_p += 2;
                        if (xxx_ref < dst)
                        {
                            goto _output_error; // Error : offset outside destination Buffer
                        }

                        // get matchlength
                        if ((length = (int)(xxx_token & ML_MASK)) == ML_MASK)
                        {
                            for (; *src_p == 255; length += 255)
                            {
                                src_p++;
                            }

                            length += *src_p++;
                        }

                        // copy repeated sequence
                        if ((dst_p - xxx_ref) < STEPSIZE_32)
                        {
                            const int dec64 = 0;

                            dst_p[0] = xxx_ref[0];
                            dst_p[1] = xxx_ref[1];
                            dst_p[2] = xxx_ref[2];
                            dst_p[3] = xxx_ref[3];
                            dst_p += 4;
                            xxx_ref += 4;
                            xxx_ref -= dec32table[dst_p - xxx_ref];
                            *(uint*)dst_p = *(uint*)xxx_ref;
                            dst_p += STEPSIZE_32 - 4;
                            xxx_ref -= dec64;
                        }
                        else
                        {
                            *(uint*)dst_p = *(uint*)xxx_ref;
                            dst_p += 4;
                            xxx_ref += 4;
                        }

                        dst_cpy = dst_p + length - (STEPSIZE_32 - 4);

                        if (dst_cpy > dst_COPYLENGTH_STEPSIZE_4)
                        {
                            if (dst_cpy > dst_LASTLITERALS)
                            {
                                goto _output_error; // Error : last 5 bytes must be literals
                            }

                            {
                                do
                                {
                                    *(uint*)dst_p = *(uint*)xxx_ref;
                                    dst_p += 4;
                                    xxx_ref += 4;
                                    *(uint*)dst_p = *(uint*)xxx_ref;
                                    dst_p += 4;
                                    xxx_ref += 4;
                                }
                                while (dst_p < dst_COPYLENGTH);
                            }

                            while (dst_p < dst_cpy)
                            {
                                *dst_p++ = *xxx_ref++;
                            }

                            dst_p = dst_cpy;
                            continue;
                        }

                        do
                        {
                            *(uint*)dst_p = *(uint*)xxx_ref;
                            dst_p += 4;
                            xxx_ref += 4;
                            *(uint*)dst_p = *(uint*)xxx_ref;
                            dst_p += 4;
                            xxx_ref += 4;
                        }
                        while (dst_p < dst_cpy);
                        dst_p = dst_cpy; // correction
                    }

                    // end of decoding
                    return (int)(src_p - src);

                // write overflow error detected
                _output_error:
                    return (int)-(src_p - src);
                }
            }
        }

        /// <summary>
        /// 使用指针方式复制内存块，每次复制4个字节、2个字节或1个字节。
        /// </summary>
        /// <param name="src">源指针</param>
        /// <param name="dst">目标指针</param>
        /// <param name="len">要复制的长度（以字节为单位）</param>
        /// <remarks>
        /// 该方法使用不安全代码，通过指针直接操作内存。
        /// 首先以4个字节为单位进行复制，如果剩余长度不足4个字节，则以2个字节为单位进行复制，
        /// 最后如果仍有剩余字节，则以1个字节为单位进行复制。
        /// </remarks>
        private static unsafe void BlockCopy32(byte* src, byte* dst, int len)
        {
            while (len >= 4)
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
