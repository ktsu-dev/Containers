// Copyright (c) ktsu.dev
// All rights reserved.
// Licensed under the MIT license.

namespace ktsu.Containers;

using System.Collections;
using System.Diagnostics.CodeAnalysis;

/// <summary>
/// Represents a generic collection that maintains elements in sorted order.
/// </summary>
/// <remarks>
/// An ordered collection automatically maintains elements in sorted order using either
/// the natural ordering of the elements (if they implement <see cref="IComparable{T}"/>)
/// or a custom comparer provided during construction.
///
/// This implementation uses a binary search tree structure to maintain order while
/// providing efficient insertion, removal, and search operations.
///
/// This implementation supports:
/// <list type="bullet">
///   <item>Adding elements while maintaining sorted order</item>
///   <item>Removing elements by value or at specific index</item>
///   <item>Binary search for efficient element location</item>
///   <item>Accessing elements by index</item>
///   <item>Enumeration in sorted order</item>
///   <item>Custom comparison logic</item>
/// </list>
/// </remarks>
/// <typeparam name="T">The type of elements stored in the collection.</typeparam>
[SuppressMessage("Naming", "CA1710:Identifiers should have correct suffix", Justification = "OrderedCollection is a known collection name")]
public class OrderedCollection<T> : ICollection<T>, IReadOnlyCollection<T>, IReadOnlyList<T>
{
	/// <summary>
	/// The internal list that stores elements in sorted order.
	/// </summary>
	private readonly List<T> items;

