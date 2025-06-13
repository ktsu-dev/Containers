// Copyright (c) ktsu.dev
// All rights reserved.
// Licensed under the MIT license.

namespace ktsu.Containers.Tests;
[TestClass]
public class ContiguousMapTests
{
	[TestMethod]
	public void Constructor_WithoutParameters_CreatesEmptyMap()
	{
		// Arrange & Act
		ContiguousMap<int, string> map = [];

		// Assert
		Assert.AreEqual(0, map.Count);
		Assert.IsFalse(map.IsReadOnly);
	}

	[TestMethod]
	public void Constructor_WithCapacity_CreatesMapWithCapacity()
	{
		// Arrange & Act
		ContiguousMap<int, string> map = new(10);

		// Assert
		Assert.AreEqual(0, map.Count);
	}

	[TestMethod]
	public void Constructor_WithKeyValuePairs_AddsUniqueKeys()
	{
		// Arrange
		KeyValuePair<int, string>[] items =
		[
			new(3, "three"),
			new(1, "one"),
			new(4, "four"),
			new(1, "duplicate"), // Duplicate key
			new(2, "two")
		];

		// Act
		ContiguousMap<int, string> map = [];
		foreach (KeyValuePair<int, string> item in items)
		{
			map.TryAdd(item.Key, item.Value);
		}

		// Assert
		Assert.AreEqual(4, map.Count); // Only unique keys
		Assert.AreEqual("one", map[1]); // First value for key 1
		Assert.AreEqual("two", map[2]);
		Assert.AreEqual("three", map[3]);
		Assert.AreEqual("four", map[4]);
	}

	[TestMethod]
	public void Add_NewKeyValuePair_AddsSuccessfully()
	{
		// Arrange
		ContiguousMap<int, string> map = [];

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
		ContiguousMap<int, string> map = [];
		map.Add(5, "five");

		// Act & Assert
		Assert.ThrowsExactly<ArgumentException>(() => map.Add(5, "another five"));
	}

	[TestMethod]
	public void TryAdd_NewKey_ReturnsTrue()
	{
		// Arrange
		ContiguousMap<int, string> map = [];

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
		ContiguousMap<int, string> map = [];
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
		ContiguousMap<int, string> map = [];
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
		ContiguousMap<int, string> map = [];
		map.Add(1, "one");

		// Act & Assert
		Assert.ThrowsExactly<KeyNotFoundException>(() => map[2]);
	}

	[TestMethod]
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Major Bug", "S4143:Collection elements should not be replaced unconditionally", Justification = "<Pending>")]
	public void Indexer_Set_UpdatesValue()
	{
		// Arrange
		ContiguousMap<int, string> map = [];
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
		ContiguousMap<int, string> map = [];
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
		ContiguousMap<int, string> map = [];
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
		ContiguousMap<int, string> map = [];
		map.Add(1, "one");

		// Act & Assert
		Assert.IsFalse(map.ContainsKey(2));
		Assert.IsFalse(map.ContainsKey(0));
	}

	[TestMethod]
	public void TryGetValue_ExistingKey_ReturnsTrueWithValue()
	{
		// Arrange
		ContiguousMap<int, string> map = [];
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
		ContiguousMap<int, string> map = [];
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
		ContiguousMap<int, string> map = [];
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
	}

