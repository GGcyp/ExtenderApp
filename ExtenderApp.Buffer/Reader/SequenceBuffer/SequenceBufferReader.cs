using System.Buffers;
using ExtenderApp.Buffer.Sequence;

namespace ExtenderApp.Buffer.Reader
{
    internal class SequenceBufferReader<T> : AbstractBufferReader<T>
    {
        public new SequenceBuffer<T> Buffer { get; internal set; }

        private SequenceBufferSegment<T>? currentSegment;
        private long segmentConsumed; // 已在当前段内消费的数量（相对于段起点）

        public override ReadOnlySequence<T> UnreadSequence
        {
            get
            {
                if (currentSegment == null)
                    return ReadOnlySequence<T>.Empty;

                return new ReadOnlySequence<T>(currentSegment, (int)segmentConsumed, Buffer.Last ?? SequenceBufferSegment<T>.Empty, (int)(Buffer.Last?.Committed ?? 0));
            }
        }

        public SequenceBufferReader()
        {
            Buffer = default!;
            segmentConsumed = 0;
        }

        public override sealed void Advance(long count)
        {
            if (currentSegment == null)
            {
                if (!TryMoveToNextSegment())
                {
                    throw new InvalidOperationException("无法找到下一个序列！");
                }
            }

            base.Advance(count);

            if (currentSegment == null)
            {
                if (!TryMoveToNextSegment())
                    ThrowIfNoFindNextSegment();
            }

            base.Advance(count);

            // 当前段中可用的剩余数量
            long availableInCurrent = currentSegment!.Committed - segmentConsumed;

            if (count < availableInCurrent)
            {
                // 仅在当前段内部分消费
                segmentConsumed += count;
                return;
            }
            else if (count == availableInCurrent)
            {
                // 恰好消费完当前段，移动到下一个段并将已消费归零
                if (!TryMoveToNextSegment())
                    ThrowIfNoFindNextSegment();
                segmentConsumed = 0;
                return;
            }

            // 需要超出当前段继续消费
            count -= availableInCurrent;
            while (true)
            {
                // 移动到下一个段
                if (!TryMoveToNextSegment())
                    ThrowIfNoFindNextSegment();

                long segLen = currentSegment!.Committed;
                if (count < segLen)
                {
                    // 在当前段内部分消费
                    segmentConsumed = count;
                    break;
                }
                else if (count == segLen)
                {
                    // 恰好消费完该段，移动到其下一段并将已消费归零
                    if (!TryMoveToNextSegment())
                        ThrowIfNoFindNextSegment();

                    segmentConsumed = 0;
                    break;
                }
                else
                {
                    // 消耗整个段并继续
                    count -= segLen;
                    // 循环继续以移动并检查下一段
                }
            }
        }

        private bool TryMoveToNextSegment()
        {
            if (currentSegment == null)
            {
                currentSegment = Buffer.First;
                segmentConsumed = 0;
                return currentSegment != null;
            }
            if (currentSegment.Next != null)
            {
                currentSegment = currentSegment.Next;
                segmentConsumed = 0;
                return true;
            }
            return false;
        }

        private void ThrowIfNoFindNextSegment()
        {
            if (currentSegment == null)
                throw new InvalidOperationException("无法找到下一个序列！");
        }
    }
}