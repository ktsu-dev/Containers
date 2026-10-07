// Copyright (c) 2023-2026 ktsu-dev contributors

namespace ktsu.Containers.Tests;

using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

[TestClass]
public class ContiguousSetTests
{
	[TestMethod]
	public void Constructor_WithoutParameters_CreatesEmptySet()
	{
		// Arrange & Act
		ContiguousSet<int> set = [];

		// Assert
		Assert.AreEqual(0, set.Count);
		Assert.IsFalse(set.IsReadOnly, "Set should not be read-only");
	}

	[TestMethod]
	public void Constructor_WithCapacity_CreatesSetWithCapacity()
	{
		// Arrange & Act
		ContiguousSet<int> set = new(10);

		// Assert
		Assert.AreEqual(0, set.Count);
	}

	[TestMethod]
	public void Constructor_WithCollection_AddsUniqueElements()
	{
		// Arrange
		int[] items = [3, 1, 4, 1, 5, 3, 2, 6]; // Has duplicates

		// Act
		ContiguousSet<int> set = [.. items];

		// Assert
		Assert.AreEqual(6, set.Count); // Only unique elements
		Assert.Contains(1, set);
		Assert.Contains(2, set);
		Assert.Contains(3, set);
		Assert.Contains(4, set);
		Assert.Contains(5, set);
		Assert.Contains(6, set);
	}

	[TestMethod]
	public void Constructor_WithNullCollection_ThrowsArgumentNullException()
	{
		// Arrange, Act & Assert
		Assert.ThrowsExactly<ArgumentNullException>(() =>
			new ContiguousSet<int>((IEnumerable<int>)null!)
		);
	}

	[TestMethod]
	public void Add_NewElement_ReturnsTrue()
	{
		// Arrange
		ContiguousSet<int> set = [];

		// Act
		bool result = set.Add(5);

		// Assert
		Assert.IsTrue(result, "Add should return true for new element");
		Assert.AreEqual(1, set.Count);
		Assert.Contains(5, set);
	}

	[TestMethod]
	public void Add_DuplicateElement_ReturnsFalse()
	{
		// Arrange
		ContiguousSet<int> set = [5];

		// Act
		bool result = set.Add(5);

		// Assert
		Assert.IsFalse(result, "Add should return false for duplicate element");
		Assert.AreEqual(1, set.Count);
	}

	[TestMethod]
	public void Add_MultipleUniqueElements_AddsAll()
	{
		// Arrange
		ContiguousSet<int> set = [];

		// Act
		bool result1 = set.Add(3);
		bool result2 = set.Add(1);
		bool result3 = set.Add(4);
		bool result4 = set.Add(1); // Duplicate

		// Assert
		Assert.IsTrue(result1, "Add should return true for first unique element");
		Assert.IsTrue(result2, "Add should return true for second unique element");
		Assert.IsTrue(result3, "Add should return true for third unique element");
		Assert.IsFalse(result4, "Add should return false for duplicate element");
		Assert.AreEqual(3, set.Count);
	}

	[TestMethod]
	public void Clear_WithElements_RemovesAllElements()
	{
		// Arrange
		ContiguousSet<int> set = [.. new int[] { 1, 2, 3, 4, 5 }];

		// Act
		set.Clear();

		// Assert
		Assert.AreEqual(0, set.Count);
	}

	[TestMethod]
	public void Contains_ExistingElement_ReturnsTrue()
	{
		// Arrange
		ContiguousSet<int> set = [.. new int[] { 1, 2, 3, 4, 5 }];

		// Act & Assert
		Assert.Contains(3, set);
		Assert.Contains(1, set);
		Assert.Contains(5, set);
	}

	[TestMethod]
	public void Contains_NonExistingElement_ReturnsFalse()
	{
		// Arrange
		ContiguousSet<int> set = [.. new int[] { 1, 2, 3, 4, 5 }];

		// Act & Assert
		Assert.DoesNotContain(6, set);
		Assert.DoesNotContain(0, set);
	}

