using System.Text;
using ExtenderApp.Data;


namespace ExtenderApp.Common.IO.Binaries
{
    public partial class BinaryConvert
    {
        /// <summary>
        /// 获取或设置二进制选项。
        /// </summary>
        public BinaryOptions BinaryOptions { get; set; }
        private BinaryCode BinaryCode => BinaryOptions.BinaryCode;
        private BinaryRang BinaryRang => BinaryOptions.BinaryRang;
        private DateTimeConstants DateTimeConstants => BinaryOptions.DateTimeConstants;
        private BinaryConvertDecoders Decoders { get; }
        public Encoding BinaryEncoding => BinaryOptions.BinaryEncoding;

        public BinaryConvert(BinaryOptions options)
        {
            BinaryOptions = options;
            Decoders = new BinaryConvertDecoders(this, options);
        }

        ///// <summary>
        ///// 将给定的消息包编码转换为对应的格式名称。
        ///// </summary>
        ///// <param name="code">消息包编码。</param>
        ///// <returns>对应的格式名称。</returns>
        //public string ToFormatName(byte code)
        //{
        //    return FormatNameTable[code];
        //}

        /// <summary>
        /// 检查给定的消息包编码是否表示可能包含符号的整数（即可能为负数）。
        /// </summary>
        /// <param name="code">消息包编码。</param>
        /// <returns>布尔值。</returns>
        public bool IsSignedInteger(byte code)
        {
            return (IsNegativeFixInt(code) ||
                code == BinaryCode.Int8 ||
                code == BinaryCode.Int16 ||
                code == BinaryCode.Int32 ||
                code == BinaryCode.Int64);
        }

        /// <summary>
        /// 检查给定的消息包编码是否属于正固定整数范围。
        /// </summary>
        /// <param name="code">消息包编码。</param>
        /// <returns>布尔值。</returns>
        public bool IsPositiveFixInt(byte code)
        {
            // 机器代码较长，使用额外的 `movzx` 指令。
            // return CheckBitmask(code, 0x80, MinFixInt); // msgpack spec: 0xxxxxxx

            // 由于数据类型限制，JIT 移除了第二个检查
            return code <= BinaryCode.MaxFixInt; // && code >= MinFixInt;
        }

        /// <summary>
        /// 检查给定的消息包编码是否属于负固定整数范围。
        /// </summary>
        /// <param name="code">消息包编码。</param>
        /// <returns>布尔值。</returns>
        public bool IsNegativeFixInt(byte code)
        {
            // 机器代码较长，使用额外的 `movzx`、`and` 指令。
            // return CheckBitmask(code, 0xe0, MinNegativeFixInt); // msgpack spec: 111xxxxx

            // 由于数据类型限制，JIT 移除了第二个检查
            return code >= BinaryCode.MinNegativeFixInt; // && code <= MaxNegativeFixInt;
        }

        /// <summary>
        /// 检查给定的消息包编码是否属于固定映射范围。
        /// </summary>
        /// <param name="code">消息包编码。</param>
        /// <returns>布尔值。</returns>
        public bool IsFixMap(byte code) => CheckBitmask(code, 0xf0, BinaryCode.MinFixMap);

        /// <summary>
        /// 检查给定的消息包编码是否属于固定数组范围。
        /// </summary>
        /// <param name="code">消息包编码。</param>
        /// <returns>布尔值。</returns>
        public bool IsFixArray(byte code) => CheckBitmask(code, 0xf0, BinaryCode.MinFixArray);

        /// <summary>
        /// 检查给定的消息包编码是否属于固定字符串范围。
        /// </summary>
        /// <param name="code">消息包编码。</param>
        /// <returns>布尔值。</returns>
        public bool IsFixStr(byte code) => CheckBitmask(code, 0xe0, BinaryCode.MinFixStr);

        /// <summary>
        /// 检查给定的消息包编码是否符合指定的位掩码。
        /// </summary>
        /// <param name="code">消息包编码。</param>
        /// <param name="bitmask">位掩码。</param>
        /// <param name="targetValue">目标值。</param>
        /// <returns>布尔值。</returns>
        private static bool CheckBitmask(byte code, byte bitmask, byte targetValue) => (code & bitmask) == targetValue;

        //private readonly BinaryType[] TypeLookupTable;
        //private readonly string[] FormatNameTable;

        ///// <summary>
        ///// 静态构造函数，初始化查找表和格式名称表。
        ///// </summary>
        //public BinaryCode()
        //{
        //    // 初始化查找表
        //    TypeLookupTable = new BinaryType[256];
        //    FormatNameTable = new string[256];

        //    for (int i = MinFixInt; i <= MaxFixInt; i++)
        //    {
        //        TypeLookupTable[i] = BinaryType.Integer;
        //        FormatNameTable[i] = "positive fixint";
        //    }

        //    for (int i = MinFixMap; i <= MaxFixMap; i++)
        //    {
        //        TypeLookupTable[i] = BinaryType.Map;
        //        FormatNameTable[i] = "fixmap";
        //    }

