namespace ExtenderApp.FFmpegEngines
{
    /// <summary>
    /// FFmpeg 流媒体输入选项预设。
    /// <para>
    /// 这些选项会通过 <c>AVDictionary</c> 传给 FFmpeg 的 <c>avformat_open_input</c>/<c>avformat_find_stream_info</c>，
    /// 用于影响“连接/超时/探测/缓冲/低延迟/重连”等行为。
    /// </para>
    /// <para>
    /// 说明：
    /// <list type="bullet">
    /// <item><description>多数超时参数单位是“微秒（us）”。</description></item>
    /// <item><description><c>probesize</c> 单位是“字节（bytes）”。</description></item>
    /// <item><description>并非所有选项对所有协议都生效；应按协议选用。</description></item>
    /// </list>
    /// </para>
    /// </summary>
    public static class FFmpegStreamingOptions
    {
        /// <summary>
        /// 默认网络超时（微秒）。
        /// <para>5 秒。</para>
        /// </summary>
        private const long DefaultTimeoutUs = 5_000_000;

        /// <summary>
        /// RTSP（稳定模式）：更偏向“不断流/可用性”。
        /// <para>
        /// 策略：优先使用 TCP 传输、设置合理超时、允许较充分的探测（更容易拿到完整流信息与首帧）。
        /// </para>
        /// <para>
        /// 典型应用：监控摄像头、NVR、对弱网容忍度要求更高的场景。
        /// </para>
        /// </summary>
        /// <param name="timeoutUs">连接/读写超时（微秒）。默认 5 秒。</param>
        /// <returns>用于传入 FFmpeg 的 options 字典。</returns>
        public static Dictionary<string, string> RtspStable(long timeoutUs = DefaultTimeoutUs)
        {
            return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                /// RTSP 传输层协议。
                ["rtsp_transport"] = "tcp",
                /// RTSP 打开阶段超时（微秒）。
                ["stimeout"] = timeoutUs.ToString(),
                /// 读写超时（微秒）。
                ["rw_timeout"] = timeoutUs.ToString(),
                /// 探测时长（微秒）。
                ["analyzeduration"] = "2000000",
                /// 探测读取的最大数据量（字节）。
                ["probesize"] = "2000000",
                /// 最大解复用延迟（微秒）。
                ["max_delay"] = "500000",
            };
        }