	[TestMethod]
	public void Remove_ExistingElement_RemovesAndReturnsTrue()
	{
		// Arrange
		ContiguousSet<int> set = [.. new int[] { 3, 1, 4, 2 }];

		// Act
		bool result = set.Remove(1);

		// Assert
		Assert.IsTrue(result, "Remove should return true for existing element");
		Assert.AreEqual(3, set.Count);
		Assert.Contains(3, set);
		Assert.DoesNotContain(1, set);
		Assert.Contains(4, set);
		Assert.Contains(2, set);
	}

	[TestMethod]
	public void Remove_NonExistingElement_ReturnsFalse()
	{
		// Arrange
		ContiguousSet<int> set = [.. new int[] { 1, 2, 3 }];

		// Act
		bool result = set.Remove(5);

		// Assert
		Assert.IsFalse(result, "Remove should return false for non-existing element");
		Assert.AreEqual(3, set.Count);
	}

	[TestMethod]
	public void CopyTo_ValidArray_CopiesElements()
	{
		// Arrange
		ContiguousSet<int> set = [.. new int[] { 3, 1, 4, 2 }];
		int[] array = new int[6];

		// Act
		set.CopyTo(array, 1);

		// Assert
		Assert.AreEqual(0, array[0]); // Unchanged
									  // Elements 1-4 should contain set elements (order may vary)
		Assert.AreEqual(0, array[5]); // Unchanged

		// Verify all set elements are in the array
		HashSet<int> copied = [.. array[1..5]];
		Assert.Contains(1, copied);
		Assert.Contains(2, copied);
		Assert.Contains(3, copied);
		Assert.Contains(4, copied);
	}

	[TestMethod]
	public void GetEnumerator_IteratesAllElements()
	{
		// Arrange
		ContiguousSet<int> set = [.. new int[] { 3, 1, 4, 1, 2 }]; // Contains duplicate
		HashSet<int> enumerated = [];

		// Act
		foreach (int item in set)
		{
			enumerated.Add(item);
		}

		// Assert
		Assert.HasCount(4, enumerated); // No duplicates
		Assert.Contains(1, enumerated);
		Assert.Contains(2, enumerated);
		Assert.Contains(3, enumerated);
		Assert.Contains(4, enumerated);
	}

	[TestMethod]
	public void AsReadOnlySpan_ReturnsCorrectSpan()
	{
		// Arrange
		ContiguousSet<int> set = [.. new int[] { 3, 1, 4, 2 }];

		// Act
		ReadOnlySpan<int> span = set.AsReadOnlySpan();

		// Assert
		Assert.HasCount(4, span);

		// Verify all elements are in the span
		HashSet<int> spanElements = [];
		for (int i = 0; i < span.Length; i++)
		{
			spanElements.Add(span[i]);
		}
		Assert.HasCount(4, spanElements);
		Assert.Contains(1, spanElements);
		Assert.Contains(2, spanElements);
		Assert.Contains(3, spanElements);
		Assert.Contains(4, spanElements);
	}

	[TestMethod]
	public void UnionWith_AddsNewElements()
	{
		// Arrange
		ContiguousSet<int> set1 = [.. new int[] { 1, 2, 3 }];
		ContiguousSet<int> set2 = [.. new int[] { 3, 4, 5 }];

		// Act
		set1.UnionWith(set2);

		// Assert
		Assert.AreEqual(5, set1.Count);
		Assert.Contains(1, set1);
		Assert.Contains(2, set1);
		Assert.Contains(3, set1);
		Assert.Contains(4, set1);
		Assert.Contains(5, set1);
	}

	[TestMethod]
	public void IntersectWith_KeepsCommonElements()
	{
		// Arrange
		ContiguousSet<int> set1 = [.. new int[] { 1, 2, 3, 4 }];
		ContiguousSet<int> set2 = [.. new int[] { 2, 4, 5, 6 }];

		// Act
		set1.IntersectWith(set2);

		// Assert
		Assert.AreEqual(2, set1.Count);
		Assert.DoesNotContain(1, set1);
		Assert.Contains(2, set1);
		Assert.DoesNotContain(3, set1);
		Assert.Contains(4, set1);
	}

	[TestMethod]
	public void ExceptWith_RemovesCommonElements()
	{
		// Arrange
		ContiguousSet<int> set1 = [.. new int[] { 1, 2, 3, 4 }];
		ContiguousSet<int> set2 = [.. new int[] { 2, 4 }];

		// Act
		set1.ExceptWith(set2);

		// Assert
		Assert.AreEqual(2, set1.Count);
		Assert.Contains(1, set1);
		Assert.DoesNotContain(2, set1);
		Assert.Contains(3, set1);
		Assert.DoesNotContain(4, set1);
	}

