// Copyright (c) ktsu.dev
// All rights reserved.
// Licensed under the MIT license.

namespace ktsu.Containers;

using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

/// <summary>
/// Represents a generic set that maintains unique elements in contiguous memory for optimal cache performance.
/// </summary>
/// <remarks>
/// A contiguous memory set maintains all unique elements in a single contiguous block of memory,
/// optimizing cache locality and memory access patterns for better performance while ensuring
/// element uniqueness.
///
/// This implementation uses a managed array for contiguous storage and a hash set for fast
/// uniqueness checks, ensuring that all elements are stored contiguously in memory while
/// providing efficient set operations.
///
/// This implementation supports:
/// <list type="bullet">
///   <item>Adding unique elements while maintaining contiguous memory layout</item>
///   <item>Removing elements by value</item>
///   <item>Fast uniqueness checks with O(1) complexity</item>
///   <item>Set operations like union, intersection, and difference</item>
///   <item>Enumeration with cache-friendly sequential access</item>
///   <item>Guaranteed contiguous memory allocation</item>
/// </list>
///
/// Performance characteristics:
/// <list type="bullet">
///   <item>Add: O(1) average for uniqueness check, O(1) amortized for insertion</item>
///   <item>Contains: O(1) average time complexity</item>
///   <item>Remove: O(n) due to element shifting to maintain contiguity</item>
///   <item>Enumeration: Optimal cache performance due to contiguous layout</item>
/// </list>
/// </remarks>
/// <typeparam name="T">The type of elements stored in the set.</typeparam>
[SuppressMessage("Naming", "CA1710:Identifiers should have correct suffix", Justification = "ContiguousSet is a known collection name")]
public class ContiguousSet<T> : ISet<T>, IReadOnlySet<T>, IReadOnlyCollection<T>
{
	/// <summary>
	/// The backing array that stores elements in contiguous memory.
	/// </summary>
	private T[] items;

	/// <summary>
	/// The internal hash set used for fast uniqueness checks.
	/// </summary>
	private readonly HashSet<T> uniquenessSet;

	/// <summary>
	/// The number of elements currently stored in the set.
	/// </summary>
	private int count;

	/// <summary>
	/// The default initial capacity for the set.
	/// </summary>
	private const int DefaultCapacity = 4;

	/// <summary>
	/// Gets the number of elements in the set.
	/// </summary>
	public int Count => count;

	/// <summary>
	/// Gets the current capacity of the set.
	/// </summary>
	public int Capacity => items.Length;

	/// <summary>
	/// Gets a value indicating whether the set is read-only.
	/// </summary>
	public bool IsReadOnly => false;

