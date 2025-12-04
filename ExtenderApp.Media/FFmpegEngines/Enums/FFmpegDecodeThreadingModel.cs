using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExtenderApp.FFmpegEngines
{
    /// <summary>
    /// 解码线程模型枚举。
    /// </summary>
    public enum FFmpegDecodeThreadingModel
    {
        /// <summary>
        /// 自动选择线程模型，由解码器根据具体情况决定使用何种线程模型。
        /// </summary>
        Auto,

        /// <summary>
        /// 所有解码任务均在单一线程中执行。
        /// </summary>
        Single,

        /// <summary>
        /// 解复用+音频在1个线程，视频在另1个线程
        /// </summary>
        Hybrid,

        /// <summary>
        /// 解复用、音频、视频分别在独立线程
        /// </summary>
        Multi
    }
}