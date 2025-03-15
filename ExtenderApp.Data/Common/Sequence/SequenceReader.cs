using System.Buffers;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace ExtenderApp.Data.Binary
{
    /// <summary>
    /// 内部引用结构体 <see cref="SequenceReader{T}"/>，用于高效地读取和遍历 <see cref="ReadOnlySequence{T}"/> 或 <see cref="ReadOnlyMemory{T}"/>。
    /// </summary>
    /// <typeparam name="T">元素的类型，必须是未管理的并且实现了 <see cref="IEquatable{T}"/> 接口。</typeparam>
    internal ref struct SequenceReader<T> where T : unmanaged, IEquatable<T>
    {
        /// <summary>
        /// 标记当前是否基于序列（<see cref="ReadOnlySequence{T}"/>）进行读取操作。
        /// </summary>
        private bool usingSequence;

        /// <summary>
        /// 当前正在读取的序列。
        /// </summary>
        private ReadOnlySequence<T> sequence;

        /// <summary>
        /// 当前读取位置。
        /// </summary>
        private SequencePosition currentPosition;

        /// <summary>
        /// 下一个读取位置。
        /// </summary>
        private SequencePosition nextPosition;

        /// <summary>
        /// 当前正在读取的只读内存。
        /// </summary>
        private ReadOnlyMemory<T> memory;

        /// <summary>
        /// 标记是否还有更多数据可读。
        /// </summary>
        private bool moreData;

        /// <summary>
        /// 序列的总长度。
        /// </summary>
        private long length;

        /// <summary>
        /// 获取一个值，指示是否已到达序列末尾。
        /// </summary>
        public readonly bool End => !moreData;

        /// <summary>
        /// 获取当前读取的序列。
        /// </summary>
        public ReadOnlySequence<T> Sequence
        {
            get
            {
                if (sequence.IsEmpty && !memory.IsEmpty)
                {
                    // 在内部实现中，如果之前是基于只读内存进行读取操作（即 sequence 字段为空但 memory 字段有数据），
                    // 会懒加载地将内存数据转换为序列形式并进行相应位置信息的初始化，最终总是以序列的形式返回实际的数据来源，
                    // 方便外部代码统一按照序列的概念来操作和获取相关信息。
                    sequence = new ReadOnlySequence<T>(memory);
                    currentPosition = sequence.Start;
                    nextPosition = sequence.End;
                }

                return sequence;
            }
        }

        /// <summary>
        /// 获取当前读取位置。
        /// </summary>
        public SequencePosition Position
            => Sequence.GetPosition(CurrentSpanIndex, currentPosition);

        /// <summary>
        /// 获取当前读取的数据片段。
        /// </summary>
        public ReadOnlySpan<T> CurrentSpan { get; private set; }

        /// <summary>
        /// 获取当前数据片段的索引。
        /// </summary>
        public int CurrentSpanIndex { get; private set; }

        /// <summary>
        /// 获取当前未读取的数据片段。
        /// </summary>
        public readonly ReadOnlySpan<T> UnreadSpan
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => CurrentSpan.Slice(CurrentSpanIndex);
        }

        /// <summary>
        /// 获取已消耗（读取）的元素数量。
        /// </summary>
        public long Consumed { get; private set; }

        /// <summary>
        /// 获取剩余未读取的元素数量。
        /// </summary>
        public long Remaining => Length - Consumed;

        /// <summary>
        /// 获取序列的总长度。
        /// </summary>
        public long Length
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                if (length < 0)
                {
                    // 使用ReadOnlySequence<T>时，初始时先将序列总长度设置为 -1，后续会在需要时进行实际计算并缓存。
                    length = Sequence.Length;
                }

                return length;
            }
        }

        /// <summary>
        /// 使用指定的只读序列初始化 <see cref="SequenceReader{T}"/>。
        /// </summary>
        /// <param name="sequence">要读取的只读序列。</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SequenceReader(scoped in ReadOnlySequence<T> sequence)
        {
            // 标记当前是基于序列（ReadOnlySequence < T >）进行读取操作，即将 usingSequence 设置为 true。
            this.usingSequence = true;
            // 将当前正在读取的数据片段索引初始化为0，表示刚开始读取，还处于第一个数据片段位置（如果有的话）。
            this.CurrentSpanIndex = 0;
            // 已消耗（读取）的元素数量初始化为0，同样表示刚开始读取操作。
            this.Consumed = 0;
            // 将传入的只读序列赋值给 sequence 字段，用于后续操作中作为整个数据来源的基础。
            this.sequence = sequence;
            // 初始化 memory 字段为默认值，因为当前是基于序列进行读取，memory 暂时不使用。
            this.memory = default;
            // 将当前位置设置为序列的起始位置，方便后续从开头进行读取操作定位。
            this.currentPosition = sequence.Start;
            // 初始时先将序列总长度设置为 -1，后续会在需要时进行实际计算并缓存。
            this.length = -1;

            // 获取序列的第一个片段的跨度（Span），用于后续初始化当前读取的数据片段以及判断是否有数据可读等操作。
            ReadOnlySpan<T> first = sequence.First.Span;
            // 根据第一个片段的长度确定下一个位置（即当前片段结束位置），为后续读取和移动位置做准备。
            this.nextPosition = sequence.GetPosition(first.Length);
            // 将当前读取的数据片段设置为序列的第一个片段跨度，以便开始读取操作。
            this.CurrentSpan = first;
            // 根据第一个片段的长度判断是否有数据可读，如果长度大于0则表示有数据，将 moreData 设置为 true，否则为 false。
            this.moreData = first.Length > 0;

            // 如果第一个片段没有数据且整个序列不是单一片段（即包含多个片段），则表示可能后续片段有数据，将 moreData 设置为 true，并尝试获取下一个有数据的片段。
            if (!this.moreData && !sequence.IsSingleSegment)
            {
                this.moreData = true;
                this.GetNextSpan();
            }
        }

        /// <summary>
        /// 使用指定的只读内存初始化 <see cref="SequenceReader{T}"/>。
        /// </summary>
        /// <param name="memory">要读取的只读内存。</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SequenceReader(ReadOnlyMemory<T> memory)
        {
            // 标记当前是基于只读内存（ReadOnlyMemory<T>）进行读取操作，即将 usingSequence 设置为 false。
            this.usingSequence = false;
            // 将当前正在读取的数据片段索引初始化为0，虽然在只读内存场景下相对简单，但保持与序列场景下的初始值一致，便于统一逻辑理解。
            this.CurrentSpanIndex = 0;
            // 已消耗（读取）的元素数量初始化为0，表示刚开始读取操作。
            this.Consumed = 0;
            // 将传入的只读内存赋值给 memory 字段，作为后续读取的数据来源。
            this.memory = memory;
            // 将当前读取的数据片段设置为只读内存对应的跨度（Span），方便后续直接从这个连续内存块中读取数据。
            this.CurrentSpan = memory.Span;
            // 获取并缓存只读内存中元素的总数量，作为整个数据量的记录，方便后续判断读取进度等操作。
            this.length = memory.Length;
            // 根据只读内存的长度判断是否有数据可读，如果长度大于0则表示有数据，将 moreData 设置为 true，否则为 false。
            this.moreData = memory.Length > 0;

            // 由于是基于只读内存读取，序列相关的起始和结束位置信息在这种简单场景下初始化为默认值，暂不使用。
            this.currentPosition = default;
            this.nextPosition = default;
            this.sequence = default;
        }

        /// <summary>
        /// 尝试从当前位置预览下一个元素。
        /// </summary>
        /// <param name="value">如果成功预览到元素，则将其值赋给该参数；否则，该参数将被设置为默认值。</param>
        /// <returns>如果成功预览到下一个元素，则返回 true；否则返回 false。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryPeek(out T value)
        {
            if (moreData)
            {
                // 如果还有数据可读（moreData 为 true），
                // 则通过当前读取的数据片段（CurrentSpan）和当前片段内的索引（CurrentSpanIndex）
                // 获取下一个值，并返回 true 表示获取成功。
                value = CurrentSpan[CurrentSpanIndex];
                return true;
            }
            else
            {
                // 如果已经到达末尾（moreData 为 false），则将 value 设置为默认值，并返回 false 表示无法查看下一个值。
                value = default;
                return false;
            }
        }

        /// <summary>
        /// 尝试从当前位置读取下一个元素。
        /// </summary>
        /// <param name="value">如果成功读取到元素，则将其值赋给该参数；否则，该参数将被设置为默认值。</param>
        /// <returns>如果成功读取到元素，则返回 true；否则返回 false。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryRead(out T value)
        {
            // 首先判断是否已经到达序列末尾（通过 End 属性判断，End 属性基于 moreData 字段判断是否还有未读数据），
            // 如果已经到达末尾，则将返回值 value 设置为类型 T 的默认值，并返回 false，表示无法读取到有效数据。
            if (End)
            {
                value = default;
                return false;
            }

            // 如果还有数据可读，从当前正在读取的数据片段（CurrentSpan）中，
            // 按照当前片段内的索引（CurrentSpanIndex）获取下一个要读取的值，赋给 value。
            value = CurrentSpan[CurrentSpanIndex];
            CurrentSpanIndex++;
            Consumed++;

            if (CurrentSpanIndex >= CurrentSpan.Length)
            {
                if (usingSequence)
                {
                    //如果是基于序列读取，调用 GetNextSpan 方法尝试获取下一个有数据的片段，以便后续继续读取操作。
                    GetNextSpan();
                }
                else
                {
                    // 如果是基于内存读取，已经读完了整个内存数据块，将 moreData 设置为 false，表示没有更多数据可读了。
                    moreData = false;
                }
            }

            return true;
        }

        /// <summary>
        /// 将读取位置回退指定的元素数量。
        /// </summary>
        /// <param name="count">要回退的元素数量。</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Rewind(long count)
        {
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }

            // 将已消耗（读取）的元素总数量（Consumed）减去要回退的数量，相当于将读取位置往回移动了相应的元素个数。
            Consumed -= count;

            // 判断当前正在读取的数据片段（CurrentSpan）内的索引（CurrentSpanIndex）是否大于等于要回退的数量，
            // 如果是，说明可以直接在当前片段内进行回退操作，将当前片段内的索引减去回退的数量（转换为 int 类型，因为 CurrentSpanIndex 是 int 类型），
            // 并将 moreData 设置为 true，表示还有数据可读（回退之后当前位置还有未读数据）。
            if (CurrentSpanIndex >= count)
            {
                CurrentSpanIndex -= (int)count;
                moreData = true;
            }
            else if (usingSequence)
            {
                // 如果当前片段内的索引不够回退指定的数量，并且当前是基于序列（usingSequence 为 true）进行读取的情况，
                // 说明需要跨越片段往回扫描，调用 RetreatToPreviousSpan 方法来处理这种复杂的回退操作，
                // 传入当前已经消耗的元素数量（Consumed）作为参数。
                // 因为要回退到之前的某个位置，可能涉及多个片段的调整，所以需要更复杂的逻辑处理。
                RetreatToPreviousSpan(Consumed);
            }
            else
            {
                throw new ArgumentOutOfRangeException("Rewind went past the start of the memory.");
            }
        }

        /// <summary>
        /// 回退到前一个数据片段。
        /// </summary>
        /// <param name="consumed">已消耗的元素数量。</param>
        [MethodImpl(MethodImplOptions.NoInlining)]
        private void RetreatToPreviousSpan(long consumed)
        {
            // 调用 ResetReader 方法，该方法会将读取器的一些关键状态（如当前片段索引、已消耗数量、位置信息等）重置到初始状态或者合适的起始状态，
            // 为后续重新定位到指定的回退位置做准备。
            ResetReader();
            // 调用 Advance 方法，传入已经消耗的元素数量（consumed）作为参数，根据这个数量将读取器向前移动到之前的位置，实现回退操作，
            // 通过先重置再根据消耗数量前进的方式，完成跨越片段回退到指定位置的复杂逻辑。
            Advance(consumed);
        }

        /// <summary>
        /// 重置读取器的状态。
        /// </summary>
        private void ResetReader()
        {
            // 将当前正在读取的数据片段内的索引（CurrentSpanIndex）重置为0，相当于回到当前片段的起始位置，
            // 这是重置读取器状态的一部分操作，方便后续重新开始读取或者进行位置调整等操作。
            CurrentSpanIndex = 0;
            // 将已消耗（读取）的元素总数量（Consumed）重置为0，同样表示回到初始未读取的状态，清除之前的读取进度记录。
            Consumed = 0;
            // 将当前位置（currentPosition）设置为整个序列（Sequence）的起始位置，重新确定读取的起始点位置信息。
            currentPosition = Sequence.Start;
            // 将下一个位置（nextPosition）也设置为当前位置（currentPosition），因为在初始状态或者重置后，
            // 下一个位置和当前位置是相同的，后续会根据读取情况进行相应变化。
            nextPosition = currentPosition;

            // 尝试从当前位置（通过 nextPosition 表示，初始时和 currentPosition 相同）获取下一个数据片段（如果有的话），
            // 将获取到的数据片段存储在 memory 变量中，并根据是否成功获取以及获取到的数据片段长度进行相应处理。
            if (Sequence.TryGet(ref nextPosition, out ReadOnlyMemory<T> memory, advance: true))
            {
                // 如果成功获取到了下一个数据片段，说明还有数据可读，将 moreData 设置为 true，表示存在未读数据。
                moreData = true;

                // 判断获取到的数据片段的长度是否为0，如果是0，说明这个片段虽然获取到了但没有实际数据，
                // 则将当前正在读取的数据片段（CurrentSpan）设置为默认值（即空的跨度），
                // 然后调用 GetNextSpan 方法尝试获取下一个有数据的片段，以便后续能正常读取操作。
                if (memory.Length == 0)
                {
                    CurrentSpan = default;
                    // 第一个跨度中没有数据，移动到有数据的跨度
                    GetNextSpan();
                }
                else
                {
                    // 如果获取到的数据片段有数据，将当前正在读取的数据片段（CurrentSpan）设置为这个数据片段对应的跨度（Span），
                    // 方便后续读取其中的数据。
                    CurrentSpan = memory.Span;
                }
            }
            else
            {
                // 如果无法获取到下一个数据片段（即已经到达序列末尾或者没有更多数据了），将 moreData 设置为 false，表示没有更多数据可读了，
                // 同时将当前正在读取的数据片段（CurrentSpan）设置为默认值（空的跨度）。
                // 在任何跨度和序列结束时都没有数据
                moreData = false;
                CurrentSpan = default;
            }
        }

        /// <summary>
        /// 获取下一个数据片段。
        /// </summary>
        private void GetNextSpan()
        {
            // 判断整个序列（Sequence）是否是单一片段，如果是单一片段，说明已经读完了整个序列（因为当前片段读完了且没有更多片段了），
            // 直接将 moreData 设置为 false，表示没有更多数据可读了，然后方法结束。
            if (!Sequence.IsSingleSegment)
            {
                // 如果序列不是单一片段，说明还有可能存在其他有数据的片段，先记录下当前的下一个位置（nextPosition）
                // 作为之前的下一个位置（previousNextPosition），
                // 后续在循环获取下一个片段过程中会用到这个值来更新当前位置信息。
                SequencePosition previousNextPosition = nextPosition;
                // 进入循环，不断尝试从当前位置（通过 nextPosition 表示）获取下一个数据片段（如果有的话），每次获取成功后会更新当前位置等相关信息。
                while (Sequence.TryGet(ref nextPosition, out ReadOnlyMemory<T> memory, advance: true))
                {
                    // 将当前位置（currentPosition）更新为之前记录的下一个位置（previousNextPosition），表示进入了新的片段，更新当前位置信息。
                    currentPosition = previousNextPosition;
                    // 判断获取到的数据片段的长度是否大于0，如果大于0，说明找到了有数据的下一个片段，
                    // 将当前正在读取的数据片段（CurrentSpan）设置为这个数据片段对应的跨度（Span），
                    // 并将当前片段内的索引（CurrentSpanIndex）重置为0，
                    // 表示可以开始从这个新的有数据的片段进行读取操作了，然后方法结束，返回新的片段供后续读取。
                    if (memory.Length > 0)
                    {
                        CurrentSpan = memory.Span;
                        CurrentSpanIndex = 0;
                        return;
                    }
                    else
                    {
                        // 如果获取到的数据片段长度为0，说明这个片段虽然获取到了但没有实际数据，
                        // 将当前正在读取的数据片段（CurrentSpan）设置为默认值（即空的跨度），
                        // 并将当前片段内的索引（CurrentSpanIndex）重置为0，
                        // 然后更新之前的下一个位置（previousNextPosition）为当前的下一个位置（nextPosition），
                        // 继续循环尝试获取下一个有数据的片段。
                        CurrentSpan = default;
                        CurrentSpanIndex = 0;
                        previousNextPosition = nextPosition;
                    }
                }
            }

            // 如果循环结束后还没有找到有数据的下一个片段（即整个序列的所有片段都已经遍历完了），
            // 将 moreData 设置为 false，表示没有更多数据可读了。
            moreData = false;
        }

        /// <summary>
        /// 将读取器前进指定的元素数量。
        /// </summary>
        /// <param name="count">要前进的元素数量。</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Advance(long count)
        {
            const long TooBigOrNegative = unchecked((long)0xFFFFFFFF80000000);
            // 首先进行一个条件判断，通过位运算检查要前进的元素数量（count）
            // 是否是合理的正数且当前数据片段（CurrentSpan）内剩余的元素数量是否足够前进指定的数量，
            // 如果满足这个条件，说明可以直接在当前片段内进行前进操作，将当前片段内的索引加上前进的数量
            // （转换为 int 类型，因为 CurrentSpanIndex 是 int 类型），
            // 并将已消耗（读取）的元素总数量（Consumed）也加上前进的数量，完成在当前片段内的读取位置前进操作。
            if ((count & TooBigOrNegative) == 0 && CurrentSpan.Length - CurrentSpanIndex > (int)count)
            {
                CurrentSpanIndex += (int)count;
                Consumed += count;
            }
            else if (usingSequence)
            {
                // 如果不满足在当前片段内直接前进的条件，并且当前是基于序列（usingSequence 为 true）进行读取的情况，
                // 说明需要跨越当前片段到下一个片段或者后续片段去满足前进指定数量元素的要求，
                // 调用 AdvanceToNextSpan 方法来处理这种复杂的前进操作，传入要前进的数量（count）作为参数。
                // 因为可能涉及多个片段的读取操作，所以需要更复杂的逻辑处理。
                AdvanceToNextSpan(count);
            }
            else if (CurrentSpan.Length - CurrentSpanIndex == (int)count)
            {
                // 如果当前片段内剩余的元素数量刚好等于要前进的数量，说明读完当前片段就刚好满足前进要求，
                // 将当前片段内的索引加上前进的数量（转换为 int 类型），并将已消耗（读取）的元素总数量（Consumed）也加上前进的数量，
                // 然后将 moreData 设置为 false，表示已经读完当前片段且没有更多数据可读了（因为刚好读完当前片段）。
                CurrentSpanIndex += (int)count;
                Consumed += count;
                moreData = false;
            }
            else
            {
                // 如果以上条件都不满足，说明传入的要前进的数量不符合逻辑要求（比如数量过大，超出了当前及后续片段可提供的范围等情况），
                // 抛出 ArgumentOutOfRangeException 异常，并以参数名（count）作为异常提示信息的一部分，表明是前进数量参数出现了问题。
                throw new ArgumentOutOfRangeException(nameof(count));
            }
        }

        /// <summary>
        /// 在当前数据片段内前进指定的元素数量。
        /// </summary>
        /// <param name="count">要前进的元素数量。</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AdvanceCurrentSpan(long count)
        {
            Consumed += count;
            CurrentSpanIndex += (int)count;
            if (usingSequence && CurrentSpanIndex >= CurrentSpan.Length)
            {
                GetNextSpan();
            }
        }

        /// <summary>
        /// 在当前数据片段内前进指定的元素数量，但不更新已消耗数量。
        /// </summary>
        /// <param name="count">要前进的元素数量。</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AdvanceWithinSpan(long count)
        {
            // 将已消耗（读取）的元素总数量（Consumed）加上要前进的元素数量，以此来更新已读取元素的计数，体现读取位置的向前移动。
            Consumed += count;
            // 将当前正在读取的数据片段内的索引（CurrentSpanIndex）加上要前进的数量（转换为 int 类型，因为 CurrentSpanIndex 是 int 类型），
            // 实现了在当前数据片段内将读取位置向前移动相应元素个数的操作。
            CurrentSpanIndex += (int)count;
        }

        /// <summary>
        /// 尝试向前移动指定的元素数量。
        /// </summary>
        /// <param name="count">要前进的元素数量。</param>
        /// <returns>如果成功前进，则返回 true；否则返回 false。</returns>
        internal bool TryAdvance(long count)
        {
            // 首先判断剩余的元素数量（通过 Remaining 属性获取，即总长度减去已消耗的数量）是否小于要前进的数量，
            // 如果剩余数量小于前进数量，说明没有足够的数据来支持前进指定的元素个数，返回 false，表示无法完成前进操作。
            if (Remaining < count)
            {
                return false;
            }

            // 如果剩余数量足够，调用 Advance 方法按照指定的数量来移动读取位置，完成前进操作，然后返回 true，表示成功完成了前进操作。
            Advance(count);
            return true;
        }

        /// <summary>
        /// 前进到下一个数据片段，并尝试读取指定的元素数量。
        /// </summary>
        /// <param name="count">要前进的元素数量。</param>
        private void AdvanceToNextSpan(long count)
        {
            // 首先检查要前进的元素数量（count）是否小于0，如果是则抛出 `ArgumentOutOfRangeException` 异常，
            // 因为前进的数量不能是负数，确保传入的参数符合逻辑要求。
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }

            // 将已消耗（读取）的元素总数量（`Consumed`）先加上要前进的数量，记录下整体上已经“消耗”（准备读取）的元素个数，
            // 后续再根据实际情况进行调整。
            Consumed += count;
            // 进入循环，只要还有数据可读（`moreData` 为 true），就持续尝试跨越片段来满足前进指定数量元素的要求。
            while (moreData)
            {
                // 计算当前数据片段（`CurrentSpan`）内剩余的元素数量，即当前片段的长度减去当前片段内已经读取到的索引位置（`CurrentSpanIndex`）。
                int remaining = CurrentSpan.Length - CurrentSpanIndex;

                // 判断当前片段内剩余的元素数量是否大于要前进的数量，如果大于，说明可以在当前片段内部分满足前进要求，
                // 将当前片段内的索引加上要前进的数量（转换为 int 类型），并将剩余要前进的数量设置为0，
                // 表示在当前片段内已经完成了相应的前进操作，然后跳出循环。
                if (remaining > count)
                {
                    CurrentSpanIndex += (int)count;
                    count = 0;
                    break;
                }

                // 如果当前片段内剩余的元素数量小于等于要前进的数量，说明读完当前片段还不能满足前进要求，
                // 需要将当前片段内的索引推进到当前片段的末尾（即将索引设置为当前片段的长度），
                // 然后将要前进的数量减去当前片段内剩余的数量，得到还需要在后续片段中前进的元素个数，
                CurrentSpanIndex += remaining;
                count -= remaining;
                Debug.Assert(count >= 0, "count >= 0");

                // 调用 `GetNextSpan` 方法获取下一个数据片段，以便继续尝试在后续片段中满足前进要求，继续循环判断下一个片段的情况。
                GetNextSpan();

                // 判断剩余要前进的数量是否已经变为0，如果是，说明已经通过跨越片段等操作满足了前进指定数量元素的要求，跳出循环。
                if (count == 0)
                {
                    break;
                }
            }

            // 如果循环结束后剩余要前进的数量仍然不为0，说明没有足够的数据来完成前进指定数量元素的操作，
            // 需要将已消耗（读取）的元素总数量（`Consumed`）减去剩余要前进的数量，回退到实际能够到达的位置，
            // 然后抛出 `ArgumentOutOfRangeException` 异常，并以参数名（`count`）作为异常提示信息的一部分，表明是前进数量参数出现了问题。
            if (count != 0)
            {
                // 没有足够的数据-调整我们实际结束和投掷的位置
                Consumed -= count;
                throw new ArgumentOutOfRangeException(nameof(count));
            }
        }

        /// <summary>
        /// 尝试将数据复制到目标跨度中。
        /// </summary>
        /// <param name="destination">目标复制目的地。</param>
        /// <returns>如果成功复制，则返回 true；否则返回 false。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool TryCopyTo(Span<T> destination)
        {
            // 获取当前数据片段（`CurrentSpan`）中尚未读取的部分（通过 `UnreadSpan` 属性获取），得到一个只读跨度（`ReadOnlySpan<T>`），代表当前可用于复制的数据范围。
            ReadOnlySpan<T> firstSpan = UnreadSpan;
            // 判断当前未读部分的长度是否大于等于目标复制目的地（`destination`）的长度，如果是，说明当前未读部分的数据足够复制到目标跨度中。
            if (firstSpan.Length >= destination.Length)
            {
                // 通过切片操作获取与目标跨度长度相等的部分，并将其复制到目标跨度（`destination`）中，完成数据复制操作，然后返回 true，表示复制成功。
                firstSpan.Slice(0, destination.Length).CopyTo(destination);
                return true;
            }

            // 如果当前未读部分的长度小于目标跨度的长度，进一步判断整个序列（`sequence`）是否为空，
            // 如果序列不为空，尝试调用 `TryCopyMultisegment` 方法进行多片段数据的复制操作，将其结果返回，该结果表示是否成功完成了数据复制到目标跨度的操作。
            return !sequence.IsEmpty && TryCopyMultisegment(destination);
        }

        /// <summary>
        /// 尝试从多个数据片段中复制数据到目标跨度中。
        /// </summary>
        /// <param name="destination">目标复制目的地。</param>
        /// <returns>如果成功复制，则返回 true；否则返回 false。</returns>
        private readonly bool TryCopyMultisegment(Span<T> destination)
        {
            // 首先判断当前结构体中缓存的序列长度（`this.length`）是否小于0，如果小于0，说明还没有缓存实际长度，
            // 通过访问 `sequence.Length` 获取整个序列的长度并赋值给 `length` 变量，
            // 如果已经缓存了长度（即 `this.length` 大于等于0），则直接使用缓存的长度值，这样后续可以基于这个长度来判断剩余数据量等情况。
            long length = this.length < 0 ? sequence.Length : this.length;
            // 计算剩余的元素数量，即整个序列的长度减去已经消耗（读取）的元素数量（`Consumed`），得到还未读取的数据量，
            // 用于后续判断是否有足够的数据可用于复制操作。
            long remaining = length - Consumed;
            // 判断剩余的元素数量是否小于目标复制目的地（`destination`）的长度，如果小于，说明没有足够的数据来填充目标跨度，
            // 返回 false，表示无法完成复制操作。
            if (remaining < destination.Length)
            {
                return false;
            }

            // 获取当前数据片段（`CurrentSpan`）中尚未读取的部分（通过 `UnreadSpan` 属性获取），
            // 得到一个只读跨度（`ReadOnlySpan<T>`），作为开始复制数据的起始部分。
            ReadOnlySpan<T> firstSpan = UnreadSpan;
            // 将当前未读部分的数据复制到目标跨度（`destination`）中，并记录已经复制的元素数量（`copied`），初始化为当前未读部分的长度。
            firstSpan.CopyTo(destination);
            int copied = firstSpan.Length;

            // 初始化一个序列位置（`SequencePosition`）变量，用于记录下一个要获取数据片段的位置，初始值为当前的下一个位置（`nextPosition`）。
            SequencePosition next = nextPosition;
            // 进入循环，不断尝试从序列（`sequence`）中获取下一个数据片段（通过 `TryGet` 方法），并在获取到有数据的片段时进行相应的数据复制操作。
            while (sequence.TryGet(ref next, out ReadOnlyMemory<T> nextSegment, true))
            {
                // 判断获取到的下一个数据片段（`nextSegment`）的长度是否大于0，如果大于0，说明这个片段有数据可以用于复制操作。
                if (nextSegment.Length > 0)
                {
                    // 获取下一个数据片段对应的跨度（`Span`）形式，方便后续进行数据复制操作。
                    ReadOnlySpan<T> nextSpan = nextSegment.Span;
                    // 计算从下一个数据片段中可以复制到目标跨度（`destination`）的元素数量，
                    // 取目标跨度剩余需要填充的长度和下一个数据片段本身长度的最小值，确保不会超出范围。
                    int toCopy = Math.Min(nextSpan.Length, destination.Length - copied);
                    // 将下一个数据片段中相应数量的元素复制到目标跨度的剩余部分（通过切片操作定位到剩余部分），完成一部分数据复制操作。
                    nextSpan.Slice(0, toCopy).CopyTo(destination.Slice(copied));
                    // 更新已经复制的元素数量（`copied`），加上本次从下一个数据片段中复制的元素个数。
                    copied += toCopy;
                    // 判断已经复制的元素数量是否已经达到或超过了目标跨度（`destination`）的长度，如果达到或超过了，
                    // 说明已经成功完成了数据复制操作，跳出循环。
                    if (copied >= destination.Length)
                    {
                        break;
                    }
                }
            }

            // 返回最终的结果，即是否成功将数据复制到了目标跨度中，根据循环结束后的情况（是否填满目标跨度）来确定返回值是 true 还是 false。
            return true;
        }
    }
}
