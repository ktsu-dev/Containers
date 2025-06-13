// Copyright (c) ktsu.dev
// All rights reserved.
// Licensed under the MIT license.

namespace ktsu.Containers.Test;

using System.Collections;
using Microsoft.VisualStudio.TestTools.UnitTesting;

/// <summary>
/// Tests for the OrderedMap class.
/// </summary>
[TestClass]
public class OrderedMapTests
{
	/// <summary>
	/// Tests that creating an empty OrderedMap with default constructor works correctly.
	/// </summary>
	[TestMethod]
	public void Constructor_DefaultConstructor_EmptyMap()
	{
		var map = new OrderedMap<int, string>();
		Assert.AreEqual(0, map.Count);
		Assert.IsFalse(map.IsReadOnly);
	}

	/// <summary>
	/// Tests that creating an OrderedMap with a custom comparer works correctly.
	/// </summary>
	[TestMethod]
	public void Constructor_WithComparer_UsesCustomComparer()
	{
		var reverseComparer = Comparer<int>.Create((x, y) => y.CompareTo(x));
		var map = new OrderedMap<int, string>(reverseComparer);

		map.Add(1, "one");
		map.Add(2, "two");
		map.Add(3, "three");

		string[] expectedOrder = ["three", "two", "one"];
		CollectionAssert.AreEqual(expectedOrder, map.Values.ToArray());
	}

	/// <summary>
	/// Tests that creating an OrderedMap with initial capacity works correctly.
	/// </summary>
	[TestMethod]
	public void Constructor_WithCapacity_EmptyMapWithCapacity()
	{
		var map = new OrderedMap<int, string>(10);
		Assert.AreEqual(0, map.Count);
	}

	/// <summary>
	/// Tests that creating an OrderedMap from an existing dictionary works correctly.
	/// </summary>
	[TestMethod]
	public void Constructor_FromDictionary_CopiesAndSortsElements()
	{
		var dictionary = new Dictionary<int, string>
		{
			[3] = "three",
			[1] = "one",
			[2] = "two"
		};

		var map = new OrderedMap<int, string>(dictionary);

		Assert.AreEqual(3, map.Count);
		int[] expectedKeys = [1, 2, 3];
		string[] expectedValues = ["one", "two", "three"];
		CollectionAssert.AreEqual(expectedKeys, map.Keys.ToArray());
		CollectionAssert.AreEqual(expectedValues, map.Values.ToArray());
	}

	/// <summary>
	/// Tests that adding elements maintains sorted order.
	/// </summary>
	[TestMethod]
	public void Add_MultipleElements_MaintainsSortedOrder()
	{
		var map = new OrderedMap<int, string>();

		map.Add(3, "three");
		map.Add(1, "one");
		map.Add(2, "two");

		int[] expectedKeys = [1, 2, 3];
		string[] expectedValues = ["one", "two", "three"];
		CollectionAssert.AreEqual(expectedKeys, map.Keys.ToArray());
		CollectionAssert.AreEqual(expectedValues, map.Values.ToArray());
	}

	/// <summary>
	/// Tests that adding a duplicate key throws an exception.
	/// </summary>
	[TestMethod]
	public void Add_DuplicateKey_ThrowsArgumentException()
	{
		var map = new OrderedMap<int, string>();
		map.Add(1, "one");

		Assert.ThrowsException<ArgumentException>(() => map.Add(1, "another one"));
	}

	/// <summary>
	/// Tests that adding a null key throws an exception.
	/// </summary>
	[TestMethod]
	public void Add_NullKey_ThrowsArgumentNullException()
	{
		var map = new OrderedMap<string, int>();

		Assert.ThrowsException<ArgumentNullException>(() => map.Add(null!, 1));
	}

	/// <summary>
	/// Tests that the indexer getter retrieves the correct value.
	/// </summary>
	[TestMethod]
	public void Indexer_Get_ReturnsCorrectValue()
	{
		var map = new OrderedMap<int, string>();
		map.Add(1, "one");
		map.Add(2, "two");

		Assert.AreEqual("one", map[1]);
		Assert.AreEqual("two", map[2]);
	}

