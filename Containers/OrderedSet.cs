// Copyright (c) ktsu.dev
// All rights reserved.
// Licensed under the MIT license.

namespace ktsu.Containers;

using System.Collections;
using System.Diagnostics.CodeAnalysis;

/// <summary>
/// Represents a generic set that maintains unique elements in sorted order.
/// </summary>
/// <remarks>
/// An ordered set automatically maintains unique elements in sorted order using either
/// the natural ordering of the elements (if they implement <see cref="IComparable{T}"/>)
/// or a custom comparer provided during construction.
///
/// This implementation uses a binary search tree structure to maintain order while
/// providing efficient insertion, removal, and search operations. Unlike OrderedCollection,
/// this set ensures all elements are unique.
///
/// This implementation supports:
/// <list type="bullet">
///   <item>Adding elements while maintaining sorted order and uniqueness</item>
///   <item>Removing elements by value</item>
///   <item>Binary search for efficient element location</item>
///   <item>Set operations like union, intersection, and difference</item>
///   <item>Enumeration in sorted order</item>
///   <item>Custom comparison logic</item>
/// </list>
/// </remarks>
/// <typeparam name="T">The type of elements stored in the set.</typeparam>
[SuppressMessage("Naming", "CA1710:Identifiers should have correct suffix", Justification = "OrderedSet is a known collection name")]
public class OrderedSet<T> : ISet<T>, IReadOnlySet<T>, IReadOnlyCollection<T>
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
	/// Gets the number of elements in the set.
	/// </summary>
	public int Count => items.Count;

	/// <summary>
	/// Gets a value indicating whether the set is read-only.
	/// </summary>
	public bool IsReadOnly => false;

	/// <summary>
	/// Initializes a new instance of the <see cref="OrderedSet{T}"/> class that uses the default comparer.
	/// </summary>
	/// <remarks>
	/// This constructor requires that <typeparamref name="T"/> implements <see cref="IComparable{T}"/>.
	/// </remarks>
	/// <exception cref="ArgumentException">Thrown when T does not implement IComparable{T}.</exception>
	public OrderedSet()
	{
		if (!typeof(IComparable<T>).IsAssignableFrom(typeof(T)) && !typeof(IComparable).IsAssignableFrom(typeof(T)))
		{
			throw new ArgumentException($"Type {typeof(T)} must implement IComparable<T> or IComparable when no comparer is provided.");
		}

		items = [];
		comparer = Comparer<T>.Default;
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="OrderedSet{T}"/> class with the specified comparer.
	/// </summary>
	/// <param name="comparer">The comparer to use for ordering elements.</param>
	/// <exception cref="ArgumentNullException">Thrown when comparer is null.</exception>
	public OrderedSet(IComparer<T> comparer)
	{
		ArgumentNullException.ThrowIfNull(comparer);

		items = [];
		this.comparer = comparer;
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="OrderedSet{T}"/> class with initial capacity.
	/// </summary>
	/// <param name="capacity">The initial capacity of the set.</param>
	/// <exception cref="ArgumentException">Thrown when T does not implement IComparable{T}.</exception>
	/// <exception cref="ArgumentOutOfRangeException">Thrown when capacity is negative.</exception>
	public OrderedSet(int capacity)
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
	/// Initializes a new instance of the <see cref="OrderedSet{T}"/> class with initial capacity and comparer.
	/// </summary>
	/// <param name="capacity">The initial capacity of the set.</param>
	/// <param name="comparer">The comparer to use for ordering elements.</param>
	/// <exception cref="ArgumentNullException">Thrown when comparer is null.</exception>
	/// <exception cref="ArgumentOutOfRangeException">Thrown when capacity is negative.</exception>
	public OrderedSet(int capacity, IComparer<T> comparer)
	{
		ArgumentOutOfRangeException.ThrowIfNegative(capacity);
		ArgumentNullException.ThrowIfNull(comparer);

		items = new List<T>(capacity);
		this.comparer = comparer;
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="OrderedSet{T}"/> class with elements from an existing collection.
	/// </summary>
	/// <param name="collection">The collection whose elements are copied to the new ordered set.</param>
	/// <exception cref="ArgumentNullException">Thrown when collection is null.</exception>
	/// <exception cref="ArgumentException">Thrown when T does not implement IComparable{T}.</exception>
	public OrderedSet(IEnumerable<T> collection)
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
	/// Initializes a new instance of the <see cref="OrderedSet{T}"/> class with elements from an existing collection and a comparer.
	/// </summary>
	/// <param name="collection">The collection whose elements are copied to the new ordered set.</param>
	/// <param name="comparer">The comparer to use for ordering elements.</param>
	/// <exception cref="ArgumentNullException">Thrown when collection or comparer is null.</exception>
	public OrderedSet(IEnumerable<T> collection, IComparer<T> comparer)
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
	/// Adds an element to the set in its correct sorted position if it doesn't already exist.
	/// </summary>
	/// <param name="item">The element to add.</param>
	/// <returns>true if the element was added; false if it already exists.</returns>
	/// <remarks>
	/// This operation has O(n) time complexity due to the need to maintain sorted order.
	/// The element is inserted at the appropriate position to maintain the sorted sequence.
	/// If the element already exists, it is not added and false is returned.
	/// </remarks>
	public bool Add(T item)
	{
		int index = BinarySearch(item);
		if (index >= 0)
		{
			// Element already exists
			return false;
		}

		// Convert negative index to insertion point
		index = ~index;
		items.Insert(index, item);
		return true;
	}

	/// <summary>
	/// Adds an element to the set. This is the explicit interface implementation for ICollection{T}.
	/// </summary>
	/// <param name="item">The element to add.</param>
	void ICollection<T>.Add(T item) => Add(item);

	/// <summary>
	/// Removes all elements from the set.
	/// </summary>
	public void Clear() => items.Clear();

	/// <summary>
	/// Determines whether the set contains the specified element.
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
	/// Copies the elements of the set to an array, starting at the specified array index.
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
	/// Removes the specified element from the set.
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
	/// Returns an enumerator that iterates through the set in sorted order.
	/// </summary>
	/// <returns>An enumerator for the set.</returns>
	public IEnumerator<T> GetEnumerator() => items.GetEnumerator();

	/// <summary>
	/// Returns an enumerator that iterates through the set in sorted order.
	/// </summary>
	/// <returns>An enumerator for the set.</returns>
	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

	/// <summary>
	/// Modifies the current set so that it contains all elements that are in the current set,
	/// in the specified collection, or in both.
	/// </summary>
	/// <param name="other">The collection to compare to the current set.</param>
	/// <exception cref="ArgumentNullException">Thrown when other is null.</exception>
	public void UnionWith(IEnumerable<T> other)
	{
		ArgumentNullException.ThrowIfNull(other);

		foreach (T item in other)
		{
			Add(item);
		}
	}

	/// <summary>
	/// Modifies the current set so that it contains only elements that are also in a specified collection.
	/// </summary>
	/// <param name="other">The collection to compare to the current set.</param>
	/// <exception cref="ArgumentNullException">Thrown when other is null.</exception>
	public void IntersectWith(IEnumerable<T> other)
	{
		ArgumentNullException.ThrowIfNull(other);

		HashSet<T> otherSet = [.. other];

		for (int i = items.Count - 1; i >= 0; i--)
		{
			if (!otherSet.Contains(items[i]))
			{
				items.RemoveAt(i);
			}
		}
	}

	/// <summary>
	/// Removes all elements in the specified collection from the current set.
	/// </summary>
	/// <param name="other">The collection of items to remove from the set.</param>
	/// <exception cref="ArgumentNullException">Thrown when other is null.</exception>
	public void ExceptWith(IEnumerable<T> other)
	{
		ArgumentNullException.ThrowIfNull(other);

		foreach (T item in other)
		{
			Remove(item);
		}
	}

	/// <summary>
	/// Modifies the current set so that it contains only elements that are present either in the current set or in the specified collection, but not both.
	/// </summary>
	/// <param name="other">The collection to compare to the current set.</param>
	/// <exception cref="ArgumentNullException">Thrown when other is null.</exception>
	public void SymmetricExceptWith(IEnumerable<T> other)
	{
		ArgumentNullException.ThrowIfNull(other);

		HashSet<T> otherSet = [.. other];

		// Create a list of items to remove from otherSet as we find them
		List<T> toRemoveFromOther = [];

		// Remove items that are in both sets
		for (int i = items.Count - 1; i >= 0; i--)
		{
			if (otherSet.Contains(items[i]))
			{
				toRemoveFromOther.Add(items[i]);
				items.RemoveAt(i);
			}
		}

		// Remove common items from other set
		foreach (T item in toRemoveFromOther)
		{
			otherSet.Remove(item);
		}

		// Add items that are only in the other set
		foreach (T item in otherSet)
		{
			Add(item);
		}
	}

	/// <summary>
	/// Determines whether the current set is a subset of a specified collection.
	/// </summary>
	/// <param name="other">The collection to compare to the current set.</param>
	/// <returns>true if the current set is a subset of other; otherwise, false.</returns>
	/// <exception cref="ArgumentNullException">Thrown when other is null.</exception>
	public bool IsSubsetOf(IEnumerable<T> other)
	{
		ArgumentNullException.ThrowIfNull(other);

		HashSet<T> otherSet = [.. other];
		return items.All(otherSet.Contains);
	}

	/// <summary>
	/// Determines whether the current set is a superset of a specified collection.
	/// </summary>
	/// <param name="other">The collection to compare to the current set.</param>
	/// <returns>true if the current set is a superset of other; otherwise, false.</returns>
	/// <exception cref="ArgumentNullException">Thrown when other is null.</exception>
	public bool IsSupersetOf(IEnumerable<T> other)
	{
		ArgumentNullException.ThrowIfNull(other);

		return other.All(Contains);
	}

	/// <summary>
	/// Determines whether the current set is a proper (strict) subset of a specified collection.
	/// </summary>
	/// <param name="other">The collection to compare to the current set.</param>
	/// <returns>true if the current set is a proper subset of other; otherwise, false.</returns>
	/// <exception cref="ArgumentNullException">Thrown when other is null.</exception>
	public bool IsProperSubsetOf(IEnumerable<T> other)
	{
		ArgumentNullException.ThrowIfNull(other);

		HashSet<T> otherSet = [.. other];
		return Count < otherSet.Count && IsSubsetOf(otherSet);
	}

	/// <summary>
	/// Determines whether the current set is a proper (strict) superset of a specified collection.
	/// </summary>
	/// <param name="other">The collection to compare to the current set.</param>
	/// <returns>true if the current set is a proper superset of other; otherwise, false.</returns>
	/// <exception cref="ArgumentNullException">Thrown when other is null.</exception>
	public bool IsProperSupersetOf(IEnumerable<T> other)
	{
		ArgumentNullException.ThrowIfNull(other);

		HashSet<T> otherSet = [.. other];
		return Count > otherSet.Count && IsSupersetOf(otherSet);
	}

	/// <summary>
	/// Determines whether the current set overlaps with the specified collection.
	/// </summary>
	/// <param name="other">The collection to compare to the current set.</param>
	/// <returns>true if the current set and other share at least one common element; otherwise, false.</returns>
	/// <exception cref="ArgumentNullException">Thrown when other is null.</exception>
	public bool Overlaps(IEnumerable<T> other)
	{
		ArgumentNullException.ThrowIfNull(other);

		return other.Any(Contains);
	}

	/// <summary>
	/// Determines whether the current set and the specified collection contain the same elements.
	/// </summary>
	/// <param name="other">The collection to compare to the current set.</param>
	/// <returns>true if the current set is equal to other; otherwise, false.</returns>
	/// <exception cref="ArgumentNullException">Thrown when other is null.</exception>
	public bool SetEquals(IEnumerable<T> other)
	{
		ArgumentNullException.ThrowIfNull(other);

		HashSet<T> otherSet = [.. other];
		return Count == otherSet.Count && IsSubsetOf(otherSet);
	}

	/// <summary>
	/// Creates a shallow copy of the ordered set.
	/// </summary>
	/// <returns>A shallow copy of the set.</returns>
	public OrderedSet<T> Clone() => new(items, comparer);
}
