// Copyright (c) ktsu.dev
// All rights reserved.
// Licensed under the MIT license.

namespace ktsu.Containers.Tests;

using System.Collections;

[TestClass]
public class OrderedCollectionTests
{
	[TestMethod]
	public void Constructor_WithoutParameters_CreatesEmptyCollection()
	{
		// Arrange & Act
		OrderedCollection<int> collection = [];

		// Assert
		Assert.AreEqual(0, collection.Count);
		Assert.IsFalse(collection.IsReadOnly);
	}

	[TestMethod]
	public void Constructor_WithComparer_UsesCustomComparer()
	{
		// Arrange
		IComparer<int> reverseComparer = Comparer<int>.Create((x, y) => y.CompareTo(x));

		// Act
		OrderedCollection<int> collection = new(reverseComparer)
		{
			1,
			3,
			2
		};

		// Assert
		Assert.AreEqual(3, collection.Count);
		Assert.AreEqual(3, collection[0]); // Should be in reverse order
		Assert.AreEqual(2, collection[1]);
		Assert.AreEqual(1, collection[2]);
	}

	[TestMethod]
	public void Constructor_WithCapacity_CreatesCollectionWithCapacity()
	{
		// Arrange & Act
		OrderedCollection<int> collection = new(10);

		// Assert
		Assert.AreEqual(0, collection.Count);
	}

