using System.Buffers;
using System.Net;
using ExtenderApp.Abstract;
using ExtenderApp.Common.ObjectPools;
using ExtenderApp.Data;

namespace ExtenderApp.Common.Networks
{
    public class LinkClient<TLinker> : DisposableObject, IClient
        where TLinker : ILinker
    {
        private static readonly ObjectPool<LinkerClientContext> _pool
            = ObjectPool.CreateDefaultPool<LinkerClientContext>();

        private readonly TLinker _linker;

        public PipelineExecute<LinkerClientContext>? InputPipeline { get; set; }
        public PipelineExecute<LinkerClientContext>? OutputPipeline { get; set; }
        public Exception? Erorr { get; protected set; }
        public IClientPluginManager? PluginManager { get; set; }

        #region ILinker 直通属性

        public virtual bool Connected => _linker.Connected;

        public virtual EndPoint? LocalEndPoint => _linker.LocalEndPoint;

        public virtual EndPoint? RemoteEndPoint => _linker.RemoteEndPoint;

        public virtual CapacityLimiter CapacityLimiter => _linker.CapacityLimiter;

        public virtual ValueCounter SendCounter => _linker.SendCounter;

        public virtual ValueCounter ReceiveCounter => _linker.ReceiveCounter;

        #endregion ILinker 直通属性

        public LinkClient(TLinker linker)
        {
            _linker = linker;
        }

        public async Task SendAsync<T>(T data)
        {
            var context = _pool.Get();

            T[] values = ArrayPool<T>.Shared.Rent(1);
            values[0] = data;
            Action<object>? completeAction = static (o) =>
            {
                if (o is not T[] array)
                    return;
                ArrayPool<T>.Shared.Return(array);
            };

            context.AddFrame(new Frame(default, new ByteBlock(), values, completeAction));
            try
            {
                Task executTask = OutputPipeline?.Invoke(context) ?? Task.CompletedTask;
                await executTask.ConfigureAwait(false);
                await _linker.SendAsync(context.MessageBlock).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Erorr = ex;
                return;
            }
            finally
            {
                if (context.ResultArray is T[] resultArray)
                {
                    ArrayPool<T>.Shared.Return(resultArray);
                }
                context.Reset();
                _pool.Release(context);
            }
        }

        public void SetClientPluginManager(IClientPluginManager pluginManager)
        {
            PluginManager = pluginManager;
            pluginManager.InvokePlugins<IPersistentPlugin>(this);
        }

        public void SetClientPipeline(IPipelineBuilder<LinkerClientContext, LinkerClientContext> builder)
        {
            InputPipeline = builder.BuildInput();
            OutputPipeline = builder.BuildOutput();
        }

        #region Linker 直通方法

        public virtual void Connect(EndPoint remoteEndPoint)
        {
            _linker.Connect(remoteEndPoint);
        }

        public virtual ValueTask ConnectAsync(EndPoint remoteEndPoint, CancellationToken token = default)
        {
            return _linker.ConnectAsync(remoteEndPoint, token);
        }

        public virtual void Disconnect()
        {
            _linker.Disconnect();
        }

        public virtual ValueTask DisconnectAsync(CancellationToken token = default)
        {
            return _linker.DisconnectAsync(token);
        }

        public virtual SocketOperationResult Receive(Memory<byte> memory)
        {
            return _linker.Receive(memory);
        }

        public virtual ValueTask<SocketOperationResult> ReceiveAsync(Memory<byte> memory, CancellationToken token = default)
        {
            return _linker.ReceiveAsync(memory, token);
        }

        public virtual SocketOperationResult Send(Memory<byte> memory)
        {
            return _linker.Send(memory);
        }

        public virtual ValueTask<SocketOperationResult> SendAsync(Memory<byte> memory, CancellationToken token = default)
        {
            return _linker.SendAsync(memory, token);
        }

        #endregion Linker 直通方法
    }
}