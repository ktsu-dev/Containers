// Copyright (c) 2023-2026 ktsu-dev contributors

namespace ktsu.Containers.Test;

using System.Collections;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;

[TestClass]
public class OrderedSetTests
{
	[TestMethod]
	public void Constructor_DefaultComparer_CreatesEmptySet()
	{
		OrderedSet<int> set = [];
		Assert.AreEqual(0, set.Count);
		Assert.IsFalse(set.IsReadOnly, "OrderedSet should not be read-only");
	}

	[TestMethod]
	public void Constructor_WithComparer_CreatesEmptySetWithCustomComparer()
	{
		OrderedSet<string> set = new(StringComparer.OrdinalIgnoreCase);
		Assert.AreEqual(0, set.Count);
		Assert.IsFalse(set.IsReadOnly, "OrderedSet should not be read-only");
	}

	[TestMethod]
	public void Constructor_WithCapacity_CreatesEmptySetWithCapacity()
	{
		OrderedSet<int> set = new(10);
		Assert.AreEqual(0, set.Count);
		Assert.IsFalse(set.IsReadOnly, "OrderedSet should not be read-only");
	}

	[TestMethod]
	public void Constructor_WithCapacityAndComparer_CreatesEmptySetWithBoth()
	{
		OrderedSet<string> set = new(10, StringComparer.OrdinalIgnoreCase);
		Assert.AreEqual(0, set.Count);
		Assert.IsFalse(set.IsReadOnly, "OrderedSet should not be read-only");
	}

	[TestMethod]
	public void Constructor_WithCollection_CreatesSetFromCollection()
	{
		int[] items = [3, 1, 4, 1, 5, 9, 2, 6];
		OrderedSet<int> set = [.. items];

		Assert.AreEqual(7, set.Count); // Duplicates removed
		int[] expected = [1, 2, 3, 4, 5, 6, 9];
		Assert.AreSequenceEqual(expected, set);
	}

	[TestMethod]
	public void Constructor_WithCollectionAndComparer_CreatesSetFromCollectionWithComparer()
	{
		string[] items = ["apple", "BANANA", "cherry", "Apple"];
		OrderedSet<string> set = new(items, StringComparer.OrdinalIgnoreCase);

		Assert.AreEqual(3, set.Count); // "apple" and "Apple" are treated as same
		string[] expected = ["apple", "BANANA", "cherry"];
		Assert.AreSequenceEqual(expected, set);
	}

	[TestMethod]
	public void Constructor_NullableTypeWithoutComparer_SortsNullFirst()
	{
		OrderedSet<int?> set = [3, null, 1, null];
		OrderedSet<int?> withCapacity = new(10) { 2, null };
		OrderedSet<int?> fromCollection = new([2, null, 2]);

		Assert.AreSequenceEqual([null, 1, 3], set);
		Assert.AreSequenceEqual([null, 2], withCapacity);
		Assert.AreSequenceEqual([null, 2], fromCollection);
	}

	[TestMethod]
	public void Constructor_NullableOfNonComparableTypeWithoutComparer_ThrowsArgumentException() =>
		Assert.ThrowsExactly<ArgumentException>(() => _ = new OrderedSet<KeyValuePair<int, int>?>());

	[TestMethod]
	public void Constructor_NonComparableTypeWithoutComparer_ThrowsArgumentException()
	{
		Assert.ThrowsExactly<ArgumentException>(() => _ = new OrderedSet<object>());
	}

	[TestMethod]
	public void Constructor_NullComparer_ThrowsArgumentNullException()
	{
		Assert.ThrowsExactly<ArgumentNullException>(() =>
			_ = new OrderedSet<int>((IComparer<int>)null!)
		);
	}