        /// <summary>
        /// RTSP（低延迟模式）：更偏向“实时性”。
        /// <para>
        /// 策略：尽量关闭/减少缓冲与探测，从而更快出画面、降低整体延迟。
        /// </para>
        /// <para>
        /// 风险：更可能出现花屏、首帧取流失败、音画不同步或更容易断流。
        /// </para>
        /// </summary>
        /// <param name="timeoutUs">连接/读写超时（微秒）。默认 5 秒。</param>
        /// <returns>用于传入 FFmpeg 的 options 字典。</returns>
        public static Dictionary<string, string> RtspLowLatency(long timeoutUs = DefaultTimeoutUs)
        {
            return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                /// RTSP 传输层协议。
                ["rtsp_transport"] = "tcp",
                /// RTSP 打开阶段超时（微秒）。
                ["stimeout"] = timeoutUs.ToString(),
                /// 读写超时（微秒）。
                ["rw_timeout"] = timeoutUs.ToString(),
                /// fflags：格式层标志。
                ["fflags"] = "nobuffer",
                /// flags：通用低延迟提示。
                ["flags"] = "low_delay",
                /// 最大解复用延迟（微秒）。
                ["max_delay"] = "0",
                /// 探测时长（微秒）。
                ["analyzeduration"] = "0",
                /// 探测最大数据量（字节）。
                ["probesize"] = "32768",
            };
        }

        /// <summary>
        /// HLS(m3u8)/HTTP（稳定模式）：更偏向“可用性/抗断线”。
        /// <para>
        /// 策略：启用重连并使用合理的超时设置；探测相对充分以获得更完整的流信息。
        /// </para>
        /// <para>
        /// 说明：HLS 本身通常具有分片缓冲，天然延迟较高；此模式主要提高稳定性。
        /// </para>
        /// </summary>
        /// <param name="timeoutUs">读写超时（微秒）。默认 5 秒。</param>
        /// <returns>用于传入 FFmpeg 的 options 字典。</returns>
        public static Dictionary<string, string> HttpHlsStable(long timeoutUs = DefaultTimeoutUs)
        {
            return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                /// 读写超时（微秒）。
                ["rw_timeout"] = timeoutUs.ToString(),
                /// 断线自动重连。
                ["reconnect"] = "1",
                /// 对“流式”输入也启用重连（HLS/HTTP 直播常用）。
                ["reconnect_streamed"] = "1",
                /// 最大重连延迟（秒）。
                ["reconnect_delay_max"] = "2",
                /// 探测时长（微秒）。
                ["analyzeduration"] = "2000000",
                /// 探测最大数据量（字节）。
                ["probesize"] = "2000000",

                // 某些站点会校验 UA；需要时再启用
                // ["user_agent"] = "ExtenderApp/1.0",
            };
        }

        /// <summary>
        /// HLS(m3u8)/HTTP（低延迟模式）：更偏向“更快开始/更低缓冲”。
        /// <para>
        /// 策略：保留重连，但减少探测与缓冲；用于尽快开始播放。
        /// </para>
        /// <para>
        /// 注意：HLS 的延迟主要来自分片策略，本选项只能降低播放器侧额外缓冲，无法从根本上做到“近实时”。
        /// </para>
        /// </summary>
        /// <param name="timeoutUs">读写超时（微秒）。默认 5 秒。</param>
        /// <returns>用于传入 FFmpeg 的 options 字典。</returns>
        public static Dictionary<string, string> HttpHlsLowLatency(long timeoutUs = DefaultTimeoutUs)
        {
            return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["rw_timeout"] = timeoutUs.ToString(),

                ["reconnect"] = "1",
                ["reconnect_streamed"] = "1",
                ["reconnect_delay_max"] = "1",

                /// 尽量减少缓冲（低延迟倾向）。
                ["fflags"] = "nobuffer",
                /// 低延迟提示。
                ["flags"] = "low_delay",
                /// 探测时长（微秒）。设为 0 以减少首帧等待。
                ["analyzeduration"] = "0",
                /// 探测最大数据量（字节）。设小以减少首帧等待。
                ["probesize"] = "32768",
            };
        }

        /// <summary>
        /// RTMP（稳定模式）：RTMP 本身基于 TCP，稳定性通常较好。
        /// <para>
        /// 策略：设置超时并允许相对充分的探测，以降低“拿不到流信息/首帧异常”的概率。
        /// </para>
        /// </summary>
        /// <param name="timeoutUs">读写超时（微秒）。默认 5 秒。</param>
        /// <returns>用于传入 FFmpeg 的 options 字典。</returns>
        public static Dictionary<string, string> RtmpStable(long timeoutUs = DefaultTimeoutUs)
        {
            return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["rw_timeout"] = timeoutUs.ToString(),

                ["analyzeduration"] = "2000000",
                ["probesize"] = "2000000",
            };
        }

        /// <summary>
        /// RTMP（低延迟模式）：减少缓冲与探测以加快播放启动。
        /// <para>
        /// 风险：探测不足可能导致音视频流识别不完整，或在某些源上出现播放不稳定。
        /// </para>
        /// </summary>
        /// <param name="timeoutUs">读写超时（微秒）。默认 5 秒。</param>
        /// <returns>用于传入 FFmpeg 的 options 字典。</returns>
        public static Dictionary<string, string> RtmpLowLatency(long timeoutUs = DefaultTimeoutUs)
        {
            return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["rw_timeout"] = timeoutUs.ToString(),
                ["fflags"] = "nobuffer",
                ["flags"] = "low_delay",
                ["analyzeduration"] = "0",
                ["probesize"] = "32768",
            };
        }

        /// <summary>
        /// SRT（稳定模式）：SRT 更偏向“弱网可用 + 低延迟”的可靠 UDP。
        /// <para>
        /// 说明：SRT 的关键参数通常在 URL query 中配置（如 latency/rcvlatency/peerlatency），
        /// 此处主要提供通用的读写超时。
        /// </para>
        /// </summary>
        /// <param name="timeoutUs">读写超时（微秒）。默认 5 秒。</param>
        /// <returns>用于传入 FFmpeg 的 options 字典。</returns>
        public static Dictionary<string, string> SrtStable(long timeoutUs = DefaultTimeoutUs)
        {
            return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["rw_timeout"] = timeoutUs.ToString(),

                // SRT 的关键“延迟/纠错窗口”通常写在 URL query：
                // srt://ip:port?mode=caller&latency=200000&rcvlatency=200000&peerlatency=200000
            };
        }

        /// <summary>
        /// UDP/RTP（低延迟典型）：适合局域网内追求最低延迟的场景。
        /// <para>
        /// 风险：UDP 无重传；丢包会直接导致马赛克/卡顿/音画异常。
        /// </para>
        /// </summary>
        /// <param name="timeoutUs">读写超时（微秒）。默认 5 秒。</param>
        /// <returns>用于传入 FFmpeg 的 options 字典。</returns>
        public static Dictionary<string, string> UdpLowLatency(long timeoutUs = DefaultTimeoutUs)
        {
            return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["rw_timeout"] = timeoutUs.ToString(),
                ["fflags"] = "nobuffer",
                ["flags"] = "low_delay",
                ["analyzeduration"] = "0",
                ["probesize"] = "32768",
            };
        }
    }
}