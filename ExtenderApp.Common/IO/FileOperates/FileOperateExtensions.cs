using ExtenderApp.Abstract;
using ExtenderApp.Data;

namespace ExtenderApp.Common
{
    /// <summary>
    /// 为 <see cref="IFileOperate"/> 提供的扩展方法，简化常见的同步/异步读写操作（针对 <see cref="ByteBuffer"/> 与 <see cref="ByteBlock"/>）。
    /// </summary>
    public static class FileOperateExtensions
    {
        #region Read

        /// <summary>
        /// 从指定文件位置开始读取至文件末尾并返回到新的 <see cref="ByteBuffer"/> 中。
        /// </summary>
        /// <param name="fileOperate">文件操作接口。</param>
        /// <param name="buffer">输出：读取到的顺序缓冲。</param>
        /// <param name="position">开始读取的位置（字节偏移），默认为 0。</param>
        /// <returns>表示操作结果。</returns>
        public static Result Read(this IFileOperate fileOperate, out ByteBuffer buffer, long position = 0)
        {
            return fileOperate.Read((int)(fileOperate.Info.Length - position), out buffer, position);
        }

        /// <summary>
        /// 从指定文件位置读取固定长度到新的 <see cref="ByteBuffer"/> 中。
        /// </summary>
        /// <param name="fileOperate">文件操作接口。</param>
        /// <param name="length">要读取的字节数。</param>
        /// <param name="buffer">输出：读取到的顺序缓冲。</param>
        /// <param name="position">开始读取的位置（字节偏移），默认为 0。</param>
        /// <returns>操作结果；若发生异常则返回封装的异常结果并释放创建的缓冲。</returns>
        public static Result Read(this IFileOperate fileOperate, int length, out ByteBuffer buffer, long position = 0)
        {
            buffer = new();
            try
            {
                buffer = new();
                var result = fileOperate.Read(buffer.GetMemory(length), position);
                if (!result.IsSuccess)
                {
                    buffer.Dispose();
                    return result;
                }
                buffer.Advance(result);
                return result;
            }
            catch (Exception ex)
            {
                buffer.Dispose();
                return Result.FromException(ex);
            }
        }

        /// <summary>
        /// 从指定文件位置读取至文件末尾并返回到新的 <see cref="ByteBlock"/> 中（兼容旧缓冲类型）。
        /// </summary>
        /// <param name="fileOperate">文件操作接口。</param>
        /// <param name="block">输出：读取到的缓冲块。</param>
        /// <param name="position">开始读取的位置（字节偏移），默认为 0。</param>
        /// <returns>操作结果。</returns>
        public static Result Read(this IFileOperate fileOperate, out ByteBlock block, long position = 0)
        {
            return fileOperate.Read((int)(fileOperate.Info.Length - position), out block, position);
        }

        /// <summary>
        /// 从指定文件位置读取固定长度到新的 <see cref="ByteBlock"/> 中（兼容旧缓冲类型）。
        /// </summary>
        /// <param name="fileOperate">文件操作接口。</param>
        /// <param name="length">要读取的字节数。</param>
        /// <param name="block">输出：读取到的缓冲块。</param>
        /// <param name="position">开始读取的位置（字节偏移），默认为 0。</param>
        /// <returns>操作结果；若发生异常则释放创建的缓冲块并返回异常结果。</returns>
        public static Result Read(this IFileOperate fileOperate, int length, out ByteBlock block, long position = 0)
        {
            block = new(length);
            try
            {
                var result = fileOperate.Read(block.GetMemory(length), position);
                if (!result.IsSuccess)
                {
                    block.Dispose();
                    return result;
                }
                block.Advance(result);
                return result;
            }
            catch (Exception ex)
            {
                block.Dispose();
                return Result.FromException(ex);
            }
        }

        #endregion Read

        #region Write

