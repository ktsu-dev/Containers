// Copyright (c) ktsu.dev
// All rights reserved.
// Licensed under the MIT license.

namespace ktsu.Containers;

using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

/// <summary>
/// Represents a generic collection that guarantees contiguous memory allocation for optimal cache performance.
/// </summary>
/// <remarks>
/// A contiguous memory collection maintains all elements in a single contiguous block of memory,
/// optimizing cache locality and memory access patterns for better performance.
///
/// This implementation uses a managed array as the backing store, ensuring that all elements
/// are stored contiguously in memory. The collection automatically grows as needed while
/// maintaining the contiguous memory guarantee.
///
/// This implementation supports:
/// <list type="bullet">
///   <item>Adding elements while maintaining contiguous memory layout</item>
///   <item>Removing elements by value or at specific index</item>
///   <item>Accessing elements by index with optimal cache performance</item>
///   <item>Enumeration with cache-friendly sequential access</item>
///   <item>Guaranteed contiguous memory allocation</item>
/// </list>
///
/// Performance characteristics:
/// <list type="bullet">
///   <item>Add: O(1) amortized, O(n) worst case when resizing</item>
///   <item>Access by index: O(1) with optimal cache performance</item>
///   <item>Remove: O(n) due to element shifting</item>
///   <item>Contains: O(n) linear search</item>
/// </list>
/// </remarks>
/// <typeparam name="T">The type of elements stored in the collection.</typeparam>
[SuppressMessage("Naming", "CA1710:Identifiers should have correct suffix", Justification = "ContiguousCollection is a known collection name")]
public class ContiguousCollection<T> : ICollection<T>, IReadOnlyCollection<T>, IReadOnlyList<T>
{
	/// <summary>
	/// The backing array that stores elements in contiguous memory.
	/// </summary>
	private T[] items;

	/// <summary>
	/// The default initial capacity for the collection.
	/// </summary>
	private const int DefaultCapacity = 4;

	/// <summary>
	/// Gets the number of elements in the collection.
	/// </summary>
	public int Count { get; private set; }

	/// <summary>
	/// Gets the current capacity of the collection.
	/// </summary>
	public int Capacity => items.Length;

	/// <summary>
	/// Gets a value indicating whether the collection is read-only.
	/// </summary>
	public bool IsReadOnly => false;

