// Copyright (c) 2023-2026 ktsu-dev contributors

namespace ktsu.Containers.Tests;

using System.Runtime.CompilerServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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
		Assert.IsFalse(map.IsReadOnly, "Map should not be read-only");
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
			new(2, "two"),
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
		Assert.IsTrue(map.ContainsKey(5), "Map should contain added key");
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
		Assert.IsTrue(result, "TryAdd should return true for new key");
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
		Assert.IsFalse(result, "TryAdd should return false for duplicate key");
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
	[System.Diagnostics.CodeAnalysis.SuppressMessage(
		"Major Bug",
		"S4143:Collection elements should not be replaced unconditionally",
		Justification = "<Pending>"
	)]
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
	public void Indexer_SetExistingKeyWithDifferentSpelling_KeepsOriginalKey()
	{
		// Arrange
		ContiguousMap<string, int> map = new(StringComparer.OrdinalIgnoreCase) { ["Apple"] = 1 };

		// Act
		map["APPLE"] = 2;

		// Assert
		Assert.AreSequenceEqual(["Apple"], map.Keys);
		Assert.AreEqual("Apple", map.AsSpan()[0].Key);
		Assert.AreEqual(new KeyValuePair<string, int>("Apple", 2), map.Single());
		Assert.AreEqual(2, map["apple"]);
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
		Assert.IsTrue(map.ContainsKey(1), "Map should contain key 1");
		Assert.IsTrue(map.ContainsKey(2), "Map should contain key 2");
	}

	[TestMethod]
	public void ContainsKey_NonExistingKey_ReturnsFalse()
	{
		// Arrange
		ContiguousMap<int, string> map = [];
		map.Add(1, "one");

		// Act & Assert
		Assert.IsFalse(map.ContainsKey(2), "Map should not contain key 2");
		Assert.IsFalse(map.ContainsKey(0), "Map should not contain key 0");
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
		Assert.IsTrue(result, "TryGetValue should return true for existing key");
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
		Assert.IsFalse(result, "TryGetValue should return false for non-existing key");
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
		Assert.IsTrue(result, "Remove should return true for existing key");
		Assert.AreEqual(2, map.Count);
		Assert.IsFalse(map.ContainsKey(2), "Map should not contain removed key");
		Assert.IsTrue(map.ContainsKey(1), "Map should still contain key 1");
		Assert.IsTrue(map.ContainsKey(3), "Map should still contain key 3");
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
		Assert.IsFalse(result, "Remove should return false for non-existing key");
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
		Assert.IsFalse(map.ContainsKey(1), "Map should not contain key 1 after clear");
		Assert.IsFalse(map.ContainsKey(2), "Map should not contain key 2 after clear");
		Assert.IsFalse(map.ContainsKey(3), "Map should not contain key 3 after clear");
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
		Assert.HasCount(4, keys);
		Assert.Contains(1, keys);
		Assert.Contains(2, keys);
		Assert.Contains(3, keys);
		Assert.Contains(4, keys);
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
		Assert.HasCount(4, values);
		Assert.Contains("one", values);
		Assert.Contains("two", values);
		Assert.Contains("three", values);
		Assert.Contains("four", values);
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
		Assert.HasCount(4, enumerated);
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
		Assert.IsTrue(map.Contains(new KeyValuePair<int, string>(1, "one")), "Map should contain key-value pair (1, 'one')");
		Assert.IsTrue(map.Contains(new KeyValuePair<int, string>(2, "two")), "Map should contain key-value pair (2, 'two')");
	}

	[TestMethod]
	public void Contains_NonExistingKeyValuePair_ReturnsFalse()
	{
		// Arrange
		ContiguousMap<int, string> map = [];
		map.Add(1, "one");

		// Act & Assert
		Assert.IsFalse(map.Contains(new KeyValuePair<int, string>(1, "ONE")), "Map should not contain key-value pair with wrong value");
		Assert.IsFalse(map.Contains(new KeyValuePair<int, string>(2, "two")), "Map should not contain key-value pair with wrong key");
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
		Assert.Contains(new KeyValuePair<int, string>(1, "one"), copied);
		Assert.Contains(new KeyValuePair<int, string>(3, "three"), copied);

		Assert.AreEqual(default, array[3]); // Unchanged
	}

	[TestMethod]
	public void CopyKeysTo_FillsDestinationInInsertionOrder()
	{
		ContiguousMap<int, string> map = new() { { 3, "three" }, { 1, "one" }, { 4, "four" }, { 2, "two" } };
		int[] destination = new int[4];

		map.CopyKeysTo(destination);

		int[] expected = [3, 1, 4, 2];
		Assert.AreSequenceEqual(expected, destination);
	}

	[TestMethod]
	public void CopyValuesTo_FillsDestinationInInsertionOrder()
	{
		ContiguousMap<int, string> map = new() { { 3, "three" }, { 1, "one" }, { 4, "four" }, { 2, "two" } };
		string[] destination = new string[4];

		map.CopyValuesTo(destination);

		string[] expected = ["three", "one", "four", "two"];
		Assert.AreSequenceEqual(expected, destination);
	}

	[TestMethod]
	public void CopyKeysTo_LongerDestination_LeavesTailUntouched()
	{
		ContiguousMap<int, string> map = new() { { 7, "seven" }, { 8, "eight" } };
		int[] destination = [-1, -1, -1, -1];

		map.CopyKeysTo(destination);

		int[] expected = [7, 8, -1, -1];
		Assert.AreSequenceEqual(expected, destination);
	}

	[TestMethod]
	public void CopyKeysTo_ShorterDestination_ThrowsWithoutWriting()
	{
		ContiguousMap<int, string> map = new() { { 7, "seven" }, { 8, "eight" } };
		int[] destination = [-1];

		ArgumentException exception = Assert.ThrowsExactly<ArgumentException>(() => map.CopyKeysTo(destination));

		Assert.AreEqual("destination", exception.ParamName);
		Assert.AreEqual(-1, destination[0]);
	}

	[TestMethod]
	public void CopyValuesTo_ShorterDestination_ThrowsWithoutWriting()
	{
		ContiguousMap<int, string> map = new() { { 7, "seven" }, { 8, "eight" } };
		string[] destination = ["untouched"];

		ArgumentException exception = Assert.ThrowsExactly<ArgumentException>(() => map.CopyValuesTo(destination));

		Assert.AreEqual("destination", exception.ParamName);
		Assert.AreEqual("untouched", destination[0]);
	}

	[TestMethod]
	public void CopyKeysAndValuesTo_DoNotAllocate()
	{
		ContiguousMap<int, int> map = [];
		for (int i = 0; i < 1_000; i++)
		{
			map.Add(i, i * 2);
		}

		int[] keys = new int[map.Count];
		int[] values = new int[map.Count];
		map.CopyKeysTo(keys);
		map.CopyValuesTo(values);

		long before = GC.GetAllocatedBytesForCurrentThread();
		map.CopyKeysTo(keys);
		map.CopyValuesTo(values);
		long allocated = GC.GetAllocatedBytesForCurrentThread() - before;

		Assert.AreEqual(0, allocated);
		Assert.AreEqual(999, keys[999]);
		Assert.AreEqual(1_998, values[999]);
	}

	[TestMethod]
	[DataRow(nameof(ContiguousMap<,>.GetKeysSpan), nameof(ContiguousMap<,>.CopyKeysTo))]
	[DataRow(nameof(ContiguousMap<,>.GetValuesSpan), nameof(ContiguousMap<,>.CopyValuesTo))]
	public void CopyingSpanAccessors_AreObsoleteAndPointAtTheirReplacement(string method, string replacement)
	{
		// The *Span names read as zero-copy views, but every call allocates and copies the whole map
		ObsoleteAttribute? obsolete = typeof(ContiguousMap<int, int>).GetMethod(method)!
			.GetCustomAttributes(typeof(ObsoleteAttribute), inherit: false)
			.Cast<ObsoleteAttribute>()
			.SingleOrDefault();

		Assert.IsNotNull(obsolete, $"{method} should be marked [Obsolete]");
		Assert.Contains(replacement, obsolete.Message!);
		Assert.Contains("AsReadOnlySpan", obsolete.Message!);
		Assert.IsFalse(obsolete.IsError, "Obsoleting must not break existing callers");
	}

