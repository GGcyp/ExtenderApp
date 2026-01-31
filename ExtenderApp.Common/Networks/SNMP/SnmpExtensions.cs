using System.Formats.Asn1;
using System.Text;

using ExtenderApp.Common.Encodings;
using ExtenderApp.Data;

namespace ExtenderApp.Common.Networks.SNMP
{
    /// <summary>
    /// 提供针对 <see cref="SnmpPdu"/> 的扩展方法，便于检查错误、获取错误描述以及管理 VarBind 列表等常用操作。
    /// </summary>
    public static class SnmpExtensions
    {
        /// <summary>
        /// 判断当前 PDU 是否表示一个错误响应（基于 <see cref="SnmpPdu.ErrorStatus"/> 字段）。
        /// </summary>
        /// <param name="pdu">要检查的 PDU 实例。</param>
        /// <returns>
        /// 当且仅当 <see cref="SnmpPdu.ErrorStatus"/>
        /// 不等于 <see
        /// cref="SnmpErrorStatus.NoError"/> 时返回 <c>true</c>。
        /// </returns>
        public static bool IsErrorResponse(this SnmpPdu pdu)
        {
            return pdu.ErrorStatus != SnmpErrorStatus.NoError;
        }

        /// <summary>
        /// 将 PDU 的错误状态映射为可读的错误消息字符串。
        /// </summary>
        /// <param name="pdu">包含错误状态的 PDU。</param>
        /// <returns>
        /// 针对常见 <see cref="SnmpErrorStatus"/>
        /// 值的描述性字符串；对于未知值返回 "未知错误。"
        /// </returns>
        public static string GetErrorMessage(this SnmpPdu pdu)
        {
            return pdu.ErrorStatus switch
            {
                SnmpErrorStatus.NoError => "没有错误。",
                SnmpErrorStatus.TooBig => "响应消息太大，无法传输。",
                SnmpErrorStatus.NoSuchName => "请求的 OID 不存在。",
                SnmpErrorStatus.BadValue => "提供的值不可接受。",
                SnmpErrorStatus.ReadOnly => "该变量为只读，无法修改。",
                SnmpErrorStatus.GenErr => "发生通用错误。",
                _ => "未知错误。"
            };
        }

        /// <summary>
        /// 向 PDU 的 VarBind 列表追加一个 <see cref="SnmpVarBind"/> 项。
        /// </summary>
        /// <param name="pdu">目标 PDU；其 <see cref="SnmpPdu.VarBinds"/> 应已初始化以便追加。</param>
        /// <param name="varBind">要追加的变量绑定项。</param>
        /// <exception cref="ArgumentNullException">当 <paramref name="pdu"/> 表示空 PDU 或不可用时抛出。</exception>
        public static void AddVarBind(this SnmpPdu pdu, SnmpVarBind varBind)
        {
            if (pdu.IsEmpty)
                throw new ArgumentNullException(nameof(pdu), "PDU不能为空");

            pdu.VarBinds.Add(varBind);
        }

        #region BER Encode Methods

        /// <summary>
        /// 将 <see cref="SnmpMessage"/> 按 BER/ASN.1 编码并追加写入目标 <see cref="ByteBlock"/>（外层 SEQUENCE）。
        /// </summary>
        /// <param name="message">要编码的 SNMP 消息。</param>
        /// <param name="block">目标字节块，编码结果追加到该块末尾。</param>
        public static void BEREncode(this SnmpMessage message, ref ByteBlock block)
        {
            ByteBlock valueBlock = new ByteBlock();
            BEREncoding.EncodeInteger(ref valueBlock, (int)message.Version);
            BEREncoding.EncodeOctetString(ref valueBlock, message.Community);
            message.Pdu.BEREncode(ref valueBlock);

            BEREncoding.EncodeSequence(ref block, valueBlock);
            valueBlock.Dispose();
        }

