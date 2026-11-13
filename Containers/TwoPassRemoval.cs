// Copyright (c) 2023-2026 ktsu-dev contributors

namespace ktsu.Containers;

/// <summary>
/// Removes the elements a predicate selects in two passes, so a predicate that throws changes nothing.
/// </summary>
/// <remarks>
/// The set operations' predicates call the user's comparer. Compacting while one of them could still
/// throw, as <see cref="List{T}.RemoveAll"/> does, left a set half compacted: an element duplicated,
/// another lost, and the hash index disagreeing with the storage (#162). The first pass only records
/// which elements to drop; the second makes no predicate call, and compacts in order.
/// </remarks>
internal static class TwoPassRemoval
{
	/// <summary>
	/// Calls <paramref name="shouldRemove"/> once per element, in order, and records its answers.
	/// </summary>
	/// <returns>A flag per element, or null when nothing is to be dropped (allocating nothing).</returns>
	internal static bool[]? Mark<T, TList>(TList items, Func<T, bool> shouldRemove)
		where TList : IList<T>
	{
		bool[]? drop = null;
		for (int index = 0; index < items.Count; index++)
		{
			if (shouldRemove(items[index]))
			{
				drop ??= new bool[items.Count];
				drop[index] = true;
			}
		}

		return drop;
	}

	/// <summary>
	/// Moves the elements not flagged in <paramref name="drop"/> to the front, preserving their order.
	/// </summary>
	/// <param name="items">The elements <paramref name="drop"/> was marked against.</param>
	/// <param name="drop">The flags from <see cref="Mark{T, TList}"/>.</param>
	/// <param name="dropped">Called for each dropped element, such as to remove it from an index.</param>
	/// <returns>How many elements were kept; the slots from there on are stale and the caller's to clear.</returns>
	internal static int Compact<T, TList>(TList items, bool[] drop, Action<T>? dropped)
		where TList : IList<T>
	{
		int write = 0;
		for (int read = 0; read < drop.Length; read++)
		{
			T item = items[read];
			if (drop[read])
			{
				dropped?.Invoke(item);
			}
			else
			{
				items[write++] = item;
			}
		}

		return write;
	}

	/// <summary>
	/// Removes every element of <paramref name="items"/> that <paramref name="shouldRemove"/> selects.
	/// </summary>
	/// <returns>true if anything was removed; otherwise the list is untouched, so its enumerators stay valid.</returns>
	internal static bool RemoveWhere<T>(List<T> items, Func<T, bool> shouldRemove, Action<T>? dropped)
	{
		bool[]? drop = Mark<T, List<T>>(items, shouldRemove);
		if (drop is null)
		{
			return false;
		}

		int kept = Compact(items, drop, dropped);
		items.RemoveRange(kept, items.Count - kept);
		return true;
	}
}