	/// <summary>
	/// Tests that the indexer getter throws when key is not found.
	/// </summary>
	[TestMethod]
	public void Indexer_GetNonExistentKey_ThrowsKeyNotFoundException()
	{
		var map = new OrderedMap<int, string>();

		Assert.ThrowsException<KeyNotFoundException>(() => _ = map[1]);
	}

	/// <summary>
	/// Tests that the indexer setter updates existing values.
	/// </summary>
	[TestMethod]
	public void Indexer_SetExistingKey_UpdatesValue()
	{
		var map = new OrderedMap<int, string>();
		map.Add(1, "one");

		map[1] = "ONE";

		Assert.AreEqual("ONE", map[1]);
		Assert.AreEqual(1, map.Count);
	}

	/// <summary>
	/// Tests that the indexer setter adds new keys.
	/// </summary>
	[TestMethod]
	public void Indexer_SetNewKey_AddsKeyValuePair()
	{
		var map = new OrderedMap<int, string>();

		map[1] = "one";
		map[3] = "three";
		map[2] = "two";

		Assert.AreEqual(3, map.Count);
		int[] expectedKeys = [1, 2, 3];
		CollectionAssert.AreEqual(expectedKeys, map.Keys.ToArray());
	}

	/// <summary>
	/// Tests that ContainsKey returns true for existing keys.
	/// </summary>
	[TestMethod]
	public void ContainsKey_ExistingKey_ReturnsTrue()
	{
		var map = new OrderedMap<int, string>();
		map.Add(1, "one");

		Assert.IsTrue(map.ContainsKey(1));
	}

	/// <summary>
	/// Tests that ContainsKey returns false for non-existing keys.
	/// </summary>
	[TestMethod]
	public void ContainsKey_NonExistingKey_ReturnsFalse()
	{
		var map = new OrderedMap<int, string>();
		map.Add(1, "one");

		Assert.IsFalse(map.ContainsKey(2));
	}

	/// <summary>
	/// Tests that TryGetValue returns true and correct value for existing keys.
	/// </summary>
	[TestMethod]
	public void TryGetValue_ExistingKey_ReturnsTrueAndValue()
	{
		var map = new OrderedMap<int, string>();
		map.Add(1, "one");

		bool found = map.TryGetValue(1, out string? value);

		Assert.IsTrue(found);
		Assert.AreEqual("one", value);
	}

	/// <summary>
	/// Tests that TryGetValue returns false for non-existing keys.
	/// </summary>
	[TestMethod]
	public void TryGetValue_NonExistingKey_ReturnsFalseAndDefault()
	{
		var map = new OrderedMap<int, string>();

		bool found = map.TryGetValue(1, out string? value);

		Assert.IsFalse(found);
		Assert.IsNull(value);
	}

	/// <summary>
	/// Tests that removing an existing key works correctly.
	/// </summary>
	[TestMethod]
	public void Remove_ExistingKey_RemovesKeyAndReturnsTrue()
	{
		var map = new OrderedMap<int, string>();
		map.Add(1, "one");
		map.Add(2, "two");
		map.Add(3, "three");

		bool removed = map.Remove(2);

		Assert.IsTrue(removed);
		Assert.AreEqual(2, map.Count);
		Assert.IsFalse(map.ContainsKey(2));
		int[] expectedKeys = [1, 3];
		CollectionAssert.AreEqual(expectedKeys, map.Keys.ToArray());
	}

	/// <summary>
	/// Tests that removing a non-existing key returns false.
	/// </summary>
	[TestMethod]
	public void Remove_NonExistingKey_ReturnsFalse()
	{
		var map = new OrderedMap<int, string>();
		map.Add(1, "one");

		bool removed = map.Remove(2);

		Assert.IsFalse(removed);
		Assert.AreEqual(1, map.Count);
	}

	/// <summary>
	/// Tests that Clear removes all elements.
	/// </summary>
	[TestMethod]
	public void Clear_RemovesAllElements()
	{
		var map = new OrderedMap<int, string>();
		map.Add(1, "one");
		map.Add(2, "two");

		map.Clear();

		Assert.AreEqual(0, map.Count);
		Assert.IsFalse(map.ContainsKey(1));
		Assert.IsFalse(map.ContainsKey(2));
	}

