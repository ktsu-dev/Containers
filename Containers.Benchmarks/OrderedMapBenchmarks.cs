// Copyright (c) ktsu.dev
// All rights reserved.
// Licensed under the MIT license.

namespace ktsu.Containers.Benchmarks;

using BenchmarkDotNet.Attributes;

/// <summary>
/// Benchmarks for OrderedMap operations comparing against standard .NET collections.
/// </summary>
[MemoryDiagnoser]
[SimpleJob]
public class OrderedMapBenchmarks
{
	private readonly Random random = new(42);
	private int[] keys = [];
	private string[] values = [];
	private KeyValuePair<int, string>[] keyValuePairs = [];

	/// <summary>
	/// Gets or sets the number of elements to use in benchmarks.
	/// </summary>
	[Params(100, 1000, 10000)]
	public int ElementCount { get; set; }

	/// <summary>
	/// Sets up test data before running benchmarks.
	/// </summary>
	[GlobalSetup]
	public void Setup()
	{
		keys = Enumerable.Range(1, ElementCount).OrderBy(x => random.Next()).ToArray();
		values = keys.Select(k => $"value{k}").ToArray();
		keyValuePairs = keys.Zip(values, (k, v) => new KeyValuePair<int, string>(k, v)).ToArray();
	}

	/// <summary>
	/// Benchmark for building an OrderedMap by adding elements one by one.
	/// </summary>
	[Benchmark]
	public OrderedMap<int, string> BuildOrderedMap()
	{
		var map = new OrderedMap<int, string>();
		for (int i = 0; i < keys.Length; i++)
		{
			map.Add(keys[i], values[i]);
		}
		return map;
	}

	/// <summary>
	/// Benchmark for building a Dictionary by adding elements one by one.
	/// </summary>
	[Benchmark]
	public Dictionary<int, string> BuildDictionary()
	{
		var dict = new Dictionary<int, string>();
		for (int i = 0; i < keys.Length; i++)
		{
			dict.Add(keys[i], values[i]);
		}
		return dict;
	}

	/// <summary>
	/// Benchmark for building a SortedDictionary by adding elements one by one.
	/// </summary>
	[Benchmark]
	public SortedDictionary<int, string> BuildSortedDictionary()
	{
		var sortedDict = new SortedDictionary<int, string>();
		for (int i = 0; i < keys.Length; i++)
		{
			sortedDict.Add(keys[i], values[i]);
		}
		return sortedDict;
	}

	/// <summary>
	/// Benchmark for creating an OrderedMap from an existing dictionary.
	/// </summary>
	[Benchmark]
	public OrderedMap<int, string> BuildOrderedMapFromDictionary()
	{
		var dict = new Dictionary<int, string>();
		for (int i = 0; i < keys.Length; i++)
		{
			dict.Add(keys[i], values[i]);
		}
		return new OrderedMap<int, string>(dict);
	}

	/// <summary>
	/// Benchmark for key lookups in OrderedMap.
	/// </summary>
	[Benchmark]
	public string LookupOrderedMap()
	{
		var map = new OrderedMap<int, string>();
		for (int i = 0; i < keys.Length; i++)
		{
			map.Add(keys[i], values[i]);
		}

		string result = "";
		for (int i = 0; i < keys.Length; i++)
		{
			result = map[keys[i]];
		}
		return result;
	}

	/// <summary>
	/// Benchmark for key lookups in Dictionary.
	/// </summary>
	[Benchmark]
	public string LookupDictionary()
	{
		var dict = new Dictionary<int, string>();
		for (int i = 0; i < keys.Length; i++)
		{
			dict.Add(keys[i], values[i]);
		}

		string result = "";
		for (int i = 0; i < keys.Length; i++)
		{
			result = dict[keys[i]];
		}
		return result;
	}

	/// <summary>
	/// Benchmark for key lookups in SortedDictionary.
	/// </summary>
	[Benchmark]
	public string LookupSortedDictionary()
	{
		var sortedDict = new SortedDictionary<int, string>();
		for (int i = 0; i < keys.Length; i++)
		{
			sortedDict.Add(keys[i], values[i]);
		}

		string result = "";
		for (int i = 0; i < keys.Length; i++)
		{
			result = sortedDict[keys[i]];
		}
		return result;
	}

