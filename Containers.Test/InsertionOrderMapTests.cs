// Copyright (c) ktsu.dev
// All rights reserved.
// Licensed under the MIT license.

namespace ktsu.Containers.Tests;
[TestClass]
public class InsertionOrderMapTests
{
	[TestMethod]
	public void Constructor_WithoutParameters_CreatesEmptyMap()
	{
		// Arrange & Act
		InsertionOrderMap<int, string> map = [];

		// Assert
		Assert.AreEqual(0, map.Count);
		Assert.IsFalse(map.IsReadOnly);
	}

	[TestMethod]
	public void Constructor_WithCapacity_CreatesMapWithCapacity()
	{
		// Arrange & Act
		InsertionOrderMap<int, string> map = new(10);

		// Assert
		Assert.AreEqual(0, map.Count);
	}

	[TestMethod]
	public void Constructor_WithKeyValuePairs_AddsInInsertionOrder()
	{
		// Arrange
		KeyValuePair<int, string>[] items =
		[
			new(3, "three"),
			new(1, "one"),
			new(4, "four"),
			new(2, "two")
		];

		// Act
		InsertionOrderMap<int, string> map = [.. items];

		// Assert
		Assert.AreEqual(4, map.Count);
		Assert.AreEqual("three", map[3]);
		Assert.AreEqual("one", map[1]);
		Assert.AreEqual("four", map[4]);
		Assert.AreEqual("two", map[2]);

		// Verify insertion order
		int[] expectedOrder = [3, 1, 4, 2];
		CollectionAssert.AreEqual(expectedOrder, map.Keys.ToArray());
	}

	[TestMethod]
	public void Add_NewKeyValuePair_AddsSuccessfully()
	{
		// Arrange
		InsertionOrderMap<int, string> map = [];

		// Act
		map.Add(5, "five");

		// Assert
		Assert.AreEqual(1, map.Count);
		Assert.AreEqual("five", map[5]);
		Assert.IsTrue(map.ContainsKey(5));
	}

	[TestMethod]
	public void Add_DuplicateKey_ThrowsArgumentException()
	{
		// Arrange
		InsertionOrderMap<int, string> map = [];
		map.Add(5, "five");

		// Act & Assert
		Assert.ThrowsExactly<ArgumentException>(() => map.Add(5, "another five"));
	}

	[TestMethod]
	public void TryAdd_NewKey_ReturnsTrue()
	{
		// Arrange
		InsertionOrderMap<int, string> map = [];

		// Act
		bool result = map.TryAdd(5, "five");

		// Assert
		Assert.IsTrue(result);
		Assert.AreEqual(1, map.Count);
		Assert.AreEqual("five", map[5]);
	}

	[TestMethod]
	public void TryAdd_DuplicateKey_ReturnsFalse()
	{
		// Arrange
		InsertionOrderMap<int, string> map = [];
		map.Add(5, "five");

		// Act
		bool result = map.TryAdd(5, "another five");

		// Assert
		Assert.IsFalse(result);
		Assert.AreEqual(1, map.Count);
		Assert.AreEqual("five", map[5]); // Original value remains
	}

	[TestMethod]
	public void Indexer_ExistingKey_ReturnsValue()
	{
		// Arrange
		InsertionOrderMap<int, string> map = [];
		map.Add(1, "one");
		map.Add(2, "two");

		// Act & Assert
		Assert.AreEqual("one", map[1]);
		Assert.AreEqual("two", map[2]);
	}

	[TestMethod]
	public void Indexer_NonExistingKey_ThrowsKeyNotFoundException()
	{
		// Arrange
		InsertionOrderMap<int, string> map = [];
		map.Add(1, "one");

		// Act & Assert
		Assert.ThrowsExactly<KeyNotFoundException>(() => map[2]);
	}

	[TestMethod]
	public void Indexer_Set_UpdatesValue()
	{
		// Arrange
		InsertionOrderMap<int, string> map = [];
		map.Add(1, "one");

		// Act
		map[1] = "ONE";

		// Assert
		Assert.AreEqual("ONE", map[1]);
		Assert.AreEqual(1, map.Count); // Count should not change
	}