	/// <summary>
	/// Tests that enumeration returns elements in sorted key order.
	/// </summary>
	[TestMethod]
	public void GetEnumerator_ReturnsElementsInSortedOrder()
	{
		var map = new OrderedMap<int, string>();
		map.Add(3, "three");
		map.Add(1, "one");
		map.Add(2, "two");

		var pairs = map.ToArray();

		Assert.AreEqual(3, pairs.Length);
		Assert.AreEqual(new KeyValuePair<int, string>(1, "one"), pairs[0]);
		Assert.AreEqual(new KeyValuePair<int, string>(2, "two"), pairs[1]);
		Assert.AreEqual(new KeyValuePair<int, string>(3, "three"), pairs[2]);
	}

	/// <summary>
	/// Tests that the Keys collection returns keys in sorted order.
	/// </summary>
	[TestMethod]
	public void Keys_ReturnsSortedKeys()
	{
		var map = new OrderedMap<int, string>();
		map.Add(3, "three");
		map.Add(1, "one");
		map.Add(2, "two");

		int[] expectedKeys = [1, 2, 3];
		CollectionAssert.AreEqual(expectedKeys, map.Keys.ToArray());
	}

	/// <summary>
	/// Tests that the Values collection returns values in order of sorted keys.
	/// </summary>
	[TestMethod]
	public void Values_ReturnsValuesInKeyOrder()
	{
		var map = new OrderedMap<int, string>();
		map.Add(3, "three");
		map.Add(1, "one");
		map.Add(2, "two");

		string[] expectedValues = ["one", "two", "three"];
		CollectionAssert.AreEqual(expectedValues, map.Values.ToArray());
	}

	/// <summary>
	/// Tests that Contains works correctly for key-value pairs.
	/// </summary>
	[TestMethod]
	public void Contains_KeyValuePair_WorksCorrectly()
	{
		var map = new OrderedMap<int, string>();
		map.Add(1, "one");
		map.Add(2, "two");

		Assert.IsTrue(map.Contains(new KeyValuePair<int, string>(1, "one")));
		Assert.IsFalse(map.Contains(new KeyValuePair<int, string>(1, "ONE")));
		Assert.IsFalse(map.Contains(new KeyValuePair<int, string>(3, "three")));
	}

	/// <summary>
	/// Tests that CopyTo works correctly.
	/// </summary>
	[TestMethod]
	public void CopyTo_CopiesElementsCorrectly()
	{
		var map = new OrderedMap<int, string>();
		map.Add(2, "two");
		map.Add(1, "one");

		var array = new KeyValuePair<int, string>[3];
		map.CopyTo(array, 1);

		Assert.AreEqual(default, array[0]);
		Assert.AreEqual(new KeyValuePair<int, string>(1, "one"), array[1]);
		Assert.AreEqual(new KeyValuePair<int, string>(2, "two"), array[2]);
	}

	/// <summary>
	/// Tests that removing a specific key-value pair works correctly.
	/// </summary>
	[TestMethod]
	public void Remove_KeyValuePair_WorksCorrectly()
	{
		var map = new OrderedMap<int, string>();
		map.Add(1, "one");
		map.Add(2, "two");

		bool removed1 = map.Remove(new KeyValuePair<int, string>(1, "one"));
		bool removed2 = map.Remove(new KeyValuePair<int, string>(2, "TWO"));

		Assert.IsTrue(removed1);
		Assert.IsFalse(removed2);
		Assert.AreEqual(1, map.Count);
		Assert.IsTrue(map.ContainsKey(2));
	}

	/// <summary>
	/// Tests that Clone creates a proper shallow copy.
	/// </summary>
	[TestMethod]
	public void Clone_CreatesShallowCopy()
	{
		var map = new OrderedMap<int, string>();
		map.Add(1, "one");
		map.Add(2, "two");

		var clone = map.Clone();

		Assert.AreEqual(map.Count, clone.Count);
		CollectionAssert.AreEqual(map.Keys.ToArray(), clone.Keys.ToArray());
		CollectionAssert.AreEqual(map.Values.ToArray(), clone.Values.ToArray());

		// Verify they are independent
		clone.Add(3, "three");
		Assert.AreNotEqual(map.Count, clone.Count);
	}

