// Copyright (c) ktsu.dev
// All rights reserved.
// Licensed under the MIT license.

namespace ktsu.Containers.Tests;
[TestClass]
public class InsertionOrderSetTests
{
	[TestMethod]
	public void Constructor_WithoutParameters_CreatesEmptySet()
	{
		// Arrange & Act
		InsertionOrderSet<int> set = [];

		// Assert
		Assert.AreEqual(0, set.Count);
		Assert.IsFalse(set.IsReadOnly);
	}

	[TestMethod]
	public void Constructor_WithCapacity_CreatesSetWithCapacity()
	{
		// Arrange & Act
		InsertionOrderSet<int> set = new(10);

		// Assert
		Assert.AreEqual(0, set.Count);
	}

	[TestMethod]
	public void Constructor_WithCollection_AddsUniqueElementsInInsertionOrder()
	{
		// Arrange
		int[] items = [3, 1, 4, 1, 5, 3, 2, 6]; // Has duplicates

		// Act
		InsertionOrderSet<int> set = [.. items];

		// Assert
		Assert.AreEqual(6, set.Count); // Only unique elements
		Assert.AreEqual(3, set.ElementAt(0)); // First unique
		Assert.AreEqual(1, set.ElementAt(1)); // Second unique
		Assert.AreEqual(4, set.ElementAt(2)); // Third unique
		Assert.AreEqual(5, set.ElementAt(3)); // Fourth unique
		Assert.AreEqual(2, set.ElementAt(4)); // Fifth unique
		Assert.AreEqual(6, set.ElementAt(5)); // Sixth unique
	}

	[TestMethod]
	public void Add_NewElement_ReturnsTrue()
	{
		// Arrange
		InsertionOrderSet<int> set = [];

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
		InsertionOrderSet<int> set = [5];

		// Act
		bool result = set.Add(5);

		// Assert
		Assert.IsFalse(result);
		Assert.AreEqual(1, set.Count);
	}

	[TestMethod]
	public void Add_MultipleElements_MaintainsInsertionOrder()
	{
		// Arrange
		InsertionOrderSet<int> set = [];

		// Act
		set.Add(3);
		set.Add(1);
		set.Add(4);
		set.Add(1); // Duplicate, should not be added
		set.Add(2);

		// Assert
		Assert.AreEqual(4, set.Count);
		Assert.AreEqual(3, set.ElementAt(0)); // First inserted
		Assert.AreEqual(1, set.ElementAt(1)); // Second inserted
		Assert.AreEqual(4, set.ElementAt(2)); // Third inserted
		Assert.AreEqual(2, set.ElementAt(3)); // Fourth inserted
	}

	[TestMethod]
	public void Clear_WithElements_RemovesAllElements()
	{
		// Arrange
		InsertionOrderSet<int> set = [.. new int[] { 1, 2, 3, 4, 5 }];

		// Act
		set.Clear();

		// Assert
		Assert.AreEqual(0, set.Count);
	}

	[TestMethod]
	public void Contains_ExistingElement_ReturnsTrue()
	{
		// Arrange
		InsertionOrderSet<int> set = [.. new int[] { 1, 2, 3, 4, 5 }];

		// Act & Assert
		Assert.IsTrue(set.Contains(3));
		Assert.IsTrue(set.Contains(1));
		Assert.IsTrue(set.Contains(5));
	}

	[TestMethod]
	public void Contains_NonExistingElement_ReturnsFalse()
	{
		// Arrange
		InsertionOrderSet<int> set = [.. new int[] { 1, 2, 3, 4, 5 }];

		// Act & Assert
		Assert.IsFalse(set.Contains(6));
		Assert.IsFalse(set.Contains(0));
	}

	[TestMethod]
	public void Remove_ExistingElement_RemovesAndReturnsTrue()
	{
		// Arrange
		InsertionOrderSet<int> set = [.. new int[] { 3, 1, 4, 2 }];

		// Act
		bool result = set.Remove(1);

		// Assert
		Assert.IsTrue(result);
		Assert.AreEqual(3, set.Count);
		Assert.AreEqual(3, set.ElementAt(0));
		Assert.AreEqual(4, set.ElementAt(1)); // Maintains order of remaining elements
		Assert.AreEqual(2, set.ElementAt(2));
		Assert.IsFalse(set.Contains(1));
	}

	[TestMethod]
	public void Remove_NonExistingElement_ReturnsFalse()
	{
		// Arrange
		InsertionOrderSet<int> set = [.. new int[] { 1, 2, 3 }];

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
		InsertionOrderSet<int> set = [.. new int[] { 3, 1, 4, 2 }];
		int[] array = new int[6];

		// Act
		set.CopyTo(array, 1);

		// Assert
		Assert.AreEqual(0, array[0]); // Unchanged
		Assert.AreEqual(3, array[1]); // First inserted
		Assert.AreEqual(1, array[2]); // Second inserted
		Assert.AreEqual(4, array[3]); // Third inserted
		Assert.AreEqual(2, array[4]); // Fourth inserted
		Assert.AreEqual(0, array[5]); // Unchanged
	}
	private static readonly int[] expected = new int[] { 3, 1, 4, 2 };

