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
/// One slot is reserved internally to distinguish the full and empty states without a
/// separate count, so the usable <see cref="Capacity"/> is one less than the (power-of-two)
/// internal array length.
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
	/// The backing store. Its length is always a power of two so that index wrapping can use
	/// a bitwise AND with <see cref="mask"/> instead of a modulo.
	/// </summary>
	private readonly T[] buffer;

	/// <summary>
	/// Bitmask equal to <c>buffer.Length - 1</c>, used to wrap indices.
	/// </summary>
	private readonly int mask;

	/// <summary>
	/// Index of the next element to be read. Owned by the consumer; read by the producer.
	/// </summary>
	private SpscPaddedIndex head;

	/// <summary>
	/// Index of the next slot to be written. Owned by the producer; read by the consumer.
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
	/// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="capacity"/> is less than one.</exception>
	public SpscRingBuffer(int capacity)
	{
		ArgumentOutOfRangeException.ThrowIfNegativeOrZero(capacity);

		// Reserve one slot to disambiguate full vs empty, then round up to a power of two.
		int arrayLength = NextPower2(capacity + 1);
		buffer = new T[arrayLength];
		mask = arrayLength - 1;
		Capacity = arrayLength - 1;
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
	/// the true count may have changed by the time the value is returned.
	/// </remarks>
	public int Count
	{
		get
		{
			int currentTail = Volatile.Read(ref tail.Value);
			int currentHead = Volatile.Read(ref head.Value);
			return (currentTail - currentHead) & mask;
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
		int nextTail = (currentTail + 1) & mask;

		// Full when advancing Tail would collide with the consumer's Head.
		// Acquire the consumer's published Head.
		if (nextTail == Volatile.Read(ref head.Value))
		{
			return false;
		}

		buffer[currentTail] = item;

		// Release: publish the written slot to the consumer.
		Volatile.Write(ref tail.Value, nextTail);
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

		item = buffer[currentHead];

		// Avoid keeping a reference alive for reference types (no-op cost for value types
		// is negligible and the JIT elides it for non-reference T).
		buffer[currentHead] = default!;

		// Release: publish the freed slot to the producer.
		Volatile.Write(ref head.Value, (currentHead + 1) & mask);
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

		item = buffer[currentHead];
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
