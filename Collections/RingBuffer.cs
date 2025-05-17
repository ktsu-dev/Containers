// Copyright (c) ktsu.dev
// All rights reserved.
// Licensed under the MIT license.

namespace ktsu.Collections;

/// <summary>
/// Represents a fixed-size circular buffer (ring buffer) for storing elements of type <typeparamref name="T"/>.
/// </summary>
/// <typeparam name="T">The type of elements stored in the buffer.</typeparam>
public class RingBuffer<T>
{
	/// <summary>
	/// Gets or sets the internal buffer array.
	/// </summary>
	private T[] Buffer { get; set; } = [];

	/// <summary>
	/// Gets or sets the index of the most recently added element.
	/// </summary>
	private int BackIndex { get; set; }

	/// <summary>
	/// Gets or sets the index of the oldest element in the buffer.
	/// </summary>
	private int FrontIndex { get; set; }

	/// <summary>
	/// Gets or sets the capacity of the buffer (always a power of two).
	/// </summary>
	private int Capacity { get; set; }

	/// <summary>
	/// Gets or sets the logical length of the buffer (number of elements to store).
	/// </summary>
	private int Length { get; set; }

	/// <summary>
	/// Gets or sets the number of valid elements in the buffer.
	/// </summary>
	private int Count { get; set; } // Track number of valid elements

	/// <summary>
	/// Initializes a new instance of the <see cref="RingBuffer{T}"/> class with the specified length.
	/// </summary>
	/// <param name="length">The number of elements the buffer should store.</param>
	public RingBuffer(int length) => AllocateBuffer(length);

	/// <summary>
	/// Initializes a new instance of the <see cref="RingBuffer{T}"/> class with the specified items and length.
	/// If more items are provided than the buffer length, only the most recent items are kept.
	/// </summary>
	/// <param name="items">The items to prefill the buffer with.</param>
	/// <param name="length">The number of elements the buffer should store.</param>
	public RingBuffer(IEnumerable<T> items, int length) : this(length)
	{
		ArgumentNullException.ThrowIfNull(items);

		// Buffer only the last 'length' items if overfilled
		var queue = new Queue<T>(items);
		while (queue.Count > length)
		{
			queue.Dequeue();
		}

		foreach (var item in queue)
		{
			PushBack(item);
		}
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="RingBuffer{T}"/> class, prefilled with copies of a single value.
	/// </summary>
	/// <param name="value">The value to prefill the buffer with.</param>
	/// <param name="length">The number of elements the buffer should store.</param>
	public RingBuffer(T value, int length) : this(length)
	{
		for (var i = 0; i < length; i++)
		{
			PushBack(value);
		}
	}

	/// <summary>
	/// Allocates and initializes the internal buffer with the specified length.
	/// </summary>
	/// <param name="length">The number of elements to allocate space for.</param>
	private void AllocateBuffer(int length)
	{
		Length = length;
		Capacity = NextPower2(Length);
		Buffer = new T[Capacity];
		BackIndex = 0;
		FrontIndex = 0;
		Count = 0;
	}

	/// <summary>
	/// Adds an element to the back of the buffer, overwriting the oldest element if the buffer is full.
	/// </summary>
	/// <param name="o">The element to add.</param>
	public void PushBack(T o)
	{
		Buffer[BackIndex] = o;
		if (Count == Length)
		{
			FrontIndex = (FrontIndex + 1) & (Capacity - 1); // Overwrite oldest
		}
		else
		{
			Count++;
		}

		BackIndex = (BackIndex + 1) & (Capacity - 1);
	}

	/// <summary>
	/// Gets the element at the specified logical index in the buffer.
	/// </summary>
	/// <param name="index">The logical index of the element to retrieve.</param>
	/// <returns>The element at the specified index.</returns>
	public T At(int index)
	{
		if (index < 0 || index >= Count)
		{
			throw new ArgumentOutOfRangeException(nameof(index));
		}

		var idx = (FrontIndex + index) & (Capacity - 1);
		return Buffer[idx];
	}

	/// <summary>
	/// Gets the element at the front of the buffer (the oldest element).
	/// </summary>
	/// <returns>The front element.</returns>
	public T Front() => Count == 0 ? throw new InvalidOperationException("Buffer is empty") : Buffer[FrontIndex];

	/// <summary>
	/// Gets the element at the back of the buffer (the most recently added element).
	/// </summary>
	/// <returns>The back element.</returns>
	public T Back() => Count == 0 ? throw new InvalidOperationException("Buffer is empty") : Buffer[(BackIndex - 1 + Capacity) & (Capacity - 1)];

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

	/// <summary>
	/// Resizes the buffer to the specified length, discarding all current contents.
	/// </summary>
	/// <param name="length">The new length of the buffer.</param>
	public void Resize(int length) => AllocateBuffer(length);

	/// <summary>
	/// Resamples the buffer to a new length, interpolating or decimating the contents as needed.
	/// </summary>
	/// <param name="length">The new length of the buffer.</param>
	public void Resample(int length)
	{
		var oldBuffer = new T[Count];
		for (var i = 0; i < Count; i++)
		{
			oldBuffer[i] = At(i);
		}

		AllocateBuffer(length);

		if (oldBuffer.Length == 0)
		{
			return;
		}

		for (var i = 0; i < Length; i++)
		{
			var oldIndex = (int)Math.Round(i * (oldBuffer.Length - 1) / (double)Math.Max(Length - 1, 1));
			PushBack(oldBuffer[oldIndex]);
		}
	}
}
