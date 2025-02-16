using ExtenderApp.Abstract;
using ExtenderApp.Common.DataBuffers;

namespace ExtenderApp.Common.IO.Binaries
{
    /// <summary>
    /// 二进制读取操作类，继承自FileStreamOperation类。
    /// </summary>
    internal class BinaryReadOperation : FileStreamOperation
    {
        /// <summary>
        /// 处理函数，用于处理文件流并返回DataBuffer对象
        /// </summary>
        /// <remarks>
        /// 该函数是一个可选的委托，用于对文件流进行处理，并返回一个DataBuffer对象。
        /// 如果不设置该函数，则默认为null。
        /// </remarks>
        private Func<FileStream, DataBuffer>? processFunc;

        private Action<Stream, DataBuffer>? processAction;

        /// <summary>
        /// 获取或设置数据缓冲区。
        /// </summary>
        public DataBuffer Data { get; set; }

        /// <summary>
        /// 初始化BinaryReadOperation类的新实例。
        /// </summary>
        /// <param name="releaseAction">释放操作的动作，类型为Action<IConcurrentOperation>。</param>
        public BinaryReadOperation(Action<IConcurrentOperation> releaseAction) : base(releaseAction)
        {
            processFunc = null;
        }

        /// <summary>
        /// 执行文件流操作。
        /// </summary>
        /// <param name="stream">文件流对象。</param>
        public override void Execute(FileStream stream)
        {
            processAction?.Invoke(stream, Data);
            Data = processFunc?.Invoke(stream);
        }

        /// <summary>
        /// 设置回调方法。
        /// </summary>
        /// <param name="func">类型为Action<Stream>的回调方法。</param>
        public void Set(Func<FileStream, DataBuffer> func)
        {
            processFunc = func;
        }

        /// <summary>
        /// 设置处理数据的动作。
        /// </summary>
        /// <param name="action">处理数据的动作。</param>
        public void Set(Action<Stream, DataBuffer> action)
        {
            processAction = action;
        }

        /// <summary>
        /// 尝试重置操作。
        /// </summary>
        /// <returns>如果成功重置，则返回true；否则返回false。</returns>
        public override bool TryReset()
        {
            processFunc = null;
            Data = null;
            processAction = null;
            return true;
        }
    }
}
