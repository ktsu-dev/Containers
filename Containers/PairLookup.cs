// Copyright (c) 2023-2026 ktsu-dev contributors

namespace ktsu.Containers;

/// <summary>
/// Finds a key-value pair in a map, for the <see cref="ICollection{T}"/> members that take a whole pair.
/// </summary>
internal static class PairLookup
{
	/// <summary>
	/// Determines whether a map holds a pair's key with an equal value.
	/// </summary>
	/// <typeparam name="TKey">The type of keys in the map.</typeparam>
	/// <typeparam name="TValue">The type of values in the map.</typeparam>
	/// <param name="map">The map to search.</param>
	/// <param name="item">The key-value pair to locate.</param>
	/// <returns><c>true</c> if the map holds the key and its value equals the pair's value; otherwise, <c>false</c>.</returns>
	/// <exception cref="ArgumentNullException">
	/// Thrown, with a ParamName of <c>item</c>, when the key of <paramref name="item"/> is null, as
	/// <see cref="OrderedMap{TKey, TValue}"/> reports it (ktsu-dev/Containers#164).
	/// </exception>
	internal static bool Contains<TKey, TValue>(IReadOnlyDictionary<TKey, TValue> map, KeyValuePair<TKey, TValue> item)
		where TKey : notnull
	{
		NullKey.ThrowIfNullItem(item.Key);

		return map.TryGetValue(item.Key, out TValue? value)
			&& EqualityComparer<TValue>.Default.Equals(value, item.Value);
	}
}
