// Copyright (c) ktsu.dev
// All rights reserved.
// Licensed under the MIT license.

namespace ktsu.Containers;

using System.Collections;
using System.Diagnostics.CodeAnalysis;

/// <summary>
/// Represents a generic set that maintains unique elements in insertion order.
/// </summary>
/// <remarks>
/// An insertion-order set maintains unique elements in the exact order they were first added,
/// preserving the insertion sequence without any sorting or reordering. Duplicate values
/// are not added, but the insertion order of unique elements is preserved.
///
/// This implementation uses a list for order preservation and a hash set for uniqueness checks,
/// providing efficient insertion-order enumeration and O(1) uniqueness verification.
///
/// This implementation supports:
/// <list type="bullet">
///   <item>Adding elements while maintaining insertion order and uniqueness</item>
///   <item>Removing elements by value</item>
///   <item>Fast uniqueness checks with O(1) complexity</item>
///   <item>Set operations like union, intersection, and difference</item>
///   <item>Enumeration in insertion order</item>
/// </list>
/// </remarks>
/// <typeparam name="T">The type of elements stored in the set.</typeparam>
[SuppressMessage("Naming", "CA1710:Identifiers should have correct suffix", Justification = "InsertionOrderSet is a known collection name")]
public class InsertionOrderSet<T> : ISet<T>, IReadOnlySet<T>, IReadOnlyCollection<T>
{
	/// <summary>
	/// The internal list that stores elements in insertion order.
	/// </summary>
	private readonly List<T> items;

	/// <summary>
	/// The internal hash set used for fast uniqueness checks.
	/// </summary>
	private readonly HashSet<T> uniquenessSet;

	/// <summary>
	/// Gets the number of elements in the set.
	/// </summary>
	public int Count => items.Count;

	/// <summary>
	/// Gets a value indicating whether the set is read-only.
	/// </summary>
	public bool IsReadOnly => false;