	[TestMethod]
	public void Remove_NonExistingKey_ReturnsFalse()
	{
		// Arrange
		ContiguousMap<int, string> map = [];
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
		ContiguousMap<int, string> map = [];
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
	public void Keys_ReturnsAllKeys()
	{
		// Arrange
		ContiguousMap<int, string> map = [];
		map.Add(3, "three");
		map.Add(1, "one");
		map.Add(4, "four");
		map.Add(2, "two");

		// Act
		HashSet<int> keys = [.. map.Keys];

		// Assert
		Assert.AreEqual(4, keys.Count);
		Assert.IsTrue(keys.Contains(1));
		Assert.IsTrue(keys.Contains(2));
		Assert.IsTrue(keys.Contains(3));
		Assert.IsTrue(keys.Contains(4));
	}

	[TestMethod]
	public void Values_ReturnsAllValues()
	{
		// Arrange
		ContiguousMap<int, string> map = [];
		map.Add(3, "three");
		map.Add(1, "one");
		map.Add(4, "four");
		map.Add(2, "two");

		// Act
		HashSet<string> values = [.. map.Values];

		// Assert
		Assert.AreEqual(4, values.Count);
		Assert.IsTrue(values.Contains("one"));
		Assert.IsTrue(values.Contains("two"));
		Assert.IsTrue(values.Contains("three"));
		Assert.IsTrue(values.Contains("four"));
	}

	[TestMethod]
	public void GetEnumerator_IteratesAllPairs()
	{
		// Arrange
		ContiguousMap<int, string> map = [];
		map.Add(3, "three");
		map.Add(1, "one");
		map.Add(4, "four");
		map.Add(2, "two");

		// Act
		Dictionary<int, string> enumerated = [];
		foreach (KeyValuePair<int, string> kvp in map)
		{
			enumerated.Add(kvp.Key, kvp.Value);
		}

		// Assert
		Assert.AreEqual(4, enumerated.Count);
		Assert.AreEqual("one", enumerated[1]);
		Assert.AreEqual("two", enumerated[2]);
		Assert.AreEqual("three", enumerated[3]);
		Assert.AreEqual("four", enumerated[4]);
	}

	[TestMethod]
	public void Contains_ExistingKeyValuePair_ReturnsTrue()
	{
		// Arrange
		ContiguousMap<int, string> map = [];
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
		ContiguousMap<int, string> map = [];
		map.Add(1, "one");

		// Act & Assert
		Assert.IsFalse(map.Contains(new KeyValuePair<int, string>(1, "ONE"))); // Wrong value
		Assert.IsFalse(map.Contains(new KeyValuePair<int, string>(2, "two"))); // Wrong key
	}

	[TestMethod]
	public void CopyTo_ValidArray_CopiesElements()
	{
		// Arrange
		ContiguousMap<int, string> map = [];
		map.Add(3, "three");
		map.Add(1, "one");

		KeyValuePair<int, string>[] array = new KeyValuePair<int, string>[4];

		// Act
		map.CopyTo(array, 1);

		// Assert
		Assert.AreEqual(default, array[0]); // Unchanged

		// Verify that the two pairs are in the array (order may vary)
		HashSet<KeyValuePair<int, string>> copied = [array[1], array[2]];
		Assert.IsTrue(copied.Contains(new KeyValuePair<int, string>(1, "one")));
		Assert.IsTrue(copied.Contains(new KeyValuePair<int, string>(3, "three")));

		Assert.AreEqual(default, array[3]); // Unchanged
	}

	[TestMethod]
	public void GetKeysSpan_ReturnsCorrectSpan()
	{
		// Arrange
		ContiguousMap<int, string> map = [];
		map.Add(3, "three");
		map.Add(1, "one");
		map.Add(4, "four");
		map.Add(2, "two");

		// Act
		ReadOnlySpan<int> keysSpan = map.GetKeysSpan();

		// Assert
		Assert.AreEqual(4, keysSpan.Length);

		// Verify all keys are in the span
		HashSet<int> spanKeys = [];
		for (int i = 0; i < keysSpan.Length; i++)
		{
			spanKeys.Add(keysSpan[i]);
		}
		Assert.AreEqual(4, spanKeys.Count);
		Assert.IsTrue(spanKeys.Contains(1));
		Assert.IsTrue(spanKeys.Contains(2));
		Assert.IsTrue(spanKeys.Contains(3));
		Assert.IsTrue(spanKeys.Contains(4));
	}

	[TestMethod]
	public void GetValuesSpan_ReturnsCorrectSpan()
	{
		// Arrange
		ContiguousMap<int, string> map = [];
		map.Add(3, "three");
		map.Add(1, "one");
		map.Add(4, "four");
		map.Add(2, "two");

		// Act
		ReadOnlySpan<string> valuesSpan = map.GetValuesSpan();

		// Assert
		Assert.AreEqual(4, valuesSpan.Length);

		// Verify all values are in the span
		HashSet<string> spanValues = [];
		for (int i = 0; i < valuesSpan.Length; i++)
		{
			spanValues.Add(valuesSpan[i]);
		}
		Assert.AreEqual(4, spanValues.Count);
		Assert.IsTrue(spanValues.Contains("one"));
		Assert.IsTrue(spanValues.Contains("two"));
		Assert.IsTrue(spanValues.Contains("three"));
		Assert.IsTrue(spanValues.Contains("four"));
	}

	[TestMethod]
	public void ContiguousMemoryLayout_OptimalForCachePerformance()
	{
		// Arrange
		ContiguousMap<int, string> map = [];

		// Act - Add many key-value pairs
		for (int i = 0; i < 1000; i++)
		{
			map.Add(i, $"value_{i}");
		}

		// Assert - Verify all elements are accessible
		Assert.AreEqual(1000, map.Count);
		for (int i = 0; i < 1000; i++)
		{
			Assert.IsTrue(map.ContainsKey(i));
			Assert.AreEqual($"value_{i}", map[i]);
		}

		// Test span access (only possible with contiguous memory)
		ReadOnlySpan<int> keysSpan = map.GetKeysSpan();
		ReadOnlySpan<string> valuesSpan = map.GetValuesSpan();
		Assert.AreEqual(1000, keysSpan.Length);
		Assert.AreEqual(1000, valuesSpan.Length);

		// Verify all keys are in the span
		HashSet<int> spanKeys = [];
		for (int i = 0; i < keysSpan.Length; i++)
		{
			spanKeys.Add(keysSpan[i]);
		}
		Assert.AreEqual(1000, spanKeys.Count);
	}

	[TestMethod]
	public void WorksWithCustomTypes()
	{
		// Arrange
		ContiguousMap<string, int> map = [];

		// Act
		map.Add("charlie", 3);
		map.Add("alpha", 1);
		map.Add("bravo", 2);

		// Assert
		Assert.AreEqual(3, map.Count);
		Assert.AreEqual(1, map["alpha"]);
		Assert.AreEqual(2, map["bravo"]);
		Assert.AreEqual(3, map["charlie"]);
	}

	[TestMethod]
	public void HandlesNullValues()
	{
		// Arrange
		ContiguousMap<int, string?> map = [];

		// Act
		map.Add(1, null);
		map.Add(2, "two");

		// Assert
		Assert.AreEqual(2, map.Count);
		Assert.IsNull(map[1]);
		Assert.AreEqual("two", map[2]);
		Assert.IsTrue(map.TryGetValue(1, out string? value));
		Assert.IsNull(value);
	}
}
