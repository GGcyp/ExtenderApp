using ExtenderApp.Common.ObjectPools;

namespace ExtenderApp.Common
{
    public class MultiThreadDataBuffer<T>
    {
        private static ObjectPool<MultiThreadDataBuffer<T>> pool = ObjectPool.Create<MultiThreadDataBuffer<T>>();

        private T item1;
        private Action callback;
        private Action<T> runAtcion;

        public static MultiThreadDataBuffer<T> Create(Action<T> runAction, T obj, Action callback = null)
        {
            if (runAction is null)
                throw new ArgumentNullException(nameof(runAction));

            var buffer = pool.Get();

            buffer.runAtcion = runAction;
            buffer.item1 = obj;
            buffer.callback = callback;
            return buffer;
        }

        public MultiThreadDataBuffer()
        {
            throw new NotImplementedException();
        }

        public void Run()
        {
            Task.Run(RunAtcion);
        }

        private void RunAtcion()
        {
            runAtcion?.Invoke(item1);
            callback?.Invoke();
        }
    }
}
