// Copyright (c) ktsu.dev
// All rights reserved.
// Licensed under the MIT license.

namespace ktsu.Containers;

using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

/// <summary>
/// Represents a fixed-size circular buffer (ring buffer) for storing elements of type <typeparamref name="T"/>.
/// </summary>
/// <remarks>
/// A ring buffer is a data structure that uses a single, fixed-size buffer as if it were connected end-to-end.
/// This implementation uses a power-of-two size to optimize the modulo operations needed for wraparound,
/// replacing division with more efficient bitwise operations.
///
/// When the buffer reaches its capacity, adding more elements will overwrite the oldest elements.
///
/// This implementation supports:
/// <list type="bullet">
///   <item>Accessing elements by index, front, or back</item>
///   <item>Resizing the buffer (which clears existing elements)</item>
///   <item>Resampling the buffer (interpolating or decimating elements)</item>
///   <item>Enumeration through all elements</item>
///   <item>Clearing the buffer to remove all elements</item>
/// </list>
/// </remarks>
/// <typeparam name="T">The type of elements stored in the buffer.</typeparam>
[SuppressMessage("Naming", "CA1710:Identifiers should have correct suffix", Justification = "RingBuffer is a known collection name")]
public class RingBuffer<T> : IEnumerable<T>, IReadOnlyCollection<T>, IReadOnlyList<T>
{
	/// <summary>
	/// Gets or sets the internal buffer array.
	/// </summary>
	/// <remarks>
	/// The internal buffer size is always a power of two, which allows for efficient modulo operations
	/// using bitwise AND when wrapping around the buffer.
	/// </remarks>
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
	/// Gets the number of valid elements in the buffer.
	/// </summary>
	public int Count { get; private set; } // Track number of valid elements

	/// <summary>
	/// Gets the element at the specified index in the buffer.
	/// </summary>
	/// <param name="index">The index of the element to get.</param>
	/// <returns>The element at the specified index.</returns>
	public T this[int index] => At(index);

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
		// Convert to array for efficient access and use Skip() to take only the last 'length' items
		T[] itemsArray = [.. items];
		if (itemsArray.Length > length)
		{
			// Take only the last 'length' items
			itemsArray = [.. itemsArray.Skip(itemsArray.Length - length)];
		}

		foreach (T item in itemsArray)
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
		for (int i = 0; i < length; i++)
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
	/// <remarks>
	/// This operation has O(1) time complexity. When the buffer reaches its capacity,
	/// the oldest element is overwritten, and the front index is adjusted accordingly.
	///
	/// The wrapping logic uses bitwise AND with (Capacity - 1) which is equivalent to modulo
	/// operation when Capacity is a power of two. This is much faster than using the % operator.
	/// For example, if Capacity = 8 (binary 1000), then (Capacity - 1) = 7 (binary 0111).
	/// Any index &amp; 0111 will wrap values 0-7 back to 0-7, effectively implementing wraparound.
	/// </remarks>
	/// <param name="o">The element to add.</param>
	public void PushBack(T o)
	{
		Buffer[BackIndex] = o;
		if (Count == Length)
		{
			// Advance front index with wraparound using bitwise AND for efficiency
			FrontIndex = (FrontIndex + 1) & (Capacity - 1);
		}
		else
		{
			Count++;
		}

		// Advance back index with wraparound using bitwise AND for efficiency
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

		// Calculate actual buffer index with wraparound using bitwise AND for efficiency
		int idx = (FrontIndex + index) & (Capacity - 1);
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
	public T Back() => Count == 0 ? throw new InvalidOperationException("Buffer is empty") : Buffer[(BackIndex - 1 + Capacity) & (Capacity - 1)]; // Calculate previous index with wraparound using bitwise AND for efficiency

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
	/// <remarks>
	/// The resampling process preserves the data pattern by applying simple linear interpolation.
	/// If the new length is smaller than the old length, data is decimated (some values are dropped).
	/// If the new length is larger, data is interpolated (new values are created between existing ones).
	///
	/// This is useful when you need to change the buffer size while preserving the overall pattern of the data.
	/// For example, resampling time-series data when changing the sampling rate.
	///
	/// If the buffer is empty, this method will resize the buffer without adding any elements.
	/// </remarks>
	/// <param name="length">The new length of the buffer.</param>
	public void Resample(int length)
	{
		// Save the current count of valid elements
		int oldCount = Count;

		// Create a temporary array to store the current valid elements
		T[] oldData = new T[oldCount];
		for (int i = 0; i < oldCount; i++)
		{
			oldData[i] = At(i);
		}

		// Allocate new buffer with the new length
		AllocateBuffer(length);

		// If there were no elements in the old buffer, we're done
		if (oldCount == 0)
		{
			return;
		}

		// Resample the data into the new buffer
		for (int i = 0; i < length; i++)
		{
			// Map the new index to the old data range
			double oldIndex = i * (oldCount - 1) / (double)Math.Max(length - 1, 1);
			int index = (int)Math.Round(oldIndex);

			// Ensure we don't go out of bounds
			index = Math.Min(index, oldCount - 1);

			PushBack(oldData[index]);
		}
	}

	/// <summary>
	/// Returns an enumerator that iterates through the buffer.
	/// </summary>
	/// <returns>An enumerator for the buffer.</returns>
	public IEnumerator<T> GetEnumerator()
	{
		for (int i = 0; i < Count; i++)
		{
			yield return At(i);
		}
	}

	/// <summary>
	/// Returns an enumerator that iterates through the buffer.
	/// </summary>
	/// <returns>An enumerator for the buffer.</returns>
	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

	/// <summary>
	/// Removes all elements from the buffer without changing its capacity.
	/// </summary>
	/// <remarks>
	/// After calling <see cref="Clear"/>, <see cref="Count"/> will be zero and the buffer will be empty, but the capacity remains unchanged.
	/// </remarks>
	public void Clear()
	{
		BackIndex = 0;
		FrontIndex = 0;
		Count = 0;
	}
}