	/// <summary>
	/// Initializes a new instance of the <see cref="ContiguousSet{T}"/> class.
	/// </summary>
	public ContiguousSet()
	{
		items = new T[DefaultCapacity];
		uniquenessSet = [];
		count = 0;
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="ContiguousSet{T}"/> class with the specified equality comparer.
	/// </summary>
	/// <param name="comparer">The equality comparer to use for determining element equality.</param>
	/// <exception cref="ArgumentNullException">Thrown when comparer is null.</exception>
	public ContiguousSet(IEqualityComparer<T> comparer)
	{
		ArgumentNullException.ThrowIfNull(comparer);

		items = new T[DefaultCapacity];
		uniquenessSet = new HashSet<T>(comparer);
		count = 0;
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="ContiguousSet{T}"/> class with initial capacity.
	/// </summary>
	/// <param name="capacity">The initial capacity of the set.</param>
	/// <exception cref="ArgumentOutOfRangeException">Thrown when capacity is negative.</exception>
	public ContiguousSet(int capacity)
	{
		ArgumentOutOfRangeException.ThrowIfNegative(capacity);

		items = capacity == 0 ? [] : new T[capacity];
		uniquenessSet = new HashSet<T>(capacity);
		count = 0;
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="ContiguousSet{T}"/> class with initial capacity and equality comparer.
	/// </summary>
	/// <param name="capacity">The initial capacity of the set.</param>
	/// <param name="comparer">The equality comparer to use for determining element equality.</param>
	/// <exception cref="ArgumentNullException">Thrown when comparer is null.</exception>
	/// <exception cref="ArgumentOutOfRangeException">Thrown when capacity is negative.</exception>
	public ContiguousSet(int capacity, IEqualityComparer<T> comparer)
	{
		ArgumentOutOfRangeException.ThrowIfNegative(capacity);
		ArgumentNullException.ThrowIfNull(comparer);

		items = capacity == 0 ? [] : new T[capacity];
		uniquenessSet = new HashSet<T>(capacity, comparer);
		count = 0;
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="ContiguousSet{T}"/> class with elements from an existing collection.
	/// </summary>
	/// <param name="collection">The collection whose elements are copied to the new contiguous set.</param>
	/// <exception cref="ArgumentNullException">Thrown when collection is null.</exception>
	public ContiguousSet(IEnumerable<T> collection)
	{
		ArgumentNullException.ThrowIfNull(collection);

		items = new T[DefaultCapacity];
		uniquenessSet = [];
		count = 0;

		foreach (T item in collection)
		{
			Add(item);
		}
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="ContiguousSet{T}"/> class with elements from an existing collection and an equality comparer.
	/// </summary>
	/// <param name="collection">The collection whose elements are copied to the new contiguous set.</param>
	/// <param name="comparer">The equality comparer to use for determining element equality.</param>
	/// <exception cref="ArgumentNullException">Thrown when collection or comparer is null.</exception>
	public ContiguousSet(IEnumerable<T> collection, IEqualityComparer<T> comparer)
	{
		ArgumentNullException.ThrowIfNull(collection);
		ArgumentNullException.ThrowIfNull(comparer);

		items = new T[DefaultCapacity];
		uniquenessSet = new HashSet<T>(comparer);
		count = 0;

		foreach (T item in collection)
		{
			Add(item);
		}
	}

	/// <summary>
	/// Adds an element to the set if it doesn't already exist, maintaining contiguous memory layout.
	/// </summary>
	/// <param name="item">The element to add.</param>
	/// <returns>true if the element was added; false if it already exists.</returns>
	/// <remarks>
	/// This operation has O(1) average time complexity for uniqueness check and O(1) amortized for insertion.
	/// The element is added at the end of the contiguous array to maintain cache-friendly layout.
	/// </remarks>
	public bool Add(T item)
	{
		if (!uniquenessSet.Add(item))
		{
			// Element already exists
			return false;
		}

		if (count == items.Length)
		{
			Grow();
		}

		items[count] = item;
		count++;
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
		if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
		{
			// Clear references to help GC
			Array.Clear(items, 0, count);
		}
		count = 0;
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
		ArgumentOutOfRangeException.ThrowIfGreaterThan(count, array.Length - arrayIndex);

		Array.Copy(items, 0, array, arrayIndex, count);
	}

	/// <summary>
	/// Removes the specified element from the set.
	/// </summary>
	/// <param name="item">The element to remove.</param>
	/// <returns>true if the element was found and removed; otherwise, false.</returns>
	/// <remarks>
	/// This operation has O(n) time complexity for finding the element in the array
	/// and O(1) average time complexity for removing from the hash set.
	/// Elements are shifted to maintain contiguous memory layout.
	/// </remarks>
	public bool Remove(T item)
	{
		if (!uniquenessSet.Remove(item))
		{
			// Element doesn't exist
			return false;
		}

		// Find and remove from the array
		int index = Array.IndexOf(items, item, 0, count);
		if (index >= 0)
		{
			count--;
			if (index < count)
			{
				Array.Copy(items, index + 1, items, index, count - index);
			}

			if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
			{
				items[count] = default!;
			}
		}

		return true;
	}

	/// <summary>
	/// Returns an enumerator that iterates through the set.
	/// </summary>
	/// <returns>An enumerator for the set.</returns>
	/// <remarks>
	/// Enumeration benefits from the contiguous memory layout with optimal cache performance.
	/// </remarks>
	public IEnumerator<T> GetEnumerator()
	{
		for (int i = 0; i < count; i++)
		{
			yield return items[i];
		}
	}

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
		for (int i = count - 1; i >= 0; i--)
		{
			T item = items[i];
			if (!otherSet.Contains(item))
			{
				Remove(item);
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

		HashSet<T> otherSet = new(other, uniquenessSet.Comparer);

		// Remove items that are in both sets
		for (int i = count - 1; i >= 0; i--)
		{
			T item = items[i];
			if (otherSet.Remove(item))
			{
				// Item exists in both - remove from current set
				Remove(item);
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
	/// Ensures that the set has enough capacity for the specified number of elements.
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
		if (count < items.Length * 0.9) // Only trim if there's significant unused space
		{
			T[] newItems = count == 0 ? [] : new T[count];
			Array.Copy(items, newItems, count);
			items = newItems;
		}
	}

	/// <summary>
	/// Gets a span representing the elements in the set.
	/// </summary>
	/// <returns>A span over the set's elements.</returns>
	/// <remarks>
	/// This method provides direct access to the contiguous memory, enabling high-performance
	/// operations and interoperability with other APIs that work with spans.
	/// </remarks>
	public Span<T> AsSpan() => new(items, 0, count);

	/// <summary>
	/// Gets a read-only span representing the elements in the set.
	/// </summary>
	/// <returns>A read-only span over the set's elements.</returns>
	/// <remarks>
	/// This method provides direct read-only access to the contiguous memory, enabling high-performance
	/// operations while preventing modifications.
	/// </remarks>
	public ReadOnlySpan<T> AsReadOnlySpan() => new(items, 0, count);

	/// <summary>
	/// Creates a shallow copy of the set.
	/// </summary>
	/// <returns>A new set containing all elements from the original set with contiguous memory layout.</returns>
	public ContiguousSet<T> Clone()
	{
		ContiguousSet<T> clone = new(count, uniquenessSet.Comparer);
		Array.Copy(items, clone.items, count);
		clone.count = count;
		foreach (T item in items.AsSpan(0, count))
		{
			clone.uniquenessSet.Add(item);
		}
		return clone;
	}

	/// <summary>
	/// Grows the set's capacity.
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
		Array.Copy(items, newItems, count);
		items = newItems;
	}
}
