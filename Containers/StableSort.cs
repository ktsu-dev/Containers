// Copyright (c) 2023-2026 ktsu-dev contributors

namespace ktsu.Containers;

/// <summary>
/// A stable merge sort, used by the sorted containers to build their backing list in bulk.
/// </summary>
/// <remarks>
/// <see cref="Array.Sort{T}(T[], IComparer{T})"/> is introspective and so not stable, which would
/// reorder elements that compare equal. The sorted containers rely on equal elements keeping their
/// original order: <see cref="OrderedSet{T}"/> keeps the first of them, and
/// <see cref="OrderedCollection{T}"/> keeps them in insertion order.
/// </remarks>
internal static class StableSort
{
	/// <summary>
	/// Ranges at or below this length are sorted by insertion sort rather than split further.
	/// </summary>
	private const int InsertionSortThreshold = 16;

	/// <summary>
	/// Sorts <paramref name="array"/> in place, keeping elements that compare equal in their original order.
	/// </summary>
	/// <typeparam name="T">The element type.</typeparam>
	/// <param name="array">The array to sort.</param>
	/// <param name="comparer">The comparer that defines the order.</param>
	/// <remarks>O(n log n) in the worst case, and O(n) when the input is already sorted.</remarks>
	internal static void Sort<T>(T[] array, IComparer<T> comparer)
	{
		if (array.Length < 2)
		{
			return;
		}

		T[] buffer = new T[(array.Length / 2) + 1];
		SortRange(array, buffer, 0, array.Length, comparer);
	}

	/// <summary>
	/// Sorts the elements of <paramref name="array"/> in [<paramref name="low"/>, <paramref name="high"/>).
	/// </summary>
	private static void SortRange<T>(T[] array, T[] buffer, int low, int high, IComparer<T> comparer)
	{
		if (high - low <= InsertionSortThreshold)
		{
			InsertionSort(array, low, high, comparer);
			return;
		}

		int mid = low + ((high - low) / 2);
		SortRange(array, buffer, low, mid, comparer);
		SortRange(array, buffer, mid, high, comparer);

		// The halves are already in order relative to each other
		if (comparer.Compare(array[mid - 1], array[mid]) <= 0)
		{
			return;
		}

		// Merge, taking from the left half on ties so equal elements keep their order
		int leftLength = mid - low;
		Array.Copy(array, low, buffer, 0, leftLength);

		int i = 0;
		int j = mid;
		int k = low;
		while (i < leftLength && j < high)
		{
			array[k++] = comparer.Compare(array[j], buffer[i]) < 0 ? array[j++] : buffer[i++];
		}

		if (i < leftLength)
		{
			Array.Copy(buffer, i, array, k, leftLength - i);
		}
	}

	/// <summary>
	/// Stable insertion sort of the elements of <paramref name="array"/> in [<paramref name="low"/>, <paramref name="high"/>).
	/// </summary>
	private static void InsertionSort<T>(T[] array, int low, int high, IComparer<T> comparer)
	{
		for (int i = low + 1; i < high; i++)
		{
			T item = array[i];
			int j = i - 1;
			while (j >= low && comparer.Compare(array[j], item) > 0)
			{
				array[j + 1] = array[j];
				j--;
			}

			array[j + 1] = item;
		}
	}
}