	/// <summary>
	/// Initializes a new instance of the <see cref="InsertionOrderSet{T}"/> class.
	/// </summary>
	public InsertionOrderSet()
	{
		items = [];
		uniquenessSet = [];
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="InsertionOrderSet{T}"/> class with the specified equality comparer.
	/// </summary>
	/// <param name="comparer">The equality comparer to use for determining element equality.</param>
	/// <exception cref="ArgumentNullException">Thrown when comparer is null.</exception>
	public InsertionOrderSet(IEqualityComparer<T> comparer)
	{
		ArgumentNullException.ThrowIfNull(comparer);

		items = [];
		uniquenessSet = new HashSet<T>(comparer);
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="InsertionOrderSet{T}"/> class with initial capacity.
	/// </summary>
	/// <param name="capacity">The initial capacity of the set.</param>
	/// <exception cref="ArgumentOutOfRangeException">Thrown when capacity is negative.</exception>
	public InsertionOrderSet(int capacity)
	{
		ArgumentOutOfRangeException.ThrowIfNegative(capacity);

		items = new List<T>(capacity);
		uniquenessSet = new HashSet<T>(capacity);
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="InsertionOrderSet{T}"/> class with initial capacity and equality comparer.
	/// </summary>
	/// <param name="capacity">The initial capacity of the set.</param>
	/// <param name="comparer">The equality comparer to use for determining element equality.</param>
	/// <exception cref="ArgumentNullException">Thrown when comparer is null.</exception>
	/// <exception cref="ArgumentOutOfRangeException">Thrown when capacity is negative.</exception>
	public InsertionOrderSet(int capacity, IEqualityComparer<T> comparer)
	{
		ArgumentOutOfRangeException.ThrowIfNegative(capacity);
		ArgumentNullException.ThrowIfNull(comparer);

		items = new List<T>(capacity);
		uniquenessSet = new HashSet<T>(capacity, comparer);
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="InsertionOrderSet{T}"/> class with elements from an existing collection.
	/// </summary>
	/// <param name="collection">The collection whose elements are copied to the new insertion-order set.</param>
	/// <exception cref="ArgumentNullException">Thrown when collection is null.</exception>
	public InsertionOrderSet(IEnumerable<T> collection)
	{
		ArgumentNullException.ThrowIfNull(collection);

		items = [];
		uniquenessSet = [];

		foreach (T item in collection)
		{
			Add(item);
		}
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="InsertionOrderSet{T}"/> class with elements from an existing collection and an equality comparer.
	/// </summary>
	/// <param name="collection">The collection whose elements are copied to the new insertion-order set.</param>
	/// <param name="comparer">The equality comparer to use for determining element equality.</param>
	/// <exception cref="ArgumentNullException">Thrown when collection or comparer is null.</exception>
	public InsertionOrderSet(IEnumerable<T> collection, IEqualityComparer<T> comparer)
	{
		ArgumentNullException.ThrowIfNull(collection);
		ArgumentNullException.ThrowIfNull(comparer);

		items = [];
		uniquenessSet = new HashSet<T>(comparer);

		foreach (T item in collection)
		{
			Add(item);
		}
	}

	/// <summary>
	/// Adds an element to the set in insertion order if it doesn't already exist.
	/// </summary>
	/// <param name="item">The element to add.</param>
	/// <returns>true if the element was added; false if it already exists.</returns>
	/// <remarks>
	/// This operation has O(1) average time complexity for uniqueness check and O(1) for insertion.
	/// The element is added at the end of the collection to maintain insertion order.
	/// If the element already exists, it is not added and false is returned.
	/// </remarks>
	public bool Add(T item)
	{
		if (!uniquenessSet.Add(item))
		{
			// Element already exists
			return false;
		}

		items.Add(item);
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
	public void Clear()
	{
		items.Clear();
		uniquenessSet.Clear();
	}

	/// <summary>
	/// Determines whether the set contains the specified element.
	/// </summary>
	/// <param name="item">The element to locate.</param>
	/// <returns>true if the element is found; otherwise, false.</returns>
	/// <remarks>
	/// This operation has O(1) average time complexity.
	/// </remarks>
	public bool Contains(T item) => uniquenessSet.Contains(item);

	/// <summary>
	/// Copies the elements of the set to an array, starting at the specified array index.
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
	/// Removes the specified element from the set.
	/// </summary>
	/// <param name="item">The element to remove.</param>
	/// <returns>true if the element was found and removed; otherwise, false.</returns>
	/// <remarks>
	/// This operation has O(n) time complexity for finding the element in the list
	/// and O(1) average time complexity for removing from the hash set.
	/// </remarks>
	public bool Remove(T item)
	{
		if (!uniquenessSet.Remove(item))
		{
			// Element doesn't exist
			return false;
		}

		items.Remove(item);
		return true;
	}

	/// <summary>
	/// Returns an enumerator that iterates through the set in insertion order.
	/// </summary>
	/// <returns>An enumerator for the set.</returns>
	public IEnumerator<T> GetEnumerator() => items.GetEnumerator();

	/// <summary>
	/// Returns an enumerator that iterates through the set.
	/// </summary>
	/// <returns>An enumerator for the set.</returns>
	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

	/// <summary>
	/// Modifies the current set to contain all elements that are present in itself, the specified collection, or both.
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
	/// Modifies the current set to contain only elements that are present in that set and in the specified collection.
	/// </summary>
	/// <param name="other">The collection to compare to the current set.</param>
	/// <exception cref="ArgumentNullException">Thrown when other is null.</exception>
	public void IntersectWith(IEnumerable<T> other)
	{
		ArgumentNullException.ThrowIfNull(other);

		HashSet<T> otherSet = new(other, uniquenessSet.Comparer);

		// Remove items that are not in the other collection
		for (int i = items.Count - 1; i >= 0; i--)
		{
			T item = items[i];
			if (!otherSet.Contains(item))
			{
				items.RemoveAt(i);
				uniquenessSet.Remove(item);
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
	/// Modifies the current set to contain only elements that are present either in that set or in the specified collection, but not both.
	/// </summary>
	/// <param name="other">The collection to compare to the current set.</param>
	/// <exception cref="ArgumentNullException">Thrown when other is null.</exception>
	public void SymmetricExceptWith(IEnumerable<T> other)
	{
		ArgumentNullException.ThrowIfNull(other);

		// Use a temporary set to avoid modifying the collection while enumerating
		HashSet<T> otherSet = new(other, uniquenessSet.Comparer);

		// Remove items that are in both sets
		for (int i = items.Count - 1; i >= 0; i--)
		{
			T item = items[i];
			if (otherSet.Remove(item))
			{
				// Item exists in both - remove from current set
				items.RemoveAt(i);
				uniquenessSet.Remove(item);
			}
		}

		// Add remaining items from other set (items that were only in other)
		foreach (T item in otherSet)
		{
			Add(item);
		}
	}

	/// <summary>
	/// Determines whether the current set is a subset of the specified collection.
	/// </summary>
	/// <param name="other">The collection to compare to the current set.</param>
	/// <returns>true if the current set is a subset of other; otherwise, false.</returns>
	/// <exception cref="ArgumentNullException">Thrown when other is null.</exception>
	public bool IsSubsetOf(IEnumerable<T> other)
	{
		ArgumentNullException.ThrowIfNull(other);
		return uniquenessSet.IsSubsetOf(other);
	}

	/// <summary>
	/// Determines whether the current set is a superset of the specified collection.
	/// </summary>
	/// <param name="other">The collection to compare to the current set.</param>
	/// <returns>true if the current set is a superset of other; otherwise, false.</returns>
	/// <exception cref="ArgumentNullException">Thrown when other is null.</exception>
	public bool IsSupersetOf(IEnumerable<T> other)
	{
		ArgumentNullException.ThrowIfNull(other);
		return uniquenessSet.IsSupersetOf(other);
	}

	/// <summary>
	/// Determines whether the current set is a proper subset of the specified collection.
	/// </summary>
	/// <param name="other">The collection to compare to the current set.</param>
	/// <returns>true if the current set is a proper subset of other; otherwise, false.</returns>
	/// <exception cref="ArgumentNullException">Thrown when other is null.</exception>
	public bool IsProperSubsetOf(IEnumerable<T> other)
	{
		ArgumentNullException.ThrowIfNull(other);
		return uniquenessSet.IsProperSubsetOf(other);
	}

	/// <summary>
	/// Determines whether the current set is a proper superset of the specified collection.
	/// </summary>
	/// <param name="other">The collection to compare to the current set.</param>
	/// <returns>true if the current set is a proper superset of other; otherwise, false.</returns>
	/// <exception cref="ArgumentNullException">Thrown when other is null.</exception>
	public bool IsProperSupersetOf(IEnumerable<T> other)
	{
		ArgumentNullException.ThrowIfNull(other);
		return uniquenessSet.IsProperSupersetOf(other);
	}

	/// <summary>
	/// Determines whether the current set and a specified collection share common elements.
	/// </summary>
	/// <param name="other">The collection to compare to the current set.</param>
	/// <returns>true if the current set and other share at least one common element; otherwise, false.</returns>
	/// <exception cref="ArgumentNullException">Thrown when other is null.</exception>
	public bool Overlaps(IEnumerable<T> other)
	{
		ArgumentNullException.ThrowIfNull(other);
		return uniquenessSet.Overlaps(other);
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
		return uniquenessSet.SetEquals(other);
	}

	/// <summary>
	/// Creates a shallow copy of the set.
	/// </summary>
	/// <returns>A new set containing all elements from the original set in the same insertion order.</returns>
	public InsertionOrderSet<T> Clone() => new(items, uniquenessSet.Comparer);
}
