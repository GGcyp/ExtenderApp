using System.Collections.Concurrent;
using ExtenderApp.Abstract;
using ExtenderApp.Common.Hash;
using ExtenderApp.Data;
using Microsoft.Extensions.DependencyInjection;

namespace ExtenderApp.Common.Networks.LinkClients
{
    /// <summary>
    /// 客户端格式化器管理器：负责注册与查找 <see cref="ILinkClientFormatter"/>。
    /// </summary>
    /// <remarks>
    /// - 提供两种索引：按数据类型哈希（MessageType）与按泛型类型 <c>TLinkClient</c>；
    /// - 当同一 <c>TLinkClient</c> 注册多个实现时，会自动以 <see cref="ClientFormatterTypeSwitch{T}"/> 聚合， 以支持基于 FormatterTypeHash 的后续分发。
    /// </remarks>
    internal class LinkClientFormatterManager : DisposableObject, ILinkClientFormatterManager
    {
        private readonly IServiceProvider _serviceProvider;

        /// <summary>
        /// 按数据类型哈希索引到格式化器（或聚合开关）的字典。
        /// </summary>
        private readonly ConcurrentDictionary<int, ILinkClientFormatter> _formatters;

        /// <summary>
        /// 初始化管理器实例。
        /// </summary>
        public LinkClientFormatterManager(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _formatters = new();
        }

        public void AddFormatter(ILinkClientFormatter formatter)
        {
            if (formatter == null)
                throw new ArgumentNullException(nameof(formatter));

            if (_formatters.TryGetValue(formatter.MessageType, out var lastFormatter))
            {
                throw new InvalidOperationException(string.Format("当前转换器已经被注册：{0} ,请先确定使用那种方式序列化。", formatter.GetType().FullName));
            }
            _formatters.TryAdd(formatter.MessageType, formatter);
        }

        public T? AddFormatter<T>() where T : class, ILinkClientFormatter
        {
            var formatter = ActivatorUtilities.CreateInstance<T>(_serviceProvider);
            _formatters.TryAdd(formatter.MessageType, formatter);
            return formatter;
        }

        public void RemoveFormatter<T>()
        {
            _formatters.TryRemove(GetTypeCode<T>(), out var _);
        }

        public Result<FrameContext> ProcessSendVlaue<T>(T value)
        {
            var typeCode = GetTypeCode<T>();
            if (!_formatters.TryGetValue(typeCode, out var f) ||
                f is not ILinkClientFormatter<T> formatter)
            {
                return Result.FromException<FrameContext>(new KeyNotFoundException($"未发现指定类型的转换器 {typeof(T).Name}"));
            }
            var buffer = formatter.Serialize(value);
            ByteBlock block = new ByteBlock(sizeof(int) + (int)buffer.Remaining);
            block.Write(typeCode);
            block.Write(buffer);
            return Result.Success(new FrameContext(block));
        }

        public Result ProcessReceivedFrame(SocketOperationValue operationValue, ref FrameContext frameContext)
        {
            ByteBlock block = frameContext;
            var typeCode = block.ReadInt32();
            if (!_formatters.TryGetValue(typeCode, out var formatter))
            {
                return Result.FromException(new KeyNotFoundException($"未发现指定类型的转换器 {typeCode}"));
            }

            frameContext.WriteNextPayload(block);
            formatter.DeserializeAndInvoke(operationValue, ref frameContext);
            return Result.Success();
        }

        public bool TryGetFormatter<T>(out ILinkClientFormatter<T> formatter)
        {
            formatter = null!;
            if (_formatters.TryGetValue(GetTypeCode<T>(), out var f))
            {
                formatter = (f as ILinkClientFormatter<T>)!;
            }
            return formatter == null;
        }

        private int GetTypeCode<T>()
        {
            return typeof(T).ComputeHash_FNV_1a();
        }
    }
}