        //    for (int i = MinFixArray; i <= MaxFixArray; i++)
        //    {
        //        TypeLookupTable[i] = BinaryType.Array;
        //        FormatNameTable[i] = "fixarray";
        //    }

        //    for (int i = MinFixStr; i <= MaxFixStr; i++)
        //    {
        //        TypeLookupTable[i] = BinaryType.String;
        //        FormatNameTable[i] = "fixstr";
        //    }

        //    TypeLookupTable[Nil] = BinaryType.Nil;
        //    TypeLookupTable[NeverUsed] = BinaryType.Unknown;
        //    TypeLookupTable[False] = BinaryType.Boolean;
        //    TypeLookupTable[True] = BinaryType.Boolean;
        //    TypeLookupTable[Bin8] = BinaryType.Binary;
        //    TypeLookupTable[Bin16] = BinaryType.Binary;
        //    TypeLookupTable[Bin32] = BinaryType.Binary;
        //    TypeLookupTable[Ext8] = BinaryType.Extension;
        //    TypeLookupTable[Ext16] = BinaryType.Extension;
        //    TypeLookupTable[Ext32] = BinaryType.Extension;
        //    TypeLookupTable[Float32] = BinaryType.Float;
        //    TypeLookupTable[Float64] = BinaryType.Float;
        //    TypeLookupTable[UInt8] = BinaryType.Integer;
        //    TypeLookupTable[UInt16] = BinaryType.Integer;
        //    TypeLookupTable[UInt32] = BinaryType.Integer;
        //    TypeLookupTable[UInt64] = BinaryType.Integer;
        //    TypeLookupTable[Int8] = BinaryType.Integer;
        //    TypeLookupTable[Int16] = BinaryType.Integer;
        //    TypeLookupTable[Int32] = BinaryType.Integer;
        //    TypeLookupTable[Int64] = BinaryType.Integer;
        //    TypeLookupTable[FixExt1] = BinaryType.Extension;
        //    TypeLookupTable[FixExt2] = BinaryType.Extension;
        //    TypeLookupTable[FixExt4] = BinaryType.Extension;
        //    TypeLookupTable[FixExt8] = BinaryType.Extension;
        //    TypeLookupTable[FixExt16] = BinaryType.Extension;
        //    TypeLookupTable[Str8] = BinaryType.String;
        //    TypeLookupTable[Str16] = BinaryType.String;
        //    TypeLookupTable[Str32] = BinaryType.String;
        //    TypeLookupTable[Array16] = BinaryType.Array;
        //    TypeLookupTable[Array32] = BinaryType.Array;
        //    TypeLookupTable[Map16] = BinaryType.Map;
        //    TypeLookupTable[Map32] = BinaryType.Map;

        //    FormatNameTable[Nil] = "nil";
        //    FormatNameTable[NeverUsed] = "(never used)";
        //    FormatNameTable[False] = "false";
        //    FormatNameTable[True] = "true";
        //    FormatNameTable[Bin8] = "bin 8";
        //    FormatNameTable[Bin16] = "bin 16";
        //    FormatNameTable[Bin32] = "bin 32";
        //    FormatNameTable[Ext8] = "ext 8";
        //    FormatNameTable[Ext16] = "ext 16";
        //    FormatNameTable[Ext32] = "ext 32";
        //    FormatNameTable[Float32] = "float 32";
        //    FormatNameTable[Float64] = "float 64";
        //    FormatNameTable[UInt8] = "uint 8";
        //    FormatNameTable[UInt16] = "uint 16";
        //    FormatNameTable[UInt32] = "uint 32";
        //    FormatNameTable[UInt64] = "uint 64";
        //    FormatNameTable[Int8] = "int 8";
        //    FormatNameTable[Int16] = "int 16";
        //    FormatNameTable[Int32] = "int 32";
        //    FormatNameTable[Int64] = "int 64";
        //    FormatNameTable[FixExt1] = "fixext 1";
        //    FormatNameTable[FixExt2] = "fixext 2";
        //    FormatNameTable[FixExt4] = "fixext 4";
        //    FormatNameTable[FixExt8] = "fixext 8";
        //    FormatNameTable[FixExt16] = "fixext 16";
        //    FormatNameTable[Str8] = "str 8";
        //    FormatNameTable[Str16] = "str 16";
        //    FormatNameTable[Str32] = "str 32";
        //    FormatNameTable[Array16] = "array 16";
        //    FormatNameTable[Array32] = "array 32";
        //    FormatNameTable[Map16] = "map 16";
        //    FormatNameTable[Map32] = "map 32";

        //    for (int i = MinNegativeFixInt; i <= MaxNegativeFixInt; i++)
        //    {
        //        TypeLookupTable[i] = BinaryType.Integer;
        //        FormatNameTable[i] = "negative fixint";
        //    }
        //}

        ///// <summary>
        ///// 将给定的消息包编码转换为对应的二进制类型。
        ///// </summary>
        ///// <param name="code">消息包编码。</param>
        ///// <returns>对应的二进制类型。</returns>
        //public BinaryType ToMessagePackType(byte code)
        //{
        //    return TypeLookupTable[code];
        //}
    }
}