	[TestMethod]
	public void GetEnumerator_IteratesInInsertionOrder()
	{
		// Arrange
		InsertionOrderSet<int> set = [.. new int[] { 3, 1, 4, 1, 2 }]; // Contains duplicate
		List<int> enumerated = [];

		// Act
		foreach (int item in set)
		{
			enumerated.Add(item);
		}

		// Assert
		CollectionAssert.AreEqual(expected, enumerated); // No duplicates, insertion order
	}

	[TestMethod]
	public void SetEquals_SameElements_ReturnsTrue()
	{
		// Arrange
		InsertionOrderSet<int> set1 = [.. new int[] { 1, 2, 3 }];
		InsertionOrderSet<int> set2 = [.. new int[] { 3, 1, 2 }]; // Different order

		// Act & Assert
		Assert.IsTrue(set1.SetEquals(set2));
		Assert.IsTrue(set2.SetEquals(set1));
	}

	[TestMethod]
	public void SetEquals_DifferentElements_ReturnsFalse()
	{
		// Arrange
		InsertionOrderSet<int> set1 = [.. new int[] { 1, 2, 3 }];
		InsertionOrderSet<int> set2 = [.. new int[] { 1, 2, 4 }];

		// Act & Assert
		Assert.IsFalse(set1.SetEquals(set2));
		Assert.IsFalse(set2.SetEquals(set1));
	}

	[TestMethod]
	public void IsSubsetOf_TrueSubset_ReturnsTrue()
	{
		// Arrange
		InsertionOrderSet<int> subset = [.. new int[] { 1, 3 }];
		InsertionOrderSet<int> superset = [.. new int[] { 1, 2, 3, 4 }];

		// Act & Assert
		Assert.IsTrue(subset.IsSubsetOf(superset));
	}

	[TestMethod]
	public void IsSubsetOf_NotSubset_ReturnsFalse()
	{
		// Arrange
		InsertionOrderSet<int> set1 = [.. new int[] { 1, 5 }];
		InsertionOrderSet<int> set2 = [.. new int[] { 1, 2, 3, 4 }];

		// Act & Assert
		Assert.IsFalse(set1.IsSubsetOf(set2));
	}

	[TestMethod]
	public void IsSupersetOf_TrueSuperset_ReturnsTrue()
	{
		// Arrange
		InsertionOrderSet<int> superset = [.. new int[] { 1, 2, 3, 4 }];
		InsertionOrderSet<int> subset = [.. new int[] { 1, 3 }];

		// Act & Assert
		Assert.IsTrue(superset.IsSupersetOf(subset));
	}

	[TestMethod]
	public void UnionWith_AddsNewElements_MaintainsInsertionOrder()
	{
		// Arrange
		InsertionOrderSet<int> set1 = [.. new int[] { 1, 3 }];
		InsertionOrderSet<int> set2 = [.. new int[] { 2, 3, 4 }];

		// Act
		set1.UnionWith(set2);

		// Assert
		Assert.AreEqual(4, set1.Count);
		Assert.AreEqual(1, set1.ElementAt(0)); // Original element
		Assert.AreEqual(3, set1.ElementAt(1)); // Original element
		Assert.AreEqual(2, set1.ElementAt(2)); // New element from set2
		Assert.AreEqual(4, set1.ElementAt(3)); // New element from set2
	}

	[TestMethod]
	public void IntersectWith_KeepsCommonElements_MaintainsInsertionOrder()
	{
		// Arrange
		InsertionOrderSet<int> set1 = [.. new int[] { 1, 2, 3, 4 }];
		InsertionOrderSet<int> set2 = [.. new int[] { 2, 4, 5, 6 }];

		// Act
		set1.IntersectWith(set2);

		// Assert
		Assert.AreEqual(2, set1.Count);
		Assert.AreEqual(2, set1.ElementAt(0)); // Common element, maintains original order
		Assert.AreEqual(4, set1.ElementAt(1)); // Common element, maintains original order
	}

	[TestMethod]
	public void ExceptWith_RemovesCommonElements()
	{
		// Arrange
		InsertionOrderSet<int> set1 = [.. new int[] { 1, 2, 3, 4 }];
		InsertionOrderSet<int> set2 = [.. new int[] { 2, 4 }];

		// Act
		set1.ExceptWith(set2);

		// Assert
		Assert.AreEqual(2, set1.Count);
		Assert.AreEqual(1, set1.ElementAt(0));
		Assert.AreEqual(3, set1.ElementAt(1));
	}

	[TestMethod]
	public void WorksWithStrings_MaintainsInsertionOrder()
	{
		// Arrange & Act
		InsertionOrderSet<string> set = [.. new string[] { "charlie", "alpha", "bravo", "alpha" }]; // Contains duplicate

		// Assert
		Assert.AreEqual(3, set.Count); // Only unique elements
		Assert.AreEqual("charlie", set.ElementAt(0)); // First unique
		Assert.AreEqual("alpha", set.ElementAt(1));   // Second unique
		Assert.AreEqual("bravo", set.ElementAt(2));   // Third unique
	}
}
