// Copyright (c) 2023-2026 ktsu-dev contributors

namespace ktsu.Containers;

/// <summary>
/// A map whose entries can be read by position and whose changes are counted, so one set of
/// enumerators in <see cref="Enumeration"/> can serve every map.
/// </summary>
/// <typeparam name="TKey">The type of the keys.</typeparam>
/// <typeparam name="TValue">The type of the values.</typeparam>
internal interface IVersionedEntries<TKey, TValue>
{
	/// <summary>Gets the number of entries.</summary>
	public int Count { get; }

	/// <summary>Gets a number that changes whenever the map does.</summary>
	public int Version { get; }

	/// <summary>Gets the key of the entry at a position.</summary>
	/// <param name="index">The position.</param>
	/// <returns>The key.</returns>
	public TKey KeyAt(int index);

	/// <summary>Gets the value of the entry at a position.</summary>
	/// <param name="index">The position.</param>
	/// <returns>The value.</returns>
	public TValue ValueAt(int index);
}
