// Copyright (c) 2023-2026 ktsu-dev contributors

namespace ktsu.Containers.Tests;

using System.Collections;
using Microsoft.VisualStudio.TestTools.UnitTesting;

/// <summary>
/// A null key reports the parameter's own name, as <see cref="Dictionary{TKey, TValue}"/> does,
/// not the text of the cast that checks it (ktsu-dev/Containers#146).
/// </summary>
[TestClass]
public class NullKeyParamNameTests
{
	private static void AssertParamName(string expected, Action action)
	{
		ArgumentNullException exception = Assert.ThrowsExactly<ArgumentNullException>(action);
		Assert.AreEqual(expected, exception.ParamName);
	}

	private static void AssertParamName(string expected, Func<object?> func) =>
		AssertParamName(expected, () => { _ = func(); });

	private static bool KeysContain(ICollection<string> keys) => keys.Contains(null!);

	[TestMethod]
	public void ContiguousMap_NullKey_ReportsParameterName()
	{
		ContiguousMap<string, int> map = new() { ["a"] = 1 };

		AssertParamName("key", () => map[null!]);
		AssertParamName("key", () => map[null!] = 1);
		AssertParamName("key", () => map.Add(null!, 1));
		AssertParamName("key", () => map.Remove(null!));
		AssertParamName("key", () => map.ContainsKey(null!));
		AssertParamName("key", () => map.TryGetValue(null!, out _));
		AssertParamName("item", () => KeysContain(map.Keys));
	}

	[TestMethod]
	public void InsertionOrderMap_NullKey_ReportsParameterName()
	{
		InsertionOrderMap<string, int> map = new() { ["a"] = 1 };

		AssertParamName("key", () => map[null!]);
		AssertParamName("key", () => map[null!] = 1);
		AssertParamName("key", () => map.Add(null!, 1));
		AssertParamName("key", () => map.Remove(null!));
		AssertParamName("key", () => map.ContainsKey(null!));
		AssertParamName("key", () => map.TryGetValue(null!, out _));
		AssertParamName("item", () => KeysContain(map.Keys));
	}

	[TestMethod]
	public void OrderedMap_NullKey_ReportsParameterName()
	{
		OrderedMap<string, int> map = new() { ["a"] = 1 };

		AssertParamName("key", () => map[null!]);
		AssertParamName("key", () => map[null!] = 1);
		AssertParamName("key", () => map.Add(null!, 1));
		AssertParamName("key", () => map.Remove(null!));
		AssertParamName("key", () => map.ContainsKey(null!));
		AssertParamName("key", () => map.TryGetValue(null!, out _));
		AssertParamName("item", () => map.Contains(new KeyValuePair<string, int>(null!, 1)));
		AssertParamName("item", () => map.Remove(new KeyValuePair<string, int>(null!, 1)));
		AssertParamName("item", () => KeysContain(map.Keys));
	}

	[TestMethod]
	public void OrderedMap_DictionaryConstructorWithNullKey_ReportsDictionary()
	{
		NullKeyDictionary source = new();

		AssertParamName("dictionary", () => new OrderedMap<string, int>(source));
		AssertParamName("dictionary", () => new OrderedMap<string, int>(source, StringComparer.Ordinal));
	}

	/// <summary>
	/// A dictionary that enumerates one pair with a null key, which <see cref="Dictionary{TKey, TValue}"/> can't hold.
	/// </summary>
	private sealed class NullKeyDictionary : IDictionary<string, int>
	{
		private readonly List<KeyValuePair<string, int>> pairs = [new(null!, 1)];

		public int this[string key] { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
		public ICollection<string> Keys => throw new NotSupportedException();
		public ICollection<int> Values => throw new NotSupportedException();
		public int Count => pairs.Count;
		public bool IsReadOnly => true;
		public void Add(string key, int value) => throw new NotSupportedException();
		public void Add(KeyValuePair<string, int> item) => throw new NotSupportedException();
		public void Clear() => throw new NotSupportedException();
		public bool Contains(KeyValuePair<string, int> item) => throw new NotSupportedException();
		public bool ContainsKey(string key) => throw new NotSupportedException();
		public void CopyTo(KeyValuePair<string, int>[] array, int arrayIndex) => pairs.CopyTo(array, arrayIndex);
		public IEnumerator<KeyValuePair<string, int>> GetEnumerator() => pairs.GetEnumerator();
		public bool Remove(string key) => throw new NotSupportedException();
		public bool Remove(KeyValuePair<string, int> item) => throw new NotSupportedException();
		public bool TryGetValue(string key, out int value) => throw new NotSupportedException();
		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	}
}