	[TestMethod]
	public void Constructor_WithNegativeCapacity_ThrowsArgumentOutOfRangeException()
	{
		// Arrange, Act & Assert
		Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => new OrderedCollection<int>(-1));
	}

	[TestMethod]
	public void Constructor_WithCapacityAndComparer_CreatesCollectionCorrectly()
	{
		// Arrange
		IComparer<int> comparer = Comparer<int>.Default;

		// Act
		OrderedCollection<int> collection = new(10, comparer);

		// Assert
		Assert.AreEqual(0, collection.Count);
	}

	[TestMethod]
	public void Constructor_WithNullComparer_ThrowsArgumentNullException()
	{
		// Arrange, Act & Assert
		Assert.ThrowsExactly<ArgumentNullException>(() => new OrderedCollection<int>((IComparer<int>)null!));
		Assert.ThrowsExactly<ArgumentNullException>(() => new OrderedCollection<int>(10, null!));
	}

	[TestMethod]
	public void Constructor_WithCollection_AddsElementsInOrder()
	{
		// Arrange
		int[] items = [3, 1, 4, 1, 5, 9, 2, 6];

		// Act
		OrderedCollection<int> collection = [.. items];

		// Assert
		Assert.AreEqual(8, collection.Count);
		Assert.AreEqual(1, collection[0]);
		Assert.AreEqual(1, collection[1]);
		Assert.AreEqual(2, collection[2]);
		Assert.AreEqual(3, collection[3]);
		Assert.AreEqual(4, collection[4]);
		Assert.AreEqual(5, collection[5]);
		Assert.AreEqual(6, collection[6]);
		Assert.AreEqual(9, collection[7]);
	}

	[TestMethod]
	public void Constructor_WithNullCollection_ThrowsArgumentNullException()
	{
		// Arrange, Act & Assert
		Assert.ThrowsExactly<ArgumentNullException>(() => new OrderedCollection<int>((IEnumerable<int>)null!));
		Assert.ThrowsExactly<ArgumentNullException>(() => new OrderedCollection<int>(null!, Comparer<int>.Default));
	}

	[TestMethod]
	public void Constructor_WithCollectionAndComparer_UsesCustomComparer()
	{
		// Arrange
		int[] items = [3, 1, 4, 1, 5];
		IComparer<int> reverseComparer = Comparer<int>.Create((x, y) => y.CompareTo(x));

		// Act
		OrderedCollection<int> collection = new(items, reverseComparer);

		// Assert
		Assert.AreEqual(5, collection.Count);
		Assert.AreEqual(5, collection[0]); // Should be in reverse order
		Assert.AreEqual(4, collection[1]);
		Assert.AreEqual(3, collection[2]);
		Assert.AreEqual(1, collection[3]);
		Assert.AreEqual(1, collection[4]);
	}

	[TestMethod]
	public void Add_SingleElement_AddsCorrectly()
	{
		// Arrange
		OrderedCollection<int> collection =
		[
			// Act
			5,
		];

		// Assert
		Assert.AreEqual(1, collection.Count);
		Assert.AreEqual(5, collection[0]);
	}

	[TestMethod]
	public void Add_MultipleElements_MaintainsSortedOrder()
	{
		// Arrange
		OrderedCollection<int> collection =
		[
			// Act
			3,
			1,
			4,
			2,
		];

		// Assert
		Assert.AreEqual(4, collection.Count);
		Assert.AreEqual(1, collection[0]);
		Assert.AreEqual(2, collection[1]);
		Assert.AreEqual(3, collection[2]);
		Assert.AreEqual(4, collection[3]);
	}

	[TestMethod]
	public void Add_DuplicateElements_AllowsDuplicates()
	{
		// Arrange
		OrderedCollection<int> collection =
		[
			// Act
			2,
			2,
			1,
			2,
		];

		// Assert
		Assert.AreEqual(4, collection.Count);
		Assert.AreEqual(1, collection[0]);
		Assert.AreEqual(2, collection[1]);
		Assert.AreEqual(2, collection[2]);
		Assert.AreEqual(2, collection[3]);
	}

	[TestMethod]
	public void Clear_WithElements_RemovesAllElements()
	{
		// Arrange
		OrderedCollection<int> collection = new([1, 2, 3, 4, 5]);

		// Act
		collection.Clear();

		// Assert
		Assert.AreEqual(0, collection.Count);
	}

	[TestMethod]
	public void Contains_ExistingElement_ReturnsTrue()
	{
		// Arrange
		OrderedCollection<int> collection = new([1, 3, 5, 7, 9]);

		// Act & Assert
		Assert.IsTrue(collection.Contains(5));
		Assert.IsTrue(collection.Contains(1));
		Assert.IsTrue(collection.Contains(9));
	}

	[TestMethod]
	public void Contains_NonExistingElement_ReturnsFalse()
	{
		// Arrange
		OrderedCollection<int> collection = new([1, 3, 5, 7, 9]);

		// Act & Assert
		Assert.IsFalse(collection.Contains(2));
		Assert.IsFalse(collection.Contains(0));
		Assert.IsFalse(collection.Contains(10));
	}

	[TestMethod]
	public void Contains_EmptyCollection_ReturnsFalse()
	{
		// Arrange
		OrderedCollection<int> collection = [];

		// Act & Assert
		Assert.IsFalse(collection.Contains(1));
	}

	[TestMethod]
	public void CopyTo_ValidArray_CopiesElements()
	{
		// Arrange
		OrderedCollection<int> collection = new([3, 1, 4, 2]);
		int[] array = new int[6];

		// Act
		collection.CopyTo(array, 1);

		// Assert
		Assert.AreEqual(0, array[0]);
		Assert.AreEqual(1, array[1]); // Elements should be copied in sorted order
		Assert.AreEqual(2, array[2]);
		Assert.AreEqual(3, array[3]);
		Assert.AreEqual(4, array[4]);
		Assert.AreEqual(0, array[5]);
	}

	[TestMethod]
	public void CopyTo_NullArray_ThrowsArgumentNullException()
	{
		// Arrange
		OrderedCollection<int> collection = new([1, 2, 3]);

		// Act & Assert
		Assert.ThrowsExactly<ArgumentNullException>(() => collection.CopyTo(null!, 0));
	}

	[TestMethod]
	public void CopyTo_NegativeArrayIndex_ThrowsArgumentOutOfRangeException()
	{
		// Arrange
		OrderedCollection<int> collection = new([1, 2, 3]);
		int[] array = new int[5];

		// Act & Assert
		Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => collection.CopyTo(array, -1));
	}

	[TestMethod]
	public void CopyTo_InsufficientSpace_ThrowsArgumentException()
	{
		// Arrange
		OrderedCollection<int> collection = new([1, 2, 3, 4, 5]);
		int[] array = new int[3];

		// Act & Assert
		Assert.ThrowsExactly<ArgumentException>(() => collection.CopyTo(array, 0));
	}

	[TestMethod]
	public void Remove_ExistingElement_RemovesAndReturnsTrue()
	{
		// Arrange
		OrderedCollection<int> collection = new([1, 2, 3, 2, 4]);

		// Act
		bool result = collection.Remove(2);

		// Assert
		Assert.IsTrue(result);
		Assert.AreEqual(4, collection.Count);
		Assert.AreEqual(1, collection[0]);
		Assert.AreEqual(2, collection[1]); // Should remove first occurrence
		Assert.AreEqual(3, collection[2]);
		Assert.AreEqual(4, collection[3]);
	}

	[TestMethod]
	public void Remove_NonExistingElement_ReturnsFalse()
	{
		// Arrange
		OrderedCollection<int> collection = new([1, 3, 5]);

		// Act
		bool result = collection.Remove(2);

		// Assert
		Assert.IsFalse(result);
		Assert.AreEqual(3, collection.Count);
	}

	[TestMethod]
	public void RemoveAt_ValidIndex_RemovesElement()
	{
		// Arrange
		OrderedCollection<int> collection = new([1, 2, 3, 4, 5]);

		// Act
		collection.RemoveAt(2);

		// Assert
		Assert.AreEqual(4, collection.Count);
		Assert.AreEqual(1, collection[0]);
		Assert.AreEqual(2, collection[1]);
		Assert.AreEqual(4, collection[2]); // Element 3 was removed
		Assert.AreEqual(5, collection[3]);
	}

	[TestMethod]
	public void RemoveAt_InvalidIndex_ThrowsArgumentOutOfRangeException()
	{
		// Arrange
		OrderedCollection<int> collection = new([1, 2, 3]);

		// Act & Assert
		Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => collection.RemoveAt(-1));
		Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => collection.RemoveAt(3));
		Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => collection.RemoveAt(10));
	}

	[TestMethod]
	public void IndexAccessor_ValidIndex_ReturnsElement()
	{
		// Arrange
		OrderedCollection<int> collection = new([3, 1, 4, 2]);

		// Act & Assert
		Assert.AreEqual(1, collection[0]);
		Assert.AreEqual(2, collection[1]);
		Assert.AreEqual(3, collection[2]);
		Assert.AreEqual(4, collection[3]);
	}

	[TestMethod]
	public void IndexAccessor_InvalidIndex_ThrowsArgumentOutOfRangeException()
	{
		// Arrange
		OrderedCollection<int> collection = new([1, 2, 3]);

		// Act & Assert
		Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => collection[-1]);
		Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => collection[3]);
		Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => collection[10]);
	}

	[TestMethod]
	public void BinarySearch_ExistingElement_ReturnsCorrectIndex()
	{
		// Arrange
		OrderedCollection<int> collection = new([1, 3, 5, 7, 9]);

		// Act & Assert
		Assert.AreEqual(0, collection.BinarySearch(1));
		Assert.AreEqual(2, collection.BinarySearch(5));
		Assert.AreEqual(4, collection.BinarySearch(9));
	}

	[TestMethod]
	public void BinarySearch_NonExistingElement_ReturnsNegativeValue()
	{
		// Arrange
		OrderedCollection<int> collection = new([1, 3, 5, 7, 9]);

		// Act
		int result = collection.BinarySearch(4);

		// Assert
		Assert.IsTrue(result < 0);
		Assert.AreEqual(2, ~result); // Should indicate insertion point between 3 and 5
	}

	[TestMethod]
	public void IndexOf_ExistingElement_ReturnsCorrectIndex()
	{
		// Arrange
		OrderedCollection<int> collection = new([1, 3, 5, 7, 9]);

		// Act & Assert
		Assert.AreEqual(2, collection.IndexOf(5));
		Assert.AreEqual(0, collection.IndexOf(1));
		Assert.AreEqual(4, collection.IndexOf(9));
	}

	[TestMethod]
	public void IndexOf_NonExistingElement_ReturnsMinusOne()
	{
		// Arrange
		OrderedCollection<int> collection = new([1, 3, 5, 7, 9]);

		// Act & Assert
		Assert.AreEqual(-1, collection.IndexOf(4));
		Assert.AreEqual(-1, collection.IndexOf(0));
		Assert.AreEqual(-1, collection.IndexOf(10));
	}

	[TestMethod]
	public void GetEnumerator_IteratesInSortedOrder()
	{
		// Arrange
		OrderedCollection<int> collection = new([3, 1, 4, 1, 5, 9, 2, 6]);
		List<int> expected = [1, 1, 2, 3, 4, 5, 6, 9];

		// Act
		List<int> actual = [.. collection];

		// Assert
		CollectionAssert.AreEqual(expected, actual);
	}

	[TestMethod]
	public void GetEnumerator_NonGeneric_IteratesInSortedOrder()
	{
		// Arrange
		OrderedCollection<int> collection = new([3, 1, 4, 2]);
		List<object> actual = [];

		// Act
		IEnumerable enumerable = collection;
		foreach (object item in enumerable)
		{
			actual.Add(item);
		}

		// Assert
		Assert.AreEqual(4, actual.Count);
		Assert.AreEqual(1, actual[0]);
		Assert.AreEqual(2, actual[1]);
		Assert.AreEqual(3, actual[2]);
		Assert.AreEqual(4, actual[3]);
	}

	[TestMethod]
	public void GetRange_ValidRange_ReturnsCorrectSubset()
	{
		// Arrange
		OrderedCollection<int> collection = new([1, 2, 3, 4, 5, 6, 7, 8, 9]);

		// Act
		OrderedCollection<int> range = collection.GetRange(2, 4);

		// Assert
		Assert.AreEqual(4, range.Count);
		Assert.AreEqual(3, range[0]);
		Assert.AreEqual(4, range[1]);
		Assert.AreEqual(5, range[2]);
		Assert.AreEqual(6, range[3]);
	}

	[TestMethod]
	public void GetRange_InvalidStartIndex_ThrowsArgumentOutOfRangeException()
	{
		// Arrange
		OrderedCollection<int> collection = new([1, 2, 3]);

		// Act & Assert
		Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => collection.GetRange(-1, 2));
		Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => collection.GetRange(3, 1));
	}

	[TestMethod]
	public void GetRange_InvalidCount_ThrowsArgumentOutOfRangeException()
	{
		// Arrange
		OrderedCollection<int> collection = new([1, 2, 3]);

		// Act & Assert
		Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => collection.GetRange(0, -1));
		Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => collection.GetRange(1, 3));
	}

	[TestMethod]
	public void Clone_CreatesShallowCopy()
	{
		// Arrange
		OrderedCollection<int> original = new([3, 1, 4, 2]);

		// Act
		OrderedCollection<int> clone = original.Clone();

		// Assert
		Assert.AreEqual(original.Count, clone.Count);
		for (int i = 0; i < original.Count; i++)
		{
			Assert.AreEqual(original[i], clone[i]);
		}

		// Verify they are independent
		clone.Add(5);
		Assert.AreNotEqual(original.Count, clone.Count);
	}

	[TestMethod]
	public void EmptyCollection_Properties_ReturnExpectedValues()
	{
		// Arrange
		OrderedCollection<int> collection = [];

		// Act & Assert
		Assert.AreEqual(0, collection.Count);
		Assert.IsFalse(collection.IsReadOnly);
	}

	[TestMethod]
	public void WorksWithStrings_MaintainsAlphabeticalOrder()
	{
		// Arrange
		OrderedCollection<string> collection =
		[
			// Act
			"zebra",
			"apple",
			"banana",
			"cherry",
		];

		// Assert
		Assert.AreEqual(4, collection.Count);
		Assert.AreEqual("apple", collection[0]);
		Assert.AreEqual("banana", collection[1]);
		Assert.AreEqual("cherry", collection[2]);
		Assert.AreEqual("zebra", collection[3]);
	}

	[TestMethod]
	public void Constructor_WithNonComparableType_WithoutComparer_ThrowsArgumentException()
	{
		// Arrange & Act & Assert
		Assert.ThrowsExactly<ArgumentException>(() => new OrderedCollection<object>());
		Assert.ThrowsExactly<ArgumentException>(() => new OrderedCollection<object>(10));
		Assert.ThrowsExactly<ArgumentException>(() => new OrderedCollection<object>([]));
	}
}
