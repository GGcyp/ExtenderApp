using System.Text;
using ExtenderApp.Data;

namespace ExtenderApp.Common.Caches
{
    public class ByteBufferAllocator : DisposableObject
    {
        private readonly SequencePool<byte> _pool;
        private SequencePool<byte>.SequenceRental rental;
        private Sequence<byte>? Sequence => rental.Value;
        private long consumed;

        public ByteBufferAllocator() : this(SequencePool<byte>.Shared)
        {

        }

        public ByteBufferAllocator(SequencePool<byte> pool)
        {
            _pool = pool ?? SequencePool<byte>.Shared;
        }

        public long Remaining
        {
            get
            {
                if (Sequence == null)
                    return 0;
                return Math.Max(0L, Sequence.Length - consumed);
            }
        }
    }
}