	/// <summary>
	/// Gets the element at the specified index.
	/// </summary>
	/// <param name="index">The zero-based index of the element to get.</param>
	/// <returns>The element at the specified index.</returns>
	/// <exception cref="ArgumentOutOfRangeException">Thrown when index is out of range.</exception>
	public T this[int index]
	{
		get
		{
			ArgumentOutOfRangeException.ThrowIfNegative(index);
			ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, Count);
			return items[index];
		}
		set
		{
			ArgumentOutOfRangeException.ThrowIfNegative(index);
			ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, Count);
			items[index] = value;
		}
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="ContiguousCollection{T}"/> class with default capacity.
	/// </summary>
	public ContiguousCollection()
	{
		items = new T[DefaultCapacity];
		Count = 0;
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="ContiguousCollection{T}"/> class with the specified initial capacity.
	/// </summary>
	/// <param name="capacity">The initial capacity of the collection.</param>
	/// <exception cref="ArgumentOutOfRangeException">Thrown when capacity is negative.</exception>
	public ContiguousCollection(int capacity)
	{
		ArgumentOutOfRangeException.ThrowIfNegative(capacity);

		items = capacity == 0 ? [] : new T[capacity];
		Count = 0;
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="ContiguousCollection{T}"/> class with elements from an existing collection.
	/// </summary>
	/// <param name="collection">The collection whose elements are copied to the new contiguous collection.</param>
	/// <exception cref="ArgumentNullException">Thrown when collection is null.</exception>
	public ContiguousCollection(IEnumerable<T> collection)
	{
		ArgumentNullException.ThrowIfNull(collection);

		if (collection is ICollection<T> col)
		{
			int capacity = col.Count;
			items = capacity == 0 ? [] : new T[capacity];
			col.CopyTo(items, 0);
			Count = capacity;
		}
		else
		{
			items = new T[DefaultCapacity];
			Count = 0;
			foreach (T item in collection)
			{
				Add(item);
			}
		}
	}

	/// <summary>
	/// Adds an element to the end of the collection.
	/// </summary>
	/// <param name="item">The element to add.</param>
	/// <remarks>
	/// This operation has O(1) amortized time complexity. When the collection needs to grow,
	/// a new contiguous memory block is allocated and all elements are copied to maintain
	/// the contiguous memory guarantee.
	/// </remarks>
	public void Add(T item)
	{
		if (Count == items.Length)
		{
			Grow();
		}

		items[Count] = item;
		Count++;
	}

	/// <summary>
	/// Removes all elements from the collection.
	/// </summary>
	public void Clear()
	{
		if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
		{
			// Clear references to help GC
			Array.Clear(items, 0, Count);
		}
		Count = 0;
	}

	/// <summary>
	/// Determines whether the collection contains the specified element.
	/// </summary>
	/// <param name="item">The element to locate.</param>
	/// <returns>true if the element is found; otherwise, false.</returns>
	/// <remarks>
	/// This operation has O(n) time complexity as it performs a linear search.
	/// However, the contiguous memory layout provides optimal cache performance during the search.
	/// </remarks>
	public bool Contains(T item) => IndexOf(item) >= 0;

	/// <summary>
	/// Copies the elements of the collection to an array, starting at the specified array index.
	/// </summary>
	/// <param name="array">The destination array.</param>
	/// <param name="arrayIndex">The zero-based index in the destination array at which copying begins.</param>
	/// <exception cref="ArgumentNullException">Thrown when array is null.</exception>
	/// <exception cref="ArgumentOutOfRangeException">Thrown when arrayIndex is negative.</exception>
	/// <exception cref="ArgumentException">Thrown when the destination array is too small.</exception>
	public void CopyTo(T[] array, int arrayIndex)
	{
		ArgumentNullException.ThrowIfNull(array);
		ArgumentOutOfRangeException.ThrowIfNegative(arrayIndex);
		ArgumentOutOfRangeException.ThrowIfGreaterThan(arrayIndex, array.Length);
		ArgumentOutOfRangeException.ThrowIfGreaterThan(Count, array.Length - arrayIndex);

		Array.Copy(items, 0, array, arrayIndex, Count);
	}

	/// <summary>
	/// Removes the first occurrence of the specified element from the collection.
	/// </summary>
	/// <param name="item">The element to remove.</param>
	/// <returns>true if the element was found and removed; otherwise, false.</returns>
	/// <remarks>
	/// This operation has O(n) time complexity as it performs a linear search
	/// and may need to shift elements to maintain contiguous memory layout.
	/// </remarks>
	public bool Remove(T item)
	{
		int index = IndexOf(item);
		if (index < 0)
		{
			return false;
		}

		RemoveAt(index);
		return true;
	}

	/// <summary>
	/// Removes the element at the specified index.
	/// </summary>
	/// <param name="index">The zero-based index of the element to remove.</param>
	/// <exception cref="ArgumentOutOfRangeException">Thrown when index is out of range.</exception>
	/// <remarks>
	/// This operation has O(n) time complexity as it may need to shift elements
	/// to maintain contiguous memory layout.
	/// </remarks>
	public void RemoveAt(int index)
	{
		ArgumentOutOfRangeException.ThrowIfNegative(index);
		ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, Count);

		Count--;
		if (index < Count)
		{
			Array.Copy(items, index + 1, items, index, Count - index);
		}

		if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
		{
			items[Count] = default!;
		}
	}

	/// <summary>
	/// Searches for the specified element and returns the zero-based index of the first occurrence.
	/// </summary>
	/// <param name="item">The element to locate.</param>
	/// <returns>The zero-based index of the first occurrence of the element, or -1 if not found.</returns>
	/// <remarks>
	/// This operation has O(n) time complexity as it performs a linear search.
	/// The contiguous memory layout provides optimal cache performance during the search.
	/// </remarks>
	public int IndexOf(T item) => Array.IndexOf(items, item, 0, Count);

	/// <summary>
	/// Inserts an element at the specified index.
	/// </summary>
	/// <param name="index">The zero-based index at which to insert the element.</param>
	/// <param name="item">The element to insert.</param>
	/// <exception cref="ArgumentOutOfRangeException">Thrown when index is out of range.</exception>
	/// <remarks>
	/// This operation has O(n) time complexity as it may need to shift elements
	/// and potentially grow the collection to maintain contiguous memory layout.
	/// </remarks>
	public void Insert(int index, T item)
	{
		ArgumentOutOfRangeException.ThrowIfNegative(index);
		ArgumentOutOfRangeException.ThrowIfGreaterThan(index, Count);

		if (Count == items.Length)
		{
			Grow();
		}

		if (index < Count)
		{
			Array.Copy(items, index, items, index + 1, Count - index);
		}

		items[index] = item;
		Count++;
	}

	/// <summary>
	/// Ensures that the collection has enough capacity for the specified number of elements.
	/// </summary>
	/// <param name="capacity">The minimum capacity required.</param>
	/// <exception cref="ArgumentOutOfRangeException">Thrown when capacity is negative.</exception>
	/// <remarks>
	/// This method can be used to pre-allocate memory and avoid multiple reallocations
	/// when the final size is known in advance. This maintains the contiguous memory guarantee.
	/// </remarks>
	public void EnsureCapacity(int capacity)
	{
		ArgumentOutOfRangeException.ThrowIfNegative(capacity);

		if (items.Length < capacity)
		{
			Grow(capacity);
		}
	}

	/// <summary>
	/// Sets the capacity to the actual number of elements, reducing memory usage.
	/// </summary>
	/// <remarks>
	/// This method creates a new contiguous memory block sized exactly for the current elements,
	/// potentially reducing memory usage while maintaining the contiguous memory guarantee.
	/// </remarks>
	public void TrimExcess()
	{
		if (Count < items.Length * 0.9) // Only trim if there's significant unused space
		{
			T[] newItems = Count == 0 ? [] : new T[Count];
			Array.Copy(items, newItems, Count);
			items = newItems;
		}
	}

	/// <summary>
	/// Returns an enumerator that iterates through the collection.
	/// </summary>
	/// <returns>An enumerator for the collection.</returns>
	/// <remarks>
	/// Enumeration benefits from the contiguous memory layout with optimal cache performance.
	/// </remarks>
	public IEnumerator<T> GetEnumerator()
	{
		for (int i = 0; i < Count; i++)
		{
			yield return items[i];
		}
	}

	/// <summary>
	/// Returns an enumerator that iterates through the collection.
	/// </summary>
	/// <returns>An enumerator for the collection.</returns>
	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

	/// <summary>
	/// Creates a shallow copy of the collection.
	/// </summary>
	/// <returns>A new collection containing all elements from the original collection with contiguous memory layout.</returns>
	public ContiguousCollection<T> Clone()
	{
		ContiguousCollection<T> clone = new(Count);
		Array.Copy(items, clone.items, Count);
		clone.Count = Count;
		return clone;
	}

	/// <summary>
	/// Creates a shallow copy of a range of elements in the collection.
	/// </summary>
	/// <param name="startIndex">The zero-based starting index of the range.</param>
	/// <param name="count">The number of elements in the range.</param>
	/// <returns>A new collection containing the specified range of elements with contiguous memory layout.</returns>
	/// <exception cref="ArgumentOutOfRangeException">Thrown when startIndex or count is invalid.</exception>
	public ContiguousCollection<T> GetRange(int startIndex, int count)
	{
		ArgumentOutOfRangeException.ThrowIfNegative(startIndex);
		ArgumentOutOfRangeException.ThrowIfNegative(count);
		ArgumentOutOfRangeException.ThrowIfGreaterThan(startIndex + count, this.Count);

		ContiguousCollection<T> result = new(count);
		Array.Copy(items, startIndex, result.items, 0, count);
		result.Count = count;
		return result;
	}

	/// <summary>
	/// Gets a span representing the elements in the collection.
	/// </summary>
	/// <returns>A span over the collection's elements.</returns>
	/// <remarks>
	/// This method provides direct access to the contiguous memory, enabling high-performance
	/// operations and interoperability with other APIs that work with spans.
	/// </remarks>
	public Span<T> AsSpan() => new(items, 0, Count);

	/// <summary>
	/// Gets a read-only span representing the elements in the collection.
	/// </summary>
	/// <returns>A read-only span over the collection's elements.</returns>
	/// <remarks>
	/// This method provides direct read-only access to the contiguous memory, enabling high-performance
	/// operations while preventing modifications.
	/// </remarks>
	public ReadOnlySpan<T> AsReadOnlySpan() => new(items, 0, Count);

	/// <summary>
	/// Grows the collection's capacity.
	/// </summary>
	/// <param name="minimumCapacity">The minimum required capacity.</param>
	private void Grow(int minimumCapacity = 0)
	{
		int newCapacity = items.Length == 0 ? DefaultCapacity : items.Length * 2;
		if (newCapacity < minimumCapacity)
		{
			newCapacity = minimumCapacity;
		}

		T[] newItems = new T[newCapacity];
		Array.Copy(items, newItems, Count);
		items = newItems;
	}
}