        /// <summary>
        /// 将 <see cref="ByteBuffer"/> 写入到指定文件位置。
        /// </summary>
        /// <param name="fileOperate">文件操作接口。</param>
        /// <param name="buffer">要写入的顺序缓冲（写入后由目标实现或调用方决定释放策略）。</param>
        /// <param name="position">写入起始位置（字节偏移），默认为 0。</param>
        /// <returns>操作结果；当缓冲长度为0时返回成功；发生异常时返回封装的异常结果。</returns>
        public static Result Write(this IFileOperate fileOperate, ByteBuffer buffer, long position = 0)
        {
            try
            {
                if (buffer.Capacity == 0)
                    return Result.Success();
                if (buffer.IsEmpty)
                    throw new ArgumentNullException(nameof(buffer));

                var result = fileOperate.Write(buffer.CommittedSequence, position);
                if (!result.IsSuccess)
                    return result;

                buffer.Advance(result);
                return result;
            }
            catch (Exception ex)
            {
                return Result.FromException(ex);
            }
        }

        /// <summary>
        /// 将 <see cref="ByteBlock"/> 写入到指定文件位置。
        /// </summary>
        /// <param name="fileOperate">文件操作接口。</param>
        /// <param name="block">要写入的缓冲块（写入后由目标实现或调用方决定释放策略）。</param>
        /// <param name="position">写入起始位置（字节偏移），默认为 0。</param>
        /// <returns>操作结果；当块长度为0时返回成功；发生异常时返回封装的异常结果。</returns>
        public static Result Write(this IFileOperate fileOperate, ref ByteBlock block, long position = 0)
        {
            try
            {
                if (block.Committed == 0)
                    return Result.Success();

                if (block.IsEmpty)
                    throw new ArgumentNullException(nameof(block));

                var result = fileOperate.Write(block.CommittedMemory, position);
                if (!result.IsSuccess)
                    return result;

                block.Advance(result);
                return result;
            }
            catch (Exception ex)
            {
                return Result.FromException(ex);
            }
        }

        #endregion Write

        #region ReadAsync

        /// <summary>
        /// 异步读取文件并返回一个包含读取结果的 <see cref="ByteBlock"/>。
        /// </summary>
        /// <param name="fileOperate">文件操作接口。</param>
        /// <param name="token">可选的取消令牌。</param>
        /// <returns>
        /// 异步返回 <see cref="Result{ByteBlock}"/>：成功时包含填充好的 <see cref="ByteBlock"/>，调用方负责释放该块。
        /// 失败时返回封装的异常信息。
        /// </returns>
        public static ValueTask<Result<ByteBlock>> ReadByteBlockAsync(this IFileOperate fileOperate, CancellationToken token = default)
        {
            return ReadByteBlockAsync(fileOperate, 0, token);
        }

        /// <summary>
        /// 异步从指定位置读取文件并返回一个包含读取结果的 <see cref="ByteBlock"/>。
        /// </summary>
        /// <param name="fileOperate">文件操作接口。</param>
        /// <param name="filePosition">起始读取位置（字节偏移）。</param>
        /// <param name="token">可选的取消令牌。</param>
        /// <returns>参见 <see cref="ReadByteBlockAsync(IFileOperate,long,CancellationToken)"/>。</returns>
        public static ValueTask<Result<ByteBlock>> ReadByteBlockAsync(this IFileOperate fileOperate, long filePosition, CancellationToken token = default)
        {
            return ReadByteBlockAsync(fileOperate, filePosition, (int)(fileOperate.Info.Length - filePosition), token);
        }

        /// <summary>
        /// 异步从指定位置读取指定长度的数据并返回一个包含读取结果的 <see cref="ByteBlock"/>。
        /// </summary>
        /// <param name="fileOperate">文件操作接口。</param>
        /// <param name="position">起始读取位置（字节偏移）。</param>
        /// <param name="length">要读取的字节数。</param>
        /// <param name="token">可选的取消令牌。</param>
        /// <returns>
        /// 异步返回 <see cref="Result{ByteBlock}"/>：成功时包含填充好的 <see cref="ByteBlock"/>，调用方负责释放该块；失败时返回封装的异常信息。
        /// </returns>
        public static async ValueTask<Result<ByteBlock>> ReadByteBlockAsync(this IFileOperate fileOperate, long position, int length, CancellationToken token = default)
        {
            try
            {
                ByteBlock block = new(length);
                await fileOperate.ReadAsync(block.GetMemory(length), position, token);
                return Result.Success(block);
            }
            catch (Exception ex)
            {
                return Result.FromException<ByteBlock>(ex);
            }
        }

        #endregion ReadAsync
    }
}