	/// <summary>
	/// Tests that the Keys collection is read-only.
	/// </summary>
	[TestMethod]
	public void Keys_IsReadOnly()
	{
		var map = new OrderedMap<int, string>();
		map.Add(1, "one");

		Assert.IsTrue(map.Keys.IsReadOnly);
		Assert.ThrowsException<NotSupportedException>(() => map.Keys.Add(2));
		Assert.ThrowsException<NotSupportedException>(() => map.Keys.Remove(1));
		Assert.ThrowsException<NotSupportedException>(() => map.Keys.Clear());
	}

	/// <summary>
	/// Tests that the Values collection is read-only.
	/// </summary>
	[TestMethod]
	public void Values_IsReadOnly()
	{
		var map = new OrderedMap<int, string>();
		map.Add(1, "one");

		Assert.IsTrue(map.Values.IsReadOnly);
		Assert.ThrowsException<NotSupportedException>(() => map.Values.Add("two"));
		Assert.ThrowsException<NotSupportedException>(() => map.Values.Remove("one"));
		Assert.ThrowsException<NotSupportedException>(() => map.Values.Clear());
	}

	/// <summary>
	/// Tests error handling for constructor with non-comparable type.
	/// </summary>
	[TestMethod]
	public void Constructor_NonComparableType_ThrowsArgumentException()
	{
		Assert.ThrowsException<ArgumentException>(() => new OrderedMap<object, string>());
	}

	/// <summary>
	/// Tests error handling for null comparer.
	/// </summary>
	[TestMethod]
	public void Constructor_NullComparer_ThrowsArgumentNullException()
	{
		Assert.ThrowsException<ArgumentNullException>(() => new OrderedMap<int, string>((IComparer<int>)null!));
	}

	/// <summary>
	/// Tests error handling for negative capacity.
	/// </summary>
	[TestMethod]
	public void Constructor_NegativeCapacity_ThrowsArgumentOutOfRangeException()
	{
		Assert.ThrowsException<ArgumentOutOfRangeException>(() => new OrderedMap<int, string>(-1));
	}

	/// <summary>
	/// Tests error handling for null dictionary.
	/// </summary>
	[TestMethod]
	public void Constructor_NullDictionary_ThrowsArgumentNullException()
	{
		Assert.ThrowsException<ArgumentNullException>(() => new OrderedMap<int, string>((IDictionary<int, string>)null!));
	}

	/// <summary>
	/// Tests that IEnumerable.GetEnumerator works correctly.
	/// </summary>
	[TestMethod]
	public void IEnumerable_GetEnumerator_WorksCorrectly()
	{
		var map = new OrderedMap<int, string>();
		map.Add(2, "two");
		map.Add(1, "one");

		var enumerable = (IEnumerable)map;
		var enumerator = enumerable.GetEnumerator();

		Assert.IsTrue(enumerator.MoveNext());
		Assert.AreEqual(new KeyValuePair<int, string>(1, "one"), enumerator.Current);

		Assert.IsTrue(enumerator.MoveNext());
		Assert.AreEqual(new KeyValuePair<int, string>(2, "two"), enumerator.Current);

		Assert.IsFalse(enumerator.MoveNext());
	}

	/// <summary>
	/// Tests large collection performance and correctness.
	/// </summary>
	[TestMethod]
	public void LargeCollection_MaintainsOrder()
	{
		var map = new OrderedMap<int, string>();
		var random = new Random(42);
		var numbers = Enumerable.Range(1, 1000).OrderBy(x => random.Next()).ToArray();

		foreach (int number in numbers)
		{
			map.Add(number, $"value{number}");
		}

		Assert.AreEqual(1000, map.Count);

		// Verify all elements are in sorted order
		var keys = map.Keys.ToArray();
		for (int i = 0; i < keys.Length; i++)
		{
			Assert.AreEqual(i + 1, keys[i]);
		}
	}
}
