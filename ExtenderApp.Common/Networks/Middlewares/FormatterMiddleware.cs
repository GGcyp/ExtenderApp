using System.Buffers;
using ExtenderApp.Abstract;
using ExtenderApp.Common.Hash;
using ExtenderApp.Common.Pipelines;
using ExtenderApp.Data;

namespace ExtenderApp.Common.Networks.Middlewares
{
    public class FormatterMiddleware<T> : MiddlewareBase<LinkerClientContext, LinkerClientContext>
    {
        private readonly IByteBufferFactory _bufferFactory;
        private readonly IBinaryFormatter<T> _formatter;
        private int dataType;

        public FormatterMiddleware(IBinaryFormatter<T> formatter, IByteBufferFactory bufferFactory)
        {
            _formatter = formatter;
            dataType = typeof(T).Name.ComputeHash_FNV_1a();
            _bufferFactory = bufferFactory;
        }

        protected override Task<bool> InputInvokeAsync(LinkerClientContext context)
        {
            var list = context.Frames;
            for (int i = 0; i < list.Count; i++)
            {
                var frame = list[i];
                if (frame.Header.DataType != dataType)
                    continue;

                ByteBuffer buffer = new(frame.Payload);
                T result = _formatter.Deserialize(ref buffer);
                T[]? values = frame.ResultArray as T[];
                frame.ResultArray = values ??= ArrayPool<T>.Shared.Rent(1);
                values[i] = result;
                frame.CompleteAction = static (o) =>
                {
                    if (o is not T[] array)
                        return;

                    ArrayPool<T>.Shared.Return(array);
                };
            }

            return Ok();
        }

        protected override Task<bool> OutputInvokeAsync(LinkerClientContext context)
        {
            var list = context.Frames;
            for (int i = 0; i < list.Count; i++)
            {
                var frame = list[i];
                if (frame.ResultArray is not T[] values)
                    continue;
                ByteBuffer buffer = new(frame.Payload);
                _formatter.Serialize(ref buffer, values[0]);
                frame.Payload = buffer;
                frame.Header.DataType = dataType;
            }
            return Ok();
        }
    }
}
