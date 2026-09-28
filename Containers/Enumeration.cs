// Copyright (c) 2023-2026 ktsu-dev contributors

namespace ktsu.Containers;

/// <summary>
/// Shared checks for the enumerators of the containers that keep a version counter.
/// </summary>
internal static class Enumeration
{
	/// <summary>
	/// Throws when a collection has changed since its enumerator was created, as the
	/// <see cref="System.Collections.Generic.IEnumerator{T}"/> contract expects.
	/// </summary>
	/// <param name="expected">The version the collection had when enumeration began.</param>
	/// <param name="actual">The version the collection has now.</param>
	/// <exception cref="InvalidOperationException">The collection was modified.</exception>
	internal static void ThrowIfModified(int expected, int actual)
	{
		if (expected != actual)
		{
			throw new InvalidOperationException("Collection was modified; enumeration operation may not execute.");
		}
	}

	/// <summary>
	/// Enumerates a map's entries, throwing if the map changes before enumeration finishes.
	/// </summary>
	/// <typeparam name="TKey">The type of the keys.</typeparam>
	/// <typeparam name="TValue">The type of the values.</typeparam>
	/// <param name="map">The map.</param>
	/// <returns>An enumerator over the entries.</returns>
	internal static IEnumerator<KeyValuePair<TKey, TValue>> Pairs<TKey, TValue>(IVersionedEntries<TKey, TValue> map) =>
		Iterate(map, map.Version, static (m, i) => new KeyValuePair<TKey, TValue>(m.KeyAt(i), m.ValueAt(i)));

	/// <summary>
	/// Enumerates a map's keys, throwing if the map changes before enumeration finishes.
	/// </summary>
	/// <typeparam name="TKey">The type of the keys.</typeparam>
	/// <typeparam name="TValue">The type of the values.</typeparam>
	/// <param name="map">The map.</param>
	/// <returns>An enumerator over the keys.</returns>
	internal static IEnumerator<TKey> Keys<TKey, TValue>(IVersionedEntries<TKey, TValue> map) =>
		Iterate(map, map.Version, static (m, i) => m.KeyAt(i));

	/// <summary>
	/// Enumerates a map's values, throwing if the map changes before enumeration finishes.
	/// </summary>
	/// <typeparam name="TKey">The type of the keys.</typeparam>
	/// <typeparam name="TValue">The type of the values.</typeparam>
	/// <param name="map">The map.</param>
	/// <returns>An enumerator over the values.</returns>
	internal static IEnumerator<TValue> Values<TKey, TValue>(IVersionedEntries<TKey, TValue> map) =>
		Iterate(map, map.Version, static (m, i) => m.ValueAt(i));

	/// <remarks>
	/// The version is captured by the caller, when the enumerator is created, not on the first
	/// <c>MoveNext</c>, so a change made in between is caught as it is by <see cref="List{T}"/>.
	/// </remarks>
	private static IEnumerator<TResult> Iterate<TKey, TValue, TResult>(
		IVersionedEntries<TKey, TValue> map,
		int expected,
		Func<IVersionedEntries<TKey, TValue>, int, TResult> select)
	{
		for (int i = 0; i < map.Count; i++)
		{
			ThrowIfModified(expected, map.Version);
			yield return select(map, i);
		}

		ThrowIfModified(expected, map.Version);
	}
}
