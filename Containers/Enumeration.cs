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
}