	[TestMethod]
	public void Indexer_SetNewKey_AddsKeyValuePair()
	{
		// Arrange
		InsertionOrderMap<int, string> map = [];
		map.Add(1, "one");

		// Act
		map[2] = "two";

		// Assert
		Assert.AreEqual("two", map[2]);
		Assert.AreEqual(2, map.Count);
	}

	[TestMethod]
	public void ContainsKey_ExistingKey_ReturnsTrue()
	{
		// Arrange
		InsertionOrderMap<int, string> map = [];
		map.Add(1, "one");
		map.Add(2, "two");

		// Act & Assert
		Assert.IsTrue(map.ContainsKey(1));
		Assert.IsTrue(map.ContainsKey(2));
	}

	[TestMethod]
	public void ContainsKey_NonExistingKey_ReturnsFalse()
	{
		// Arrange
		InsertionOrderMap<int, string> map = [];
		map.Add(1, "one");

		// Act & Assert
		Assert.IsFalse(map.ContainsKey(2));
		Assert.IsFalse(map.ContainsKey(0));
	}

	[TestMethod]
	public void TryGetValue_ExistingKey_ReturnsTrueWithValue()
	{
		// Arrange
		InsertionOrderMap<int, string> map = [];
		map.Add(1, "one");

		// Act
		bool result = map.TryGetValue(1, out string? value);

		// Assert
		Assert.IsTrue(result);
		Assert.AreEqual("one", value);
	}

	[TestMethod]
	public void TryGetValue_NonExistingKey_ReturnsFalseWithDefault()
	{
		// Arrange
		InsertionOrderMap<int, string> map = [];
		map.Add(1, "one");

		// Act
		bool result = map.TryGetValue(2, out string? value);

		// Assert
		Assert.IsFalse(result);
		Assert.IsNull(value);
	}

	[TestMethod]
	public void Remove_ExistingKey_RemovesAndReturnsTrue()
	{
		// Arrange
		InsertionOrderMap<int, string> map = [];
		map.Add(1, "one");
		map.Add(2, "two");
		map.Add(3, "three");

		// Act
		bool result = map.Remove(2);

		// Assert
		Assert.IsTrue(result);
		Assert.AreEqual(2, map.Count);
		Assert.IsFalse(map.ContainsKey(2));
		Assert.IsTrue(map.ContainsKey(1));
		Assert.IsTrue(map.ContainsKey(3));

		// Verify order maintained
		int[] expectedOrder = [1, 3];
		CollectionAssert.AreEqual(expectedOrder, map.Keys.ToArray());
	}

	[TestMethod]
	public void Remove_NonExistingKey_ReturnsFalse()
	{
		// Arrange
		InsertionOrderMap<int, string> map = [];
		map.Add(1, "one");

		// Act
		bool result = map.Remove(2);

		// Assert
		Assert.IsFalse(result);
		Assert.AreEqual(1, map.Count);
	}

	[TestMethod]
	public void Clear_WithElements_RemovesAllElements()
	{
		// Arrange
		InsertionOrderMap<int, string> map = [];
		map.Add(1, "one");
		map.Add(2, "two");
		map.Add(3, "three");

		// Act
		map.Clear();

		// Assert
		Assert.AreEqual(0, map.Count);
		Assert.IsFalse(map.ContainsKey(1));
		Assert.IsFalse(map.ContainsKey(2));
		Assert.IsFalse(map.ContainsKey(3));
	}

	[TestMethod]
	public void Keys_ReturnsKeysInInsertionOrder()
	{
		// Arrange
		InsertionOrderMap<int, string> map = [];
		map.Add(3, "three");
		map.Add(1, "one");
		map.Add(4, "four");
		map.Add(2, "two");

		// Act
		int[] keys = [.. map.Keys];

		// Assert
		int[] expectedOrder = [3, 1, 4, 2];
		CollectionAssert.AreEqual(expectedOrder, keys);
	}

