using ExtenderApp.Abstract;
using ExtenderApp.Data;

namespace ExtenderApp.Common
{
    /// <summary>
    /// 为 <see cref="IFileOperate"/> 提供的扩展方法，简化常见的同步读写操作（针对 <see cref="ByteBuffer"/> 与 <see cref="ByteBlock"/>）。
    /// </summary>
    public static class FileOperateExtensions
    {
        #region Read

        /// <summary>
        /// 从文件起始位置读取全部内容到新的 <see cref="ByteBuffer"/> 中。
        /// </summary>
        /// <param name="fileOperate">文件操作接口。</param>
        /// <param name="buffer">输出：读取到的顺序缓冲。</param>
        /// <returns>操作结果，成功时包含已填充的 <paramref name="buffer"/>；失败时 <paramref name="buffer"/> 已被释放并包含异常信息。</returns>
        public static Result Read(this IFileOperate fileOperate, out ByteBuffer buffer)
        {
            return fileOperate.Read(0, out buffer);
        }

        /// <summary>
        /// 从指定文件位置开始读取至文件末尾并返回到新的 <see cref="ByteBuffer"/> 中。
        /// </summary>
        /// <param name="fileOperate">文件操作接口。</param>
        /// <param name="filePosition">开始读取的位置（字节偏移）。</param>
        /// <param name="buffer">输出：读取到的顺序缓冲。</param>
        /// <returns>操作结果。</returns>
        public static Result Read(this IFileOperate fileOperate, long filePosition, out ByteBuffer buffer)
        {
            return fileOperate.Read(filePosition, (int)(fileOperate.Info.Length - filePosition), out buffer);
        }

        /// <summary>
        /// 从指定文件位置读取固定长度到新的 <see cref="ByteBuffer"/> 中。
        /// </summary>
        /// <param name="fileOperate">文件操作接口。</param>
        /// <param name="filePosition">开始读取的位置（字节偏移）。</param>
        /// <param name="length">要读取的字节数。</param>
        /// <param name="buffer">输出：读取到的顺序缓冲。</param>
        /// <returns>操作结果；若发生异常则返回封装的异常结果并释放创建的缓冲。</returns>
        public static Result Read(this IFileOperate fileOperate, long filePosition, int length, out ByteBuffer buffer)
        {
            buffer = new();
            try
            {
                buffer = ByteBuffer.CreateBuffer();
                var result = fileOperate.Read(filePosition, buffer.GetMemory(length));
                if (!result.IsSuccess)
                {
                    buffer.Dispose();
                    return result;
                }
                buffer.WriteAdvance(result);
                return result;
            }
            catch (Exception ex)
            {
                buffer.Dispose();
                return Result.FromException(ex);
            }
        }

        /// <summary>
        /// 从文件起始位置读取全部内容到新的 <see cref="ByteBlock"/> 中（兼容旧缓冲类型）。
        /// </summary>
        /// <param name="fileOperate">文件操作接口。</param>
        /// <param name="block">输出：读取到的缓冲块。</param>
        /// <returns>操作结果。</returns>
        public static Result Read(this IFileOperate fileOperate, out ByteBlock block)
        {
            return fileOperate.Read(0, out block);
        }

        /// <summary>
        /// 从指定文件位置读取至文件末尾并返回到新的 <see cref="ByteBlock"/> 中（兼容旧缓冲类型）。
        /// </summary>
        /// <param name="fileOperate">文件操作接口。</param>
        /// <param name="filePosition">开始读取的位置（字节偏移）。</param>
        /// <param name="block">输出：读取到的缓冲块。</param>
        /// <returns>操作结果。</returns>
        public static Result Read(this IFileOperate fileOperate, long filePosition, out ByteBlock block)
        {
            return fileOperate.Read(filePosition, (int)(fileOperate.Info.Length - filePosition), out block);
        }

