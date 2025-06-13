// Copyright (c) ktsu.dev
// All rights reserved.
// Licensed under the MIT license.

namespace ktsu.Containers.Tests;

using System.Collections;

[TestClass]
public class InsertionOrderCollectionTests
{
	[TestMethod]
	public void Constructor_WithoutParameters_CreatesEmptyCollection()
	{
		// Arrange & Act
		InsertionOrderCollection<int> collection = [];

		// Assert
		Assert.AreEqual(0, collection.Count);
		Assert.IsFalse(collection.IsReadOnly);
	}

	[TestMethod]
	public void Constructor_WithCapacity_CreatesCollectionWithCapacity()
	{
		// Arrange & Act
		InsertionOrderCollection<int> collection = new(10);

		// Assert
		Assert.AreEqual(0, collection.Count);
	}

	[TestMethod]
	public void Constructor_WithNegativeCapacity_ThrowsArgumentOutOfRangeException()
	{
		// Arrange, Act & Assert
		Assert.ThrowsException<ArgumentOutOfRangeException>(() => new InsertionOrderCollection<int>(-1));
	}

	[TestMethod]
	public void Constructor_WithCollection_AddsElementsInInsertionOrder()
	{
		// Arrange
		int[] items = [3, 1, 4, 1, 5, 9, 2, 6];

		// Act
		InsertionOrderCollection<int> collection = [.. items];

		// Assert
		Assert.AreEqual(8, collection.Count);
		Assert.AreEqual(3, collection[0]); // First inserted
		Assert.AreEqual(1, collection[1]); // Second inserted
		Assert.AreEqual(4, collection[2]); // Third inserted
		Assert.AreEqual(1, collection[3]); // Fourth inserted (allows duplicates)
		Assert.AreEqual(5, collection[4]);
		Assert.AreEqual(9, collection[5]);
		Assert.AreEqual(2, collection[6]);
		Assert.AreEqual(6, collection[7]); // Last inserted
	}

	[TestMethod]
	public void Constructor_WithNullCollection_ThrowsArgumentNullException()
	{
		// Arrange, Act & Assert
		Assert.ThrowsException<ArgumentNullException>(() => new InsertionOrderCollection<int>((IEnumerable<int>)null!));
	}

	[TestMethod]
	public void Add_SingleElement_AddsCorrectly()
	{
		// Arrange
		InsertionOrderCollection<int> collection =
		[
			// Act
			5,
		];

		// Assert
		Assert.AreEqual(1, collection.Count);
		Assert.AreEqual(5, collection[0]);
	}

	[TestMethod]
	public void Add_MultipleElements_MaintainsInsertionOrder()
	{
		// Arrange
		InsertionOrderCollection<int> collection =
		[
			// Act
			3,
			1,
			4,
			2,
		];

		// Assert
		Assert.AreEqual(4, collection.Count);
		Assert.AreEqual(3, collection[0]); // First inserted
		Assert.AreEqual(1, collection[1]); // Second inserted
		Assert.AreEqual(4, collection[2]); // Third inserted
		Assert.AreEqual(2, collection[3]); // Fourth inserted
	}

	[TestMethod]
	public void Add_DuplicateElements_AllowsDuplicates()
	{
		// Arrange
		InsertionOrderCollection<int> collection =
		[
			// Act
			2,
			2,
			1,
			2,
		];

		// Assert
		Assert.AreEqual(4, collection.Count);
		Assert.AreEqual(2, collection[0]); // First 2
		Assert.AreEqual(2, collection[1]); // Second 2
		Assert.AreEqual(1, collection[2]); // The 1
		Assert.AreEqual(2, collection[3]); // Third 2
	}

	[TestMethod]
	public void Clear_WithElements_RemovesAllElements()
	{
		// Arrange
		InsertionOrderCollection<int> collection = [.. new int[] { 1, 2, 3, 4, 5 }];

		// Act
		collection.Clear();

		// Assert
		Assert.AreEqual(0, collection.Count);
	}

	[TestMethod]
	public void Contains_ExistingElement_ReturnsTrue()
	{
		// Arrange
		InsertionOrderCollection<int> collection = [.. new int[] { 1, 2, 3, 4, 5 }];

		// Act & Assert
		Assert.IsTrue(collection.Contains(3));
		Assert.IsTrue(collection.Contains(1));
		Assert.IsTrue(collection.Contains(5));
	}