	[TestMethod]
	public void Values_ReturnsValuesInInsertionOrder()
	{
		// Arrange
		InsertionOrderMap<int, string> map = [];
		map.Add(3, "three");
		map.Add(1, "one");
		map.Add(4, "four");
		map.Add(2, "two");

		// Act
		string[] values = [.. map.Values];

		// Assert
		string[] expectedOrder = ["three", "one", "four", "two"];
		CollectionAssert.AreEqual(expectedOrder, values);
	}

	[TestMethod]
	public void GetEnumerator_IteratesInInsertionOrder()
	{
		// Arrange
		InsertionOrderMap<int, string> map = [];
		map.Add(3, "three");
		map.Add(1, "one");
		map.Add(4, "four");
		map.Add(2, "two");

		// Act
		List<KeyValuePair<int, string>> enumerated = [];
		foreach (KeyValuePair<int, string> kvp in map)
		{
			enumerated.Add(kvp);
		}

		// Assert
		Assert.AreEqual(4, enumerated.Count);
		Assert.AreEqual(3, enumerated[0].Key);
		Assert.AreEqual("three", enumerated[0].Value);
		Assert.AreEqual(1, enumerated[1].Key);
		Assert.AreEqual("one", enumerated[1].Value);
		Assert.AreEqual(4, enumerated[2].Key);
		Assert.AreEqual("four", enumerated[2].Value);
		Assert.AreEqual(2, enumerated[3].Key);
		Assert.AreEqual("two", enumerated[3].Value);
	}

	[TestMethod]
	public void Contains_ExistingKeyValuePair_ReturnsTrue()
	{
		// Arrange
		InsertionOrderMap<int, string> map = [];
		map.Add(1, "one");
		map.Add(2, "two");

		// Act & Assert
		Assert.IsTrue(map.Contains(new KeyValuePair<int, string>(1, "one")));
		Assert.IsTrue(map.Contains(new KeyValuePair<int, string>(2, "two")));
	}

	[TestMethod]
	public void Contains_NonExistingKeyValuePair_ReturnsFalse()
	{
		// Arrange
		InsertionOrderMap<int, string> map = [];
		map.Add(1, "one");

		// Act & Assert
		Assert.IsFalse(map.Contains(new KeyValuePair<int, string>(1, "ONE"))); // Wrong value
		Assert.IsFalse(map.Contains(new KeyValuePair<int, string>(2, "two"))); // Wrong key
	}

	[TestMethod]
	public void CopyTo_ValidArray_CopiesElements()
	{
		// Arrange
		InsertionOrderMap<int, string> map = [];
		map.Add(3, "three");
		map.Add(1, "one");

		KeyValuePair<int, string>[] array = new KeyValuePair<int, string>[4];

		// Act
		map.CopyTo(array, 1);

		// Assert
		Assert.AreEqual(default, array[0]); // Unchanged
		Assert.AreEqual(new KeyValuePair<int, string>(3, "three"), array[1]);
		Assert.AreEqual(new KeyValuePair<int, string>(1, "one"), array[2]);
		Assert.AreEqual(default, array[3]); // Unchanged
	}

	[TestMethod]
	public void MaintainsInsertionOrder_AfterMultipleOperations()
	{
		// Arrange
		InsertionOrderMap<int, string> map = [];

		// Act
		map.Add(5, "five");
		map.Add(1, "one");
		map.Add(3, "three");
		map.Remove(1);
		map.Add(2, "two");
		map.Add(4, "four");
		map.Remove(3);

		// Assert
		int[] expectedOrder = [5, 2, 4]; // Insertion order minus removed items
		CollectionAssert.AreEqual(expectedOrder, map.Keys.ToArray());
	}

	[TestMethod]
	public void WorksWithCustomTypes()
	{
		// Arrange
		InsertionOrderMap<string, int> map = [];

		// Act
		map.Add("charlie", 3);
		map.Add("alpha", 1);
		map.Add("bravo", 2);

		// Assert
		string[] expectedKeyOrder = ["charlie", "alpha", "bravo"];
		CollectionAssert.AreEqual(expectedKeyOrder, map.Keys.ToArray());
	}
}