	[TestMethod]
	public void Constructor_NegativeCapacity_ThrowsArgumentOutOfRangeException()
	{
		Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => _ = new OrderedSet<int>(-1));
	}

	[TestMethod]
	public void Constructor_NullCollection_ThrowsArgumentNullException()
	{
		Assert.ThrowsExactly<ArgumentNullException>(() =>
			_ = new OrderedSet<int>((IEnumerable<int>)null!)
		);
	}

	[TestMethod]
	public void Add_SingleElement_AddsAndMaintainsOrder()
	{
		OrderedSet<int> set = [];
		bool added = set.Add(5);

		Assert.IsTrue(added, "Add should return true for new element");
		Assert.AreEqual(1, set.Count);
		Assert.Contains(5, set);
	}

	[TestMethod]
	public void Add_MultipleElements_MaintainsSortedOrder()
	{
		OrderedSet<int> set = [];
		bool[] results = [set.Add(5), set.Add(2), set.Add(8), set.Add(1)];

		Assert.IsTrue(results.All(r => r), "All Add operations should return true");
		Assert.AreEqual(4, set.Count);

		int[] expected = [1, 2, 5, 8];
		Assert.AreSequenceEqual(expected, set);
	}

	[TestMethod]
	public void Add_DuplicateElement_ReturnsFalseAndDoesNotAdd()
	{
		OrderedSet<int> set = [5];
		bool addedDuplicate = set.Add(5);

		Assert.IsFalse(addedDuplicate, "Add should return false for duplicate element");
		Assert.AreEqual(1, set.Count);
	}

	[TestMethod]
	public void Add_WithCustomComparer_UsesCaseInsensitiveComparison()
	{
		IComparer<string> comparer = StringComparer.OrdinalIgnoreCase;
		OrderedSet<string> set = new(comparer) { "Apple" };
		bool addedDuplicate = set.Add("apple");

		Assert.IsFalse(addedDuplicate, "Add should return false for case-insensitive duplicate");
		Assert.AreEqual(1, set.Count);
		Assert.Contains("Apple", set);
		Assert.Contains("apple", set);
	}

	[TestMethod]
	public void Clear_RemovesAllElements()
	{
		OrderedSet<int> set = new([1, 2, 3, 4, 5]);
		set.Clear();

		Assert.AreEqual(0, set.Count);
		Assert.DoesNotContain(3, set);
	}

	[TestMethod]
	public void Contains_ExistingElement_ReturnsTrue()
	{
		OrderedSet<int> set = new([1, 2, 3, 4, 5]);
		Assert.Contains(3, set);
	}

	[TestMethod]
	public void Contains_NonExistingElement_ReturnsFalse()
	{
		OrderedSet<int> set = new([1, 2, 3, 4, 5]);
		Assert.DoesNotContain(6, set);
	}

	[TestMethod]
	public void Contains_EmptySet_ReturnsFalse()
	{
		OrderedSet<int> set = [];
		Assert.DoesNotContain(1, set);
	}

	[TestMethod]
	public void CopyTo_ValidParameters_CopiesElements()
	{
		OrderedSet<int> set = new([3, 1, 4, 1, 5]);
		int[] array = new int[5];
		set.CopyTo(array, 0);

		int[] expected = [1, 3, 4, 5]; // Note: only 4 unique elements
		Assert.AreSequenceEqual(expected, array.Take(4));
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

		Assert.IsTrue(removed, "Remove should return true for existing element");
		Assert.AreEqual(4, set.Count);
		Assert.DoesNotContain(3, set);

		int[] expected = [1, 2, 4, 5];
		Assert.AreSequenceEqual(expected, set);
	}

	[TestMethod]
	public void Remove_NonExistingElement_ReturnsFalse()
	{
		OrderedSet<int> set = new([1, 2, 3, 4, 5]);
		bool removed = set.Remove(6);

		Assert.IsFalse(removed, "Remove should return false for non-existing element");
		Assert.AreEqual(5, set.Count);
	}

	[TestMethod]
	public void Remove_FromEmptySet_ReturnsFalse()
	{
		OrderedSet<int> set = [];
		bool removed = set.Remove(1);

		Assert.IsFalse(removed, "Remove should return false for empty set");
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

		Assert.IsLessThan(0, index);
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
		Assert.AreSequenceEqual(expected, result);
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
		Assert.AreSequenceEqual(expected, result);
	}

	[TestMethod]
	public void UnionWith_DisjointSets_AddsAllElements()
	{
		OrderedSet<int> set1 = new([1, 3, 5]);
		int[] set2 = [2, 4, 6];

		set1.UnionWith(set2);

		Assert.AreEqual(6, set1.Count);
		int[] expected = [1, 2, 3, 4, 5, 6];
		Assert.AreSequenceEqual(expected, set1);
	}

	[TestMethod]
	public void UnionWith_OverlappingSets_AddsUniqueElements()
	{
		OrderedSet<int> set1 = new([1, 3, 5]);
		int[] set2 = [3, 4, 5, 6];

		set1.UnionWith(set2);

		Assert.AreEqual(5, set1.Count);
		int[] expected = [1, 3, 4, 5, 6];
		Assert.AreSequenceEqual(expected, set1);
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
		Assert.AreSequenceEqual(expected, set1);
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
		Assert.AreSequenceEqual(expected, set1);
	}

	[TestMethod]
	public void ExceptWith_Self_EmptiesSet()
	{
		OrderedSet<int> set = [.. new int[] { 1, 2, 3, 4 }];

		set.ExceptWith(set);

		Assert.IsEmpty(set);
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
		Assert.AreSequenceEqual(expected, set1);
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

		Assert.IsTrue(set1.IsSubsetOf(set2), "Set should be a subset");
	}

	[TestMethod]
	public void IsSubsetOf_NotSubset_ReturnsFalse()
	{
		OrderedSet<int> set1 = new([1, 2, 6]);
		int[] set2 = [1, 2, 3, 4, 5];

		Assert.IsFalse(set1.IsSubsetOf(set2), "Set should not be a subset");
	}

	[TestMethod]
	public void IsSubsetOf_EqualSets_ReturnsTrue()
	{
		OrderedSet<int> set1 = new([1, 2, 3]);
		int[] set2 = [1, 2, 3];

		Assert.IsTrue(set1.IsSubsetOf(set2), "Equal sets should be subsets of each other");
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

		Assert.IsTrue(set1.IsSupersetOf(set2), "Set should be a superset");
	}

	[TestMethod]
	public void IsSupersetOf_NotSuperset_ReturnsFalse()
	{
		OrderedSet<int> set1 = new([1, 2, 3]);
		int[] set2 = [1, 2, 3, 4, 5];

		Assert.IsFalse(set1.IsSupersetOf(set2), "Set should not be a superset");
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

		Assert.IsTrue(set1.IsProperSubsetOf(set2), "Set should be a proper subset");
	}

	[TestMethod]
	public void IsProperSubsetOf_EqualSets_ReturnsFalse()
	{
		OrderedSet<int> set1 = new([1, 2, 3]);
		int[] set2 = [1, 2, 3];

		Assert.IsFalse(set1.IsProperSubsetOf(set2), "Equal sets are not proper subsets");
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

		Assert.IsTrue(set1.IsProperSupersetOf(set2), "Set should be a proper superset");
	}

	[TestMethod]
	public void IsProperSupersetOf_EqualSets_ReturnsFalse()
	{
		OrderedSet<int> set1 = new([1, 2, 3]);
		int[] set2 = [1, 2, 3];

		Assert.IsFalse(set1.IsProperSupersetOf(set2), "Equal sets are not proper supersets");
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

		Assert.IsTrue(set1.Overlaps(set2), "Sets with common elements should overlap");
	}

	[TestMethod]
	public void Overlaps_WithoutCommonElements_ReturnsFalse()
	{
		OrderedSet<int> set1 = new([1, 2, 3]);
		int[] set2 = [4, 5, 6];

		Assert.IsFalse(set1.Overlaps(set2), "Sets without common elements should not overlap");
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

		Assert.IsTrue(set1.SetEquals(set2), "Sets with same elements should be equal");
	}

	[TestMethod]
	public void SetEquals_DifferentSets_ReturnsFalse()
	{
		OrderedSet<int> set1 = new([1, 2, 3]);
		int[] set2 = [1, 2, 4];

		Assert.IsFalse(set1.SetEquals(set2), "Sets with different elements should not be equal");
	}

	[TestMethod]
	public void SetEquals_DifferentSizes_ReturnsFalse()
	{
		OrderedSet<int> set1 = new([1, 2, 3]);
		int[] set2 = [1, 2, 3, 4];

		Assert.IsFalse(set1.SetEquals(set2), "Sets with different sizes should not be equal");
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
		Assert.AreSequenceEqual(original, clone);

		// Verify they are independent
		clone.Add(4);
		Assert.AreNotEqual(original.Count, clone.Count);
		Assert.DoesNotContain(4, original);
		Assert.Contains(4, clone);
	}

	[TestMethod]
	public void ICollectionAdd_CallsMainAddMethod()
	{
		OrderedSet<int> orderedSet = [];
		ICollection<int> set = orderedSet;
		set.Add(5);

		Assert.HasCount(1, set);
		Assert.Contains(5, set);
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
			Assert.IsLessThan(result[i], result[i - 1], "Elements should be in sorted order");
		}

		// Verify uniqueness
		Assert.HasCount(result.Length, result.Distinct(), "All elements should be unique");
	}

	/// <summary>
	/// Builds a set whose notion of equality is case-insensitive, so that the set's own comparer
	/// and <see cref="EqualityComparer{T}.Default"/> disagree about which strings are the same.
	/// </summary>
	private static OrderedSet<string> CaseInsensitiveSet(params string[] items) =>
		new(items, StringComparer.OrdinalIgnoreCase);

	[TestMethod]
	public void IntersectWith_CustomComparer_KeepsElementsThatMatchUnderThatComparer()
	{
		OrderedSet<string> set = CaseInsensitiveSet("Hello", "World");

		set.IntersectWith(["HELLO"]);

		Assert.HasCount(1, set, "Only the element matching under the set's comparer should remain");
		Assert.IsTrue(set.Contains("Hello"), "\"Hello\" matches \"HELLO\" under the set's comparer and must survive");
	}

	[TestMethod]
	public void IsSubsetOf_CustomComparer_MatchesUnderThatComparer()
	{
		OrderedSet<string> set = CaseInsensitiveSet("Hello");

		Assert.IsTrue(set.IsSubsetOf(["HELLO", "World"]), "\"Hello\" is contained in the collection under the set's comparer");
	}

	[TestMethod]
	public void IsProperSubsetOf_CustomComparer_MatchesUnderThatComparer()
	{
		OrderedSet<string> set = CaseInsensitiveSet("Hello");

		Assert.IsTrue(set.IsProperSubsetOf(["HELLO", "World"]), "The set is a strictly smaller subset under its own comparer");
	}

	[TestMethod]
	public void IsProperSubsetOf_CustomComparer_CountsDistinctElementsUnderThatComparer()
	{
		OrderedSet<string> set = CaseInsensitiveSet("Hello");

		Assert.IsFalse(
			set.IsProperSubsetOf(["HELLO", "hello"]),
			"The collection holds one distinct element under the set's comparer, so the set is not strictly smaller"
		);
	}

	[TestMethod]
	public void IsProperSupersetOf_CustomComparer_CountsDistinctElementsUnderThatComparer()
	{
		OrderedSet<string> set = CaseInsensitiveSet("Hello", "World");

		Assert.IsTrue(
			set.IsProperSupersetOf(["HELLO", "hello"]),
			"The collection holds one distinct element under the set's comparer, so the set is strictly larger"
		);
	}

	[TestMethod]
	public void SetEquals_CustomComparer_CountsDistinctElementsUnderThatComparer()
	{
		OrderedSet<string> set = CaseInsensitiveSet("Hello", "World");

		Assert.IsTrue(
			set.SetEquals(["HELLO", "hello", "WORLD"]),
			"The collection holds the same two distinct elements under the set's comparer"
		);
	}

	[TestMethod]
	public void SymmetricExceptWith_CustomComparer_TreatsMatchesUnderThatComparerAsCommon()
	{
		OrderedSet<string> set = CaseInsensitiveSet("Hello", "World");

		set.SymmetricExceptWith(["HELLO", "Foo"]);

		Assert.HasCount(2, set, "The common element should be dropped and the unmatched one added");
		Assert.IsFalse(set.Contains("Hello"), "\"Hello\" is common to both under the set's comparer and must be dropped");
		Assert.IsTrue(set.Contains("World"), "\"World\" is only in the set and must remain");
		Assert.IsTrue(set.Contains("Foo"), "\"Foo\" is only in the other collection and must be added");
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

		// Remove even elements (0, 2, 4, ..., 98)
		for (int i = 0; i < 100; i += 2)
		{
			set.Remove(i);
		}

		// Verify correct count (50 odd numbers: 1, 3, 5, ..., 99)
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
			Assert.IsLessThan(result[i], result[i - 1], "Elements should remain sorted");
		}
	}

	// Filling the backing list one binary-search insert at a time shifts the tail on every insert, so
	// building from 1,000,000 descending items takes over a minute; a stable sort takes well under a second.
	private const int BulkSize = 1_000_000;
	private const int BulkBudgetMilliseconds = 5_000;

	/// <summary>
	/// The budget for a thousand one-element unions into a <see cref="BulkSize"/> set: inserting in place
	/// takes tens of milliseconds, rebuilding the set for each takes several seconds.
	/// </summary>
	private const int SmallUnionsBudgetMilliseconds = 1_500;

	private static readonly IComparer<(int Key, int Index)> ByKey =
		Comparer<(int Key, int Index)>.Create((x, y) => x.Key.CompareTo(y.Key));

	[TestMethod]
	public void Constructor_FromLargeDescendingSequence_RunsInLinearithmicTime()
	{
		int[] descending = [.. Enumerable.Range(0, BulkSize).Reverse()];

		Stopwatch stopwatch = Stopwatch.StartNew();
		OrderedSet<int> set = new(descending, Comparer<int>.Default);
		stopwatch.Stop();

		Assert.HasCount(BulkSize, set);
		Assert.AreSequenceEqual(Enumerable.Range(0, BulkSize), set);
		Assert.IsLessThan(BulkBudgetMilliseconds, stopwatch.ElapsedMilliseconds);
	}

	[TestMethod]
	public void IsSubsetOf_LargeDescendingArray_RunsInLinearithmicTime()
	{
		OrderedSet<int> set = [.. Enumerable.Range(0, BulkSize)];
		int[] descending = [.. Enumerable.Range(0, BulkSize).Reverse()];

		Stopwatch stopwatch = Stopwatch.StartNew();
		bool isSubset = set.IsSubsetOf(descending);
		stopwatch.Stop();

		Assert.IsTrue(isSubset);
		Assert.IsLessThan(BulkBudgetMilliseconds, stopwatch.ElapsedMilliseconds);
	}

	[TestMethod]
	public void SetEquals_LargeDescendingArray_RunsInLinearithmicTime()
	{
		OrderedSet<int> set = [.. Enumerable.Range(0, BulkSize)];
		int[] descending = [.. Enumerable.Range(0, BulkSize).Reverse()];

		Stopwatch stopwatch = Stopwatch.StartNew();
		bool equal = set.SetEquals(descending);
		stopwatch.Stop();

		Assert.IsTrue(equal);
		Assert.IsLessThan(BulkBudgetMilliseconds, stopwatch.ElapsedMilliseconds);
	}

	[TestMethod]
	public void SymmetricExceptWith_LargeDescendingArray_RunsInLinearithmicTime()
	{
		OrderedSet<int> set = [.. Enumerable.Range(0, BulkSize)];
		int[] descending = [.. Enumerable.Range(BulkSize / 2, BulkSize).Reverse()];

		Stopwatch stopwatch = Stopwatch.StartNew();
		set.SymmetricExceptWith(descending);
		stopwatch.Stop();

		Assert.AreSequenceEqual(Enumerable.Range(0, BulkSize / 2).Concat(Enumerable.Range(BulkSize, BulkSize / 2)), set);
		Assert.IsLessThan(BulkBudgetMilliseconds, stopwatch.ElapsedMilliseconds);
	}

	[TestMethod]
	public void UnionWith_LargeDescendingArray_RunsInLinearithmicTime()
	{
		OrderedSet<int> set = [.. Enumerable.Range(0, BulkSize)];
		int[] descending = [.. Enumerable.Range(BulkSize / 2, BulkSize).Reverse()];

		Stopwatch stopwatch = Stopwatch.StartNew();
		set.UnionWith(descending);
		stopwatch.Stop();

		Assert.AreSequenceEqual(Enumerable.Range(0, BulkSize + (BulkSize / 2)), set);
		Assert.IsLessThan(BulkBudgetMilliseconds, stopwatch.ElapsedMilliseconds);
	}

	[TestMethod]
	public void UnionWith_LargeCollection_KeepsThisSetsElementOnTie()
	{
		OrderedSet<(int Key, int Index)> set = new([.. Enumerable.Range(0, 100).Select(i => (i * 2, 0))], ByKey);
		(int Key, int Index)[] other = [.. Enumerable.Range(0, 100).Select(i => (i, 1))];

		set.UnionWith(other);

		Assert.AreSequenceEqual(Enumerable.Range(0, 200).Where(k => k < 100 || k % 2 == 0), set.Select(x => x.Key));
		Assert.IsTrue(set.Where(x => x.Key % 2 == 0).All(x => x.Index == 0), "a tie replaced this set's element");
		Assert.IsTrue(set.Where(x => x.Key % 2 == 1).All(x => x.Index == 1));
	}

	[TestMethod]
	public void UnionWith_NothingNewDuringEnumeration_DoesNotInvalidateEnumerator()
	{
		OrderedSet<int> set = [.. Enumerable.Range(0, 100)];
		int[] subset = [.. Enumerable.Range(0, 50)];
		int visited = 0;

		// Every element of subset is already in the set, so the union changes nothing, as HashSet<T> and SortedSet<T> allow
		foreach (int item in set)
		{
			if (item == 10)
			{
				set.UnionWith(subset);
			}

			visited++;
		}

		Assert.AreEqual(100, visited);
		Assert.AreSequenceEqual(Enumerable.Range(0, 100), set);
	}

	[TestMethod]
	public void SymmetricExceptWith_EmptyDuringEnumeration_DoesNotInvalidateEnumerator()
	{
		OrderedSet<int> set = [.. Enumerable.Range(0, 100)];
		int visited = 0;

		foreach (int item in set)
		{
			if (item == 10)
			{
				set.SymmetricExceptWith([]);
			}

			visited++;
		}

		Assert.AreEqual(100, visited);
		Assert.AreSequenceEqual(Enumerable.Range(0, 100), set);
	}

	[TestMethod]
	public void UnionWith_ManySingleElementSequences_CostAboutAsMuchAsAdd()
	{
		OrderedSet<int> set = [.. Enumerable.Range(0, BulkSize).Select(i => i * 2)];

		// Rebuilding the whole set for each one-element union took seconds; inserting in place takes milliseconds
		Stopwatch stopwatch = Stopwatch.StartNew();
		for (int i = 0; i < 1_000; i++)
		{
			set.UnionWith(One((i * 2) + 1));
		}

		stopwatch.Stop();

		Assert.HasCount(BulkSize + 1_000, set);
		Assert.IsTrue(set.Contains(1) && set.Contains(1_999), "a single-element union was lost");
		Assert.IsLessThan(SmallUnionsBudgetMilliseconds, stopwatch.ElapsedMilliseconds);

		static IEnumerable<int> One(int value)
		{
			yield return value;
		}
	}

	[TestMethod]
	public void UnionWith_LargeLazySequence_AddsOnlyTheNewElementsInOrder()
	{
		OrderedSet<int> set = [.. Enumerable.Range(0, 100).Select(i => i * 3)];

		// More new elements than are inserted one at a time, so this takes the merge path
		set.UnionWith(Enumerable.Range(0, 300).Where(i => i % 2 == 0));

		Assert.AreSequenceEqual(Enumerable.Range(0, 300).Where(i => i % 2 == 0 || i % 3 == 0), set);
	}

	[TestMethod]
	public void UnionWith_Itself_LeavesSetUnchanged()
	{
		OrderedSet<int> set = [.. Enumerable.Range(0, 100)];

		set.UnionWith(set);

		Assert.AreSequenceEqual(Enumerable.Range(0, 100), set);
	}

	[TestMethod]
	public void IntersectWith_LargeDescendingArray_RunsInLinearithmicTime()
	{
		OrderedSet<int> set = [.. Enumerable.Range(0, BulkSize)];
		int[] descending = [.. Enumerable.Range(BulkSize / 2, BulkSize).Reverse()];

		Stopwatch stopwatch = Stopwatch.StartNew();
		set.IntersectWith(descending);
		stopwatch.Stop();

		Assert.AreSequenceEqual(Enumerable.Range(BulkSize / 2, BulkSize / 2), set);
		Assert.IsLessThan(BulkBudgetMilliseconds, stopwatch.ElapsedMilliseconds);
	}

	[TestMethod]
	public void ExceptWith_LargeEvens_RunsInLinearithmicTime()
	{
		// One RemoveAt per excepted element shifts the whole tail each time: about 2.4e11 moves at this size,
		// over ten seconds even as a memmove, where one compaction pass takes about a hundred milliseconds
		const int size = 800_000;
		const int budgetMilliseconds = 1_500;
		OrderedSet<int> set = [.. Enumerable.Range(0, size)];
		int[] evens = [.. Enumerable.Range(0, size / 2).Select(i => i * 2)];

		Stopwatch stopwatch = Stopwatch.StartNew();
		set.ExceptWith(evens);
		stopwatch.Stop();

		Assert.AreSequenceEqual(Enumerable.Range(0, size / 2).Select(i => (i * 2) + 1), set);
		Assert.IsLessThan(budgetMilliseconds, stopwatch.ElapsedMilliseconds);
	}

	[TestMethod]
	public void Constructor_FromSequenceWithDuplicates_KeepsFirstOccurrence()
	{
		// Enough elements that the bulk path merges rather than only insertion sorting
		(int Key, int Index)[] source = [.. Enumerable.Range(0, 1_000).Select(i => (i * 7 % 10, i))];

		OrderedSet<(int Key, int Index)> set = new(source, ByKey);

		(int Key, int Index)[] expected = [.. Enumerable.Range(0, 10).Select(key => source.First(item => item.Key == key))];
		Assert.AreSequenceEqual(expected, set);
	}

	[TestMethod]
	public void Constructor_FromSequenceWithCustomComparer_HonoursComparer()
	{
		OrderedSet<string> set = new(["b", "A", "a", "B", "c"], StringComparer.OrdinalIgnoreCase);

		Assert.AreSequenceEqual(["A", "b", "c"], set);
	}

	[TestMethod]
	public void Constructor_FromSequenceWithReverseComparer_SortsDescending()
	{
		OrderedSet<int> set = new(Enumerable.Range(0, 100), Comparer<int>.Create((x, y) => y.CompareTo(x)));

		Assert.AreSequenceEqual(Enumerable.Range(0, 100).Reverse(), set);
	}

	[TestMethod]
	public void Constructor_FromSequence_MatchesRepeatedAdd()
	{
		Random random = new(42);
		int[] source = [.. Enumerable.Range(0, 5_000).Select(_ => random.Next(1_000))];
		OrderedSet<int> added = [];
		foreach (int item in source)
		{
			added.Add(item);
		}

		OrderedSet<int> built = new(source, Comparer<int>.Default);

		Assert.AreSequenceEqual(added, built);
	}

	[TestMethod]
	public void SymmetricExceptWith_Self_EmptiesSet()
	{
		OrderedSet<int> set = [1, 2, 3];

		set.SymmetricExceptWith(set);

		Assert.HasCount(0, set);
	}

	[TestMethod]
	public void SymmetricExceptWith_OrderedSetWithSameComparer_KeepsElementsInOnlyOne()
	{
		OrderedSet<int> set = [1, 2, 3, 4];
		OrderedSet<int> other = [3, 4, 5, 6];

		set.SymmetricExceptWith(other);

		Assert.AreSequenceEqual([1, 2, 5, 6], set);
		Assert.AreSequenceEqual([3, 4, 5, 6], other);
	}

	/// <summary>An element ordered by <see cref="Key"/> alone, so elements with the same key tie.</summary>
	private readonly record struct Tied(int Key, int Order) : IComparable<Tied>
	{
		public int CompareTo(Tied other) => Key.CompareTo(other.Key);
	}

	private sealed class NoOrdering;

	[TestMethod]
	public void CollectionExpression_IsBuiltByCreate()
	{
		CollectionBuilderAttribute? builder = typeof(OrderedSet<>).GetCustomAttributes(typeof(CollectionBuilderAttribute), false).OfType<CollectionBuilderAttribute>().SingleOrDefault();

		Assert.IsNotNull(builder);
		Assert.AreEqual(typeof(OrderedSet), builder.BuilderType);
		Assert.AreEqual(nameof(OrderedSet.Create), builder.MethodName);
	}

	[TestMethod]
	public void CollectionExpression_MatchesConstructor()
	{
		Random random = new(42);
		int[] data = [.. Enumerable.Range(0, 1_000).Select(_ => random.Next(100))];

		OrderedSet<int> fromExpression = [.. data];

		Assert.AreSequenceEqual(new OrderedSet<int>(data), fromExpression);
	}

	[TestMethod]
	public void CollectionExpression_Empty_CreatesEmptyInstance()
	{
		OrderedSet<int> empty = [];

		Assert.IsEmpty(empty);
	}

	[TestMethod]
	public void Create_TypeWithoutDefaultOrdering_ThrowsArgumentException()
	{
		NoOrdering[] data = [new(), new()];

		Assert.ThrowsExactly<ArgumentException>(() => OrderedSet.Create<NoOrdering>(data));
		Assert.ThrowsExactly<ArgumentException>(() => OrderedSet.Create<NoOrdering>([]));
	}

	[TestMethod]
	public void CollectionExpression_FromLargeDescendingSequence_RunsInLinearithmicTime()
	{
		// Adding one at a time inserts each element at the front, which takes many seconds at this size
		const int size = 1_000_000;
		int[] descending = [.. Enumerable.Range(0, size).Reverse()];

		Stopwatch stopwatch = Stopwatch.StartNew();
		OrderedSet<int> fromExpression = [.. descending];
		stopwatch.Stop();

		Assert.AreSequenceEqual(Enumerable.Range(0, size), fromExpression);
		Assert.IsLessThan(5_000, stopwatch.ElapsedMilliseconds);
	}

	[TestMethod]
	public void CollectionExpression_ComparerTies_KeepsFirstElement()
	{
		Tied[] data = [new(2, 0), new(1, 1), new(2, 2), new(1, 3), new(3, 4)];

		OrderedSet<Tied> fromExpression = [.. data];

		Assert.AreSequenceEqual([new Tied(1, 1), new Tied(2, 0), new Tied(3, 4)], fromExpression);
		Assert.AreSequenceEqual(new OrderedSet<Tied>(data), fromExpression);
	}
}
