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
		keys = [.. Enumerable.Range(1, ElementCount).OrderBy(x => random.Next())];
		values = [.. keys.Select(k => $"value{k}")];
		keyValuePairs = [.. keys.Zip(values, (k, v) => new KeyValuePair<int, string>(k, v))];
	}

	/// <summary>
	/// Benchmark for building an OrderedMap by adding elements one by one.
	/// </summary>
	[Benchmark]
	public OrderedMap<int, string> BuildOrderedMap()
	{
		OrderedMap<int, string> map = [];
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
		Dictionary<int, string> dict = [];
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
		SortedDictionary<int, string> sortedDict = [];
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
		Dictionary<int, string> dict = [];
		for (int i = 0; i < keys.Length; i++)
		{
			dict.Add(keys[i], values[i]);
		}
		return [.. dict];
	}

	/// <summary>
	/// Benchmark for key lookups in OrderedMap.
	/// </summary>
	[Benchmark]
	public string LookupOrderedMap()
	{
		OrderedMap<int, string> map = [];
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
		Dictionary<int, string> dict = [];
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
		SortedDictionary<int, string> sortedDict = [];
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
		OrderedMap<int, string> map = [];
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
		Dictionary<int, string> dict = [];
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
		SortedDictionary<int, string> sortedDict = [];
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
		OrderedMap<int, string> map = [];
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
		Dictionary<int, string> dict = [];
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
		SortedDictionary<int, string> sortedDict = [];
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
		OrderedMap<int, string> map = [];
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
		Dictionary<int, string> dict = [];
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
		SortedDictionary<int, string> sortedDict = [];
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
		OrderedMap<int, string> map = [];
		for (int i = 0; i < keys.Length; i++)
		{
			map.Add(keys[i], values[i]);
		}

		int count = 0;
		foreach (KeyValuePair<int, string> kvp in map)
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
		Dictionary<int, string> dict = [];
		for (int i = 0; i < keys.Length; i++)
		{
			dict.Add(keys[i], values[i]);
		}

		int count = 0;
		foreach (KeyValuePair<int, string> kvp in dict)
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
		SortedDictionary<int, string> sortedDict = [];
		for (int i = 0; i < keys.Length; i++)
		{
			sortedDict.Add(keys[i], values[i]);
		}

		int count = 0;
		foreach (KeyValuePair<int, string> kvp in sortedDict)
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
		OrderedMap<int, string> map = [];
		for (int i = 0; i < keys.Length; i++)
		{
			map.Add(keys[i], values[i]);
		}

		return [.. map.Keys];
	}

	/// <summary>
	/// Benchmark for accessing keys collection in Dictionary.
	/// </summary>
	[Benchmark]
	public int[] KeysDictionary()
	{
		Dictionary<int, string> dict = [];
		for (int i = 0; i < keys.Length; i++)
		{
			dict.Add(keys[i], values[i]);
		}

		return [.. dict.Keys];
	}

	/// <summary>
	/// Benchmark for accessing keys collection in SortedDictionary.
	/// </summary>
	[Benchmark]
	public int[] KeysSortedDictionary()
	{
		SortedDictionary<int, string> sortedDict = [];
		for (int i = 0; i < keys.Length; i++)
		{
			sortedDict.Add(keys[i], values[i]);
		}

		return [.. sortedDict.Keys];
	}

	/// <summary>
	/// Benchmark for accessing values collection in OrderedMap.
	/// </summary>
	[Benchmark]
	public string[] ValuesOrderedMap()
	{
		OrderedMap<int, string> map = [];
		for (int i = 0; i < keys.Length; i++)
		{
			map.Add(keys[i], values[i]);
		}

		return [.. map.Values];
	}

	/// <summary>
	/// Benchmark for accessing values collection in Dictionary.
	/// </summary>
	[Benchmark]
	public string[] ValuesDictionary()
	{
		Dictionary<int, string> dict = [];
		for (int i = 0; i < keys.Length; i++)
		{
			dict.Add(keys[i], values[i]);
		}

		return [.. dict.Values];
	}

	/// <summary>
	/// Benchmark for accessing values collection in SortedDictionary.
	/// </summary>
	[Benchmark]
	public string[] ValuesSortedDictionary()
	{
		SortedDictionary<int, string> sortedDict = [];
		for (int i = 0; i < keys.Length; i++)
		{
			sortedDict.Add(keys[i], values[i]);
		}

		return [.. sortedDict.Values];
	}
}
