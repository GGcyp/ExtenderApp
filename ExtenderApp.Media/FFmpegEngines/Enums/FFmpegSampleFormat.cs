namespace ExtenderApp.Media.FFmpegEngines
{
    /// <summary>
    /// FFmpeg 音频采样格式枚举。 对应 FFmpeg 的
    /// AVSampleFormat，定义了常见的音频采样数据类型，
    /// 包括整型、浮点型、双精度型及其平面（planar）和交错（packed）存储方式。 用于音频解码、重采样、格式转换等场景。
    /// </summary>
    public enum FFmpegSampleFormat
    {
        SAMPLE_FMT_NONE = -1,

        /// <summary>
        /// 无符号8位整型（交错）
        /// </summary>
        SAMPLE_FMT_U8 = 0,

        /// <summary>
        /// 有符号16位整型（交错）
        /// </summary>
        SAMPLE_FMT_S16 = 1,

        /// <summary>
        /// 有符号32位整型（交错）
        /// </summary>
        SAMPLE_FMT_S32 = 2,

        /// <summary>
        /// 32位浮点型（交错）
        /// </summary>
        SAMPLE_FMT_FLT = 3,

        /// <summary>
        /// 64位双精度浮点型（交错）
        /// </summary>
        SAMPLE_FMT_DBL = 4,

        /// <summary>
        /// 无符号8位整型（平面）
        /// </summary>
        SAMPLE_FMT_U8P = 5,

        /// <summary>
        /// 有符号16位整型（平面）
        /// </summary>
        SAMPLE_FMT_S16P = 6,

        /// <summary>
        /// 有符号32位整型（平面）
        /// </summary>
        SAMPLE_FMT_S32P = 7,

        /// <summary>
        /// 32位浮点型（平面）
        /// </summary>
        SAMPLE_FMT_FLTP = 8,

        /// <summary>
        /// 64位双精度浮点型（平面）
        /// </summary>
        SAMPLE_FMT_DBLP = 9,

        /// <summary>
        /// 有符号64位整型（交错）
        /// </summary>
        SAMPLE_FMT_S64 = 10,

        /// <summary>
        /// 有符号64位整型（平面）
        /// </summary>
        SAMPLE_FMT_S64P = 11,

        /// <summary>
        /// 采样格式数量（仅用于内部计数，动态链接时请勿使用）
        /// </summary>
        SAMPLE_FMT_NB = 12,
    }
}