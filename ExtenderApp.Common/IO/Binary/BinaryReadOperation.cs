using System.IO.MemoryMappedFiles;
using ExtenderApp.Abstract;
using ExtenderApp.Common.DataBuffers;

namespace ExtenderApp.Common.IO.Binaries
{
    /// <summary>
    /// 二进制读取操作类，继承自FileStreamOperation类。
    /// </summary>
    internal class BinaryReadOperation : FileOperation
    {
        /// <summary>
        /// 处理函数，用于处理文件流并返回DataBuffer对象
        /// </summary>
        /// <remarks>
        /// 该函数是一个可选的委托，用于对文件流进行处理，并返回一个DataBuffer对象。
        /// 如果不设置该函数，则默认为null。
        /// </remarks>
        private Func<MemoryMappedViewAccessor, long, long, DataBuffer>? processFunc;

        /// <summary>
        /// 处理操作的委托
        /// </summary>
        /// <remarks>
        /// 该委托用于处理内存映射视图访问器、读取位置和数据缓冲区。
        /// </remarks>
        private Action<MemoryMappedViewAccessor, long, long, DataBuffer?>? processAction;

        /// <summary>
        /// 读取位置
        /// </summary>
        /// <remarks>
        /// 表示从内存映射文件中读取数据的起始位置。
        /// </remarks>
        private long readPosition;

        /// <summary>
        /// 读取长度
        /// </summary>
        /// <remarks>
        /// 表示从内存映射文件中读取数据的长度。
        /// </remarks>
        private long readLength;

        /// <summary>
        /// 获取或设置数据缓冲区。
        /// </summary>
        public DataBuffer? Data { get; set; }

        /// <summary>
        /// 初始化BinaryReadOperation类的新实例。
        /// </summary>
        /// <param name="releaseAction">释放操作的动作，类型为Action<IConcurrentOperation>。</param>
        public BinaryReadOperation(Action<IConcurrentOperation> releaseAction) : base(releaseAction)
        {
            processFunc = null;
        }

        ///// <summary>
        ///// 执行文件流操作。
        ///// </summary>
        ///// <param name="stream">文件流对象。</param>
        //public override void Execute(FileStream stream)
        //{
        //    stream.Seek(readPosition, SeekOrigin.Begin);
        //    processAction?.Invoke(stream, Data);
        //    Data = processFunc?.Invoke(stream);
        //}

        public override void Execute(MemoryMappedViewAccessor item)
        {
            readLength = readLength == -1 ? item.Capacity : readLength;
            Data = processFunc?.Invoke(item, readPosition, readLength);
            processAction?.Invoke(item, readPosition, readLength, Data);
        }

        /// <summary>
        /// 设置处理函数，默认位置为0。
        /// </summary>
        /// <param name="func">处理函数，用于处理MemoryMappedViewAccessor并返回一个DataBuffer对象。</param>
        public void Set(Func<MemoryMappedViewAccessor, long, long, DataBuffer> func)
        {
            Set(func, 0);
        }

        /// <summary>
        /// 设置处理函数，并指定读取位置。
        /// </summary>
        /// <param name="func">处理函数，用于处理MemoryMappedViewAccessor并返回一个DataBuffer对象。</param>
        /// <param name="position">读取位置。</param>
        public void Set(Func<MemoryMappedViewAccessor, long, long, DataBuffer> func, long position)
        {
            Set(func, position, -1);
        }

        /// <summary>
        /// 设置用于读取数据的函数和位置信息
        /// </summary>
        /// <param name="func">读取数据的函数，参数为 MemoryMappedViewAccessor 和两个 long 类型的起始位置和长度，返回值为 DataBuffer 类型</param>
        /// <param name="position">读取数据的起始位置</param>
        /// <param name="lenght">要读取的数据长度</param>
        public void Set(Func<MemoryMappedViewAccessor, long, long, DataBuffer> func, long position, int lenght)
        {
            processFunc = func;
            readPosition = position;
            readLength = lenght;
        }

        /// <summary>
        /// 设置处理动作，默认位置为0。
        /// </summary>
        /// <param name="action">处理动作，用于处理MemoryMappedViewAccessor和DataBuffer对象。</param>
        public void Set(Action<MemoryMappedViewAccessor, long, long, DataBuffer?> action)
        {
            Set(action, 0);
        }

        /// <summary>
        /// 设置处理动作，并指定读取位置。
        /// </summary>
        /// <param name="action">处理动作，用于处理MemoryMappedViewAccessor和DataBuffer对象。</param>
        /// <param name="position">读取位置。</param>
        public void Set(Action<MemoryMappedViewAccessor, long, long, DataBuffer?> action, long position)
        {
            Set(action, position, -1);
        }

        /// <summary>
        /// 设置操作函数和数据读取位置及长度
        /// </summary>
        /// <param name="action">操作函数，接收一个 MemoryMappedViewAccessor 对象，一个 long 类型的偏移量和一个 DataBuffer? 类型的缓冲区</param>
        /// <param name="position">数据读取的起始位置</param>
        /// <param name="lenght">数据读取的长度</param>
        public void Set(Action<MemoryMappedViewAccessor, long, long, DataBuffer?> action, long position, int lenght)
        {
            processAction = action;
            readPosition = position;
            readLength = lenght;
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
            readPosition = 0;
            return true;
        }
    }
}
