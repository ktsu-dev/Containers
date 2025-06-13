// Copyright (c) ktsu.dev
// All rights reserved.
// Licensed under the MIT license.

namespace ktsu.Containers.Test;

using System.Collections;
using Microsoft.VisualStudio.TestTools.UnitTesting;

[TestClass]
public class OrderedSetTests
{
	[TestMethod]
	public void Constructor_DefaultComparer_CreatesEmptySet()
	{
		OrderedSet<int> set = [];
		Assert.AreEqual(0, set.Count);
		Assert.IsFalse(set.IsReadOnly);
	}

	[TestMethod]
	public void Constructor_WithComparer_CreatesEmptySetWithCustomComparer()
	{
		OrderedSet<string> set = new(StringComparer.OrdinalIgnoreCase);
		Assert.AreEqual(0, set.Count);
		Assert.IsFalse(set.IsReadOnly);
	}

	[TestMethod]
	public void Constructor_WithCapacity_CreatesEmptySetWithCapacity()
	{
		OrderedSet<int> set = new(10);
		Assert.AreEqual(0, set.Count);
		Assert.IsFalse(set.IsReadOnly);
	}

	[TestMethod]
	public void Constructor_WithCapacityAndComparer_CreatesEmptySetWithBoth()
	{
		OrderedSet<string> set = new(10, StringComparer.OrdinalIgnoreCase);
		Assert.AreEqual(0, set.Count);
		Assert.IsFalse(set.IsReadOnly);
	}

	[TestMethod]
	public void Constructor_WithCollection_CreatesSetFromCollection()
	{
		int[] items = [3, 1, 4, 1, 5, 9, 2, 6];
		OrderedSet<int> set = [.. items];

		Assert.AreEqual(7, set.Count); // Duplicates removed
		int[] expected = [1, 2, 3, 4, 5, 6, 9];
		CollectionAssert.AreEqual(expected, set.ToArray());
	}

	[TestMethod]
	public void Constructor_WithCollectionAndComparer_CreatesSetFromCollectionWithComparer()
	{
		string[] items = ["apple", "BANANA", "cherry", "Apple"];
		OrderedSet<string> set = new(items, StringComparer.OrdinalIgnoreCase);

		Assert.AreEqual(3, set.Count); // "apple" and "Apple" are treated as same
		string[] expected = ["apple", "BANANA", "cherry"];
		CollectionAssert.AreEqual(expected, set.ToArray());
	}

	[TestMethod]
	public void Constructor_NonComparableTypeWithoutComparer_ThrowsArgumentException()
	{
		Assert.ThrowsExactly<ArgumentException>(() => _ = new OrderedSet<object>());
	}

	[TestMethod]
	public void Constructor_NullComparer_ThrowsArgumentNullException()
	{
		Assert.ThrowsExactly<ArgumentNullException>(() => _ = new OrderedSet<int>((IComparer<int>)null!));
	}

