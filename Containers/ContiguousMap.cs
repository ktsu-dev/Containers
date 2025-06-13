// Copyright (c) ktsu.dev
// All rights reserved.
// Licensed under the MIT license.

namespace ktsu.Containers;

using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

/// <summary>
/// Represents a generic map/dictionary that maintains key-value pairs in contiguous memory for optimal cache performance.
/// </summary>
/// <remarks>
/// A contiguous memory map maintains all key-value pairs in a single contiguous block of memory,
/// optimizing cache locality and memory access patterns for better performance.
///
/// This implementation uses a managed array for contiguous storage of entries and a dictionary
/// for fast key-based lookups, ensuring that all key-value pairs are stored contiguously in
/// memory while providing efficient map operations.
///
/// This implementation supports:
/// <list type="bullet">
///   <item>Adding key-value pairs while maintaining contiguous memory layout</item>
///   <item>Removing key-value pairs by key</item>
///   <item>Fast key lookup with O(1) complexity</item>
///   <item>Accessing values by key with O(1) complexity</item>
///   <item>Enumeration with cache-friendly sequential access</item>
///   <item>Guaranteed contiguous memory allocation</item>
/// </list>
///
/// Performance characteristics:
/// <list type="bullet">
///   <item>Add: O(1) average for key lookup, O(1) amortized for insertion</item>
///   <item>Access by key: O(1) average time complexity</item>
///   <item>Remove: O(n) due to element shifting to maintain contiguity</item>
///   <item>Enumeration: Optimal cache performance due to contiguous layout</item>
/// </list>
/// </remarks>
/// <typeparam name="TKey">The type of keys in the map.</typeparam>
/// <typeparam name="TValue">The type of values in the map.</typeparam>
[SuppressMessage("Naming", "CA1710:Identifiers should have correct suffix", Justification = "ContiguousMap is a known collection name")]
public class ContiguousMap<TKey, TValue> : IDictionary<TKey, TValue>, IReadOnlyDictionary<TKey, TValue>
	where TKey : notnull
{
	/// <summary>
	/// Represents a key-value pair stored in the map.
	/// </summary>
	/// <remarks>
	/// Initializes a new instance of the <see cref="Entry"/> struct.
	/// </remarks>
	/// <param name="key">The key of the entry.</param>
	/// <param name="value">The value of the entry.</param>
	public readonly struct Entry(TKey key, TValue value)
	{
		/// <summary>
		/// Gets the key of the entry.
		/// </summary>
		public TKey Key { get; } = key;

		/// <summary>
		/// Gets the value of the entry.
		/// </summary>
		public TValue Value { get; } = value;

		public override bool Equals(object obj)
		{
			throw new NotImplementedException();
		}

		public override int GetHashCode()
		{
			throw new NotImplementedException();
		}

		public static bool operator ==(Entry left, Entry right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(Entry left, Entry right)
		{
			return !(left == right);
		}
	}

	/// <summary>
	/// The backing array that stores key-value pairs in contiguous memory.
	/// </summary>
	private Entry[] items;

	/// <summary>
	/// The internal dictionary used for fast key-based lookups to array indices.
	/// </summary>
	private readonly Dictionary<TKey, int> keyToIndex;

	/// <summary>
	/// The default initial capacity for the map.
	/// </summary>
	private const int DefaultCapacity = 4;

	/// <summary>
	/// Gets the number of key-value pairs in the map.
	/// </summary>
	public int Count { get; private set; }

	/// <summary>
	/// Gets the current capacity of the map.
	/// </summary>
	public int Capacity => items.Length;

	/// <summary>
	/// Gets a value indicating whether the map is read-only.
	/// </summary>
	public bool IsReadOnly => false;

	/// <summary>
	/// Gets or sets the value associated with the specified key.
	/// </summary>
	/// <param name="key">The key of the value to get or set.</param>
	/// <returns>The value associated with the specified key.</returns>
	/// <exception cref="ArgumentNullException">Thrown when key is null.</exception>
	/// <exception cref="KeyNotFoundException">Thrown when getting a value and the key is not found.</exception>
	public TValue this[TKey key]
	{
		get
		{
			ArgumentNullException.ThrowIfNull(key);

			return !keyToIndex.TryGetValue(key, out int index)
				? throw new KeyNotFoundException($"The key '{key}' was not found in the map.")
				: items[index].Value;
		}
		set
		{
			ArgumentNullException.ThrowIfNull(key);

			if (keyToIndex.TryGetValue(key, out int index))
			{
				// Key exists, update the value
				items[index] = new Entry(key, value);
			}
			else
			{
				// Key doesn't exist, add new entry
				if (Count == items.Length)
				{
					Grow();
				}

				index = Count;
				items[index] = new Entry(key, value);
				keyToIndex[key] = index;
				Count++;
			}
		}
	}

	/// <summary>
	/// Gets a collection containing the keys in the map.
	/// </summary>
	public ICollection<TKey> Keys => new KeyCollection(this);

	/// <summary>
	/// Gets a collection containing the values in the map.
	/// </summary>
	public ICollection<TValue> Values => new ValueCollection(this);

	/// <summary>
	/// Gets the keys in the map (read-only).
	/// </summary>
	IEnumerable<TKey> IReadOnlyDictionary<TKey, TValue>.Keys => Keys;

	/// <summary>
	/// Gets the values in the map (read-only).
	/// </summary>
	IEnumerable<TValue> IReadOnlyDictionary<TKey, TValue>.Values => Values;

	/// <summary>
	/// Initializes a new instance of the <see cref="ContiguousMap{TKey, TValue}"/> class.
	/// </summary>
	public ContiguousMap()
	{
		items = new Entry[DefaultCapacity];
		keyToIndex = [];
		Count = 0;
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="ContiguousMap{TKey, TValue}"/> class with the specified key equality comparer.
	/// </summary>
	/// <param name="comparer">The equality comparer to use for keys.</param>
	/// <exception cref="ArgumentNullException">Thrown when comparer is null.</exception>
	public ContiguousMap(IEqualityComparer<TKey> comparer)
	{
		ArgumentNullException.ThrowIfNull(comparer);

		items = new Entry[DefaultCapacity];
		keyToIndex = new Dictionary<TKey, int>(comparer);
		Count = 0;
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="ContiguousMap{TKey, TValue}"/> class with initial capacity.
	/// </summary>
	/// <param name="capacity">The initial capacity of the map.</param>
	/// <exception cref="ArgumentOutOfRangeException">Thrown when capacity is negative.</exception>
	public ContiguousMap(int capacity)
	{
		ArgumentOutOfRangeException.ThrowIfNegative(capacity);

		items = capacity == 0 ? [] : new Entry[capacity];
		keyToIndex = new Dictionary<TKey, int>(capacity);
		Count = 0;
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="ContiguousMap{TKey, TValue}"/> class with initial capacity and key equality comparer.
	/// </summary>
	/// <param name="capacity">The initial capacity of the map.</param>
	/// <param name="comparer">The equality comparer to use for keys.</param>
	/// <exception cref="ArgumentNullException">Thrown when comparer is null.</exception>
	/// <exception cref="ArgumentOutOfRangeException">Thrown when capacity is negative.</exception>
	public ContiguousMap(int capacity, IEqualityComparer<TKey> comparer)
	{
		ArgumentOutOfRangeException.ThrowIfNegative(capacity);
		ArgumentNullException.ThrowIfNull(comparer);

		items = capacity == 0 ? [] : new Entry[capacity];
		keyToIndex = new Dictionary<TKey, int>(capacity, comparer);
		Count = 0;
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="ContiguousMap{TKey, TValue}"/> class with key-value pairs from an existing dictionary.
	/// </summary>
	/// <param name="dictionary">The dictionary whose key-value pairs are copied to the new contiguous map.</param>
	/// <exception cref="ArgumentNullException">Thrown when dictionary is null.</exception>
	public ContiguousMap(IDictionary<TKey, TValue> dictionary)
	{
		ArgumentNullException.ThrowIfNull(dictionary);

		int capacity = dictionary.Count == 0 ? DefaultCapacity : dictionary.Count;
		items = new Entry[capacity];
		keyToIndex = [];
		Count = 0;

		foreach (KeyValuePair<TKey, TValue> pair in dictionary)
		{
			Add(pair.Key, pair.Value);
		}
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="ContiguousMap{TKey, TValue}"/> class with key-value pairs from an existing dictionary and a key equality comparer.
	/// </summary>
	/// <param name="dictionary">The dictionary whose key-value pairs are copied to the new contiguous map.</param>
	/// <param name="comparer">The equality comparer to use for keys.</param>
	/// <exception cref="ArgumentNullException">Thrown when dictionary or comparer is null.</exception>
	public ContiguousMap(IDictionary<TKey, TValue> dictionary, IEqualityComparer<TKey> comparer)
	{
		ArgumentNullException.ThrowIfNull(dictionary);
		ArgumentNullException.ThrowIfNull(comparer);

		int capacity = dictionary.Count == 0 ? DefaultCapacity : dictionary.Count;
		items = new Entry[capacity];
		keyToIndex = new Dictionary<TKey, int>(comparer);
		Count = 0;

		foreach (KeyValuePair<TKey, TValue> pair in dictionary)
		{
			Add(pair.Key, pair.Value);
		}
	}

	/// <summary>
	/// Adds a key-value pair to the map, maintaining contiguous memory layout.
	/// </summary>
	/// <param name="key">The key of the element to add.</param>
	/// <param name="value">The value of the element to add.</param>
	/// <exception cref="ArgumentNullException">Thrown when key is null.</exception>
	/// <exception cref="ArgumentException">Thrown when a key with the same value already exists.</exception>
	/// <remarks>
	/// This operation has O(1) average time complexity.
	/// The key-value pair is added at the end of the contiguous array.
	/// </remarks>
	public void Add(TKey key, TValue value)
	{
		ArgumentNullException.ThrowIfNull(key);

		if (keyToIndex.ContainsKey(key))
		{
			throw new ArgumentException($"An element with the key '{key}' already exists.", nameof(key));
		}

		if (Count == items.Length)
		{
			Grow();
		}

		int index = Count;
		items[index] = new Entry(key, value);
		keyToIndex[key] = index;
		Count++;
	}

	/// <summary>
	/// Removes the element with the specified key from the map.
	/// </summary>
	/// <param name="key">The key of the element to remove.</param>
	/// <returns>true if the element was found and removed; otherwise, false.</returns>
	/// <exception cref="ArgumentNullException">Thrown when key is null.</exception>
	/// <remarks>
	/// This operation has O(n) time complexity as it may need to update indices after removal
	/// and shift elements to maintain contiguous memory layout.
	/// </remarks>
	public bool Remove(TKey key)
	{
		ArgumentNullException.ThrowIfNull(key);

		if (!keyToIndex.TryGetValue(key, out int index))
		{
			return false;
		}

		// Remove from the array by shifting elements
		Count--;
		if (index < Count)
		{
			Array.Copy(items, index + 1, items, index, Count - index);
		}

		if (RuntimeHelpers.IsReferenceOrContainsReferences<Entry>())
		{
			items[Count] = default;
		}

		keyToIndex.Remove(key);

		// Update indices in the dictionary for all elements after the removed one
		for (int i = index; i < Count; i++)
		{
			TKey itemKey = items[i].Key;
			keyToIndex[itemKey] = i;
		}

		return true;
	}

	/// <summary>
	/// Determines whether the map contains the specified key.
	/// </summary>
	/// <param name="key">The key to locate.</param>
	/// <returns>true if the key is found; otherwise, false.</returns>
	/// <exception cref="ArgumentNullException">Thrown when key is null.</exception>
	/// <remarks>
	/// This operation has O(1) average time complexity.
	/// </remarks>
	public bool ContainsKey(TKey key)
	{
		ArgumentNullException.ThrowIfNull(key);
		return keyToIndex.ContainsKey(key);
	}

	/// <summary>
	/// Gets the value associated with the specified key.
	/// </summary>
	/// <param name="key">The key whose value to get.</param>
	/// <param name="value">When this method returns, the value associated with the specified key, if found; otherwise, the default value.</param>
	/// <returns>true if the key was found; otherwise, false.</returns>
	/// <exception cref="ArgumentNullException">Thrown when key is null.</exception>
	/// <remarks>
	/// This operation has O(1) average time complexity.
	/// </remarks>
	public bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value)
	{
		ArgumentNullException.ThrowIfNull(key);

		if (keyToIndex.TryGetValue(key, out int index))
		{
			value = items[index].Value;
			return true;
		}

		value = default;
		return false;
	}

	/// <summary>
	/// Adds a key-value pair to the map.
	/// </summary>
	/// <param name="item">The key-value pair to add.</param>
	/// <exception cref="ArgumentException">Thrown when a key with the same value already exists.</exception>
	public void Add(KeyValuePair<TKey, TValue> item) => Add(item.Key, item.Value);

	/// <summary>
	/// Removes all key-value pairs from the map.
	/// </summary>
	public void Clear()
	{
		if (RuntimeHelpers.IsReferenceOrContainsReferences<Entry>())
		{
			// Clear references to help GC
			Array.Clear(items, 0, Count);
		}
		Count = 0;
		keyToIndex.Clear();
	}

	/// <summary>
	/// Determines whether the map contains the specified key-value pair.
	/// </summary>
	/// <param name="item">The key-value pair to locate.</param>
	/// <returns>true if the key-value pair is found; otherwise, false.</returns>
	public bool Contains(KeyValuePair<TKey, TValue> item)
	{
		return keyToIndex.TryGetValue(item.Key, out int index) && EqualityComparer<TValue>.Default.Equals(items[index].Value, item.Value);
	}

	/// <summary>
	/// Copies the key-value pairs of the map to an array, starting at the specified array index.
	/// </summary>
	/// <param name="array">The destination array.</param>
	/// <param name="arrayIndex">The zero-based index in the destination array at which copying begins.</param>
	/// <exception cref="ArgumentNullException">Thrown when array is null.</exception>
	/// <exception cref="ArgumentOutOfRangeException">Thrown when arrayIndex is negative.</exception>
	/// <exception cref="ArgumentException">Thrown when the destination array is too small.</exception>
	public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
	{
		ArgumentNullException.ThrowIfNull(array);
		ArgumentOutOfRangeException.ThrowIfNegative(arrayIndex);
		ArgumentOutOfRangeException.ThrowIfGreaterThan(arrayIndex, array.Length);
		ArgumentOutOfRangeException.ThrowIfGreaterThan(Count, array.Length - arrayIndex);

		for (int i = 0; i < Count; i++)
		{
			Entry entry = items[i];
			array[arrayIndex + i] = new KeyValuePair<TKey, TValue>(entry.Key, entry.Value);
		}
	}

	/// <summary>
	/// Removes the specified key-value pair from the map.
	/// </summary>
	/// <param name="item">The key-value pair to remove.</param>
	/// <returns>true if the key-value pair was found and removed; otherwise, false.</returns>
	public bool Remove(KeyValuePair<TKey, TValue> item)
	{
		return !keyToIndex.TryGetValue(item.Key, out int index)
			? false
			: EqualityComparer<TValue>.Default.Equals(items[index].Value, item.Value) && Remove(item.Key);
	}

	/// <summary>
	/// Returns an enumerator that iterates through the map.
	/// </summary>
	/// <returns>An enumerator for the map.</returns>
	/// <remarks>
	/// Enumeration benefits from the contiguous memory layout with optimal cache performance.
	/// </remarks>
	public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
	{
		for (int i = 0; i < Count; i++)
		{
			Entry entry = items[i];
			yield return new KeyValuePair<TKey, TValue>(entry.Key, entry.Value);
		}
	}

	/// <summary>
	/// Returns an enumerator that iterates through the map.
	/// </summary>
	/// <returns>An enumerator for the map.</returns>
	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

	/// <summary>
	/// Ensures that the map has enough capacity for the specified number of elements.
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
			Entry[] newItems = Count == 0 ? [] : new Entry[Count];
			Array.Copy(items, newItems, Count);
			items = newItems;
		}
	}

	/// <summary>
	/// Gets a span representing the entries in the map.
	/// </summary>
	/// <returns>A span over the map's entries.</returns>
	/// <remarks>
	/// This method provides direct access to the contiguous memory, enabling high-performance
	/// operations and interoperability with other APIs that work with spans.
	/// </remarks>
	public Span<Entry> AsSpan() => new(items, 0, Count);

	/// <summary>
	/// Gets a read-only span representing the entries in the map.
	/// </summary>
	/// <returns>A read-only span over the map's entries.</returns>
	/// <remarks>
	/// This method provides direct read-only access to the contiguous memory, enabling high-performance
	/// operations while preventing modifications.
	/// </remarks>
	public ReadOnlySpan<Entry> AsReadOnlySpan() => new(items, 0, Count);

	/// <summary>
	/// Creates a shallow copy of the map.
	/// </summary>
	/// <returns>A new map containing all key-value pairs from the original map with contiguous memory layout.</returns>
	public ContiguousMap<TKey, TValue> Clone()
	{
		ContiguousMap<TKey, TValue> clone = new(Count, keyToIndex.Comparer);
		Array.Copy(items, clone.items, Count);
		clone.Count = Count;
		for (int i = 0; i < Count; i++)
		{
			clone.keyToIndex[items[i].Key] = i;
		}
		return clone;
	}

	/// <summary>
	/// Grows the map's capacity.
	/// </summary>
	/// <param name="minimumCapacity">The minimum required capacity.</param>
	private void Grow(int minimumCapacity = 0)
	{
		int newCapacity = items.Length == 0 ? DefaultCapacity : items.Length * 2;
		if (newCapacity < minimumCapacity)
		{
			newCapacity = minimumCapacity;
		}

		Entry[] newItems = new Entry[newCapacity];
		Array.Copy(items, newItems, Count);
		items = newItems;
	}

	/// <summary>
	/// Collection class for keys.
	/// </summary>
	private sealed class KeyCollection(ContiguousMap<TKey, TValue> map) : ICollection<TKey>
	{
		private readonly ContiguousMap<TKey, TValue> map = map;

		public int Count => map.Count;
		public bool IsReadOnly => true;

		public void Add(TKey item) => throw new NotSupportedException("Keys collection is read-only.");
		public void Clear() => throw new NotSupportedException("Keys collection is read-only.");
		public bool Remove(TKey item) => throw new NotSupportedException("Keys collection is read-only.");

		public bool Contains(TKey item)
		{
			ArgumentNullException.ThrowIfNull(item);
			return map.ContainsKey(item);
		}

		public void CopyTo(TKey[] array, int arrayIndex)
		{
			ArgumentNullException.ThrowIfNull(array);
			ArgumentOutOfRangeException.ThrowIfNegative(arrayIndex);
			ArgumentOutOfRangeException.ThrowIfGreaterThan(arrayIndex, array.Length);
			ArgumentOutOfRangeException.ThrowIfGreaterThan(Count, array.Length - arrayIndex);

			for (int i = 0; i < map.Count; i++)
			{
				array[arrayIndex + i] = map.items[i].Key;
			}
		}

		public IEnumerator<TKey> GetEnumerator()
		{
			for (int i = 0; i < map.Count; i++)
			{
				yield return map.items[i].Key;
			}
		}

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	}

	/// <summary>
	/// Collection class for values.
	/// </summary>
	private sealed class ValueCollection(ContiguousMap<TKey, TValue> map) : ICollection<TValue>
	{
		private readonly ContiguousMap<TKey, TValue> map = map;

		public int Count => map.Count;
		public bool IsReadOnly => true;

		public void Add(TValue item) => throw new NotSupportedException("Values collection is read-only.");
		public void Clear() => throw new NotSupportedException("Values collection is read-only.");
		public bool Remove(TValue item) => throw new NotSupportedException("Values collection is read-only.");

		public bool Contains(TValue item)
		{
			for (int i = 0; i < map.Count; i++)
			{
				if (EqualityComparer<TValue>.Default.Equals(map.items[i].Value, item))
				{
					return true;
				}
			}
			return false;
		}

		public void CopyTo(TValue[] array, int arrayIndex)
		{
			ArgumentNullException.ThrowIfNull(array);
			ArgumentOutOfRangeException.ThrowIfNegative(arrayIndex);
			ArgumentOutOfRangeException.ThrowIfGreaterThan(arrayIndex, array.Length);
			ArgumentOutOfRangeException.ThrowIfGreaterThan(Count, array.Length - arrayIndex);

			for (int i = 0; i < map.Count; i++)
			{
				array[arrayIndex + i] = map.items[i].Value;
			}
		}

		public IEnumerator<TValue> GetEnumerator()
		{
			for (int i = 0; i < map.Count; i++)
			{
				yield return map.items[i].Value;
			}
		}

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	}
}