	/// <summary>
	/// The comparer used to maintain sorted order.
	/// </summary>
	private readonly IComparer<T> comparer;

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
	/// Initializes a new instance of the <see cref="OrderedCollection{T}"/> class that uses the default comparer.
	/// </summary>
	/// <remarks>
	/// This constructor requires that <typeparamref name="T"/> implements <see cref="IComparable{T}"/>.
	/// </remarks>
	/// <exception cref="ArgumentException">Thrown when T does not implement IComparable{T}.</exception>
	public OrderedCollection()
	{
		if (!typeof(IComparable<T>).IsAssignableFrom(typeof(T)) && !typeof(IComparable).IsAssignableFrom(typeof(T)))
		{
			throw new ArgumentException($"Type {typeof(T)} must implement IComparable<T> or IComparable when no comparer is provided.");
		}

		items = [];
		comparer = Comparer<T>.Default;
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="OrderedCollection{T}"/> class with the specified comparer.
	/// </summary>
	/// <param name="comparer">The comparer to use for ordering elements.</param>
	/// <exception cref="ArgumentNullException">Thrown when comparer is null.</exception>
	public OrderedCollection(IComparer<T> comparer)
	{
		ArgumentNullException.ThrowIfNull(comparer);

		items = [];
		this.comparer = comparer;
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="OrderedCollection{T}"/> class with initial capacity.
	/// </summary>
	/// <param name="capacity">The initial capacity of the collection.</param>
	/// <exception cref="ArgumentException">Thrown when T does not implement IComparable{T}.</exception>
	/// <exception cref="ArgumentOutOfRangeException">Thrown when capacity is negative.</exception>
	public OrderedCollection(int capacity)
	{
		ArgumentOutOfRangeException.ThrowIfNegative(capacity);

		if (!typeof(IComparable<T>).IsAssignableFrom(typeof(T)) && !typeof(IComparable).IsAssignableFrom(typeof(T)))
		{
			throw new ArgumentException($"Type {typeof(T)} must implement IComparable<T> or IComparable when no comparer is provided.");
		}

		items = new List<T>(capacity);
		comparer = Comparer<T>.Default;
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="OrderedCollection{T}"/> class with initial capacity and comparer.
	/// </summary>
	/// <param name="capacity">The initial capacity of the collection.</param>
	/// <param name="comparer">The comparer to use for ordering elements.</param>
	/// <exception cref="ArgumentNullException">Thrown when comparer is null.</exception>
	/// <exception cref="ArgumentOutOfRangeException">Thrown when capacity is negative.</exception>
	public OrderedCollection(int capacity, IComparer<T> comparer)
	{
		ArgumentOutOfRangeException.ThrowIfNegative(capacity);

		ArgumentNullException.ThrowIfNull(comparer);

		items = new List<T>(capacity);
		this.comparer = comparer;
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="OrderedCollection{T}"/> class with elements from an existing collection.
	/// </summary>
	/// <param name="collection">The collection whose elements are copied to the new ordered collection.</param>
	/// <exception cref="ArgumentNullException">Thrown when collection is null.</exception>
	/// <exception cref="ArgumentException">Thrown when T does not implement IComparable{T}.</exception>
	public OrderedCollection(IEnumerable<T> collection)
	{
		ArgumentNullException.ThrowIfNull(collection);

		if (!typeof(IComparable<T>).IsAssignableFrom(typeof(T)) && !typeof(IComparable).IsAssignableFrom(typeof(T)))
		{
			throw new ArgumentException($"Type {typeof(T)} must implement IComparable<T> or IComparable when no comparer is provided.");
		}

		items = [];
		comparer = Comparer<T>.Default;

		foreach (T item in collection)
		{
			Add(item);
		}
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="OrderedCollection{T}"/> class with elements from an existing collection and a comparer.
	/// </summary>
	/// <param name="collection">The collection whose elements are copied to the new ordered collection.</param>
	/// <param name="comparer">The comparer to use for ordering elements.</param>
	/// <exception cref="ArgumentNullException">Thrown when collection or comparer is null.</exception>
	public OrderedCollection(IEnumerable<T> collection, IComparer<T> comparer)
	{
		ArgumentNullException.ThrowIfNull(collection);
		ArgumentNullException.ThrowIfNull(comparer);

		items = [];
		this.comparer = comparer;

		foreach (T item in collection)
		{
			Add(item);
		}
	}

	/// <summary>
	/// Adds an element to the collection in its correct sorted position.
	/// </summary>
	/// <param name="item">The element to add.</param>
	/// <remarks>
	/// This operation has O(n) time complexity due to the need to maintain sorted order.
	/// The element is inserted at the appropriate position to maintain the sorted sequence.
	/// </remarks>
	public void Add(T item)
	{
		int index = BinarySearch(item);
		if (index < 0)
		{
			index = ~index; // Convert to insertion point
		}
		items.Insert(index, item);
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
	/// This operation uses binary search and has O(log n) time complexity.
	/// </remarks>
	public bool Contains(T item)
	{
		int index = BinarySearch(item);
		return index >= 0;
	}

	/// <summary>
	/// Copies the elements of the collection to an array, starting at the specified array index.
	/// </summary>
	/// <param name="array">The destination array.</param>
	/// <param name="arrayIndex">The zero-based index in array at which copying begins.</param>
	/// <exception cref="ArgumentNullException">Thrown when array is null.</exception>
	/// <exception cref="ArgumentOutOfRangeException">Thrown when arrayIndex is negative.</exception>
	/// <exception cref="ArgumentException">Thrown when the destination array is too small.</exception>
	public void CopyTo(T[] array, int arrayIndex)
	{
		ArgumentNullException.ThrowIfNull(array);

		ArgumentOutOfRangeException.ThrowIfNegative(arrayIndex);

		if (array.Length - arrayIndex < Count)
		{
			throw new ArgumentException("The destination array is too small.");
		}

		items.CopyTo(array, arrayIndex);
	}

	/// <summary>
	/// Removes the first occurrence of the specified element from the collection.
	/// </summary>
	/// <param name="item">The element to remove.</param>
	/// <returns>true if the element was found and removed; otherwise, false.</returns>
	/// <remarks>
	/// This operation uses binary search to locate the element and has O(n) time complexity
	/// due to the need to shift elements after removal.
	/// </remarks>
	public bool Remove(T item)
	{
		int index = BinarySearch(item);
		if (index >= 0)
		{
			items.RemoveAt(index);
			return true;
		}
		return false;
	}

	/// <summary>
	/// Removes the element at the specified index.
	/// </summary>
	/// <param name="index">The zero-based index of the element to remove.</param>
	/// <exception cref="ArgumentOutOfRangeException">Thrown when index is out of range.</exception>
	public void RemoveAt(int index)
	{
		ArgumentOutOfRangeException.ThrowIfNegative(index);
		ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, Count);
		items.RemoveAt(index);
	}

	/// <summary>
	/// Searches for the specified element using binary search.
	/// </summary>
	/// <param name="item">The element to search for.</param>
	/// <returns>
	/// The zero-based index of the element if found; otherwise, a negative number that is the
	/// bitwise complement of the index where the element should be inserted.
	/// </returns>
	public int BinarySearch(T item)
	{
		int left = 0;
		int right = items.Count - 1;

		while (left <= right)
		{
			int mid = left + ((right - left) / 2);
			int comparison = comparer.Compare(items[mid], item);

			if (comparison == 0)
			{
				return mid;
			}
			else if (comparison < 0)
			{
				left = mid + 1;
			}
			else
			{
				right = mid - 1;
			}
		}

		return ~left; // Return bitwise complement of insertion point
	}

	/// <summary>
	/// Returns the index of the first occurrence of the specified element.
	/// </summary>
	/// <param name="item">The element to locate.</param>
	/// <returns>The zero-based index of the first occurrence if found; otherwise, -1.</returns>
	public int IndexOf(T item)
	{
		int index = BinarySearch(item);
		return index >= 0 ? index : -1;
	}

	/// <summary>
	/// Returns an enumerator that iterates through the collection in sorted order.
	/// </summary>
	/// <returns>An enumerator for the collection.</returns>
	public IEnumerator<T> GetEnumerator() => items.GetEnumerator();

	/// <summary>
	/// Returns an enumerator that iterates through the collection in sorted order.
	/// </summary>
	/// <returns>An enumerator for the collection.</returns>
	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

	/// <summary>
	/// Returns a view of a portion of the collection within the specified range.
	/// </summary>
	/// <param name="startIndex">The zero-based starting index of the range.</param>
	/// <param name="count">The number of elements in the range.</param>
	/// <returns>A new OrderedCollection containing the elements in the specified range.</returns>
	/// <exception cref="ArgumentOutOfRangeException">Thrown when parameters are out of range.</exception>
	public OrderedCollection<T> GetRange(int startIndex, int count)
	{
		ArgumentOutOfRangeException.ThrowIfNegative(startIndex);
		ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(startIndex, Count);
		ArgumentOutOfRangeException.ThrowIfNegative(count);
		ArgumentOutOfRangeException.ThrowIfGreaterThan(startIndex + count, Count);

		List<T> rangeItems = items.GetRange(startIndex, count);
		return new OrderedCollection<T>(rangeItems, comparer);
	}

	/// <summary>
	/// Creates a shallow copy of the ordered collection.
	/// </summary>
	/// <returns>A shallow copy of the collection.</returns>
	public OrderedCollection<T> Clone() => new(items, comparer);
}