	[TestMethod]
	public void ExceptWith_Self_EmptiesSet()
	{
		ContiguousSet<int> set = [.. new int[] { 1, 2, 3, 4 }];

		set.ExceptWith(set);

		Assert.IsEmpty(set);
	}

	[TestMethod]
	public void SymmetricExceptWith_KeepsUniqueElements()
	{
		// Arrange
		ContiguousSet<int> set1 = [.. new int[] { 1, 2, 3 }];
		ContiguousSet<int> set2 = [.. new int[] { 2, 3, 4 }];

		// Act
		set1.SymmetricExceptWith(set2);

		// Assert
		Assert.AreEqual(2, set1.Count);
		Assert.Contains(1, set1);
		Assert.DoesNotContain(2, set1);
		Assert.DoesNotContain(3, set1);
		Assert.Contains(4, set1);
	}

	[TestMethod]
	public void SetEquals_SameElements_ReturnsTrue()
	{
		// Arrange
		ContiguousSet<int> set1 = [.. new int[] { 1, 2, 3 }];
		ContiguousSet<int> set2 = [.. new int[] { 3, 1, 2 }]; // Different order

		// Act & Assert
		Assert.IsTrue(set1.SetEquals(set2), "Sets with same elements should be equal");
		Assert.IsTrue(set2.SetEquals(set1), "SetEquals should be symmetric");
	}

	[TestMethod]
	public void SetEquals_DifferentElements_ReturnsFalse()
	{
		// Arrange
		ContiguousSet<int> set1 = [.. new int[] { 1, 2, 3 }];
		ContiguousSet<int> set2 = [.. new int[] { 1, 2, 4 }];

		// Act & Assert
		Assert.IsFalse(set1.SetEquals(set2), "Sets with different elements should not be equal");
		Assert.IsFalse(set2.SetEquals(set1), "SetEquals should be symmetric for non-equal sets");
	}

	[TestMethod]
	public void IsSubsetOf_TrueSubset_ReturnsTrue()
	{
		// Arrange
		ContiguousSet<int> subset = [.. new int[] { 1, 3 }];
		ContiguousSet<int> superset = [.. new int[] { 1, 2, 3, 4 }];

		// Act & Assert
		Assert.IsTrue(subset.IsSubsetOf(superset), "Subset should be recognized as subset of superset");
	}

	[TestMethod]
	public void IsSubsetOf_NotSubset_ReturnsFalse()
	{
		// Arrange
		ContiguousSet<int> set1 = [.. new int[] { 1, 5 }];
		ContiguousSet<int> set2 = [.. new int[] { 1, 2, 3, 4 }];

		// Act & Assert
		Assert.IsFalse(set1.IsSubsetOf(set2), "Set with elements not in other should not be subset");
	}

	[TestMethod]
	public void IsSupersetOf_TrueSuperset_ReturnsTrue()
	{
		// Arrange
		ContiguousSet<int> superset = [.. new int[] { 1, 2, 3, 4 }];
		ContiguousSet<int> subset = [.. new int[] { 1, 3 }];

		// Act & Assert
		Assert.IsTrue(superset.IsSupersetOf(subset), "Superset should be recognized as superset of subset");
	}

	[TestMethod]
	public void IsProperSubsetOf_TrueProperSubset_ReturnsTrue()
	{
		// Arrange
		ContiguousSet<int> subset = [.. new int[] { 1, 3 }];
		ContiguousSet<int> superset = [.. new int[] { 1, 2, 3, 4 }];

		// Act & Assert
		Assert.IsTrue(subset.IsProperSubsetOf(superset), "Proper subset should be recognized");
		Assert.IsFalse(superset.IsProperSubsetOf(subset), "Superset should not be proper subset of its subset");
	}

