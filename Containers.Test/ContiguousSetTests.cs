// Copyright (c) ktsu.dev
// All rights reserved.
// Licensed under the MIT license.

namespace ktsu.Containers.Tests;
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
		Assert.IsFalse(set.IsReadOnly);
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
		Assert.IsTrue(set.Contains(1));
		Assert.IsTrue(set.Contains(2));
		Assert.IsTrue(set.Contains(3));
		Assert.IsTrue(set.Contains(4));
		Assert.IsTrue(set.Contains(5));
		Assert.IsTrue(set.Contains(6));
	}

	[TestMethod]
	public void Constructor_WithNullCollection_ThrowsArgumentNullException()
	{
		// Arrange, Act & Assert
		Assert.ThrowsException<ArgumentNullException>(() => new ContiguousSet<int>((IEnumerable<int>)null!));
	}

	[TestMethod]
	public void Add_NewElement_ReturnsTrue()
	{
		// Arrange
		ContiguousSet<int> set = [];

		// Act
		bool result = set.Add(5);

		// Assert
		Assert.IsTrue(result);
		Assert.AreEqual(1, set.Count);
		Assert.IsTrue(set.Contains(5));
	}

	[TestMethod]
	public void Add_DuplicateElement_ReturnsFalse()
	{
		// Arrange
		ContiguousSet<int> set = [5];

		// Act
		bool result = set.Add(5);

		// Assert
		Assert.IsFalse(result);
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
		Assert.IsTrue(result1);
		Assert.IsTrue(result2);
		Assert.IsTrue(result3);
		Assert.IsFalse(result4);
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
		Assert.IsTrue(set.Contains(3));
		Assert.IsTrue(set.Contains(1));
		Assert.IsTrue(set.Contains(5));
	}

	[TestMethod]
	public void Contains_NonExistingElement_ReturnsFalse()
	{
		// Arrange
		ContiguousSet<int> set = [.. new int[] { 1, 2, 3, 4, 5 }];

		// Act & Assert
		Assert.IsFalse(set.Contains(6));
		Assert.IsFalse(set.Contains(0));
	}

	[TestMethod]
	public void Remove_ExistingElement_RemovesAndReturnsTrue()
	{
		// Arrange
		ContiguousSet<int> set = [.. new int[] { 3, 1, 4, 2 }];

		// Act
		bool result = set.Remove(1);

		// Assert
		Assert.IsTrue(result);
		Assert.AreEqual(3, set.Count);
		Assert.IsTrue(set.Contains(3));
		Assert.IsFalse(set.Contains(1));
		Assert.IsTrue(set.Contains(4));
		Assert.IsTrue(set.Contains(2));
	}

	[TestMethod]
	public void Remove_NonExistingElement_ReturnsFalse()
	{
		// Arrange
		ContiguousSet<int> set = [.. new int[] { 1, 2, 3 }];

		// Act
		bool result = set.Remove(5);

		// Assert
		Assert.IsFalse(result);
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
		Assert.IsTrue(copied.Contains(1));
		Assert.IsTrue(copied.Contains(2));
		Assert.IsTrue(copied.Contains(3));
		Assert.IsTrue(copied.Contains(4));
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
		Assert.AreEqual(4, enumerated.Count); // No duplicates
		Assert.IsTrue(enumerated.Contains(1));
		Assert.IsTrue(enumerated.Contains(2));
		Assert.IsTrue(enumerated.Contains(3));
		Assert.IsTrue(enumerated.Contains(4));
	}

	[TestMethod]
	public void AsReadOnlySpan_ReturnsCorrectSpan()
	{
		// Arrange
		ContiguousSet<int> set = [.. new int[] { 3, 1, 4, 2 }];

		// Act
		ReadOnlySpan<int> span = set.AsReadOnlySpan();

		// Assert
		Assert.AreEqual(4, span.Length);

		// Verify all elements are in the span
		HashSet<int> spanElements = [];
		for (int i = 0; i < span.Length; i++)
		{
			spanElements.Add(span[i]);
		}
		Assert.AreEqual(4, spanElements.Count);
		Assert.IsTrue(spanElements.Contains(1));
		Assert.IsTrue(spanElements.Contains(2));
		Assert.IsTrue(spanElements.Contains(3));
		Assert.IsTrue(spanElements.Contains(4));
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
		Assert.IsTrue(set1.Contains(1));
		Assert.IsTrue(set1.Contains(2));
		Assert.IsTrue(set1.Contains(3));
		Assert.IsTrue(set1.Contains(4));
		Assert.IsTrue(set1.Contains(5));
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
		Assert.IsFalse(set1.Contains(1));
		Assert.IsTrue(set1.Contains(2));
		Assert.IsFalse(set1.Contains(3));
		Assert.IsTrue(set1.Contains(4));
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
		Assert.IsTrue(set1.Contains(1));
		Assert.IsFalse(set1.Contains(2));
		Assert.IsTrue(set1.Contains(3));
		Assert.IsFalse(set1.Contains(4));
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
		Assert.IsTrue(set1.Contains(1));
		Assert.IsFalse(set1.Contains(2));
		Assert.IsFalse(set1.Contains(3));
		Assert.IsTrue(set1.Contains(4));
	}

	[TestMethod]
	public void SetEquals_SameElements_ReturnsTrue()
	{
		// Arrange
		ContiguousSet<int> set1 = [.. new int[] { 1, 2, 3 }];
		ContiguousSet<int> set2 = [.. new int[] { 3, 1, 2 }]; // Different order

		// Act & Assert
		Assert.IsTrue(set1.SetEquals(set2));
		Assert.IsTrue(set2.SetEquals(set1));
	}

	[TestMethod]
	public void SetEquals_DifferentElements_ReturnsFalse()
	{
		// Arrange
		ContiguousSet<int> set1 = [.. new int[] { 1, 2, 3 }];
		ContiguousSet<int> set2 = [.. new int[] { 1, 2, 4 }];

		// Act & Assert
		Assert.IsFalse(set1.SetEquals(set2));
		Assert.IsFalse(set2.SetEquals(set1));
	}

	[TestMethod]
	public void IsSubsetOf_TrueSubset_ReturnsTrue()
	{
		// Arrange
		ContiguousSet<int> subset = [.. new int[] { 1, 3 }];
		ContiguousSet<int> superset = [.. new int[] { 1, 2, 3, 4 }];

		// Act & Assert
		Assert.IsTrue(subset.IsSubsetOf(superset));
	}

	[TestMethod]
	public void IsSubsetOf_NotSubset_ReturnsFalse()
	{
		// Arrange
		ContiguousSet<int> set1 = [.. new int[] { 1, 5 }];
		ContiguousSet<int> set2 = [.. new int[] { 1, 2, 3, 4 }];

		// Act & Assert
		Assert.IsFalse(set1.IsSubsetOf(set2));
	}

	[TestMethod]
	public void IsSupersetOf_TrueSuperset_ReturnsTrue()
	{
		// Arrange
		ContiguousSet<int> superset = [.. new int[] { 1, 2, 3, 4 }];
		ContiguousSet<int> subset = [.. new int[] { 1, 3 }];

		// Act & Assert
		Assert.IsTrue(superset.IsSupersetOf(subset));
	}

	[TestMethod]
	public void IsProperSubsetOf_TrueProperSubset_ReturnsTrue()
	{
		// Arrange
		ContiguousSet<int> subset = [.. new int[] { 1, 3 }];
		ContiguousSet<int> superset = [.. new int[] { 1, 2, 3, 4 }];

		// Act & Assert
		Assert.IsTrue(subset.IsProperSubsetOf(superset));
		Assert.IsFalse(superset.IsProperSubsetOf(subset));
	}

	[TestMethod]
	public void Overlaps_WithCommonElements_ReturnsTrue()
	{
		// Arrange
		ContiguousSet<int> set1 = [.. new int[] { 1, 2, 3 }];
		ContiguousSet<int> set2 = [.. new int[] { 3, 4, 5 }];

		// Act & Assert
		Assert.IsTrue(set1.Overlaps(set2));
		Assert.IsTrue(set2.Overlaps(set1));
	}

	[TestMethod]
	public void Overlaps_WithoutCommonElements_ReturnsFalse()
	{
		// Arrange
		ContiguousSet<int> set1 = [.. new int[] { 1, 2, 3 }];
		ContiguousSet<int> set2 = [.. new int[] { 4, 5, 6 }];

		// Act & Assert
		Assert.IsFalse(set1.Overlaps(set2));
		Assert.IsFalse(set2.Overlaps(set1));
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
			Assert.IsTrue(set.Contains(i));
		}

		// Test span access (only possible with contiguous memory)
		ReadOnlySpan<int> span = set.AsReadOnlySpan();
		Assert.AreEqual(1000, span.Length);

		// Verify all elements are in the span
		HashSet<int> spanElements = [];
		for (int i = 0; i < span.Length; i++)
		{
			spanElements.Add(span[i]);
		}
		Assert.AreEqual(1000, spanElements.Count);
	}

	[TestMethod]
	public void WorksWithStrings()
	{
		// Arrange & Act
		ContiguousSet<string> set = [.. new string[] { "charlie", "alpha", "bravo", "alpha" }]; // Contains duplicate

		// Assert
		Assert.AreEqual(3, set.Count); // Only unique elements
		Assert.IsTrue(set.Contains("charlie"));
		Assert.IsTrue(set.Contains("alpha"));
		Assert.IsTrue(set.Contains("bravo"));
	}
}
