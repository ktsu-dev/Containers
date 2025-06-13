// Copyright (c) ktsu.dev
// All rights reserved.
// Licensed under the MIT license.

namespace ktsu.Containers;

using System.Collections;
using System.Diagnostics.CodeAnalysis;

/// <summary>
/// Represents a generic collection that maintains elements in insertion order.
/// </summary>
/// <remarks>
/// An insertion-order collection maintains elements in the exact order they were added,
/// preserving the insertion sequence without any sorting or reordering.
///
/// This implementation uses a simple list structure to maintain insertion order while
/// providing efficient append operations and indexed access.
///
/// This implementation supports:
/// <list type="bullet">
///   <item>Adding elements at the end (preserving insertion order)</item>
///   <item>Removing elements by value or at specific index</item>
///   <item>Accessing elements by index</item>
///   <item>Enumeration in insertion order</item>
///   <item>Linear search for element location</item>
/// </list>
/// </remarks>
/// <typeparam name="T">The type of elements stored in the collection.</typeparam>
[SuppressMessage("Naming", "CA1710:Identifiers should have correct suffix", Justification = "InsertionOrderCollection is a known collection name")]
public class InsertionOrderCollection<T> : ICollection<T>, IReadOnlyCollection<T>, IReadOnlyList<T>
{
	/// <summary>
	/// The internal list that stores elements in insertion order.
	/// </summary>
	private readonly List<T> items;

	/// <summary>
	/// Gets the number of elements in the collection.
	/// </summary>
	public int Count => items.Count;

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
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="InsertionOrderCollection{T}"/> class.
	/// </summary>
	public InsertionOrderCollection()
	{
		items = [];
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="InsertionOrderCollection{T}"/> class with initial capacity.
	/// </summary>
	/// <param name="capacity">The initial capacity of the collection.</param>
	/// <exception cref="ArgumentOutOfRangeException">Thrown when capacity is negative.</exception>
	public InsertionOrderCollection(int capacity)
	{
		ArgumentOutOfRangeException.ThrowIfNegative(capacity);
		items = new List<T>(capacity);
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="InsertionOrderCollection{T}"/> class with elements from an existing collection.
	/// </summary>
	/// <param name="collection">The collection whose elements are copied to the new insertion-order collection.</param>
	/// <exception cref="ArgumentNullException">Thrown when collection is null.</exception>
	public InsertionOrderCollection(IEnumerable<T> collection)
	{
		ArgumentNullException.ThrowIfNull(collection);
		items = new List<T>(collection);
	}

	/// <summary>
	/// Adds an element to the end of the collection, preserving insertion order.
	/// </summary>
	/// <param name="item">The element to add.</param>
	/// <remarks>
	/// This operation has O(1) amortized time complexity.
	/// The element is added at the end of the collection to maintain insertion order.
	/// </remarks>
	public void Add(T item)
	{
		items.Add(item);
	}

	/// <summary>
	/// Removes all elements from the collection.
	/// </summary>
	public void Clear() => items.Clear();

	/// <summary>
	/// Determines whether the collection contains the specified element.
	/// </summary>
	/// <param name="item">The element to locate.</param>
	/// <returns>true if the element is found; otherwise, false.</returns>
	/// <remarks>
	/// This operation has O(n) time complexity as it performs a linear search.
	/// </remarks>
	public bool Contains(T item)
	{
		return items.Contains(item);
	}

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

		items.CopyTo(array, arrayIndex);
	}

	/// <summary>
	/// Removes the first occurrence of the specified element from the collection.
	/// </summary>
	/// <param name="item">The element to remove.</param>
	/// <returns>true if the element was found and removed; otherwise, false.</returns>
	/// <remarks>
	/// This operation has O(n) time complexity as it performs a linear search
	/// and may need to shift elements to maintain insertion order.
	/// </remarks>
	public bool Remove(T item)
	{
		return items.Remove(item);
	}

	/// <summary>
	/// Removes the element at the specified index.
	/// </summary>
	/// <param name="index">The zero-based index of the element to remove.</param>
	/// <exception cref="ArgumentOutOfRangeException">Thrown when index is out of range.</exception>
	/// <remarks>
	/// This operation has O(n) time complexity as it may need to shift elements
	/// to maintain insertion order.
	/// </remarks>
	public void RemoveAt(int index)
	{
		ArgumentOutOfRangeException.ThrowIfNegative(index);
		ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, Count);

		items.RemoveAt(index);
	}

	/// <summary>
	/// Searches for the specified element and returns the zero-based index of the first occurrence.
	/// </summary>
	/// <param name="item">The element to locate.</param>
	/// <returns>The zero-based index of the first occurrence of the element, or -1 if not found.</returns>
	/// <remarks>
	/// This operation has O(n) time complexity as it performs a linear search.
	/// </remarks>
	public int IndexOf(T item)
	{
		return items.IndexOf(item);
	}

	/// <summary>
	/// Returns an enumerator that iterates through the collection in insertion order.
	/// </summary>
	/// <returns>An enumerator for the collection.</returns>
	public IEnumerator<T> GetEnumerator() => items.GetEnumerator();

	/// <summary>
	/// Returns an enumerator that iterates through the collection.
	/// </summary>
	/// <returns>An enumerator for the collection.</returns>
	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

	/// <summary>
	/// Creates a shallow copy of a range of elements in the collection.
	/// </summary>
	/// <param name="startIndex">The zero-based starting index of the range.</param>
	/// <param name="count">The number of elements in the range.</param>
	/// <returns>A new collection containing the specified range of elements.</returns>
	/// <exception cref="ArgumentOutOfRangeException">Thrown when startIndex or count is invalid.</exception>
	public InsertionOrderCollection<T> GetRange(int startIndex, int count)
	{
		ArgumentOutOfRangeException.ThrowIfNegative(startIndex);
		ArgumentOutOfRangeException.ThrowIfNegative(count);
		ArgumentOutOfRangeException.ThrowIfGreaterThan(startIndex + count, Count);

		return new InsertionOrderCollection<T>(items.GetRange(startIndex, count));
	}

	/// <summary>
	/// Creates a shallow copy of the collection.
	/// </summary>
	/// <returns>A new collection containing all elements from the original collection.</returns>
	public InsertionOrderCollection<T> Clone() => new(items);
}