	[TestMethod]
	public void Overlaps_WithCommonElements_ReturnsTrue()
	{
		// Arrange
		ContiguousSet<int> set1 = [.. new int[] { 1, 2, 3 }];
		ContiguousSet<int> set2 = [.. new int[] { 3, 4, 5 }];

		// Act & Assert
		Assert.IsTrue(set1.Overlaps(set2), "Sets with common elements should overlap");
		Assert.IsTrue(set2.Overlaps(set1), "Overlaps should be symmetric");
	}

	[TestMethod]
	public void Overlaps_WithoutCommonElements_ReturnsFalse()
	{
		// Arrange
		ContiguousSet<int> set1 = [.. new int[] { 1, 2, 3 }];
		ContiguousSet<int> set2 = [.. new int[] { 4, 5, 6 }];

		// Act & Assert
		Assert.IsFalse(set1.Overlaps(set2), "Sets without common elements should not overlap");
		Assert.IsFalse(set2.Overlaps(set1), "Overlaps should be symmetric for non-overlapping sets");
	}

	[TestMethod]
	public void ContiguousMemoryLayout_OptimalForCachePerformance()
	{
		// Arrange
		ContiguousSet<int> set = [];

		// Act - Add many unique elements
		for (int i = 0; i < 1000; i++)
		{
			set.Add(i);
		}

		// Assert - Verify all elements are accessible
		Assert.AreEqual(1000, set.Count);
		for (int i = 0; i < 1000; i++)
		{
			Assert.IsTrue(set.Contains(i), $"Set should contain element {i}");
		}

		// Test span access (only possible with contiguous memory)
		ReadOnlySpan<int> span = set.AsReadOnlySpan();
		Assert.HasCount(1000, span);

		// Verify all elements are in the span
		HashSet<int> spanElements = [];
		for (int i = 0; i < span.Length; i++)
		{
			spanElements.Add(span[i]);
		}
		Assert.HasCount(1000, spanElements);
	}

	[TestMethod]
	public void WorksWithStrings()
	{
		// Arrange & Act
		ContiguousSet<string> set = [.. new string[] { "charlie", "alpha", "bravo", "alpha" }]; // Contains duplicate

		// Assert
		Assert.AreEqual(3, set.Count); // Only unique elements
		Assert.Contains("charlie", set);
		Assert.Contains("alpha", set);
		Assert.Contains("bravo", set);
	}

	[TestMethod]
	public void Remove_WithCustomComparer_RemovesItemEqualOnlyUnderComparer()
	{
		// Arrange
		ContiguousSet<string> set = new(StringComparer.OrdinalIgnoreCase) { "Apple", "Banana" };

		// Act
		bool removed = set.Remove("APPLE");

		// Assert
		Assert.IsTrue(removed);
		Assert.HasCount(1, set);
		Assert.IsFalse(set.Contains("apple"));
		string[] expectedItems = ["Banana"];
		Assert.AreSequenceEqual(expectedItems, set);
	}

	[TestMethod]
	public void Remove_WithCustomComparer_ThenAdd_KeepsSingleElement()
	{
		// Arrange
		ContiguousSet<string> set = new(StringComparer.OrdinalIgnoreCase) { "Apple" };

		// Act
		set.Remove("APPLE");
		set.Add("apple");

		// Assert
		Assert.HasCount(1, set);
		string[] expectedItems = ["apple"];
		Assert.AreSequenceEqual(expectedItems, set);
	}

	[TestMethod]
	public void Enumerate_RemovingDuringForeach_Throws()
	{
		ContiguousSet<int> set = [1, 2, 3, 4];

		Assert.ThrowsExactly<InvalidOperationException>(() =>
		{
			foreach (int item in set)
			{
				set.Remove(item);
			}
		});
	}

	[TestMethod]
	public void Enumerate_ChangesMadeThroughEveryMutator_Throw()
	{
		Action<ContiguousSet<int>>[] mutations =
		[
			s => s.Add(9),
			s => s.Remove(1),
			s => s.Clear(),
		];

		foreach (Action<ContiguousSet<int>> mutate in mutations)
		{
			ContiguousSet<int> set = [1, 2, 3];
			using IEnumerator<int> enumerator = set.GetEnumerator();
			Assert.IsTrue(enumerator.MoveNext());

			mutate(set);

			Assert.ThrowsExactly<InvalidOperationException>(() => enumerator.MoveNext());
		}
	}

