// Copyright (c) ktsu.dev
// All rights reserved.
// Licensed under the MIT license.

namespace ktsu.Containers.Tests;
[TestClass]
public class ContiguousCollectionTests
{
	[TestMethod]
	public void Constructor_WithoutParameters_CreatesEmptyCollection()
	{
		// Arrange & Act
		ContiguousCollection<int> collection = [];

		// Assert
		Assert.AreEqual(0, collection.Count);
		Assert.IsFalse(collection.IsReadOnly);
		Assert.IsTrue(collection.Capacity >= 0);
	}

	[TestMethod]
	public void Constructor_WithCapacity_CreatesCollectionWithCapacity()
	{
		// Arrange & Act
		ContiguousCollection<int> collection = new(10);

		// Assert
		Assert.AreEqual(0, collection.Count);
		Assert.AreEqual(10, collection.Capacity);
	}

	[TestMethod]
	public void Constructor_WithZeroCapacity_CreatesEmptyCollection()
	{
		// Arrange & Act
		ContiguousCollection<int> collection = [];

		// Assert
		Assert.AreEqual(0, collection.Count);
		Assert.AreEqual(0, collection.Capacity);
	}

	[TestMethod]
	public void Constructor_WithNegativeCapacity_ThrowsArgumentOutOfRangeException()
	{
		// Arrange, Act & Assert
		Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => new ContiguousCollection<int>(-1));
	}

	[TestMethod]
	public void Constructor_WithCollection_CopiesElementsContiguously()
	{
		// Arrange
		int[] items = [3, 1, 4, 1, 5, 9, 2, 6];

		// Act
		ContiguousCollection<int> collection = [.. items];

		// Assert
		Assert.AreEqual(8, collection.Count);
		for (int i = 0; i < items.Length; i++)
		{
			Assert.AreEqual(items[i], collection[i]);
		}
	}

	[TestMethod]
	public void Constructor_WithNullCollection_ThrowsArgumentNullException()
	{
		// Arrange, Act & Assert
		Assert.ThrowsExactly<ArgumentNullException>(() => new ContiguousCollection<int>(null!));
	}

	[TestMethod]
	public void Add_SingleElement_AddsToEnd()
	{
		// Arrange
		ContiguousCollection<int> collection = [];

		// Act
		collection.Add(5);

		// Assert
		Assert.AreEqual(1, collection.Count);
		Assert.AreEqual(5, collection[0]);
	}

	[TestMethod]
	public void Add_MultipleElements_MaintainsOrder()
	{
		// Arrange
		ContiguousCollection<int> collection = [];

		// Act
		collection.Add(3);
		collection.Add(1);
		collection.Add(4);
		collection.Add(2);

		// Assert
		Assert.AreEqual(4, collection.Count);
		Assert.AreEqual(3, collection[0]);
		Assert.AreEqual(1, collection[1]);
		Assert.AreEqual(4, collection[2]);
		Assert.AreEqual(2, collection[3]);
	}

	[TestMethod]
	public void Add_ExceedsCapacity_GrowsAutomatically()
	{
		// Arrange
		ContiguousCollection<int> collection = new(2)
		{
			// Act
			1,
			2,
			3 // Should trigger growth
		};

		// Assert
		Assert.AreEqual(3, collection.Count);
		Assert.IsTrue(collection.Capacity >= 3);
		Assert.AreEqual(1, collection[0]);
		Assert.AreEqual(2, collection[1]);
		Assert.AreEqual(3, collection[2]);
	}

	[TestMethod]
	public void Clear_WithElements_RemovesAllElements()
	{
		// Arrange
		ContiguousCollection<int> collection = [.. new int[] { 1, 2, 3, 4, 5 }];

		// Act
		collection.Clear();

		// Assert
		Assert.AreEqual(0, collection.Count);
	}

	[TestMethod]
	public void Contains_ExistingElement_ReturnsTrue()
	{
		// Arrange
		ContiguousCollection<int> collection = [.. new int[] { 1, 2, 3, 4, 5 }];

		// Act & Assert
		Assert.IsTrue(collection.Contains(3));
		Assert.IsTrue(collection.Contains(1));
		Assert.IsTrue(collection.Contains(5));
	}

	[TestMethod]
	public void Contains_NonExistingElement_ReturnsFalse()
	{
		// Arrange
		ContiguousCollection<int> collection = [.. new int[] { 1, 2, 3, 4, 5 }];

		// Act & Assert
		Assert.IsFalse(collection.Contains(6));
		Assert.IsFalse(collection.Contains(0));
	}

	[TestMethod]
	public void IndexOf_ExistingElement_ReturnsCorrectIndex()
	{
		// Arrange
		ContiguousCollection<int> collection = [.. new int[] { 3, 1, 4, 1, 2 }];

		// Act & Assert
		Assert.AreEqual(0, collection.IndexOf(3));
		Assert.AreEqual(1, collection.IndexOf(1)); // Returns first occurrence
		Assert.AreEqual(2, collection.IndexOf(4));
		Assert.AreEqual(4, collection.IndexOf(2));
	}

	[TestMethod]
	public void IndexOf_NonExistingElement_ReturnsMinusOne()
	{
		// Arrange
		ContiguousCollection<int> collection = [.. new int[] { 1, 2, 3 }];

		// Act & Assert
		Assert.AreEqual(-1, collection.IndexOf(5));
	}

	[TestMethod]
	public void Indexer_ValidIndex_ReturnsElement()
	{
		// Arrange
		ContiguousCollection<int> collection = [.. new int[] { 3, 1, 4, 2 }];

		// Act & Assert
		Assert.AreEqual(3, collection[0]);
		Assert.AreEqual(1, collection[1]);
		Assert.AreEqual(4, collection[2]);
		Assert.AreEqual(2, collection[3]);
	}

	[TestMethod]
	public void Indexer_InvalidIndex_ThrowsArgumentOutOfRangeException()
	{
		// Arrange
		ContiguousCollection<int> collection = [.. new int[] { 1, 2, 3 }];

		// Act & Assert
		Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => collection[-1]);
		Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => collection[3]);
	}

	[TestMethod]
	public void Indexer_Set_UpdatesElement()
	{
		// Arrange
		ContiguousCollection<int> collection = [.. new int[] { 1, 2, 3 }];

		// Act
		collection[1] = 20;

		// Assert
		Assert.AreEqual(20, collection[1]);
		Assert.AreEqual(3, collection.Count); // Count unchanged
	}

	[TestMethod]
	public void Insert_ValidIndex_InsertsElement()
	{
		// Arrange
		ContiguousCollection<int> collection = [.. new int[] { 1, 3, 4 }];

		// Act
		collection.Insert(1, 2);

		// Assert
		Assert.AreEqual(4, collection.Count);
		Assert.AreEqual(1, collection[0]);
		Assert.AreEqual(2, collection[1]); // Inserted
		Assert.AreEqual(3, collection[2]); // Shifted
		Assert.AreEqual(4, collection[3]); // Shifted
	}

	[TestMethod]
	public void Insert_InvalidIndex_ThrowsArgumentOutOfRangeException()
	{
		// Arrange
		ContiguousCollection<int> collection = [.. new int[] { 1, 2, 3 }];

		// Act & Assert
		Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => collection.Insert(-1, 0));
		Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => collection.Insert(4, 0));
	}

	[TestMethod]
	public void Remove_ExistingElement_RemovesAndReturnsTrue()
	{
		// Arrange
		ContiguousCollection<int> collection = [.. new int[] { 3, 1, 4, 1, 2 }];

		// Act
		bool result = collection.Remove(1); // Should remove first occurrence

		// Assert
		Assert.IsTrue(result);
		Assert.AreEqual(4, collection.Count);
		Assert.AreEqual(3, collection[0]);
		Assert.AreEqual(4, collection[1]); // 4 moved forward
		Assert.AreEqual(1, collection[2]); // Second 1 remains
		Assert.AreEqual(2, collection[3]);
	}

	[TestMethod]
	public void Remove_NonExistingElement_ReturnsFalse()
	{
		// Arrange
		ContiguousCollection<int> collection = [.. new int[] { 1, 2, 3 }];

		// Act
		bool result = collection.Remove(5);

		// Assert
		Assert.IsFalse(result);
		Assert.AreEqual(3, collection.Count);
	}

	[TestMethod]
	public void RemoveAt_ValidIndex_RemovesElement()
	{
		// Arrange
		ContiguousCollection<int> collection = [.. new int[] { 3, 1, 4, 2 }];

		// Act
		collection.RemoveAt(1);

		// Assert
		Assert.AreEqual(3, collection.Count);
		Assert.AreEqual(3, collection[0]);
		Assert.AreEqual(4, collection[1]); // 4 moved forward
		Assert.AreEqual(2, collection[2]);
	}

	[TestMethod]
	public void RemoveAt_InvalidIndex_ThrowsArgumentOutOfRangeException()
	{
		// Arrange
		ContiguousCollection<int> collection = [.. new int[] { 1, 2, 3 }];

		// Act & Assert
		Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => collection.RemoveAt(-1));
		Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => collection.RemoveAt(3));
	}

	[TestMethod]
	public void CopyTo_ValidArray_CopiesElements()
	{
		// Arrange
		ContiguousCollection<int> collection = [.. new int[] { 3, 1, 4, 2 }];
		int[] array = new int[6];

		// Act
		collection.CopyTo(array, 1);

		// Assert
		Assert.AreEqual(0, array[0]); // Unchanged
		Assert.AreEqual(3, array[1]);
		Assert.AreEqual(1, array[2]);
		Assert.AreEqual(4, array[3]);
		Assert.AreEqual(2, array[4]);
		Assert.AreEqual(0, array[5]); // Unchanged
	}
	private static readonly int[] expected = [3, 1, 4, 2];

	[TestMethod]
	public void GetEnumerator_IteratesInOrder()
	{
		// Arrange
		ContiguousCollection<int> collection = [.. new int[] { 3, 1, 4, 2 }];
		List<int> enumerated = [];

		// Act
		foreach (int item in collection)
		{
			enumerated.Add(item);
		}

		// Assert
		CollectionAssert.AreEqual(expected, enumerated);
	}

	[TestMethod]
	public void AsSpan_ReturnsCorrectSpan()
	{
		// Arrange
		ContiguousCollection<int> collection = [.. new int[] { 3, 1, 4, 2 }];

		// Act
		Span<int> span = collection.AsSpan();

		// Assert
		Assert.AreEqual(4, span.Length);
		Assert.AreEqual(3, span[0]);
		Assert.AreEqual(1, span[1]);
		Assert.AreEqual(4, span[2]);
		Assert.AreEqual(2, span[3]);
	}

	[TestMethod]
	public void AsReadOnlySpan_ReturnsCorrectSpan()
	{
		// Arrange
		ContiguousCollection<int> collection = [.. new int[] { 3, 1, 4, 2 }];

		// Act
		ReadOnlySpan<int> span = collection.AsReadOnlySpan();

		// Assert
		Assert.AreEqual(4, span.Length);
		Assert.AreEqual(3, span[0]);
		Assert.AreEqual(1, span[1]);
		Assert.AreEqual(4, span[2]);
		Assert.AreEqual(2, span[3]);
	}

	[TestMethod]
	public void GetRange_ValidRange_ReturnsCorrectSubset()
	{
		// Arrange
		ContiguousCollection<int> collection = [.. new int[] { 3, 1, 4, 1, 5, 9, 2 }];

		// Act
		ContiguousCollection<int> range = collection.GetRange(2, 3);

		// Assert
		Assert.AreEqual(3, range.Count);
		Assert.AreEqual(4, range[0]);
		Assert.AreEqual(1, range[1]);
		Assert.AreEqual(5, range[2]);
	}

	[TestMethod]
	public void GetRange_InvalidParameters_ThrowsArgumentOutOfRangeException()
	{
		// Arrange
		ContiguousCollection<int> collection = [.. new int[] { 1, 2, 3 }];

		// Act & Assert
		Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => collection.GetRange(-1, 2));
		Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => collection.GetRange(4, 1));
		Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => collection.GetRange(1, -1));
		Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => collection.GetRange(1, 3));
	}

	[TestMethod]
	public void Clone_CreatesShallowCopy()
	{
		// Arrange
		ContiguousCollection<int> original = [.. new int[] { 3, 1, 4, 2 }];

		// Act
		ContiguousCollection<int> clone = original.Clone();

		// Assert
		Assert.AreNotSame(original, clone);
		Assert.AreEqual(original.Count, clone.Count);
		for (int i = 0; i < original.Count; i++)
		{
			Assert.AreEqual(original[i], clone[i]);
		}

		// Verify independence
		clone.Add(5);
		Assert.AreNotEqual(original.Count, clone.Count);
	}

	[TestMethod]
	public void EnsureCapacity_IncreasesCapacity()
	{
		// Arrange
		ContiguousCollection<int> collection = new(5);

		// Act
		collection.EnsureCapacity(10);

		// Assert
		Assert.IsTrue(collection.Capacity >= 10);
	}

	[TestMethod]
	public void TrimExcess_ReducesCapacityToCount()
	{
		// Arrange
		ContiguousCollection<int> collection = new(10)
		{
			1,
			2
		};

		// Act
		collection.TrimExcess();

		// Assert
		Assert.AreEqual(2, collection.Capacity);
		Assert.AreEqual(2, collection.Count);
	}

	[TestMethod]
	public void ContiguousMemoryLayout_OptimalForCachePerformance()
	{
		// Arrange
		ContiguousCollection<int> collection = [];

		// Act - Add many elements to test contiguous allocation
		for (int i = 0; i < 1000; i++)
		{
			collection.Add(i);
		}

		// Assert - Verify all elements are accessible and in order
		for (int i = 0; i < 1000; i++)
		{
			Assert.AreEqual(i, collection[i]);
		}

		// Test span access (only possible with contiguous memory)
		ReadOnlySpan<int> span = collection.AsReadOnlySpan();
		Assert.AreEqual(1000, span.Length);
		for (int i = 0; i < 1000; i++)
		{
			Assert.AreEqual(i, span[i]);
		}
	}

	[TestMethod]
	public void WorksWithReferenceTypes()
	{
		// Arrange & Act
		ContiguousCollection<string> collection = [.. new string[] { "charlie", "alpha", "bravo" }];

		// Assert
		Assert.AreEqual(3, collection.Count);
		Assert.AreEqual("charlie", collection[0]);
		Assert.AreEqual("alpha", collection[1]);
		Assert.AreEqual("bravo", collection[2]);
	}
}