	/// <summary>
	/// Benchmark for TryGetValue operations in OrderedMap.
	/// </summary>
	[Benchmark]
	public int TryGetValueOrderedMap()
	{
		var map = new OrderedMap<int, string>();
		for (int i = 0; i < keys.Length; i++)
		{
			map.Add(keys[i], values[i]);
		}

		int foundCount = 0;
		for (int i = 0; i < keys.Length; i++)
		{
			if (map.TryGetValue(keys[i], out _))
			{
				foundCount++;
			}
		}
		return foundCount;
	}

	/// <summary>
	/// Benchmark for TryGetValue operations in Dictionary.
	/// </summary>
	[Benchmark]
	public int TryGetValueDictionary()
	{
		var dict = new Dictionary<int, string>();
		for (int i = 0; i < keys.Length; i++)
		{
			dict.Add(keys[i], values[i]);
		}

		int foundCount = 0;
		for (int i = 0; i < keys.Length; i++)
		{
			if (dict.TryGetValue(keys[i], out _))
			{
				foundCount++;
			}
		}
		return foundCount;
	}

	/// <summary>
	/// Benchmark for TryGetValue operations in SortedDictionary.
	/// </summary>
	[Benchmark]
	public int TryGetValueSortedDictionary()
	{
		var sortedDict = new SortedDictionary<int, string>();
		for (int i = 0; i < keys.Length; i++)
		{
			sortedDict.Add(keys[i], values[i]);
		}

		int foundCount = 0;
		for (int i = 0; i < keys.Length; i++)
		{
			if (sortedDict.TryGetValue(keys[i], out _))
			{
				foundCount++;
			}
		}
		return foundCount;
	}

	/// <summary>
	/// Benchmark for ContainsKey operations in OrderedMap.
	/// </summary>
	[Benchmark]
	public int ContainsKeyOrderedMap()
	{
		var map = new OrderedMap<int, string>();
		for (int i = 0; i < keys.Length; i++)
		{
			map.Add(keys[i], values[i]);
		}

		int foundCount = 0;
		for (int i = 0; i < keys.Length; i++)
		{
			if (map.ContainsKey(keys[i]))
			{
				foundCount++;
			}
		}
		return foundCount;
	}

	/// <summary>
	/// Benchmark for ContainsKey operations in Dictionary.
	/// </summary>
	[Benchmark]
	public int ContainsKeyDictionary()
	{
		var dict = new Dictionary<int, string>();
		for (int i = 0; i < keys.Length; i++)
		{
			dict.Add(keys[i], values[i]);
		}

		int foundCount = 0;
		for (int i = 0; i < keys.Length; i++)
		{
			if (dict.ContainsKey(keys[i]))
			{
				foundCount++;
			}
		}
		return foundCount;
	}

	/// <summary>
	/// Benchmark for ContainsKey operations in SortedDictionary.
	/// </summary>
	[Benchmark]
	public int ContainsKeySortedDictionary()
	{
		var sortedDict = new SortedDictionary<int, string>();
		for (int i = 0; i < keys.Length; i++)
		{
			sortedDict.Add(keys[i], values[i]);
		}

		int foundCount = 0;
		for (int i = 0; i < keys.Length; i++)
		{
			if (sortedDict.ContainsKey(keys[i]))
			{
				foundCount++;
			}
		}
		return foundCount;
	}

	/// <summary>
	/// Benchmark for removing elements from OrderedMap.
	/// </summary>
	[Benchmark]
	public int RemoveOrderedMap()
	{
		var map = new OrderedMap<int, string>();
		for (int i = 0; i < keys.Length; i++)
		{
			map.Add(keys[i], values[i]);
		}

		int removedCount = 0;
		for (int i = 0; i < keys.Length / 2; i++)
		{
			if (map.Remove(keys[i]))
			{
				removedCount++;
			}
		}
		return removedCount;
	}

	/// <summary>
	/// Benchmark for removing elements from Dictionary.
	/// </summary>
	[Benchmark]
	public int RemoveDictionary()
	{
		var dict = new Dictionary<int, string>();
		for (int i = 0; i < keys.Length; i++)
		{
			dict.Add(keys[i], values[i]);
		}

		int removedCount = 0;
		for (int i = 0; i < keys.Length / 2; i++)
		{
			if (dict.Remove(keys[i]))
			{
				removedCount++;
			}
		}
		return removedCount;
	}