        /// <summary>
        /// 将 <see cref="SnmpPdu"/> 按 SNMP PDU 的编码规则（Application class + 内部 SEQUENCE）编码并写入目标块。
        /// </summary>
        /// <param name="pdu">要编码的 PDU。</param>
        /// <param name="block">目标字节块，编码结果追加到该块末尾。</param>
        public static void BEREncode(this SnmpPdu pdu, ref ByteBlock block)
        {
            ByteBlock valueBlock = new();
            BEREncoding.EncodeInteger(ref valueBlock, pdu.RequestId);
            BEREncoding.EncodeInteger(ref valueBlock, (int)pdu.ErrorStatus);
            BEREncoding.EncodeInteger(ref valueBlock, pdu.ErrorIndex);

            ByteBlock varBindsBlock = new();
            var list = pdu.VarBinds;
            if (list != null)
            {
                for (var i = 0; i < list.Count; i++)
                {
                    list[i].BEREncode(ref varBindsBlock);
                }
            }
            BEREncoding.EncodeSequence(ref valueBlock, varBindsBlock);

            BEREncoding.Encode(ref block, valueBlock, new Asn1Tag(TagClass.Application, (int)pdu.PduType, true));

            valueBlock.Dispose();
            varBindsBlock.Dispose();
        }

        /// <summary>
        /// 将单个 <see cref="SnmpVarBind"/>（OID + Value）编码为 SEQUENCE 并写入目标块。
        /// </summary>
        /// <param name="varBind">要编码的变量绑定项。</param>
        /// <param name="block">目标字节块，编码结果追加到该块末尾。</param>
        public static void BEREncode(this SnmpVarBind varBind, ref ByteBlock block)
        {
            ByteBlock valueBlock = new ByteBlock();
            varBind.Oid.BEREncode(ref valueBlock);
            varBind.Value.BEREncode(ref valueBlock);
            BEREncoding.EncodeSequence(ref block, valueBlock);
            valueBlock.Dispose();
        }

        /// <summary>
        /// 将 <see cref="SnmpOid"/> 的点分表示按 OBJECT IDENTIFIER 的 BER base-128 编码写入目标块（包含 Tag/Length/Content）。
        /// </summary>
        /// <param name="oid">要编码的 OID。</param>
        /// <param name="block">目标字节块，编码结果追加到该块末尾。</param>
        /// <exception cref="ArgumentException">当 OID 为空或包含非法字符时抛出。</exception>
        public static void BEREncode(this SnmpOid oid, ref ByteBlock block)
        {
            ReadOnlySpan<char> s = oid.Oid.AsSpan();
            if (s.Length == 0)
                throw new ArgumentException("OID 不能为空");

            // 1) 计算 components 数量并估算内容长度（不包含 Tag/Length）
            int pos = 0;
            int compIndex = 0;
            ulong first = 0, second = 0;
            int contentLen = 0;
            while (pos < s.Length)
            {
                ulong v = 0;
                while (pos < s.Length && s[pos] != '.')
                {
                    char c = s[pos++];
                    if (c < '0' || c > '9')
                        throw new ArgumentException("OID 含非法字符");

                    v = v * 10 + (ulong)(c - '0');
                }

                if (compIndex == 0) first = v;
                else if (compIndex == 1) second = v;
                else
                {
                    // 计算 base-128 所需字节数
                    if (v == 0) contentLen += 1;
                    else
                    {
                        ulong tv = v;
                        while (tv != 0)
                        {
                            contentLen++;
                            tv >>= 7;
                        }
                    }
                }
                compIndex++;
                if (pos < s.Length && s[pos] == '.') pos++;
            }

            if (compIndex < 2) throw new ArgumentException("OID 至少两个分量");

            // 第一个字节为 40*first + second
            contentLen += 1;

            // 2) 写 Tag (OBJECT IDENTIFIER = 0x06)
            // 与 Length（使用 BEREncoding 的写入函数）
            block.Write((byte)UniversalTagNumber.ObjectIdentifier); // tag 0x06
            BEREncoding.EncodeLength(ref block, contentLen);

            // 3) 再次扫描并写入内容：先写合并的第一个字节，然后写其余分量的
            // base-128 编码
            block.Write((byte)(checked((int)(first * 40 + second))));

            pos = 0;
            compIndex = 0;
            Span<byte> tmp = stackalloc byte[10];
            while (pos < s.Length)
            {
                ulong v = 0;
                while (pos < s.Length && s[pos] != '.')
                {
                    v = v * 10 + (ulong)(s[pos++] - '0');
                }

                if (compIndex >= 2)
                {
                    int tlen = 0;
                    if (v == 0)
                    {
                        tmp[0] = 0;
                        tlen = 1;
                    }
                    else
                    {
                        while (v != 0)
                        {
                            tmp[tlen++] = (byte)(v & 0x7F);
                            v >>= 7;
                        }
                    }

                    for (int i = tlen - 1; i >= 0; i--)
                    {
                        byte b = tmp[i];
                        if (i != 0) b |= 0x80;
                        block.Write(b);
                    }
                }

                compIndex++;
                if (pos < s.Length && s[pos] == '.') pos++;
            }
        }

