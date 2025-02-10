using ExtenderApp.Common.Files.Splitter;
using ExtenderApp.Common.ObjectPools;

namespace ExtenderApp.Common.File.Splitter
{
    internal class SplitterOperationFactory
    {
        private class SplitterOperationPool<T> : PooledObjectPolicy<T> where T : SplitterOperation, new()
        {
            public override T Create()
            {
                var result = new T();
                result.ReleaseAction = o => Release((T)o);
                return result;
            }

            public override bool Release(T obj)
            {
                return obj.TryReset();
            }
        }

        private readonly ObjectPool<WriteOperation> _writePool;
        private readonly ObjectPool<ReadOperation> _readPool;
        private readonly ObjectPool<ReadChunkOperation> _readChunkPool;

        public SplitterOperationFactory()
        {
            _writePool = ObjectPool.Create(new SplitterOperationPool<WriteOperation>());
            _readPool = ObjectPool.Create(new SplitterOperationPool<ReadOperation>());
            _readChunkPool = ObjectPool.Create(new SplitterOperationPool<ReadChunkOperation>());
        }

        public WriteOperation GetWriteOperation(SplitterOperate operate)
        {
            var result = _writePool.Get();
            result.Inject(operate);
            return result;
        }

        public ReadOperation GetReadOperation(SplitterOperate operate)
        {
            var result = _readPool.Get();
            result.Inject(operate);
            return result;
        }

        public ReadChunkOperation GetReadChunkOperation(SplitterOperate operate)
        {
            var result = _readChunkPool.Get();
            result.Inject(operate);
            return result;
        }
    }
}
