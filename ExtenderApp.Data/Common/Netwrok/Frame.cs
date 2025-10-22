
namespace ExtenderApp.Data
{
    public struct Frame : IDisposable
    {
        public LinkHeader Header;
        public ByteBlock Payload;
        public object? ResultArray;
        public Action<object?>? CompleteAction;

        public Frame(LinkHeader header, ByteBlock payload, object? resultArray, Action<object?>? completeAction) : this(header, payload)
        {
            ResultArray = resultArray;
            CompleteAction = completeAction;
        }

        public Frame(LinkHeader header, ByteBlock payload)
        {
            Header = header;
            Payload = payload;
        }

        public void Dispose()
        {
            CompleteAction?.Invoke(ResultArray);
            Payload.Dispose();
        }
    }
}