        /// <summary>
        /// 将 <see cref="SnmpValue"/> 按其内部类型编码写入目标块（支持 Null、Integer、OctetString、Boolean、ObjectIdentifier）。
        /// </summary>
        /// <param name="value">要编码的 SNMP 值。</param>
        /// <param name="block">目标字节块，编码结果追加到该块末尾。</param>
        public static void BEREncode(this SnmpValue value, ref ByteBlock block)
        {
            if (value.IsEmpty)
            {
                BEREncoding.EncodeNull(ref block);
                return;
            }

            switch (value.Type)
            {
                case UniversalTagNumber.Null:
                    BEREncoding.EncodeNull(ref block);
                    break;

                case UniversalTagNumber.Integer:
                    BEREncoding.EncodeInteger(ref block, value.GetValue<long>());
                    break;

                case UniversalTagNumber.OctetString:
                    if (value.Buffer is DataBuffer<string> strBuffer)
                    {
                        BEREncoding.EncodeOctetString(ref block, strBuffer.Item1!);
                    }
                    else if (value.Buffer is DataBuffer<ByteBlock> byteBlockBuffer)
                    {
                        BEREncoding.Encode(ref block, byteBlockBuffer.Item1!, Asn1Tag.PrimitiveOctetString);
                    }
                    else
                    {
                        throw new InvalidCastException("无法将 SnmpValue 的值转换为字符串以进行 OctetString 编码。");
                    }
                    break;

                case UniversalTagNumber.Boolean:
                    BEREncoding.EncodeBoolean(ref block, value.GetValue<bool>());
                    break;

                case UniversalTagNumber.ObjectIdentifier:
                    value.GetValue<SnmpOid>().BEREncode(ref block);
                    break;
            }
        }

        /// <summary>
        /// 从 <see cref="SnmpValue"/> 中以指定类型获取值（若类型不匹配抛出异常）。
        /// </summary>
        /// <typeparam name="T">期望的值类型。</typeparam>
        /// <param name="snmpValue">源 SnmpValue。</param>
        /// <returns>缓冲内实际存储的值。</returns>
        /// <exception cref="InvalidOperationException">当 <paramref name="snmpValue"/> 为空时抛出。</exception>
        /// <exception cref="InvalidCastException">当内部缓冲与请求类型不匹配时抛出。</exception>
        public static T GetValue<T>(this SnmpValue snmpValue)
        {
            if (snmpValue.IsEmpty)
                throw new InvalidOperationException("SnmpValue 为空，无法获取值。");
            if (snmpValue.Buffer is DataBuffer<T> dataBuffer)
            {
                return dataBuffer.Item1!;
            }
            else
            {
                throw new InvalidCastException($"无法将 SnmpValue 的值转换为类型 {typeof(T).FullName}。");
            }
        }

        #endregion

        #region BER Decode Methods

        /// <summary>
        /// 尝试从 <see cref="ByteBlock"/> 解析一个完整的 SNMP Message（外层 SEQUENCE 包含 version/community/pdu）。
        /// </summary>
        /// <param name="block">包含待解析数据的字节块（引用传入）。</param>
        /// <param name="message">解析成功时输出的 <see cref="SnmpMessage"/> 实例。</param>
        /// <returns>成功解析返回 <c>true</c>；否则返回 <c>false</c>（通常不会推进读取位置）。</returns>
        public static bool TryBERDecode(ref ByteBlock block, out SnmpMessage message, SnmpPduType pduType = SnmpPduType.GetRequest)
        {
            message = default;
            if (!BEREncoding.TryDecodeSequence(ref block))
                return false;

            SnmpVersionType versionType = (SnmpVersionType)BEREncoding.DecodeInteger(ref block);
            if (!BEREncoding.TryDecodeOctetString(ref block, out ByteBlock communityBlock))
                return false;

            string community = communityBlock.ReadString(Encoding.ASCII);
            if (!TryBERDecode(ref block, out SnmpPdu pdu, pduType))
                return false;

            message = new SnmpMessage(pdu, versionType, community);
            return true;
        }

