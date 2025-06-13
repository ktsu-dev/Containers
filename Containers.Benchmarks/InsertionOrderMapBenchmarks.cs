// Copyright (c) ktsu.dev
// All rights reserved.
// Licensed under the MIT license.

namespace ktsu.Containers.Benchmarks;
using BenchmarkDotNet.Attributes;
using ktsu.Containers;

/// <summary>
/// Benchmarks for InsertionOrderMap performance compared to built-in collections.
/// </summary>
[MemoryDiagnoser]
[SimpleJob]
public class InsertionOrderMapBenchmarks
{
	private readonly Random random = new(42); // Fixed seed for reproducible results
	private KeyValuePair<int, string>[] testData = [];
	private int[] searchKeys = [];

	/// <summary>
	/// Gets or sets the number of elements to use in benchmarks.
	/// </summary>
	[Params(100, 1000, 10000)]
	public int ElementCount { get; set; }

	/// <summary>
	/// Setup method called before each benchmark iteration.
	/// </summary>
	[GlobalSetup]
	public void Setup()
	{
		// Generate test data
		testData = new KeyValuePair<int, string>[ElementCount];
		for (int i = 0; i < ElementCount; i++)
		{
			int key = random.Next(0, ElementCount / 2); // Creates some duplicate keys
			string value = $"Value_{key}_{i}";
			testData[i] = new KeyValuePair<int, string>(key, value);
		}

		// Generate search keys (mix of existing and non-existing keys)
		searchKeys = new int[100];
		for (int i = 0; i < 50; i++)
		{
			searchKeys[i] = testData[random.Next(testData.Length)].Key; // Existing keys
		}
		for (int i = 50; i < 100; i++)
		{
			searchKeys[i] = random.Next(ElementCount, ElementCount * 2); // Non-existing keys
		}
	}

	/// <summary>
	/// Benchmark adding elements to InsertionOrderMap.
	/// </summary>
	[Benchmark]
	public InsertionOrderMap<int, string> InsertionOrderMap_Add()
	{
		InsertionOrderMap<int, string> map = [];
		foreach (KeyValuePair<int, string> item in testData)
		{
			map.TryAdd(item.Key, item.Value);
		}
		return map;
	}

	/// <summary>
	/// Benchmark adding elements to Dictionary.
	/// </summary>
	[Benchmark]
	public Dictionary<int, string> Dictionary_Add()
	{
		Dictionary<int, string> dict = [];
		foreach (KeyValuePair<int, string> item in testData)
		{
			dict.TryAdd(item.Key, item.Value);
		}
		return dict;
	}

	/// <summary>
	/// Benchmark adding elements to SortedDictionary.
	/// </summary>
	[Benchmark]
	public SortedDictionary<int, string> SortedDictionary_Add()
	{
		SortedDictionary<int, string> sortedDict = [];
		foreach (KeyValuePair<int, string> item in testData)
		{
			sortedDict.TryAdd(item.Key, item.Value);
		}
		return sortedDict;
	}

	/// <summary>
	/// Benchmark searching in InsertionOrderMap using ContainsKey.
	/// </summary>
	[Benchmark]
	public int InsertionOrderMap_ContainsKey()
	{
		InsertionOrderMap<int, string> map = [];
		foreach (KeyValuePair<int, string> item in testData)
		{
			map.TryAdd(item.Key, item.Value);
		}

		int found = 0;
		foreach (int key in searchKeys.Where(map.ContainsKey))
		{
			found++;
		}
		return found;
	}

	/// <summary>
	/// Benchmark searching in Dictionary using ContainsKey.
	/// </summary>
	[Benchmark]
	public int Dictionary_ContainsKey()
	{
		Dictionary<int, string> dict = [];
		foreach (KeyValuePair<int, string> item in testData)
		{
			dict.TryAdd(item.Key, item.Value);
		}

		int found = 0;
		foreach (int key in searchKeys.Where(dict.ContainsKey))
		{
			found++;
		}
		return found;
	}