	/// <summary>
	/// Benchmark for removing elements from SortedDictionary.
	/// </summary>
	[Benchmark]
	public int RemoveSortedDictionary()
	{
		var sortedDict = new SortedDictionary<int, string>();
		for (int i = 0; i < keys.Length; i++)
		{
			sortedDict.Add(keys[i], values[i]);
		}

		int removedCount = 0;
		for (int i = 0; i < keys.Length / 2; i++)
		{
			if (sortedDict.Remove(keys[i]))
			{
				removedCount++;
			}
		}
		return removedCount;
	}

	/// <summary>
	/// Benchmark for enumerating all key-value pairs in OrderedMap.
	/// </summary>
	[Benchmark]
	public int EnumerateOrderedMap()
	{
		var map = new OrderedMap<int, string>();
		for (int i = 0; i < keys.Length; i++)
		{
			map.Add(keys[i], values[i]);
		}

		int count = 0;
		foreach (var kvp in map)
		{
			count++;
		}
		return count;
	}

	/// <summary>
	/// Benchmark for enumerating all key-value pairs in Dictionary.
	/// </summary>
	[Benchmark]
	public int EnumerateDictionary()
	{
		var dict = new Dictionary<int, string>();
		for (int i = 0; i < keys.Length; i++)
		{
			dict.Add(keys[i], values[i]);
		}

		int count = 0;
		foreach (var kvp in dict)
		{
			count++;
		}
		return count;
	}

	/// <summary>
	/// Benchmark for enumerating all key-value pairs in SortedDictionary.
	/// </summary>
	[Benchmark]
	public int EnumerateSortedDictionary()
	{
		var sortedDict = new SortedDictionary<int, string>();
		for (int i = 0; i < keys.Length; i++)
		{
			sortedDict.Add(keys[i], values[i]);
		}

		int count = 0;
		foreach (var kvp in sortedDict)
		{
			count++;
		}
		return count;
	}

	/// <summary>
	/// Benchmark for accessing keys collection in OrderedMap.
	/// </summary>
	[Benchmark]
	public int[] KeysOrderedMap()
	{
		var map = new OrderedMap<int, string>();
		for (int i = 0; i < keys.Length; i++)
		{
			map.Add(keys[i], values[i]);
		}

		return map.Keys.ToArray();
	}

	/// <summary>
	/// Benchmark for accessing keys collection in Dictionary.
	/// </summary>
	[Benchmark]
	public int[] KeysDictionary()
	{
		var dict = new Dictionary<int, string>();
		for (int i = 0; i < keys.Length; i++)
		{
			dict.Add(keys[i], values[i]);
		}

		return dict.Keys.ToArray();
	}

	/// <summary>
	/// Benchmark for accessing keys collection in SortedDictionary.
	/// </summary>
	[Benchmark]
	public int[] KeysSortedDictionary()
	{
		var sortedDict = new SortedDictionary<int, string>();
		for (int i = 0; i < keys.Length; i++)
		{
			sortedDict.Add(keys[i], values[i]);
		}

		return sortedDict.Keys.ToArray();
	}

	/// <summary>
	/// Benchmark for accessing values collection in OrderedMap.
	/// </summary>
	[Benchmark]
	public string[] ValuesOrderedMap()
	{
		var map = new OrderedMap<int, string>();
		for (int i = 0; i < keys.Length; i++)
		{
			map.Add(keys[i], values[i]);
		}

		return map.Values.ToArray();
	}

	/// <summary>
	/// Benchmark for accessing values collection in Dictionary.
	/// </summary>
	[Benchmark]
	public string[] ValuesDictionary()
	{
		var dict = new Dictionary<int, string>();
		for (int i = 0; i < keys.Length; i++)
		{
			dict.Add(keys[i], values[i]);
		}

		return dict.Values.ToArray();
	}

	/// <summary>
	/// Benchmark for accessing values collection in SortedDictionary.
	/// </summary>
	[Benchmark]
	public string[] ValuesSortedDictionary()
	{
		var sortedDict = new SortedDictionary<int, string>();
		for (int i = 0; i < keys.Length; i++)
		{
			sortedDict.Add(keys[i], values[i]);
		}

		return sortedDict.Values.ToArray();
	}
}