	[TestMethod]
	public void Constructor_NegativeCapacity_ThrowsArgumentOutOfRangeException()
	{
		Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => _ = new OrderedSet<int>(-1));
	}

	[TestMethod]
	public void Constructor_NullCollection_ThrowsArgumentNullException()
	{
		Assert.ThrowsExactly<ArgumentNullException>(() => _ = new OrderedSet<int>((IEnumerable<int>)null!));
	}

	[TestMethod]
	public void Add_SingleElement_AddsAndMaintainsOrder()
	{
		OrderedSet<int> set = [];
		bool added = set.Add(5);

		Assert.IsTrue(added);
		Assert.AreEqual(1, set.Count);
		Assert.IsTrue(set.Contains(5));
	}

	[TestMethod]
	public void Add_MultipleElements_MaintainsSortedOrder()
	{
		OrderedSet<int> set = [];
		bool[] results = [set.Add(5), set.Add(2), set.Add(8), set.Add(1)];

		Assert.IsTrue(results.All(r => r));
		Assert.AreEqual(4, set.Count);

		int[] expected = [1, 2, 5, 8];
		CollectionAssert.AreEqual(expected, set.ToArray());
	}

	[TestMethod]
	public void Add_DuplicateElement_ReturnsFalseAndDoesNotAdd()
	{
		OrderedSet<int> set = [5];
		bool addedDuplicate = set.Add(5);

		Assert.IsFalse(addedDuplicate);
		Assert.AreEqual(1, set.Count);
	}

	[TestMethod]
	public void Add_WithCustomComparer_UsesCaseInsensitiveComparison()
	{
		IComparer<string> comparer = StringComparer.OrdinalIgnoreCase;
		OrderedSet<string> set = new(comparer)
		{
			"Apple"
		};
		bool addedDuplicate = set.Add("apple");

		Assert.IsFalse(addedDuplicate);
		Assert.AreEqual(1, set.Count);
		Assert.IsTrue(set.Contains("Apple"));
		Assert.IsTrue(set.Contains("apple"));
	}

	[TestMethod]
	public void Clear_RemovesAllElements()
	{
		OrderedSet<int> set = new([1, 2, 3, 4, 5]);
		set.Clear();

		Assert.AreEqual(0, set.Count);
		Assert.IsFalse(set.Contains(3));
	}

	[TestMethod]
	public void Contains_ExistingElement_ReturnsTrue()
	{
		OrderedSet<int> set = new([1, 2, 3, 4, 5]);
		Assert.IsTrue(set.Contains(3));
	}

	[TestMethod]
	public void Contains_NonExistingElement_ReturnsFalse()
	{
		OrderedSet<int> set = new([1, 2, 3, 4, 5]);
		Assert.IsFalse(set.Contains(6));
	}

	[TestMethod]
	public void Contains_EmptySet_ReturnsFalse()
	{
		OrderedSet<int> set = [];
		Assert.IsFalse(set.Contains(1));
	}

	[TestMethod]
	public void CopyTo_ValidParameters_CopiesElements()
	{
		OrderedSet<int> set = new([3, 1, 4, 1, 5]);
		int[] array = new int[5];
		set.CopyTo(array, 0);

		int[] expected = [1, 3, 4, 5, 0]; // Note: only 4 unique elements
		CollectionAssert.AreEqual(expected, array[0..4]);
	}

	[TestMethod]
	public void CopyTo_NullArray_ThrowsArgumentNullException()
	{
		OrderedSet<int> set = new([1, 2, 3]);
		Assert.ThrowsExactly<ArgumentNullException>(() => set.CopyTo(null!, 0));
	}

	[TestMethod]
	public void CopyTo_NegativeArrayIndex_ThrowsArgumentOutOfRangeException()
	{
		OrderedSet<int> set = new([1, 2, 3]);
		int[] array = new int[5];
		Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => set.CopyTo(array, -1));
	}

	[TestMethod]
	public void CopyTo_InsufficientSpace_ThrowsArgumentException()
	{
		OrderedSet<int> set = new([1, 2, 3]);
		int[] array = new int[2];
		Assert.ThrowsExactly<ArgumentException>(() => set.CopyTo(array, 0));
	}

	[TestMethod]
	public void Remove_ExistingElement_RemovesAndReturnsTrue()
	{
		OrderedSet<int> set = new([1, 2, 3, 4, 5]);
		bool removed = set.Remove(3);

		Assert.IsTrue(removed);
		Assert.AreEqual(4, set.Count);
		Assert.IsFalse(set.Contains(3));

		int[] expected = [1, 2, 4, 5];
		CollectionAssert.AreEqual(expected, set.ToArray());
	}

	[TestMethod]
	public void Remove_NonExistingElement_ReturnsFalse()
	{
		OrderedSet<int> set = new([1, 2, 3, 4, 5]);
		bool removed = set.Remove(6);

		Assert.IsFalse(removed);
		Assert.AreEqual(5, set.Count);
	}

	[TestMethod]
	public void Remove_FromEmptySet_ReturnsFalse()
	{
		OrderedSet<int> set = [];
		bool removed = set.Remove(1);

		Assert.IsFalse(removed);
		Assert.AreEqual(0, set.Count);
	}

	[TestMethod]
	public void BinarySearch_ExistingElement_ReturnsCorrectIndex()
	{
		OrderedSet<int> set = new([1, 3, 5, 7, 9]);
		int index = set.BinarySearch(5);

		Assert.AreEqual(2, index);
	}

	[TestMethod]
	public void BinarySearch_NonExistingElement_ReturnsNegativeIndex()
	{
		OrderedSet<int> set = new([1, 3, 5, 7, 9]);
		int index = set.BinarySearch(4);

		Assert.IsTrue(index < 0);
		int insertionPoint = ~index;
		Assert.AreEqual(2, insertionPoint);
	}

	[TestMethod]
	public void BinarySearch_EmptySet_ReturnsMinusOne()
	{
		OrderedSet<int> set = [];
		int index = set.BinarySearch(5);

		Assert.AreEqual(-1, index);
	}

	[TestMethod]
	public void GetEnumerator_Generic_IteratesInSortedOrder()
	{
		OrderedSet<int> set = new([5, 2, 8, 1, 9]);
		List<int> result = [];

		foreach (int item in set)
		{
			result.Add(item);
		}

		int[] expected = [1, 2, 5, 8, 9];
		CollectionAssert.AreEqual(expected, result);
	}

	[TestMethod]
	public void GetEnumerator_NonGeneric_IteratesInSortedOrder()
	{
		OrderedSet<int> set = new([5, 2, 8, 1, 9]);
		List<object> result = [];

		IEnumerable enumerable = set;
		foreach (object item in enumerable)
		{
			result.Add(item);
		}

		object[] expected = [1, 2, 5, 8, 9];
		CollectionAssert.AreEqual(expected, result);
	}

	[TestMethod]
	public void UnionWith_DisjointSets_AddsAllElements()
	{
		OrderedSet<int> set1 = new([1, 3, 5]);
		int[] set2 = [2, 4, 6];

		set1.UnionWith(set2);

		Assert.AreEqual(6, set1.Count);
		int[] expected = [1, 2, 3, 4, 5, 6];
		CollectionAssert.AreEqual(expected, set1.ToArray());
	}

	[TestMethod]
	public void UnionWith_OverlappingSets_AddsUniqueElements()
	{
		OrderedSet<int> set1 = new([1, 3, 5]);
		int[] set2 = [3, 4, 5, 6];

		set1.UnionWith(set2);

		Assert.AreEqual(5, set1.Count);
		int[] expected = [1, 3, 4, 5, 6];
		CollectionAssert.AreEqual(expected, set1.ToArray());
	}

	[TestMethod]
	public void UnionWith_NullCollection_ThrowsArgumentNullException()
	{
		OrderedSet<int> set = new([1, 2, 3]);
		Assert.ThrowsExactly<ArgumentNullException>(() => set.UnionWith(null!));
	}

	[TestMethod]
	public void IntersectWith_OverlappingSets_KeepsCommonElements()
	{
		OrderedSet<int> set1 = new([1, 2, 3, 4, 5]);
		int[] set2 = [3, 4, 5, 6, 7];

		set1.IntersectWith(set2);

		Assert.AreEqual(3, set1.Count);
		int[] expected = [3, 4, 5];
		CollectionAssert.AreEqual(expected, set1.ToArray());
	}

	[TestMethod]
	public void IntersectWith_DisjointSets_ResultsInEmptySet()
	{
		OrderedSet<int> set1 = new([1, 2, 3]);
		int[] set2 = [4, 5, 6];

		set1.IntersectWith(set2);

		Assert.AreEqual(0, set1.Count);
	}

	[TestMethod]
	public void IntersectWith_NullCollection_ThrowsArgumentNullException()
	{
		OrderedSet<int> set = new([1, 2, 3]);
		Assert.ThrowsExactly<ArgumentNullException>(() => set.IntersectWith(null!));
	}

	[TestMethod]
	public void ExceptWith_RemovesSpecifiedElements()
	{
		OrderedSet<int> set1 = new([1, 2, 3, 4, 5]);
		int[] set2 = [2, 4, 6];

		set1.ExceptWith(set2);

		Assert.AreEqual(3, set1.Count);
		int[] expected = [1, 3, 5];
		CollectionAssert.AreEqual(expected, set1.ToArray());
	}

	[TestMethod]
	public void ExceptWith_NullCollection_ThrowsArgumentNullException()
	{
		OrderedSet<int> set = new([1, 2, 3]);
		Assert.ThrowsExactly<ArgumentNullException>(() => set.ExceptWith(null!));
	}

	[TestMethod]
	public void SymmetricExceptWith_KeepsElementsInEitherButNotBoth()
	{
		OrderedSet<int> set1 = new([1, 2, 3, 4]);
		int[] set2 = [3, 4, 5, 6];

		set1.SymmetricExceptWith(set2);

		Assert.AreEqual(4, set1.Count);
		int[] expected = [1, 2, 5, 6];
		CollectionAssert.AreEqual(expected, set1.ToArray());
	}

	[TestMethod]
	public void SymmetricExceptWith_NullCollection_ThrowsArgumentNullException()
	{
		OrderedSet<int> set = new([1, 2, 3]);
		Assert.ThrowsExactly<ArgumentNullException>(() => set.SymmetricExceptWith(null!));
	}

	[TestMethod]
	public void IsSubsetOf_ValidSubset_ReturnsTrue()
	{
		OrderedSet<int> set1 = new([1, 2, 3]);
		int[] set2 = [1, 2, 3, 4, 5];

		Assert.IsTrue(set1.IsSubsetOf(set2));
	}

	[TestMethod]
	public void IsSubsetOf_NotSubset_ReturnsFalse()
	{
		OrderedSet<int> set1 = new([1, 2, 6]);
		int[] set2 = [1, 2, 3, 4, 5];

		Assert.IsFalse(set1.IsSubsetOf(set2));
	}

	[TestMethod]
	public void IsSubsetOf_EqualSets_ReturnsTrue()
	{
		OrderedSet<int> set1 = new([1, 2, 3]);
		int[] set2 = [1, 2, 3];

		Assert.IsTrue(set1.IsSubsetOf(set2));
	}

	[TestMethod]
	public void IsSubsetOf_NullCollection_ThrowsArgumentNullException()
	{
		OrderedSet<int> set = new([1, 2, 3]);
		Assert.ThrowsExactly<ArgumentNullException>(() => set.IsSubsetOf(null!));
	}

	[TestMethod]
	public void IsSupersetOf_ValidSuperset_ReturnsTrue()
	{
		OrderedSet<int> set1 = new([1, 2, 3, 4, 5]);
		int[] set2 = [1, 2, 3];

		Assert.IsTrue(set1.IsSupersetOf(set2));
	}

	[TestMethod]
	public void IsSupersetOf_NotSuperset_ReturnsFalse()
	{
		OrderedSet<int> set1 = new([1, 2, 3]);
		int[] set2 = [1, 2, 3, 4, 5];

		Assert.IsFalse(set1.IsSupersetOf(set2));
	}

	[TestMethod]
	public void IsSupersetOf_NullCollection_ThrowsArgumentNullException()
	{
		OrderedSet<int> set = new([1, 2, 3]);
		Assert.ThrowsExactly<ArgumentNullException>(() => set.IsSupersetOf(null!));
	}

	[TestMethod]
	public void IsProperSubsetOf_ValidProperSubset_ReturnsTrue()
	{
		OrderedSet<int> set1 = new([1, 2, 3]);
		int[] set2 = [1, 2, 3, 4, 5];

		Assert.IsTrue(set1.IsProperSubsetOf(set2));
	}

	[TestMethod]
	public void IsProperSubsetOf_EqualSets_ReturnsFalse()
	{
		OrderedSet<int> set1 = new([1, 2, 3]);
		int[] set2 = [1, 2, 3];

		Assert.IsFalse(set1.IsProperSubsetOf(set2));
	}

	[TestMethod]
	public void IsProperSubsetOf_NullCollection_ThrowsArgumentNullException()
	{
		OrderedSet<int> set = new([1, 2, 3]);
		Assert.ThrowsExactly<ArgumentNullException>(() => set.IsProperSubsetOf(null!));
	}

	[TestMethod]
	public void IsProperSupersetOf_ValidProperSuperset_ReturnsTrue()
	{
		OrderedSet<int> set1 = new([1, 2, 3, 4, 5]);
		int[] set2 = [1, 2, 3];

		Assert.IsTrue(set1.IsProperSupersetOf(set2));
	}

	[TestMethod]
	public void IsProperSupersetOf_EqualSets_ReturnsFalse()
	{
		OrderedSet<int> set1 = new([1, 2, 3]);
		int[] set2 = [1, 2, 3];

		Assert.IsFalse(set1.IsProperSupersetOf(set2));
	}

	[TestMethod]
	public void IsProperSupersetOf_NullCollection_ThrowsArgumentNullException()
	{
		OrderedSet<int> set = new([1, 2, 3]);
		Assert.ThrowsExactly<ArgumentNullException>(() => set.IsProperSupersetOf(null!));
	}

	[TestMethod]
	public void Overlaps_WithCommonElements_ReturnsTrue()
	{
		OrderedSet<int> set1 = new([1, 2, 3]);
		int[] set2 = [3, 4, 5];

		Assert.IsTrue(set1.Overlaps(set2));
	}

	[TestMethod]
	public void Overlaps_WithoutCommonElements_ReturnsFalse()
	{
		OrderedSet<int> set1 = new([1, 2, 3]);
		int[] set2 = [4, 5, 6];

		Assert.IsFalse(set1.Overlaps(set2));
	}

	[TestMethod]
	public void Overlaps_NullCollection_ThrowsArgumentNullException()
	{
		OrderedSet<int> set = new([1, 2, 3]);
		Assert.ThrowsExactly<ArgumentNullException>(() => set.Overlaps(null!));
	}

	[TestMethod]
	public void SetEquals_EqualSets_ReturnsTrue()
	{
		OrderedSet<int> set1 = new([1, 2, 3]);
		int[] set2 = [3, 1, 2]; // Different order, same elements

		Assert.IsTrue(set1.SetEquals(set2));
	}

	[TestMethod]
	public void SetEquals_DifferentSets_ReturnsFalse()
	{
		OrderedSet<int> set1 = new([1, 2, 3]);
		int[] set2 = [1, 2, 4];

		Assert.IsFalse(set1.SetEquals(set2));
	}

	[TestMethod]
	public void SetEquals_DifferentSizes_ReturnsFalse()
	{
		OrderedSet<int> set1 = new([1, 2, 3]);
		int[] set2 = [1, 2, 3, 4];

		Assert.IsFalse(set1.SetEquals(set2));
	}

	[TestMethod]
	public void SetEquals_NullCollection_ThrowsArgumentNullException()
	{
		OrderedSet<int> set = new([1, 2, 3]);
		Assert.ThrowsExactly<ArgumentNullException>(() => set.SetEquals(null!));
	}

	[TestMethod]
	public void Clone_CreatesIndependentCopy()
	{
		OrderedSet<int> original = new([1, 2, 3]);
		OrderedSet<int> clone = original.Clone();

		// Verify they have the same content
		CollectionAssert.AreEqual(original.ToArray(), clone.ToArray());

		// Verify they are independent
		clone.Add(4);
		Assert.AreNotEqual(original.Count, clone.Count);
		Assert.IsFalse(original.Contains(4));
		Assert.IsTrue(clone.Contains(4));
	}

	[TestMethod]
	public void ICollectionAdd_CallsMainAddMethod()
	{
		OrderedSet<int> orderedSet = [];
		ICollection<int> set = orderedSet;
		set.Add(5);

		Assert.AreEqual(1, set.Count);
		Assert.IsTrue(set.Contains(5));
	}

	[TestMethod]
	public void LargeDataSet_MaintainsPerformanceAndOrder()
	{
		OrderedSet<int> set = [];
		Random random = new(42); // Fixed seed for reproducible tests

		// Add 1000 random numbers
		List<int> numbersToAdd = [];
		for (int i = 0; i < 1000; i++)
		{
			numbersToAdd.Add(random.Next(0, 500)); // Some duplicates expected
		}

		foreach (int number in numbersToAdd)
		{
			set.Add(number);
		}

		// Verify sorted order
		int[] result = [.. set];
		for (int i = 1; i < result.Length; i++)
		{
			Assert.IsTrue(result[i - 1] < result[i], "Elements should be in sorted order");
		}

		// Verify uniqueness
		Assert.AreEqual(result.Distinct().Count(), result.Length, "All elements should be unique");
	}

	[TestMethod]
	public void StressTest_MultipleOperations()
	{
		OrderedSet<int> set = [];

		// Add elements
		for (int i = 0; i < 100; i++)
		{
			set.Add(i);
		}

		// Remove some elements
		for (int i = 0; i < 50; i += 2)
		{
			set.Remove(i);
		}

		// Verify correct count
		Assert.AreEqual(50, set.Count);

		// Verify all remaining elements are odd
		foreach (int item in set)
		{
			Assert.AreEqual(1, item % 2, "All remaining elements should be odd");
		}

		// Verify sorted order
		int[] result = [.. set];
		for (int i = 1; i < result.Length; i++)
		{
			Assert.IsTrue(result[i - 1] < result[i], "Elements should remain sorted");
		}
	}
}