	[TestMethod]
	public void Contains_NonExistingElement_ReturnsFalse()
	{
		// Arrange
		InsertionOrderCollection<int> collection = [.. new int[] { 1, 2, 3, 4, 5 }];

		// Act & Assert
		Assert.IsFalse(collection.Contains(6));
		Assert.IsFalse(collection.Contains(0));
	}

	[TestMethod]
	public void Contains_EmptyCollection_ReturnsFalse()
	{
		// Arrange
		InsertionOrderCollection<int> collection = [];

		// Act & Assert
		Assert.IsFalse(collection.Contains(1));
	}

	[TestMethod]
	public void CopyTo_ValidArray_CopiesElements()
	{
		// Arrange
		InsertionOrderCollection<int> collection = [.. new int[] { 3, 1, 4, 2 }];
		int[] array = new int[6];

		// Act
		collection.CopyTo(array, 1);

		// Assert
		Assert.AreEqual(0, array[0]); // Unchanged
		Assert.AreEqual(3, array[1]); // First inserted
		Assert.AreEqual(1, array[2]); // Second inserted
		Assert.AreEqual(4, array[3]); // Third inserted
		Assert.AreEqual(2, array[4]); // Fourth inserted
		Assert.AreEqual(0, array[5]); // Unchanged
	}

	[TestMethod]
	public void CopyTo_NullArray_ThrowsArgumentNullException()
	{
		// Arrange
		InsertionOrderCollection<int> collection = [.. new int[] { 1, 2, 3 }];

		// Act & Assert
		Assert.ThrowsException<ArgumentNullException>(() => collection.CopyTo(null!, 0));
	}

	[TestMethod]
	public void CopyTo_NegativeArrayIndex_ThrowsArgumentOutOfRangeException()
	{
		// Arrange
		InsertionOrderCollection<int> collection = [.. new int[] { 1, 2, 3 }];
		int[] array = new int[5];

		// Act & Assert
		Assert.ThrowsException<ArgumentOutOfRangeException>(() => collection.CopyTo(array, -1));
	}

	[TestMethod]
	public void CopyTo_InsufficientSpace_ThrowsArgumentOutOfRangeException()
	{
		// Arrange
		InsertionOrderCollection<int> collection = [.. new int[] { 1, 2, 3, 4, 5 }];
		int[] array = new int[3];

		// Act & Assert
		Assert.ThrowsException<ArgumentOutOfRangeException>(() => collection.CopyTo(array, 0));
	}

	[TestMethod]
	public void Remove_ExistingElement_RemovesAndReturnsTrue()
	{
		// Arrange
		InsertionOrderCollection<int> collection = [.. new int[] { 3, 1, 4, 1, 2 }];

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
		InsertionOrderCollection<int> collection = [.. new int[] { 1, 2, 3 }];

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
		InsertionOrderCollection<int> collection = [.. new int[] { 3, 1, 4, 2 }];

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
		InsertionOrderCollection<int> collection = [.. new int[] { 1, 2, 3 }];

		// Act & Assert
		Assert.ThrowsException<ArgumentOutOfRangeException>(() => collection.RemoveAt(-1));
		Assert.ThrowsException<ArgumentOutOfRangeException>(() => collection.RemoveAt(3));
	}

	[TestMethod]
	public void IndexAccessor_ValidIndex_ReturnsElement()
	{
		// Arrange
		InsertionOrderCollection<int> collection = [.. new int[] { 3, 1, 4, 2 }];

		// Act & Assert
		Assert.AreEqual(3, collection[0]); // First inserted
		Assert.AreEqual(1, collection[1]); // Second inserted
		Assert.AreEqual(4, collection[2]); // Third inserted
		Assert.AreEqual(2, collection[3]); // Fourth inserted
	}

	[TestMethod]
	public void IndexAccessor_InvalidIndex_ThrowsArgumentOutOfRangeException()
	{
		// Arrange
		InsertionOrderCollection<int> collection = [.. new int[] { 1, 2, 3 }];

		// Act & Assert
		Assert.ThrowsException<ArgumentOutOfRangeException>(() => collection[-1]);
		Assert.ThrowsException<ArgumentOutOfRangeException>(() => collection[3]);
	}