	[TestMethod]
	public void Enumerate_AddingADuplicateOrRemovingAMissingItem_DoesNotThrow()
	{
		// Neither changes the set, so an enumeration in progress is still valid.
		ContiguousSet<int> set = [1, 2, 3];
		List<int> seen = [];

		foreach (int item in set)
		{
			set.Add(item);
			set.Remove(99);
			seen.Add(item);
		}

		Assert.AreSequenceEqual([1, 2, 3], seen);
	}

	[TestMethod]
	public void UnionWith_Itself_DoesNotThrow()
	{
		ContiguousSet<int> set = [1, 2, 3];

		set.UnionWith(set);

		Assert.AreSequenceEqual([1, 2, 3], set);
	}

	// Removing k of n elements one Remove at a time rescans and shifts the array per element: about
	// 23 s for n = 100k in a Debug build. A single compaction pass takes milliseconds, so the bound
	// below is far from both and does not depend on the machine.
	private const int LargeSetSize = 100_000;
	private const int LargeSetBudgetMilliseconds = 1_500;

	[TestMethod]
	public void IntersectWith_LargeSetWithEmpty_RunsInLinearTime()
	{
		ContiguousSet<int> set = [.. Enumerable.Range(0, LargeSetSize)];

		Stopwatch stopwatch = Stopwatch.StartNew();
		set.IntersectWith([]);
		stopwatch.Stop();

		Assert.IsEmpty(set);
		Assert.IsLessThan(LargeSetBudgetMilliseconds, stopwatch.ElapsedMilliseconds);
	}

	[TestMethod]
	public void SymmetricExceptWith_LargeSetWithSameElements_RunsInLinearTime()
	{
		ContiguousSet<int> set = [.. Enumerable.Range(0, LargeSetSize)];

		Stopwatch stopwatch = Stopwatch.StartNew();
		set.SymmetricExceptWith(Enumerable.Range(0, LargeSetSize));
		stopwatch.Stop();

		Assert.IsEmpty(set);
		Assert.IsLessThan(LargeSetBudgetMilliseconds, stopwatch.ElapsedMilliseconds);
	}

	[TestMethod]
	public void IntersectWith_KeepsSurvivorsInInsertionOrderAndUpdatesLookups()
	{
		ContiguousSet<int> set = [5, 1, 4, 2, 3];

		set.IntersectWith([3, 4, 5, 9]);

		Assert.AreSequenceEqual([5, 4, 3], set);
		Assert.IsFalse(set.Contains(1));
		Assert.IsTrue(set.Add(1));
		Assert.AreSequenceEqual([5, 4, 3, 1], set);
	}

	[TestMethod]
	public void SymmetricExceptWith_KeepsSurvivorsInOrderThenAppendsOtherOnlyItems()
	{
		ContiguousSet<int> set = [5, 1, 4, 2, 3];

		set.SymmetricExceptWith([4, 9, 1]);

		Assert.AreSequenceEqual([5, 2, 3, 9], set);
		Assert.IsFalse(set.Contains(4));
		Assert.IsTrue(set.Contains(9));
	}

	[TestMethod]
	public void IntersectWith_RemovingElements_InvalidatesEnumerators()
	{
		ContiguousSet<int> set = [1, 2, 3];
		using IEnumerator<int> enumerator = set.GetEnumerator();
		Assert.IsTrue(enumerator.MoveNext());

		set.IntersectWith([2]);

		Assert.ThrowsExactly<InvalidOperationException>(() => enumerator.MoveNext());
	}

	[TestMethod]
	public void AsSpan_IsReadOnly_SoElementsCannotBypassTheUniquenessIndex()
	{
		// A writable span would let a caller turn {1, 2, 3} into {2, 2, 3} while Contains(1) stayed true.
		Type returnType = typeof(ContiguousSet<int>).GetMethod(nameof(ContiguousSet<>.AsSpan))!.ReturnType;

		Assert.AreEqual(typeof(ReadOnlySpan<int>), returnType);
	}

	[TestMethod]
	public void AsSpan_ReturnsElementsInInsertionOrder()
	{
		ContiguousSet<int> set = [3, 1, 2];

		ReadOnlySpan<int> span = set.AsSpan();

		Assert.AreSequenceEqual([3, 1, 2], span.ToArray());
	}
}
