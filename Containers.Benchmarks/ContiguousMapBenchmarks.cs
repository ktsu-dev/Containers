// Copyright (c) ktsu.dev
// All rights reserved.
// Licensed under the MIT license.

namespace ktsu.Containers.Benchmarks;
using BenchmarkDotNet.Attributes;
using ktsu.Containers;

/// <summary>
/// Benchmarks for ContiguousMap performance compared to built-in collections.
/// Focus on key-value pairs with contiguous memory layout for optimal cache performance.
/// </summary>
[MemoryDiagnoser]
[SimpleJob]
public class ContiguousMapBenchmarks
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
	/// Benchmark adding elements to ContiguousMap.
	/// </summary>
	[Benchmark]
	public ContiguousMap<int, string> ContiguousMap_Add()
	{
		ContiguousMap<int, string> map = [];
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
	/// Benchmark searching in ContiguousMap using ContainsKey (cache-friendly linear search).
	/// </summary>
	[Benchmark]
	public int ContiguousMap_ContainsKey()
	{
		ContiguousMap<int, string> map = [];
		foreach (KeyValuePair<int, string> item in testData)
		{
			map.TryAdd(item.Key, item.Value);
		}

		int found = 0;
		foreach (int key in searchKeys)
		{
			if (map.ContainsKey(key))
			{
				found++;
			}
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
		foreach (int key in searchKeys)
		{
			if (dict.ContainsKey(key))
			{
				found++;
			}
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
		foreach (int key in searchKeys)
		{
			if (sortedDict.ContainsKey(key))
			{
				found++;
			}
		}
		return found;
	}

	/// <summary>
	/// Benchmark enumeration of ContiguousMap (optimal cache access).
	/// </summary>
	[Benchmark]
	public int ContiguousMap_Enumerate()
	{
		ContiguousMap<int, string> map = [];
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
	/// Benchmark span access in ContiguousMap for keys (zero-copy operations).
	/// </summary>
	[Benchmark]
	public int ContiguousMap_SpanAccess()
	{
		ContiguousMap<int, string> map = [];
		foreach (KeyValuePair<int, string> item in testData)
		{
			map.TryAdd(item.Key, item.Value);
		}

		ReadOnlySpan<int> keysSpan = map.GetKeysSpan();
		int sum = 0;
		for (int i = 0; i < keysSpan.Length; i++)
		{
			sum += keysSpan[i];
		}
		return sum;
	}

	/// <summary>
	/// Benchmark removing elements from ContiguousMap.
	/// </summary>
	[Benchmark]
	public ContiguousMap<int, string> ContiguousMap_Remove()
	{
		ContiguousMap<int, string> map = [];
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
	/// Benchmark memory-intensive operations showing contiguous memory benefits.
	/// </summary>
	[Benchmark]
	public long ContiguousMap_MemoryIntensiveOp()
	{
		ContiguousMap<int, string> map = [];
		foreach (KeyValuePair<int, string> item in testData)
		{
			map.TryAdd(item.Key, item.Value);
		}

		long result = 0;

		// Simulate cache-friendly sequential access pattern
		for (int pass = 0; pass < 3; pass++)
		{
			foreach (KeyValuePair<int, string> kvp in map)
			{
				result += kvp.Key * (pass + 1);
			}
		}

		return result;
	}

	/// <summary>
	/// Benchmark memory-intensive operations on Dictionary.
	/// </summary>
	[Benchmark]
	public long Dictionary_MemoryIntensiveOp()
	{
		Dictionary<int, string> dict = [];
		foreach (KeyValuePair<int, string> item in testData)
		{
			dict.TryAdd(item.Key, item.Value);
		}

		long result = 0;

		// Simulate access pattern (less cache-friendly)
		for (int pass = 0; pass < 3; pass++)
		{
			foreach (KeyValuePair<int, string> kvp in dict)
			{
				result += kvp.Key * (pass + 1);
			}
		}

		return result;
	}

	/// <summary>
	/// Benchmark value retrieval operations in ContiguousMap.
	/// </summary>
	[Benchmark]
	public int ContiguousMap_ValueRetrieval()
	{
		ContiguousMap<int, string> map = [];
		foreach (KeyValuePair<int, string> item in testData)
		{
			map.TryAdd(item.Key, item.Value);
		}

		int retrieved = 0;
		foreach (int key in searchKeys)
		{
			if (map.TryGetValue(key, out string? value) && value != null)
			{
				retrieved++;
			}
		}
		return retrieved;
	}

	/// <summary>
	/// Benchmark value retrieval operations in Dictionary.
	/// </summary>
	[Benchmark]
	public int Dictionary_ValueRetrieval()
	{
		Dictionary<int, string> dict = [];
		foreach (KeyValuePair<int, string> item in testData)
		{
			dict.TryAdd(item.Key, item.Value);
		}

		int retrieved = 0;
		foreach (int key in searchKeys)
		{
			if (dict.TryGetValue(key, out string? value) && value != null)
			{
				retrieved++;
			}
		}
		return retrieved;
	}

	/// <summary>
	/// Benchmark mixed operations (add, remove, search) on ContiguousMap.
	/// </summary>
	[Benchmark]
	public ContiguousMap<int, string> ContiguousMap_MixedOperations()
	{
		ContiguousMap<int, string> map = [];

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