        /// <summary>
        /// 尝试从 <see cref="ByteBlock"/> 解析一个 PDU（包含 request-id/error-status/error-index/varbindList）。
        /// </summary>
        /// <param name="block">包含待解析数据的字节块（引用传入）。</param>
        /// <param name="pdu">解析成功时输出的 <see cref="SnmpPdu"/> 实例。</param>
        /// <returns>成功解析返回 <c>true</c>；否则返回 <c>false</c>（通常不会推进读取位置）。</returns>
        public static bool TryBERDecode(ref ByteBlock block, out SnmpPdu pdu, SnmpPduType pduType = SnmpPduType.GetRequest)
        {
            pdu = default;
            if (!TryBERDecodeSequence(ref block, pduType))
                return false;

            int requestId = (int)BEREncoding.DecodeInteger(ref block);
            SnmpErrorStatus errorStatus = (SnmpErrorStatus)BEREncoding.DecodeInteger(ref block);
            int errorIndex = (int)BEREncoding.DecodeInteger(ref block);

            if (!BEREncoding.TryDecodeSequence(ref block))
                return false;

            pdu = new(requestId, errorStatus, errorIndex);

            while (true)
            {
                if (TryBERDecode(ref block, out SnmpVarBind varBind))
                {
                    pdu.AddVarBind(varBind);
                }
                else
                {
                    break;
                }
            }

            return true;
        }

        /// <summary>
        /// 尝试从 <see cref="ByteBlock"/> 解析一个 VarBind（OID + Value）。
        /// </summary>
        /// <param name="block">包含待解析数据的字节块（引用传入）。</param>
        /// <param name="varBind">解析成功时输出的 <see cref="SnmpVarBind"/> 实例。</param>
        /// <returns>成功解析返回 <c>true</c>；否则返回 <c>false</c>（通常不会推进读取位置）。</returns>
        public static bool TryBERDecode(ref ByteBlock block, out SnmpVarBind varBind)
        {
            varBind = default;
            if (!BEREncoding.TryDecodeSequence(ref block))
                return false;
            if (!TryBERDecode(ref block, out SnmpOid oid))
                return false;
            if (!TryBERDecode(ref block, out SnmpValue value))
                return false;
            varBind = new SnmpVarBind(oid, value);
            return true;
        }

        /// <summary>
        /// 尝试从 <see cref="ByteBlock"/> 解析一个 OBJECT IDENTIFIER 并构造 <see cref="SnmpOid"/>。
        /// </summary>
        /// <param name="block">包含待解析数据的字节块（引用传入）。</param>
        /// <param name="snmpOid">解析成功时输出的 <see cref="SnmpOid"/> 实例。</param>
        /// <returns>成功解析返回 <c>true</c>；否则返回 <c>false</c>（通常不会推进读取位置）。</returns>
        public static bool TryBERDecode(ref ByteBlock block, out SnmpOid snmpOid)
        {
            // 读取并校验 Tag
            snmpOid = default;
            var tempBlock = block.PeekByteBlock();
            var tag = BEREncoding.DecodeTag(ref tempBlock);
            if (tag.ToUTagNumber() != UniversalTagNumber.ObjectIdentifier || tag.IsConstructed)
                return false;

            // 读取长度并获取内容 bytes
            int length = BEREncoding.DecodeLength(ref tempBlock);
            if (length < 1)
                return false;

            block.ReadAdvance(tempBlock.Consumed - block.Consumed);

            ReadOnlySpan<byte> data = block.Read(length);
            if (data.Length != length)
                throw new System.IO.InvalidDataException("数据长度不足，无法读取完整 OID 内容");

            // 解析内容：第一个字节合并为 first 和 second，其余为
            // base-128 编码分量
            int pos = 0;
            byte firstByte = data[pos++];
            int first = firstByte / 40;
            int second = firstByte % 40;

            var sb = new System.Text.StringBuilder();
            sb.Append(first).Append('.').Append(second);

            while (pos < data.Length)
            {
                ulong value = 0;
                bool seenAny = false;
                while (pos < data.Length)
                {
                    byte b = data[pos++];
                    seenAny = true;
                    value = (value << 7) | (ulong)(b & 0x7F);
                    // 高位为0表示当前整数结束
                    if ((b & 0x80) == 0) break;
                    // 若到达末尾但未见结束位，判定为格式错误
                    if (pos == data.Length && (b & 0x80) != 0)
                        throw new System.IO.InvalidDataException("OID base-128 编码不完整");
                }
                if (!seenAny)
                    break;
                sb.Append('.').Append(value);
            }

            snmpOid = new SnmpOid(sb.ToString());
            return true;
        }