#pragma warning disable CS0618 // GetKeysSpan/GetValuesSpan are obsolete but still supported
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
		Assert.HasCount(4, keysSpan);

		// Verify all keys are in the span
		HashSet<int> spanKeys = [];
		for (int i = 0; i < keysSpan.Length; i++)
		{
			spanKeys.Add(keysSpan[i]);
		}
		Assert.HasCount(4, spanKeys);
		Assert.Contains(1, spanKeys);
		Assert.Contains(2, spanKeys);
		Assert.Contains(3, spanKeys);
		Assert.Contains(4, spanKeys);
	}
#pragma warning restore CS0618

#pragma warning disable CS0618 // GetKeysSpan/GetValuesSpan are obsolete but still supported
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
		Assert.HasCount(4, valuesSpan);

		// Verify all values are in the span
		HashSet<string> spanValues = [];
		for (int i = 0; i < valuesSpan.Length; i++)
		{
			spanValues.Add(valuesSpan[i]);
		}
		Assert.HasCount(4, spanValues);
		Assert.Contains("one", spanValues);
		Assert.Contains("two", spanValues);
		Assert.Contains("three", spanValues);
		Assert.Contains("four", spanValues);
	}
#pragma warning restore CS0618

#pragma warning disable CS0618 // GetKeysSpan/GetValuesSpan are obsolete but still supported
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
			Assert.IsTrue(map.ContainsKey(i), $"Map should contain key {i}");
			Assert.AreEqual($"value_{i}", map[i]);
		}

		// Test span access (only possible with contiguous memory)
		ReadOnlySpan<int> keysSpan = map.GetKeysSpan();
		ReadOnlySpan<string> valuesSpan = map.GetValuesSpan();
		Assert.HasCount(1000, keysSpan);
		Assert.HasCount(1000, valuesSpan);

		// Verify all keys are in the span
		HashSet<int> spanKeys = [];
		for (int i = 0; i < keysSpan.Length; i++)
		{
			spanKeys.Add(keysSpan[i]);
		}
		Assert.HasCount(1000, spanKeys);
	}
