// Copyright (c) 2023-2026 ktsu-dev contributors

namespace ktsu.Containers;

using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Threading;

/// <summary>
/// A lock-free, wait-free, fixed-capacity single-producer/single-consumer (SPSC) ring buffer.
/// </summary>
/// <remarks>
/// This buffer is designed for handing data across a thread boundary without locks or
/// allocations on the hot path, which makes it suitable for real-time scenarios such as
/// publishing meter/scope telemetry from an audio callback to a UI thread.
///
/// <para>
/// <b>Threading contract.</b> Exactly one thread may call the producer operations
/// (<see cref="TryEnqueue"/>) and exactly one (different or same) thread may call the
/// consumer operations (<see cref="TryDequeue"/>). Concurrent use by more than one
/// producer or more than one consumer is undefined behaviour. The producer and consumer
/// may run fully concurrently with respect to each other.
/// </para>
///
/// <para>
/// <b>Real-time safety.</b> After construction, neither <see cref="TryEnqueue"/> nor
/// <see cref="TryDequeue"/> allocates, takes a lock, or blocks. Both are O(1) and never
/// throw. The producer side is therefore safe to call from an audio/render thread; the
/// consumer side is intended for the UI/worker thread.
/// </para>
///
/// <para>
/// Correctness relies on acquire/release ordering: the producer publishes the written
/// slot by releasing the tail index, and the consumer observes it by acquiring the tail
/// index (and vice versa for the head index). On the .NET memory model this is provided by
/// <see cref="Volatile"/> acquire/release reads and writes.
/// </para>
///
/// <para>
/// The head and tail indices are free-running counters, so full (the tail a whole array ahead
/// of the head) and empty (the two equal) are told apart without reserving a slot or keeping a
/// separate count: <see cref="Capacity"/> is the whole (power-of-two) internal array length.
/// </para>
/// </remarks>
/// <typeparam name="T">The type of elements stored in the buffer.</typeparam>
[SuppressMessage(
	"Naming",
	"CA1711:Identifiers should not have incorrect suffix",
	Justification = "Buffer accurately describes the ring buffer."
)]
public sealed class SpscRingBuffer<T>
{
	/// <summary>
	/// The largest capacity accepted: the backing array is a power of two no larger than 2^30. At that
	/// size a full buffer's <c>tail - head</c> is still 2^30, well clear of the 32-bit counters wrapping
	/// into an empty one.
	/// </summary>
	private const int MaxCapacity = 1 << 30;

	/// <summary>
	/// The backing store. Its length is always a power of two so that index wrapping can use
	/// a bitwise AND with <see cref="mask"/> instead of a modulo.
	/// </summary>
	private readonly T[] buffer;

	/// <summary>
	/// Bitmask equal to <c>buffer.Length - 1</c>, used to wrap indices.
	/// </summary>
	private readonly int mask;

	/// <summary>
	/// Number of elements ever read, wrapping at 2^32. Owned by the consumer; read by the producer.
	/// It is masked only when indexing <see cref="buffer"/>, so it does not repeat after one lap.
	/// </summary>
	private SpscPaddedIndex head;

	/// <summary>
	/// Number of elements ever written, wrapping at 2^32. Owned by the producer; read by the consumer.
	/// It is masked only when indexing <see cref="buffer"/>, so it does not repeat after one lap.
	/// </summary>
	private SpscPaddedIndex tail;

	/// <summary>
	/// Gets the maximum number of elements the buffer can hold at once.
	/// </summary>
	public int Capacity { get; }

	/// <summary>
	/// Initializes a new instance of the <see cref="SpscRingBuffer{T}"/> class able to hold at
	/// least <paramref name="capacity"/> elements.
	/// </summary>
	/// <param name="capacity">The minimum number of elements the buffer must be able to hold.</param>
	/// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="capacity"/> is less than one or greater than 2^30.</exception>
	public SpscRingBuffer(int capacity)
	{
		ArgumentOutOfRangeException.ThrowIfNegativeOrZero(capacity);
		ArgumentOutOfRangeException.ThrowIfGreaterThan(capacity, MaxCapacity);

		// Round up to a power of two. The free-running indices tell full from empty, so no slot is reserved.
		int arrayLength = NextPower2(capacity);
		buffer = new T[arrayLength];
		mask = arrayLength - 1;
		Capacity = arrayLength;
	}

	/// <summary>
	/// Gets a value indicating whether the buffer currently appears empty.
	/// </summary>
	/// <remarks>
	/// The result is a snapshot and may be stale the moment it is read if the other thread is
	/// active. It is exact only when observed from the consumer thread with no concurrent producer.
	/// </remarks>
	public bool IsEmpty => Volatile.Read(ref head.Value) == Volatile.Read(ref tail.Value);