	/// <summary>
	/// Benchmark searching in SortedDictionary using ContainsKey.
	/// </summary>
	[Benchmark]
	public int SortedDictionary_ContainsKey()
	{
		SortedDictionary<int, string> sortedDict = [];
		foreach (KeyValuePair<int, string> item in testData)
		{
			sortedDict.TryAdd(item.Key, item.Value);
		}

		int found = 0;
		foreach (int key in searchKeys.Where(sortedDict.ContainsKey))
		{
			found++;
		}
		return found;
	}

	/// <summary>
	/// Benchmark enumeration of InsertionOrderMap.
	/// </summary>
	[Benchmark]
	public int InsertionOrderMap_Enumerate()
	{
		InsertionOrderMap<int, string> map = [];
		foreach (KeyValuePair<int, string> item in testData)
		{
			map.TryAdd(item.Key, item.Value);
		}

		int sum = 0;
		foreach (KeyValuePair<int, string> kvp in map)
		{
			sum += kvp.Key;
		}
		return sum;
	}

	/// <summary>
	/// Benchmark enumeration of Dictionary.
	/// </summary>
	[Benchmark]
	public int Dictionary_Enumerate()
	{
		Dictionary<int, string> dict = [];
		foreach (KeyValuePair<int, string> item in testData)
		{
			dict.TryAdd(item.Key, item.Value);
		}

		int sum = 0;
		foreach (KeyValuePair<int, string> kvp in dict)
		{
			sum += kvp.Key;
		}
		return sum;
	}

	/// <summary>
	/// Benchmark removing elements from InsertionOrderMap.
	/// </summary>
	[Benchmark]
	public InsertionOrderMap<int, string> InsertionOrderMap_Remove()
	{
		InsertionOrderMap<int, string> map = [];
		foreach (KeyValuePair<int, string> item in testData)
		{
			map.TryAdd(item.Key, item.Value);
		}

		// Remove every 10th unique key
		int[] keysToRemove = [.. map.Keys.Where((_, index) => index % 10 == 0)];
		foreach (int key in keysToRemove)
		{
			map.Remove(key);
		}

		return map;
	}

	/// <summary>
	/// Benchmark removing elements from Dictionary.
	/// </summary>
	[Benchmark]
	public Dictionary<int, string> Dictionary_Remove()
	{
		Dictionary<int, string> dict = [];
		foreach (KeyValuePair<int, string> item in testData)
		{
			dict.TryAdd(item.Key, item.Value);
		}

		// Remove every 10th unique key
		int[] keysToRemove = [.. dict.Keys.Where((_, index) => index % 10 == 0)];
		foreach (int key in keysToRemove)
		{
			dict.Remove(key);
		}

		return dict;
	}

	/// <summary>
	/// Benchmark mixed operations (add, remove, search) on InsertionOrderMap.
	/// </summary>
	[Benchmark]
	public InsertionOrderMap<int, string> InsertionOrderMap_MixedOperations()
	{
		InsertionOrderMap<int, string> map = [];

		// Add elements
		foreach (KeyValuePair<int, string> item in testData[..^100])
		{
			map.TryAdd(item.Key, item.Value);
		}

		// Remove some elements
		int[] keysToRemove = [.. map.Keys.Take(10)];
		foreach (int key in keysToRemove)
		{
			map.Remove(key);
		}

		// Add more elements
		foreach (KeyValuePair<int, string> item in testData[^100..])
		{
			map.TryAdd(item.Key, item.Value);
		}

		return map;
	}

	/// <summary>
	/// Benchmark mixed operations (add, remove, search) on Dictionary.
	/// </summary>
	[Benchmark]
	public Dictionary<int, string> Dictionary_MixedOperations()
	{
		Dictionary<int, string> dict = [];

		// Add elements
		foreach (KeyValuePair<int, string> item in testData[..^100])
		{
			dict.TryAdd(item.Key, item.Value);
		}

		// Remove some elements
		int[] keysToRemove = [.. dict.Keys.Take(10)];
		foreach (int key in keysToRemove)
		{
			dict.Remove(key);
		}

		// Add more elements
		foreach (KeyValuePair<int, string> item in testData[^100..])
		{
			dict.TryAdd(item.Key, item.Value);
		}

		return dict;
	}
}