#pragma warning restore CS0618

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
		Assert.IsTrue(map.TryGetValue(1, out string? value), "TryGetValue should return true for key with null value");
		Assert.IsNull(value);
	}

	[TestMethod]
	public void Entry_Equals_SameKeyAndValue_ReturnsTrue()
	{
		// Arrange
		ContiguousMap<int, string>.Entry left = new(1, "one");
		ContiguousMap<int, string>.Entry right = new(1, "one");

		// Act & Assert
		Assert.IsTrue(left.Equals(right), "Entries with the same key and value should be equal");
		Assert.IsTrue(left.Equals((object)right), "Equals(object) should agree with Equals(Entry)");
		Assert.IsTrue(left == right, "operator == should report equality");
		Assert.IsFalse(left != right, "operator != should report equality");
		Assert.AreEqual(left.GetHashCode(), right.GetHashCode());
	}

	[TestMethod]
	public void Entry_Equals_DifferentKeyOrValue_ReturnsFalse()
	{
		// Arrange
		ContiguousMap<int, string>.Entry entry = new(1, "one");
		ContiguousMap<int, string>.Entry differentKey = new(2, "one");
		ContiguousMap<int, string>.Entry differentValue = new(1, "two");

		// Act & Assert
		Assert.IsFalse(entry.Equals(differentKey), "Entries with different keys should not be equal");
		Assert.IsFalse(entry.Equals(differentValue), "Entries with different values should not be equal");
		Assert.IsTrue(entry != differentKey, "operator != should report inequality");
		Assert.IsFalse(entry == differentValue, "operator == should report inequality");
	}

	[TestMethod]
	public void Entry_Equals_NonEntryObject_ReturnsFalse()
	{
		// Arrange
		ContiguousMap<int, string>.Entry entry = new(1, "one");

		// Act & Assert
		Assert.IsFalse(entry.Equals(null), "An entry should not equal null");
		Assert.IsFalse(entry.Equals("not an entry"), "An entry should not equal an unrelated type");
	}

	[TestMethod]
	public void Entry_KeyAndValue_ExposeConstructorArguments()
	{
		// Arrange & Act
		ContiguousMap<int, string>.Entry entry = new(7, "seven");

		// Assert
		Assert.AreEqual(7, entry.Key);
		Assert.AreEqual("seven", entry.Value);
	}

	[TestMethod]
	public void Enumerate_RemovingDuringForeach_Throws() =>
		MapEnumerationAssertions.RemovingDuringForeachThrows(() => new ContiguousMap<int, string>());

	[TestMethod]
	public void Enumerate_ChangesMadeThroughEveryMutator_Throw() =>
		MapEnumerationAssertions.EveryMutatorInvalidatesEnumerators(() => new ContiguousMap<int, string>());

	[TestMethod]
	public void Enumerate_Unchanged_VisitsEveryEntry() =>
		MapEnumerationAssertions.UnchangedEnumerationVisitsEveryEntry(() => new ContiguousMap<int, string>());

	[TestMethod]
	public void AsSpan_IsReadOnly_SoKeysCannotBypassTheKeyIndex()
	{
		// A writable span would let a caller overwrite entry "a" with key "b", after which map["a"] read the new value.
		Type returnType = typeof(ContiguousMap<string, int>).GetMethod(nameof(ContiguousMap<,>.AsSpan))!.ReturnType;

		Assert.AreEqual(typeof(ReadOnlySpan<ContiguousMap<string, int>.Entry>), returnType);
	}

	[TestMethod]
	public void GetValueRefOrNullRef_ExistingKey_UpdatesValueInPlace()
	{
		ContiguousMap<string, int> map = new() { ["a"] = 1, ["b"] = 2 };

		ref int value = ref map.GetValueRefOrNullRef("a");
		value = 99;

		Assert.AreEqual(99, map["a"]);
		Assert.AreEqual(2, map["b"]);
		Assert.AreSequenceEqual(["a", "b"], map.Keys);
		Assert.AreEqual(99, map.AsSpan()[0].Value);
	}

	[TestMethod]
	public void GetValueRefOrNullRef_ExistingKey_KeepsKeyIndexConsistent()
	{
		ContiguousMap<string, int> map = new(StringComparer.OrdinalIgnoreCase) { ["Apple"] = 1 };

		map.GetValueRefOrNullRef("APPLE") += 41;

		Assert.AreEqual(42, map["apple"]);
		Assert.AreSequenceEqual(["Apple"], map.Keys);
		Assert.IsTrue(map.Remove("Apple"));
		Assert.IsEmpty(map);
	}

	[TestMethod]
	public void GetValueRefOrNullRef_MissingKey_ReturnsNullRef()
	{
		ContiguousMap<string, int> map = new() { ["a"] = 1 };

		Assert.IsTrue(Unsafe.IsNullRef(ref map.GetValueRefOrNullRef("z")));
		Assert.IsFalse(Unsafe.IsNullRef(ref map.GetValueRefOrNullRef("a")));
	}

	[TestMethod]
	public void GetValueRefOrNullRef_NullKey_ThrowsArgumentNullException()
	{
		ContiguousMap<string, int> map = [];

		Assert.ThrowsExactly<ArgumentNullException>(() => map.GetValueRefOrNullRef(null!));
	}

	private const int LargeCount = 1_000_000;

	[TestMethod]
	public void EnsureCapacity_ThenAdds_AllocatesAboutAsMuchAsTheCapacityConstructor()
	{
		long presized = AllocatedBy(() =>
		{
			ContiguousMap<int, int> map = new(LargeCount);
			for (int i = 0; i < LargeCount; i++)
			{
				map.Add(i, i);
			}
		});
		long ensured = AllocatedBy(() =>
		{
			ContiguousMap<int, int> map = [];
			map.EnsureCapacity(LargeCount);
			for (int i = 0; i < LargeCount; i++)
			{
				map.Add(i, i);
			}
		});

		// Without presizing the hash index, EnsureCapacity allocates about twice as much (it rehashes ~18 times).
		Assert.IsLessThan(presized * 1.25, ensured, $"EnsureCapacity path allocated {ensured} bytes, capacity constructor {presized}");
	}

	[TestMethod]
	[DoNotParallelize]
	public void TrimExcess_AfterClear_ReleasesTheKeyIndex()
	{
		ContiguousMap<int, int> map = [];
		for (int i = 0; i < LargeCount; i++)
		{
			map.Add(i, i);
		}

		map.Clear();

		long before = GC.GetTotalMemory(forceFullCollection: true);
		map.TrimExcess();
		long after = GC.GetTotalMemory(forceFullCollection: true);

		// The entry array alone is ~8 MB; the Dictionary index for 1M ints is ~25 MB more.
		Assert.IsGreaterThan(20_000_000L, before - after, $"TrimExcess released only {before - after} bytes");
		Assert.IsEmpty(map);
		map.Add(1, 2);
		Assert.AreEqual(2, map[1]);
		GC.KeepAlive(map);
	}

	private static long AllocatedBy(Action action)
	{
		long start = GC.GetAllocatedBytesForCurrentThread();
		action();
		return GC.GetAllocatedBytesForCurrentThread() - start;
	}
}
