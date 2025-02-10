using System.Collections.Concurrent;
using ExtenderApp.Abstract;
using ExtenderApp.Common.File.Splitter;
using ExtenderApp.Common.ObjectPools;
using ExtenderApp.Data;

namespace ExtenderApp.Common.Files.Splitter
{
    /// <summary>
    /// 文件分割操作类，实现了可重置和可释放接口。
    /// </summary>
    /// <remarks>
    /// 创建一个文件分割信息的文件，用于存储文件分割信息
    /// </remarks>
    internal class SplitterOperate : IResettable, IDisposable
    {
        /// <summary>
        /// 二进制解析器。
        /// </summary>
        private readonly IBinaryParser _parser;

        /// <summary>
        /// 文件分割数据队列。
        /// </summary>
        private readonly ConcurrentQueue<SplitterOperation> _queue;

        /// <summary>
        /// 计划任务。
        /// </summary>
        private readonly ScheduledTask _task;

        /// <summary>
        /// 一个委托，用于释放资源。
        /// </summary>
        /// <param name="_releaseAction">用于释放资源的委托。</param>
        private readonly Action<SplitterOperate> _releaseAction;

        /// <summary>
        /// 用于操作分割文件的 FileOperate 实例。
        /// </summary>
        private FileOperate splitterFileOperate;

        /// <summary>
        /// 用于操作分割信息文件的 FileOperate 实例。
        /// </summary>
        private FileOperate splitterInfoFileOperate;

        /// <summary>
        /// 文件分割信息。
        /// </summary>
        private FileSplitterInfo? splitterInfo;

        /// <summary>
        /// 获取文件分割信息。
        /// </summary>
        public FileSplitterInfo? SplitterInfo => splitterInfo;

        /// <summary>
        /// 获取或设置本地文件信息
        /// </summary>
        public ExpectLocalFileInfo ExpectFileInfo { get; private set; }

        /// <summary>
        /// 文件流。
        /// </summary>
        private FileStream? stream;

        /// <summary>
        /// 初始化FileSplitterOperate类的新实例。
        /// </summary>
        /// <param name="parser">二进制解析器。</param>
        public SplitterOperate(IBinaryParser parser, Action<SplitterOperate> action)
        {
            _parser = parser;
            _queue = new();
            _task = new ScheduledTask();
            _releaseAction = action;
        }

        /// <summary>
        /// 打开文件并准备进行分块处理
        /// </summary>
        /// <param name="splitterOperate">用于操作分割文件的文件操作对象</param>
        /// <param name="splitterInfoOperate">用于操作分割信息文件的文件操作对象</param>
        /// <exception cref="Exception">如果分块信息文件解析失败，抛出异常</exception>
        /// <exception cref="InvalidOperationException">如果当前分块文件信息缺失，抛出异常</exception>
        public void OpenFile(FileOperate splitterOperate, FileOperate splitterInfoOperate, ExpectLocalFileInfo expectLocalFileInfo)
        {
            splitterFileOperate = splitterOperate;
            splitterInfoFileOperate = splitterInfoOperate;
            ExpectFileInfo = expectLocalFileInfo;
            splitterInfo = _parser.Deserialize<FileSplitterInfo>(splitterInfoFileOperate);

            if (splitterInfo == null)
            {
                throw new Exception(string.Format("分块信息文件解析错误：{0}", splitterInfoFileOperate.LocalFileInfo.FilePath));
            }
            else if (splitterInfo.IsEmpty)
            {
                throw new InvalidOperationException(string.Format("当前分块文件信息缺失：{0}", splitterInfoFileOperate.LocalFileInfo.FilePath));
            }

            stream = splitterFileOperate.OpenFile();
            _task.Start(o => Execute(), null, TimeSpan.Zero, TimeSpan.FromSeconds(1));
        }

        /// <summary>
        /// 将给定的SplitterOperation对象添加到队列中。
        /// </summary>
        /// <param name="operation">要添加的SplitterOperation对象。</param>
        public void Set(SplitterOperation operation)
        {
            _queue.Enqueue(operation);
        }

        /// <summary>
        /// 执行操作
        /// </summary>
        private void Execute()
        {
            if (_queue.Count <= 0) return;

            while (_queue.Count > 0)
            {
                if (!_queue.TryDequeue(out var operation))
                {
                    break;
                }

                operation.Execute(stream);

                // 更新进度
                splitterInfo!.Progress++;
            }

            //如果已经加载完成，就切换成成目标文件扩展名
            if (splitterInfo!.Progress == splitterInfo.ChunkCount)
            {
                ConvertToTargetFile();
                return;
            }

            UpdateSplitterInfo();
        }

        /// <summary>
        /// 将分块文件转换成目标文件。
        /// </summary>
        private void ConvertToTargetFile()
        {
            stream?.Dispose();
            stream = null;

            if (string.IsNullOrEmpty(splitterInfo!.TargetExtensions))
            {
                throw new InvalidOperationException("目标文件扩展名为空");
            }

            var targetFileLocalInfo = splitterFileOperate.LocalFileInfo.ChangeFileExtension(SplitterInfo!.TargetExtensions);

            if(targetFileLocalInfo.Exists)
            {
                targetFileLocalInfo.AppendFileName();
            }

            splitterFileOperate.Move(targetFileLocalInfo);

            // 删除分块信息文件
            splitterInfoFileOperate.LocalFileInfo.FileInfo.Delete();

            //回收
            _releaseAction.Invoke(this);
        }

        /// <summary>
        /// 更新文件分割信息。
        /// </summary>
        private void UpdateSplitterInfo()
        {
            lock (_task)
            {
                _parser.Serialize(splitterInfoFileOperate, SplitterInfo);
            }
        }

        /// <summary>
        /// 尝试重置。
        /// </summary>
        /// <returns>是否重置成功。</returns>
        public bool TryReset()
        {
            if (_queue.Count > 0) return false;

            if (splitterInfo!.Progress != splitterInfo.ChunkCount)
            {
                UpdateSplitterInfo();
            }

            stream?.Dispose();
            stream = null;
            splitterInfoFileOperate = FileOperate.Empty;
            splitterFileOperate = FileOperate.Empty;
            splitterInfo = null;
            _task.Pause();
            return true;
        }

        /// <summary>
        /// 释放资源。
        /// </summary>
        public void Dispose()
        {
            TryReset();
        }
    }
}
