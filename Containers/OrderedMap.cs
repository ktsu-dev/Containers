// Copyright (c) ktsu.dev
// All rights reserved.
// Licensed under the MIT license.

namespace ktsu.Containers;

using System.Collections;
using System.Diagnostics.CodeAnalysis;

/// <summary>
/// Represents a generic map/dictionary that maintains key-value pairs in sorted order by key.
/// </summary>
/// <remarks>
/// An ordered map automatically maintains key-value pairs in sorted order by key using either
/// the natural ordering of the keys (if they implement <see cref="IComparable{T}"/>)
/// or a custom comparer provided during construction.
///
/// This implementation uses a binary search tree structure to maintain order while
/// providing efficient insertion, removal, and search operations. Unlike standard dictionaries,
/// this map ensures keys are always enumerated in sorted order.
///
/// This implementation supports:
/// <list type="bullet">
///   <item>Adding key-value pairs while maintaining sorted order by key</item>
///   <item>Removing key-value pairs by key</item>
///   <item>Binary search for efficient key location</item>
///   <item>Accessing values by key with O(log n) complexity</item>
///   <item>Enumeration in sorted key order</item>
///   <item>Custom key comparison logic</item>
/// </list>
/// </remarks>
/// <typeparam name="TKey">The type of keys in the map.</typeparam>
/// <typeparam name="TValue">The type of values in the map.</typeparam>
[SuppressMessage("Naming", "CA1710:Identifiers should have correct suffix", Justification = "OrderedMap is a known collection name")]
public class OrderedMap<TKey, TValue>(IComparer<TKey>? comparer = null) : IDictionary<TKey, TValue>, IReadOnlyDictionary<TKey, TValue>
	where TKey : notnull
{
	/// <summary>
	/// Represents a key-value pair stored in the map.
	/// </summary>
	private struct Entry(TKey key, TValue value)
	{
		public TKey Key { get; set; } = key;
		public TValue Value { get; set; } = value;
	}

	/// <summary>
	/// The internal list that stores key-value pairs in sorted order by key.
	/// </summary>
	private readonly List<Entry> items = comparer is null && !typeof(IComparable<TKey>).IsAssignableFrom(typeof(TKey)) && !typeof(IComparable).IsAssignableFrom(typeof(TKey))
		? throw new ArgumentException($"Type {typeof(TKey)} must implement IComparable<TKey> or IComparable when no comparer is provided.")
		: [];

	/// <summary>
	/// The comparer used to maintain sorted order by key.
	/// </summary>
	private readonly IComparer<TKey> comparer = comparer ?? Comparer<TKey>.Default;

	/// <summary>
	/// Gets the number of key-value pairs in the map.
	/// </summary>
	public int Count => items.Count;

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

			int index = BinarySearchByKey(key);
			return index < 0 ? throw new KeyNotFoundException($"The key '{key}' was not found in the map.") : items[index].Value;
		}
		set
		{
			ArgumentNullException.ThrowIfNull(key);

			int index = BinarySearchByKey(key);
			if (index >= 0)
			{
				// Key exists, update the value
				Entry entry = items[index];
				entry.Value = value;
				items[index] = entry;
			}
			else
			{
				// Key doesn't exist, add new entry
				index = ~index; // Convert to insertion point
				items.Insert(index, new Entry(key, value));
			}
		}
	}

	/// <summary>
	/// Gets a collection containing the keys in the map in sorted order.
	/// </summary>
	public ICollection<TKey> Keys => new KeyCollection(this);

	/// <summary>
	/// Gets a collection containing the values in the map in the order of their corresponding sorted keys.
	/// </summary>
	public ICollection<TValue> Values => new ValueCollection(this);

	/// <summary>
	/// Gets the keys in the map in sorted order (read-only).
	/// </summary>
	IEnumerable<TKey> IReadOnlyDictionary<TKey, TValue>.Keys => Keys;

	/// <summary>
	/// Gets the values in the map in the order of their corresponding sorted keys (read-only).
	/// </summary>
	IEnumerable<TValue> IReadOnlyDictionary<TKey, TValue>.Values => Values;

	/// <summary>
	/// Initializes a new instance of the <see cref="OrderedMap{TKey, TValue}"/> class with initial capacity.
	/// </summary>
	/// <param name="capacity">The initial capacity of the map.</param>
	/// <exception cref="ArgumentException">Thrown when TKey does not implement IComparable{TKey}.</exception>
	/// <exception cref="ArgumentOutOfRangeException">Thrown when capacity is negative.</exception>
	public OrderedMap(int capacity) : this()
	{
		ArgumentOutOfRangeException.ThrowIfNegative(capacity);

		if (!typeof(IComparable<TKey>).IsAssignableFrom(typeof(TKey)) && !typeof(IComparable).IsAssignableFrom(typeof(TKey)))
		{
			throw new ArgumentException($"Type {typeof(TKey)} must implement IComparable<TKey> or IComparable when no comparer is provided.");
		}

		items = new List<Entry>(capacity);
		comparer = Comparer<TKey>.Default;
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="OrderedMap{TKey, TValue}"/> class with initial capacity and key comparer.
	/// </summary>
	/// <param name="capacity">The initial capacity of the map.</param>
	/// <param name="comparer">The comparer to use for ordering keys.</param>
	/// <exception cref="ArgumentNullException">Thrown when comparer is null.</exception>
	/// <exception cref="ArgumentOutOfRangeException">Thrown when capacity is negative.</exception>
	public OrderedMap(int capacity, IComparer<TKey> comparer) : this(comparer)
	{
		ArgumentOutOfRangeException.ThrowIfNegative(capacity);
		ArgumentNullException.ThrowIfNull(comparer);

		items = new List<Entry>(capacity);
		this.comparer = comparer;
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="OrderedMap{TKey, TValue}"/> class with key-value pairs from an existing dictionary.
	/// </summary>
	/// <param name="dictionary">The dictionary whose key-value pairs are copied to the new ordered map.</param>
	/// <exception cref="ArgumentNullException">Thrown when dictionary is null.</exception>
	/// <exception cref="ArgumentException">Thrown when TKey does not implement IComparable{TKey}.</exception>
	public OrderedMap(IDictionary<TKey, TValue> dictionary) : this()
	{
		ArgumentNullException.ThrowIfNull(dictionary);

		if (!typeof(IComparable<TKey>).IsAssignableFrom(typeof(TKey)) && !typeof(IComparable).IsAssignableFrom(typeof(TKey)))
		{
			throw new ArgumentException($"Type {typeof(TKey)} must implement IComparable<TKey> or IComparable when no comparer is provided.");
		}

		items = [];
		comparer = Comparer<TKey>.Default;

		foreach (KeyValuePair<TKey, TValue> pair in dictionary)
		{
			Add(pair.Key, pair.Value);
		}
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="OrderedMap{TKey, TValue}"/> class with key-value pairs from an existing dictionary and a key comparer.
	/// </summary>
	/// <param name="dictionary">The dictionary whose key-value pairs are copied to the new ordered map.</param>
	/// <param name="comparer">The comparer to use for ordering keys.</param>
	/// <exception cref="ArgumentNullException">Thrown when dictionary or comparer is null.</exception>
	public OrderedMap(IDictionary<TKey, TValue> dictionary, IComparer<TKey> comparer) : this(comparer)
	{
		ArgumentNullException.ThrowIfNull(dictionary);
		ArgumentNullException.ThrowIfNull(comparer);

		items = [];
		this.comparer = comparer;

		foreach (KeyValuePair<TKey, TValue> pair in dictionary)
		{
			Add(pair.Key, pair.Value);
		}
	}

	/// <summary>
	/// Adds a key-value pair to the map.
	/// </summary>
	/// <param name="key">The key of the element to add.</param>
	/// <param name="value">The value of the element to add.</param>
	/// <exception cref="ArgumentNullException">Thrown when key is null.</exception>
	/// <exception cref="ArgumentException">Thrown when key already exists in the map.</exception>
	public void Add(TKey key, TValue value)
	{
		ArgumentNullException.ThrowIfNull(key);

		int index = BinarySearchByKey(key);
		if (index >= 0)
		{
			throw new ArgumentException($"The key '{key}' already exists in the map.");
		}

		index = ~index; // Convert to insertion point
		items.Insert(index, new Entry(key, value));
	}

	/// <summary>
	/// Removes a key-value pair from the map.
	/// </summary>
	/// <param name="key">The key of the element to remove.</param>
	/// <returns>True if the element is successfully found and removed; otherwise, false.</returns>
	/// <exception cref="ArgumentNullException">Thrown when key is null.</exception>
	public bool Remove(TKey key)
	{
		ArgumentNullException.ThrowIfNull(key);

		int index = BinarySearchByKey(key);
		if (index < 0)
		{
			return false;
		}

		items.RemoveAt(index);
		return true;
	}

	/// <summary>
	/// Determines whether the map contains an element with the specified key.
	/// </summary>
	/// <param name="key">The key to locate in the map.</param>
	/// <returns>True if the map contains an element with the key; otherwise, false.</returns>
	/// <exception cref="ArgumentNullException">Thrown when key is null.</exception>
	public bool ContainsKey(TKey key)
	{
		ArgumentNullException.ThrowIfNull(key);

		return BinarySearchByKey(key) >= 0;
	}

	/// <summary>
	/// Tries to get the value associated with the specified key.
	/// </summary>
	/// <param name="key">The key of the value to get.</param>
	/// <param name="value">When this method returns, contains the value associated with the specified key, if the key is found; otherwise, the default value for the type of the value parameter.</param>
	/// <returns>True if the map contains an element with the specified key; otherwise, false.</returns>
	/// <exception cref="ArgumentNullException">Thrown when key is null.</exception>
	public bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value)
	{
		ArgumentNullException.ThrowIfNull(key);

		int index = BinarySearchByKey(key);
		if (index >= 0)
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
	/// <exception cref="ArgumentNullException">Thrown when key is null.</exception>
	/// <exception cref="ArgumentException">Thrown when key already exists in the map.</exception>
	public void Add(KeyValuePair<TKey, TValue> item) => Add(item.Key, item.Value);

	/// <summary>
	/// Removes all key-value pairs from the map.
	/// </summary>
	public void Clear() => items.Clear();

	/// <summary>
	/// Determines whether the map contains a specific key-value pair.
	/// </summary>
	/// <param name="item">The key-value pair to locate.</param>
	/// <returns>True if the key-value pair is found; otherwise, false.</returns>
	public bool Contains(KeyValuePair<TKey, TValue> item)
	{
		ArgumentNullException.ThrowIfNull(item.Key);

		int index = BinarySearchByKey(item.Key);
		return (index >= 0) && EqualityComparer<TValue>.Default.Equals(items[index].Value, item.Value);
	}

	/// <summary>
	/// Copies the key-value pairs to an array, starting at the specified array index.
	/// </summary>
	/// <param name="array">The destination array.</param>
	/// <param name="arrayIndex">The zero-based index in array at which copying begins.</param>
	/// <exception cref="ArgumentNullException">Thrown when array is null.</exception>
	/// <exception cref="ArgumentOutOfRangeException">Thrown when arrayIndex is out of range.</exception>
	/// <exception cref="ArgumentException">Thrown when the destination array is too small.</exception>
	public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
	{
		ArgumentNullException.ThrowIfNull(array);
		ArgumentOutOfRangeException.ThrowIfNegative(arrayIndex);
		ArgumentOutOfRangeException.ThrowIfGreaterThan(arrayIndex, array.Length);

		if (array.Length - arrayIndex < Count)
		{
			throw new ArgumentException("The destination array is too small to contain all elements.", nameof(array));
		}

		for (int i = 0; i < items.Count; i++)
		{
			Entry entry = items[i];
			array[arrayIndex + i] = new KeyValuePair<TKey, TValue>(entry.Key, entry.Value);
		}
	}

	/// <summary>
	/// Removes the specified key-value pair from the map.
	/// </summary>
	/// <param name="item">The key-value pair to remove.</param>
	/// <returns>True if the key-value pair was successfully removed; otherwise, false.</returns>
	public bool Remove(KeyValuePair<TKey, TValue> item)
	{
		ArgumentNullException.ThrowIfNull(item.Key);

		int index = BinarySearchByKey(item.Key);
		if (index < 0)
		{
			return false;
		}

		// Check if the value matches
		if (!EqualityComparer<TValue>.Default.Equals(items[index].Value, item.Value))
		{
			return false;
		}

		items.RemoveAt(index);
		return true;
	}

	/// <summary>
	/// Returns an enumerator that iterates through the map.
	/// </summary>
	/// <returns>An enumerator that can be used to iterate through the map.</returns>
	public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
	{
		for (int i = 0; i < items.Count; i++)
		{
			Entry entry = items[i];
			yield return new KeyValuePair<TKey, TValue>(entry.Key, entry.Value);
		}
	}

	/// <summary>
	/// Returns an enumerator that iterates through the map.
	/// </summary>
	/// <returns>An enumerator that can be used to iterate through the map.</returns>
	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

	/// <summary>
	/// Creates a shallow copy of the ordered map.
	/// </summary>
	/// <returns>A new ordered map that is a shallow copy of the original.</returns>
	public OrderedMap<TKey, TValue> Clone() => new(this, comparer);

	/// <summary>
	/// Returns the index of the specified key in the sorted list.
	/// </summary>
	/// <param name="key">The key to locate in the sorted list.</param>
	/// <returns>The zero-based index of key in the sorted list, if key is found; otherwise, a negative number that is the bitwise complement of the index of the next element that is larger than key or, if there is no larger element, the bitwise complement of the index of the last element in the list.</returns>
	/// <exception cref="ArgumentNullException">Thrown when key is null.</exception>
	private int BinarySearchByKey(TKey key)
	{
		ArgumentNullException.ThrowIfNull(key);

		int left = 0;
		int right = items.Count - 1;

		while (left <= right)
		{
			int mid = left + ((right - left) / 2);
			int comparison = comparer.Compare(key, items[mid].Key);

			if (comparison == 0)
			{
				return mid;
			}
			else if (comparison < 0)
			{
				right = mid - 1;
			}
			else
			{
				left = mid + 1;
			}
		}

		return ~left;
	}

	/// <summary>
	/// Collection wrapper for keys that maintains sorted order.
	/// </summary>
	private sealed class KeyCollection(OrderedMap<TKey, TValue> map) : ICollection<TKey>
	{
		private readonly OrderedMap<TKey, TValue> map = map;

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

			if (array.Length - arrayIndex < Count)
			{
				throw new ArgumentException("The destination array is too small to contain all elements.", nameof(array));
			}

			for (int i = 0; i < map.items.Count; i++)
			{
				array[arrayIndex + i] = map.items[i].Key;
			}
		}

		public IEnumerator<TKey> GetEnumerator()
		{
			for (int i = 0; i < map.items.Count; i++)
			{
				yield return map.items[i].Key;
			}
		}

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	}

	/// <summary>
	/// Collection wrapper for values in the order of their corresponding sorted keys.
	/// </summary>
	private sealed class ValueCollection(OrderedMap<TKey, TValue> map) : ICollection<TValue>
	{
		private readonly OrderedMap<TKey, TValue> map = map;

		public int Count => map.Count;
		public bool IsReadOnly => true;

		public void Add(TValue item) => throw new NotSupportedException("Values collection is read-only.");
		public void Clear() => throw new NotSupportedException("Values collection is read-only.");
		public bool Remove(TValue item) => throw new NotSupportedException("Values collection is read-only.");

		public bool Contains(TValue item) => map.items.Any(entry => EqualityComparer<TValue>.Default.Equals(entry.Value, item));

		public void CopyTo(TValue[] array, int arrayIndex)
		{
			ArgumentNullException.ThrowIfNull(array);
			ArgumentOutOfRangeException.ThrowIfNegative(arrayIndex);
			ArgumentOutOfRangeException.ThrowIfGreaterThan(arrayIndex, array.Length);

			if (array.Length - arrayIndex < Count)
			{
				throw new ArgumentException("The destination array is too small to contain all elements.", nameof(array));
			}

			for (int i = 0; i < map.items.Count; i++)
			{
				array[arrayIndex + i] = map.items[i].Value;
			}
		}

		public IEnumerator<TValue> GetEnumerator()
		{
			for (int i = 0; i < map.items.Count; i++)
			{
				yield return map.items[i].Value;
			}
		}

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	}
}