	[TestMethod]
	public void IndexOf_ExistingElement_ReturnsCorrectIndex()
	{
		// Arrange
		InsertionOrderCollection<int> collection = [.. new int[] { 3, 1, 4, 1, 2 }];

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
		InsertionOrderCollection<int> collection = [.. new int[] { 1, 2, 3 }];

		// Act & Assert
		Assert.AreEqual(-1, collection.IndexOf(5));
	}

	[TestMethod]
	public void GetEnumerator_IteratesInInsertionOrder()
	{
		// Arrange
		InsertionOrderCollection<int> collection = [.. new int[] { 3, 1, 4, 2 }];
		List<int> enumerated = [];

		// Act
		foreach (int item in collection)
		{
			enumerated.Add(item);
		}

		// Assert
		CollectionAssert.AreEqual(new int[] { 3, 1, 4, 2 }, enumerated);
	}

	[TestMethod]
	public void GetEnumerator_NonGeneric_IteratesInInsertionOrder()
	{
		// Arrange
		InsertionOrderCollection<int> collection = [.. new int[] { 3, 1, 4, 2 }];
		List<object> enumerated = [];

		// Act
		IEnumerable nonGenericCollection = collection;
		foreach (object item in nonGenericCollection)
		{
			enumerated.Add(item);
		}

		// Assert
		Assert.AreEqual(4, enumerated.Count);
		Assert.AreEqual(3, enumerated[0]);
		Assert.AreEqual(1, enumerated[1]);
		Assert.AreEqual(4, enumerated[2]);
		Assert.AreEqual(2, enumerated[3]);
	}

	[TestMethod]
	public void GetRange_ValidRange_ReturnsCorrectSubset()
	{
		// Arrange
		InsertionOrderCollection<int> collection = [.. new int[] { 3, 1, 4, 1, 5, 9, 2 }];

		// Act
		InsertionOrderCollection<int> range = collection.GetRange(2, 3);

		// Assert
		Assert.AreEqual(3, range.Count);
		Assert.AreEqual(4, range[0]);
		Assert.AreEqual(1, range[1]);
		Assert.AreEqual(5, range[2]);
	}

	[TestMethod]
	public void GetRange_InvalidStartIndex_ThrowsArgumentOutOfRangeException()
	{
		// Arrange
		InsertionOrderCollection<int> collection = [.. new int[] { 1, 2, 3 }];

		// Act & Assert
		Assert.ThrowsException<ArgumentOutOfRangeException>(() => collection.GetRange(-1, 2));
		Assert.ThrowsException<ArgumentOutOfRangeException>(() => collection.GetRange(4, 1));
	}

	[TestMethod]
	public void GetRange_InvalidCount_ThrowsArgumentOutOfRangeException()
	{
		// Arrange
		InsertionOrderCollection<int> collection = [.. new int[] { 1, 2, 3 }];

		// Act & Assert
		Assert.ThrowsException<ArgumentOutOfRangeException>(() => collection.GetRange(1, -1));
		Assert.ThrowsException<ArgumentOutOfRangeException>(() => collection.GetRange(1, 3));
	}

	[TestMethod]
	public void Clone_CreatesShallowCopy()
	{
		// Arrange
		InsertionOrderCollection<int> original = [.. new int[] { 3, 1, 4, 2 }];

		// Act
		InsertionOrderCollection<int> clone = original.Clone();

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
	public void EmptyCollection_Properties_ReturnExpectedValues()
	{
		// Arrange
		InsertionOrderCollection<int> collection = [];

		// Act & Assert
		Assert.AreEqual(0, collection.Count);
		Assert.IsFalse(collection.IsReadOnly);
	}

	[TestMethod]
	public void WorksWithStrings_MaintainsInsertionOrder()
	{
		// Arrange & Act
		InsertionOrderCollection<string> collection = [.. new string[] { "charlie", "alpha", "bravo", "alpha" }];

		// Assert
		Assert.AreEqual(4, collection.Count);
		Assert.AreEqual("charlie", collection[0]); // First inserted
		Assert.AreEqual("alpha", collection[1]);   // Second inserted
		Assert.AreEqual("bravo", collection[2]);   // Third inserted
		Assert.AreEqual("alpha", collection[3]);   // Fourth inserted (duplicate allowed)
	}
}