        /// <summary>
        /// 尝试从 <see cref="ByteBlock"/> 当前读取位置解析一个 BER 编码的 SNMP 值（自动识别常见类型）。
        /// </summary>
        /// <param name="block">包含待解析数据的字节块（引用传入）。</param>
        /// <param name="snmpValue">解析成功时输出的 <see cref="SnmpValue"/> 实例。</param>
        /// <returns>若当前位置为可识别的 SNMP 值并成功解析，返回 <c>true</c>；否则返回 <c>false</c>（通常不推进读取位置）。</returns>
        public static bool TryBERDecode(ref ByteBlock block, out SnmpValue snmpValue)
        {
            snmpValue = default;
            if (block.IsEmpty)
                return false;

            // 先预览 Tag/Length，决定处理分支且在需要时推进原始块的读取位置。
            var temp = block.PeekByteBlock();
            var tag = BEREncoding.DecodeTag(ref temp);

            int len = 0;
            ReadOnlySpan<byte> data = default!;
            var tagNumber = tag.ToUTagNumber();
            switch (tagNumber)
            {
                case UniversalTagNumber.Null:
                    len = BEREncoding.DecodeLength(ref temp);
                    block.ReadAdvance(temp.Consumed - block.Consumed);
                    if (len > 0)
                        block.Read(len); // 跳过可能存在的值字节（规范上应为0）
                    snmpValue = SnmpValue.Empty;
                    break;

                case UniversalTagNumber.Integer:
                    snmpValue = new SnmpValue(UniversalTagNumber.Integer, DataBuffer<long>.Get(BEREncoding.DecodeInteger(ref block)));
                    break;

                case UniversalTagNumber.OctetString:
                    len = BEREncoding.DecodeLength(ref temp);
                    block.ReadAdvance(temp.Consumed - block.Consumed);
                    if (block.Remaining < len)
                        throw new System.IO.InvalidDataException("数据长度不足，无法读取完整 OctetString 内容");
                    data = block.Read(len);
                    var bb = new ByteBlock(data);
                    snmpValue = new SnmpValue(UniversalTagNumber.OctetString, DataBuffer<ByteBlock>.Get(bb));
                    break;
                //case BEREncoding.ObjectIdentifierTag:
                //    if (!SnmpOid.TryBERDecodeSequence(ref block, out var oid))
                //        return false;
                //    // 将 OID 的点分字符串以 UTF8 存入 ByteBlock 作为值的表示（调用方可按需求转换）
                //    var oidBytes = Encoding.UTF8.GetBytes(oid.Oid);
                //    var ob = new ByteBlock(oidBytes);
                //    snmpValue = new SnmpValue(SnmpDataType.ObjectIdentifier, DataBuffer<ByteBlock>.Get(ob));
                //    break;
                case UniversalTagNumber.Sequence:
                    if (!tag.IsConstructed)
                        return false;

                    len = BEREncoding.DecodeLength(ref temp);
                    block.ReadAdvance(temp.Consumed - block.Consumed);
                    data = block.Read(len);
                    if (data.Length != len)
                        throw new System.IO.InvalidDataException("数据长度不足，无法读取完整 Sequence 内容");

                    var sb = new ByteBlock(data);
                    snmpValue = new SnmpValue(UniversalTagNumber.Sequence, DataBuffer<ByteBlock>.Get(sb));
                    break;
            }
            return true;
        }

        public static bool TryBERDecodeSequence(ref ByteBlock block, SnmpPduType type)
        {
            var temp = block.PeekByteBlock();
            if (block.Remaining == 0)
                return false;

            var tag = BEREncoding.DecodeTag(ref temp);
            if (tag.TagValue != (int)type || !tag.IsConstructed)
                return false;

            block.ReadAdvance(temp.Consumed - block.Consumed);
            int length = BEREncoding.DecodeLength(ref block);
            if (block.Remaining < length)
                return false;

            return true;
        }

        #endregion
    }
}