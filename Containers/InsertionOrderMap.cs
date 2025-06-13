// Copyright (c) ktsu.dev
// All rights reserved.
// Licensed under the MIT license.

namespace ktsu.Containers;

using System.Collections;
using System.Diagnostics.CodeAnalysis;

/// <summary>
/// Represents a generic map/dictionary that maintains key-value pairs in insertion order by key.
/// </summary>
/// <remarks>
/// An insertion-order map maintains key-value pairs in the exact order keys were first added,
/// preserving the insertion sequence without any sorting or reordering.
///
/// This implementation uses a list for order preservation and a dictionary for fast key lookups,
/// providing efficient insertion-order enumeration and O(1) key-based access.
///
/// This implementation supports:
/// <list type="bullet">
///   <item>Adding key-value pairs while maintaining insertion order by key</item>
///   <item>Removing key-value pairs by key</item>
///   <item>Fast key lookup with O(1) complexity</item>
///   <item>Accessing values by key with O(1) complexity</item>
///   <item>Enumeration in insertion key order</item>
/// </list>
/// </remarks>
/// <typeparam name="TKey">The type of keys in the map.</typeparam>
/// <typeparam name="TValue">The type of values in the map.</typeparam>
[SuppressMessage("Naming", "CA1710:Identifiers should have correct suffix", Justification = "InsertionOrderMap is a known collection name")]
public class InsertionOrderMap<TKey, TValue> : IDictionary<TKey, TValue>, IReadOnlyDictionary<TKey, TValue>
	where TKey : notnull
{
	/// <summary>
	/// Represents a key-value pair stored in the map.
	/// </summary>
	private struct Entry
	{
		public TKey Key { get; set; }
		public TValue Value { get; set; }

		public Entry(TKey key, TValue value)
		{
			Key = key;
			Value = value;
		}
	}

	/// <summary>
	/// The internal list that stores key-value pairs in insertion order by key.
	/// </summary>
	private readonly List<Entry> items;

	/// <summary>
	/// The internal dictionary used for fast key-based lookups.
	/// </summary>
	private readonly Dictionary<TKey, int> keyToIndex;

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

			if (!keyToIndex.TryGetValue(key, out int index))
			{
				throw new KeyNotFoundException($"The key '{key}' was not found in the map.");
			}

			return items[index].Value;
		}
		set
		{
			ArgumentNullException.ThrowIfNull(key);

			if (keyToIndex.TryGetValue(key, out int index))
			{
				// Key exists, update the value
				Entry entry = items[index];
				entry.Value = value;
				items[index] = entry;
			}
			else
			{
				// Key doesn't exist, add new entry
				index = items.Count;
				items.Add(new Entry(key, value));
				keyToIndex[key] = index;
			}
		}
	}

	/// <summary>
	/// Gets a collection containing the keys in the map in insertion order.
	/// </summary>
	public ICollection<TKey> Keys => new KeyCollection(this);

	/// <summary>
	/// Gets a collection containing the values in the map in the order of their corresponding insertion keys.
	/// </summary>
	public ICollection<TValue> Values => new ValueCollection(this);

	/// <summary>
	/// Gets the keys in the map in insertion order (read-only).
	/// </summary>
	IEnumerable<TKey> IReadOnlyDictionary<TKey, TValue>.Keys => Keys;

	/// <summary>
	/// Gets the values in the map in the order of their corresponding insertion keys (read-only).
	/// </summary>
	IEnumerable<TValue> IReadOnlyDictionary<TKey, TValue>.Values => Values;

	/// <summary>
	/// Initializes a new instance of the <see cref="InsertionOrderMap{TKey, TValue}"/> class.
	/// </summary>
	public InsertionOrderMap()
	{
		items = [];
		keyToIndex = [];
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="InsertionOrderMap{TKey, TValue}"/> class with the specified key equality comparer.
	/// </summary>
	/// <param name="comparer">The equality comparer to use for keys.</param>
	/// <exception cref="ArgumentNullException">Thrown when comparer is null.</exception>
	public InsertionOrderMap(IEqualityComparer<TKey> comparer)
	{
		ArgumentNullException.ThrowIfNull(comparer);

		items = [];
		keyToIndex = new Dictionary<TKey, int>(comparer);
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="InsertionOrderMap{TKey, TValue}"/> class with initial capacity.
	/// </summary>
	/// <param name="capacity">The initial capacity of the map.</param>
	/// <exception cref="ArgumentOutOfRangeException">Thrown when capacity is negative.</exception>
	public InsertionOrderMap(int capacity)
	{
		ArgumentOutOfRangeException.ThrowIfNegative(capacity);

		items = new List<Entry>(capacity);
		keyToIndex = new Dictionary<TKey, int>(capacity);
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="InsertionOrderMap{TKey, TValue}"/> class with initial capacity and key equality comparer.
	/// </summary>
	/// <param name="capacity">The initial capacity of the map.</param>
	/// <param name="comparer">The equality comparer to use for keys.</param>
	/// <exception cref="ArgumentNullException">Thrown when comparer is null.</exception>
	/// <exception cref="ArgumentOutOfRangeException">Thrown when capacity is negative.</exception>
	public InsertionOrderMap(int capacity, IEqualityComparer<TKey> comparer)
	{
		ArgumentOutOfRangeException.ThrowIfNegative(capacity);
		ArgumentNullException.ThrowIfNull(comparer);

		items = new List<Entry>(capacity);
		keyToIndex = new Dictionary<TKey, int>(capacity, comparer);
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="InsertionOrderMap{TKey, TValue}"/> class with key-value pairs from an existing dictionary.
	/// </summary>
	/// <param name="dictionary">The dictionary whose key-value pairs are copied to the new insertion-order map.</param>
	/// <exception cref="ArgumentNullException">Thrown when dictionary is null.</exception>
	public InsertionOrderMap(IDictionary<TKey, TValue> dictionary)
	{
		ArgumentNullException.ThrowIfNull(dictionary);

		items = [];
		keyToIndex = [];

		foreach (KeyValuePair<TKey, TValue> pair in dictionary)
		{
			Add(pair.Key, pair.Value);
		}
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="InsertionOrderMap{TKey, TValue}"/> class with key-value pairs from an existing dictionary and a key equality comparer.
	/// </summary>
	/// <param name="dictionary">The dictionary whose key-value pairs are copied to the new insertion-order map.</param>
	/// <param name="comparer">The equality comparer to use for keys.</param>
	/// <exception cref="ArgumentNullException">Thrown when dictionary or comparer is null.</exception>
	public InsertionOrderMap(IDictionary<TKey, TValue> dictionary, IEqualityComparer<TKey> comparer)
	{
		ArgumentNullException.ThrowIfNull(dictionary);
		ArgumentNullException.ThrowIfNull(comparer);

		items = [];
		keyToIndex = new Dictionary<TKey, int>(comparer);

		foreach (KeyValuePair<TKey, TValue> pair in dictionary)
		{
			Add(pair.Key, pair.Value);
		}
	}

	/// <summary>
	/// Adds a key-value pair to the map in insertion order.
	/// </summary>
	/// <param name="key">The key of the element to add.</param>
	/// <param name="value">The value of the element to add.</param>
	/// <exception cref="ArgumentNullException">Thrown when key is null.</exception>
	/// <exception cref="ArgumentException">Thrown when a key with the same value already exists.</exception>
	/// <remarks>
	/// This operation has O(1) average time complexity.
	/// The key-value pair is added at the end of the collection to maintain insertion order.
	/// </remarks>
	public void Add(TKey key, TValue value)
	{
		ArgumentNullException.ThrowIfNull(key);

		if (keyToIndex.ContainsKey(key))
		{
			throw new ArgumentException($"An element with the key '{key}' already exists.", nameof(key));
		}

		int index = items.Count;
		items.Add(new Entry(key, value));
		keyToIndex[key] = index;
	}

	/// <summary>
	/// Removes the element with the specified key from the map.
	/// </summary>
	/// <param name="key">The key of the element to remove.</param>
	/// <returns>true if the element was found and removed; otherwise, false.</returns>
	/// <exception cref="ArgumentNullException">Thrown when key is null.</exception>
	/// <remarks>
	/// This operation has O(n) time complexity as it may need to update indices after removal.
	/// </remarks>
	public bool Remove(TKey key)
	{
		ArgumentNullException.ThrowIfNull(key);

		if (!keyToIndex.TryGetValue(key, out int index))
		{
			return false;
		}

		// Remove from the list
		items.RemoveAt(index);
		keyToIndex.Remove(key);

		// Update indices in the dictionary for all elements after the removed one
		for (int i = index; i < items.Count; i++)
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
		items.Clear();
		keyToIndex.Clear();
	}

	/// <summary>
	/// Determines whether the map contains the specified key-value pair.
	/// </summary>
	/// <param name="item">The key-value pair to locate.</param>
	/// <returns>true if the key-value pair is found; otherwise, false.</returns>
	public bool Contains(KeyValuePair<TKey, TValue> item)
	{
		if (!keyToIndex.TryGetValue(item.Key, out int index))
		{
			return false;
		}

		return EqualityComparer<TValue>.Default.Equals(items[index].Value, item.Value);
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
	/// <returns>true if the key-value pair was found and removed; otherwise, false.</returns>
	public bool Remove(KeyValuePair<TKey, TValue> item)
	{
		if (!keyToIndex.TryGetValue(item.Key, out int index))
		{
			return false;
		}

		if (!EqualityComparer<TValue>.Default.Equals(items[index].Value, item.Value))
		{
			return false;
		}

		return Remove(item.Key);
	}

	/// <summary>
	/// Returns an enumerator that iterates through the map in insertion order.
	/// </summary>
	/// <returns>An enumerator for the map.</returns>
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
	/// <returns>An enumerator for the map.</returns>
	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

	/// <summary>
	/// Creates a shallow copy of the map.
	/// </summary>
	/// <returns>A new map containing all key-value pairs from the original map in the same insertion order.</returns>
	public InsertionOrderMap<TKey, TValue> Clone() => new(this, keyToIndex.Comparer);

	/// <summary>
	/// Collection class for keys that maintains insertion order.
	/// </summary>
	private sealed class KeyCollection(InsertionOrderMap<TKey, TValue> map) : ICollection<TKey>
	{
		private readonly InsertionOrderMap<TKey, TValue> map = map;

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
	/// Collection class for values that maintains insertion order.
	/// </summary>
	private sealed class ValueCollection(InsertionOrderMap<TKey, TValue> map) : ICollection<TValue>
	{
		private readonly InsertionOrderMap<TKey, TValue> map = map;

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
			ArgumentOutOfRangeException.ThrowIfGreaterThan(Count, array.Length - arrayIndex);

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