	/// <summary>
	/// Gets the approximate number of elements currently stored in the buffer.
	/// </summary>
	/// <remarks>
	/// This is a best-effort snapshot intended for diagnostics/metering. Under concurrent access
	/// the true count may have changed by the time the value is returned, but it is always a count
	/// the buffer actually held while the property was being read, even when read from a thread
	/// that is neither the producer nor the consumer.
	/// </remarks>
	public int Count
	{
		get
		{
			// Head and tail are read one after the other, so between the two reads the producer and
			// consumer can both move on. If head passed the tail that was read, the difference
			// would wrap to a value that the buffer never held. Reading head on both
			// sides of tail, and retrying until it did not move, gives a tail observed while head
			// held that value, so the difference is a count the buffer really had.
			while (true)
			{
				int currentHead = Volatile.Read(ref head.Value);
				int currentTail = Volatile.Read(ref tail.Value);
				if (Volatile.Read(ref head.Value) == currentHead)
				{
					// Head and tail are free-running, so a matching head means the same lap: a stale
					// match would need 2^32 dequeues between the two reads.
					return unchecked(currentTail - currentHead);
				}
			}
		}
	}

	/// <summary>
	/// Attempts to add an element to the buffer. Must only be called from the single producer thread.
	/// </summary>
	/// <param name="item">The element to add.</param>
	/// <returns><see langword="true"/> if the element was added; <see langword="false"/> if the buffer was full.</returns>
	/// <remarks>This operation is lock-free, allocation-free, and never blocks or throws.</remarks>
	public bool TryEnqueue(T item)
	{
		// The producer owns Tail, so a plain read is sufficient here.
		int currentTail = tail.Value;

		// Full when Tail is a whole Capacity ahead of the consumer's Head.
		// Acquire the consumer's published Head.
		if (unchecked(currentTail - Volatile.Read(ref head.Value)) == Capacity)
		{
			return false;
		}

		buffer[currentTail & mask] = item;

		// Release: publish the written slot to the consumer.
		Volatile.Write(ref tail.Value, unchecked(currentTail + 1));
		return true;
	}

	/// <summary>
	/// Attempts to remove and return the oldest element in the buffer. Must only be called from the
	/// single consumer thread.
	/// </summary>
	/// <param name="item">When this method returns <see langword="true"/>, contains the dequeued element; otherwise the default value.</param>
	/// <returns><see langword="true"/> if an element was removed; <see langword="false"/> if the buffer was empty.</returns>
	/// <remarks>This operation is lock-free, allocation-free, and never blocks or throws.</remarks>
	public bool TryDequeue([MaybeNullWhen(false)] out T item)
	{
		// The consumer owns Head, so a plain read is sufficient here.
		int currentHead = head.Value;

		// Empty when Head has caught up to the producer's published Tail. Acquire Tail.
		if (currentHead == Volatile.Read(ref tail.Value))
		{
			item = default;
			return false;
		}

		int index = currentHead & mask;
		item = buffer[index];

		// Avoid keeping a reference alive for reference types (no-op cost for value types
		// is negligible and the JIT elides it for non-reference T).
		buffer[index] = default!;

		// Release: publish the freed slot to the producer.
		Volatile.Write(ref head.Value, unchecked(currentHead + 1));
		return true;
	}

	/// <summary>
	/// Attempts to return the oldest element without removing it. Must only be called from the
	/// single consumer thread.
	/// </summary>
	/// <param name="item">When this method returns <see langword="true"/>, contains the oldest element; otherwise the default value.</param>
	/// <returns><see langword="true"/> if an element was available; <see langword="false"/> if the buffer was empty.</returns>
	public bool TryPeek([MaybeNullWhen(false)] out T item)
	{
		int currentHead = head.Value;
		if (currentHead == Volatile.Read(ref tail.Value))
		{
			item = default;
			return false;
		}

		item = buffer[currentHead & mask];
		return true;
	}

	/// <summary>
	/// Calculates the next power of two greater than or equal to the specified value.
	/// </summary>
	/// <param name="v">The value to round up.</param>
	/// <returns>The next power of two.</returns>
	private static int NextPower2(int v)
	{
		v--;
		v |= v >> 1;
		v |= v >> 2;
		v |= v >> 4;
		v |= v >> 8;
		v |= v >> 16;
		v++;
		return v;
	}
}

/// <summary>
/// A 32-bit index padded onto its own cache line to avoid false sharing between the producer and
/// consumer of a <see cref="SpscRingBuffer{T}"/>.
/// </summary>
/// <remarks>
/// This is a non-generic struct so that it is permitted to carry an explicit layout (a struct nested
/// in a generic type cannot). The value sits at offset 64 with 64 bytes of trailing padding, isolating
/// it from any adjacent field on a typical 64-byte cache line.
/// </remarks>
[StructLayout(LayoutKind.Explicit, Size = 128)]
internal struct SpscPaddedIndex
{
	[FieldOffset(64)]
	public int Value;
}