        /// <summary>
        /// 从指定文件位置读取固定长度到新的 <see cref="ByteBlock"/> 中（兼容旧缓冲类型）。
        /// </summary>
        /// <param name="fileOperate">文件操作接口。</param>
        /// <param name="filePosition">开始读取的位置（字节偏移）。</param>
        /// <param name="length">要读取的字节数。</param>
        /// <param name="block">输出：读取到的缓冲块。</param>
        /// <returns>操作结果；若发生异常则释放创建的缓冲块并返回异常结果。</returns>
        public static Result Read(this IFileOperate fileOperate, long filePosition, int length, out ByteBlock block)
        {
            block = new(length);
            try
            {
                var result = fileOperate.Read(filePosition, block.GetMemory(length));
                if (!result.IsSuccess)
                {
                    block.Dispose();
                    return result;
                }
                block.WriteAdvance(result);
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
        /// 将 <see cref="ByteBuffer"/> 从开头写入到文件（默认从位置0开始）。
        /// </summary>
        /// <param name="fileOperate">文件操作接口。</param>
        /// <param name="buffer">要写入的顺序缓冲（写入后调用方负责释放）。</param>
        /// <returns>操作结果；当缓冲为空时返回成功。</returns>
        public static Result Write(this IFileOperate fileOperate, ref ByteBuffer buffer)
        {
            return fileOperate.Write(0, ref buffer);
        }

        /// <summary>
        /// 将 <see cref="ByteBuffer"/> 写入到指定文件位置。
        /// </summary>
        /// <param name="fileOperate">文件操作接口。</param>
        /// <param name="filePosition">写入起始位置（字节偏移）。</param>
        /// <param name="buffer">要写入的顺序缓冲（写入后由目标实现或调用方决定释放策略）。</param>
        /// <returns>操作结果；当缓冲长度为0时返回成功；发生异常时返回封装的异常结果。</returns>
        public static Result Write(this IFileOperate fileOperate, long filePosition, ref ByteBuffer buffer)
        {
            try
            {
                if (buffer.Length == 0)
                    return Result.Success();
                if (buffer.IsEmpty)
                    throw new ArgumentNullException(nameof(buffer));

                var result = fileOperate.Write(filePosition, buffer.UnreadSequence);
                if (!result.IsSuccess)
                    return result;

                buffer.WriteAdvance(result);
                return result;
            }
            catch (Exception ex)
            {
                return Result.FromException(ex);
            }
        }

        /// <summary>
        /// 将 <see cref="ByteBlock"/> 从开头写入到文件（默认从位置0开始）。
        /// </summary>
        /// <param name="fileOperate">文件操作接口。</param>
        /// <param name="block">要写入的缓冲块（写入后调用方负责释放）。</param>
        /// <returns>操作结果。</returns>
        public static Result Write(this IFileOperate fileOperate, ref ByteBlock block)
        {
            return fileOperate.Write(0, ref block);
        }

        /// <summary>
        /// 将 <see cref="ByteBlock"/> 写入到指定文件位置。
        /// </summary>
        /// <param name="fileOperate">文件操作接口。</param>
        /// <param name="filePosition">写入起始位置（字节偏移）。</param>
        /// <param name="block">要写入的缓冲块（写入后由目标实现或调用方决定释放策略）。</param>
        /// <returns>操作结果；当块长度为0时返回成功；发生异常时返回封装的异常结果。</returns>
        public static Result Write(this IFileOperate fileOperate, long filePosition, ref ByteBlock block)
        {
            try
            {
                if (block.Length == 0)
                    return Result.Success();

                if (block.IsEmpty)
                    throw new ArgumentNullException(nameof(block));

                var result = fileOperate.Write(filePosition, block.UnreadMemory);
                if (!result.IsSuccess)
                    return result;

                block.WriteAdvance(result);
                return result;
            }
            catch (Exception ex)
            {
                return Result.FromException(ex);
            }
        }

        #endregion Write

        #region ReadAsync

        public static ValueTask<Result<ByteBlock>> ReadByteBlockAsync(this IFileOperate fileOperate, CancellationToken token = default)
        {
            return ReadByteBlockAsync(fileOperate, 0, token);
        }

        public static ValueTask<Result<ByteBlock>> ReadByteBlockAsync(this IFileOperate fileOperate, long filePosition, CancellationToken token = default)
        {
            return ReadByteBlockAsync(fileOperate, filePosition, (int)(fileOperate.Info.Length - filePosition), token);
        }

        public static async ValueTask<Result<ByteBlock>> ReadByteBlockAsync(this IFileOperate fileOperate, long filePosition, int length, CancellationToken token = default)
        {
            try
            {
                ByteBlock block = new(length);
                await fileOperate.ReadAsync(filePosition, block.GetMemory(length), token);
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