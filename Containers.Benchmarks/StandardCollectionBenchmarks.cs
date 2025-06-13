// Copyright (c) ktsu.dev
// All rights reserved.
// Licensed under the MIT license.

namespace ktsu.Containers.Benchmarks;
using System.Collections.Concurrent;
using BenchmarkDotNet.Attributes;

/// <summary>
/// Baseline benchmarks for standard .NET collections to establish performance references.
/// </summary>
[MemoryDiagnoser]
[SimpleJob]
public class StandardCollectionBenchmarks
{
	private readonly Random random = new(42); // Fixed seed for reproducible results
	private int[] testData = [];
	private int[] searchData = [];
	private KeyValuePair<int, string>[] keyValueData = [];

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
		testData = new int[ElementCount];
		for (int i = 0; i < ElementCount; i++)
		{
			testData[i] = random.Next(0, ElementCount * 2);
		}

		// Generate search data (mix of existing and non-existing values)
		searchData = new int[100];
		for (int i = 0; i < 50; i++)
		{
			searchData[i] = testData[random.Next(testData.Length)]; // Existing values
		}
		for (int i = 50; i < 100; i++)
		{
			searchData[i] = random.Next(ElementCount * 2, ElementCount * 3); // Non-existing values
		}

		// Generate key-value data for dictionary benchmarks
		keyValueData = new KeyValuePair<int, string>[ElementCount];
		for (int i = 0; i < ElementCount; i++)
		{
			keyValueData[i] = new KeyValuePair<int, string>(testData[i], $"Value_{testData[i]}");
		}
	}

	// ================================
	// List<T> Benchmarks
	// ================================

	/// <summary>
	/// Benchmark adding elements to List.
	/// </summary>
	[Benchmark]
	public List<int> ListAdd()
	{
		List<int> list = [];
		foreach (int item in testData)
		{
			list.Add(item);
		}
		return list;
	}

	/// <summary>
	/// Benchmark List construction with known capacity.
	/// </summary>
	[Benchmark]
	public List<int> ListAddWithCapacity()
	{
		List<int> list = new(ElementCount);
		foreach (int item in testData)
		{
			list.Add(item);
		}
		return list;
	}

	/// <summary>
	/// Benchmark searching in List using Contains.
	/// </summary>
	[Benchmark]
	public int ListContains()
	{
		List<int> list = [.. testData];
		int found = 0;
		foreach (int item in searchData)
		{
			if (list.Contains(item))
			{
				found++;
			}
		}
		return found;
	}

	/// <summary>
	/// Benchmark List enumeration.
	/// </summary>
	[Benchmark]
	public int ListEnumerate()
	{
		List<int> list = [.. testData];
		int sum = 0;
		foreach (int item in list)
		{
			sum += item;
		}
		return sum;
	}

	/// <summary>
	/// Benchmark List index access.
	/// </summary>
	[Benchmark]
	public int ListIndexAccess()
	{
		List<int> list = [.. testData];
		int sum = 0;
		for (int i = 0; i < list.Count; i++)
		{
			sum += list[i];
		}
		return sum;
	}

	// ================================
	// HashSet<T> Benchmarks
	// ================================

	/// <summary>
	/// Benchmark adding elements to HashSet.
	/// </summary>
	[Benchmark]
	public HashSet<int> HashSetAdd()
	{
		HashSet<int> set = [];
		foreach (int item in testData)
		{
			set.Add(item);
		}
		return set;
	}

	/// <summary>
	/// Benchmark HashSet construction with known capacity.
	/// </summary>
	[Benchmark]
	public HashSet<int> HashSetAddWithCapacity()
	{
		HashSet<int> set = new(ElementCount);
		foreach (int item in testData)
		{
			set.Add(item);
		}
		return set;
	}

	/// <summary>
	/// Benchmark searching in HashSet using Contains.
	/// </summary>
	[Benchmark]
	public int HashSetContains()
	{
		HashSet<int> set = [.. testData];
		int found = 0;
		foreach (int item in searchData)
		{
			if (set.Contains(item))
			{
				found++;
			}
		}
		return found;
	}

	/// <summary>
	/// Benchmark HashSet enumeration.
	/// </summary>
	[Benchmark]
	public int HashSetEnumerate()
	{
		HashSet<int> set = [.. testData];
		int sum = 0;
		foreach (int item in set)
		{
			sum += item;
		}
		return sum;
	}

	// ================================
	// SortedSet<T> Benchmarks
	// ================================

	/// <summary>
	/// Benchmark adding elements to SortedSet.
	/// </summary>
	[Benchmark]
	public SortedSet<int> SortedSetAdd()
	{
		SortedSet<int> set = [];
		foreach (int item in testData)
		{
			set.Add(item);
		}
		return set;
	}

	/// <summary>
	/// Benchmark searching in SortedSet using Contains.
	/// </summary>
	[Benchmark]
	public int SortedSetContains()
	{
		SortedSet<int> set = [.. testData];
		int found = 0;
		foreach (int item in searchData)
		{
			if (set.Contains(item))
			{
				found++;
			}
		}
		return found;
	}

	/// <summary>
	/// Benchmark SortedSet enumeration.
	/// </summary>
	[Benchmark]
	public int SortedSetEnumerate()
	{
		SortedSet<int> set = [.. testData];
		int sum = 0;
		foreach (int item in set)
		{
			sum += item;
		}
		return sum;
	}

	// ================================
	// Dictionary<TKey, TValue> Benchmarks
	// ================================

	/// <summary>
	/// Benchmark adding elements to Dictionary.
	/// </summary>
	[Benchmark]
	public Dictionary<int, string> DictionaryAdd()
	{
		Dictionary<int, string> dict = [];
		foreach (KeyValuePair<int, string> kvp in keyValueData)
		{
			dict.TryAdd(kvp.Key, kvp.Value);
		}
		return dict;
	}

	/// <summary>
	/// Benchmark Dictionary construction with known capacity.
	/// </summary>
	[Benchmark]
	public Dictionary<int, string> DictionaryAddWithCapacity()
	{
		Dictionary<int, string> dict = new(ElementCount);
		foreach (KeyValuePair<int, string> kvp in keyValueData)
		{
			dict.TryAdd(kvp.Key, kvp.Value);
		}
		return dict;
	}

	/// <summary>
	/// Benchmark searching in Dictionary using ContainsKey.
	/// </summary>
	[Benchmark]
	public int DictionaryContainsKey()
	{
		Dictionary<int, string> dict = [];
		foreach (KeyValuePair<int, string> kvp in keyValueData)
		{
			dict.TryAdd(kvp.Key, kvp.Value);
		}

		int found = 0;
		foreach (int key in searchData)
		{
			if (dict.ContainsKey(key))
			{
				found++;
			}
		}
		return found;
	}

	/// <summary>
	/// Benchmark Dictionary enumeration.
	/// </summary>
	[Benchmark]
	public int DictionaryEnumerate()
	{
		Dictionary<int, string> dict = [];
		foreach (KeyValuePair<int, string> kvp in keyValueData)
		{
			dict.TryAdd(kvp.Key, kvp.Value);
		}

		int sum = 0;
		foreach (KeyValuePair<int, string> kvp in dict)
		{
			sum += kvp.Key;
		}
		return sum;
	}

	// ================================
	// SortedDictionary<TKey, TValue> Benchmarks
	// ================================

	/// <summary>
	/// Benchmark adding elements to SortedDictionary.
	/// </summary>
	[Benchmark]
	public SortedDictionary<int, string> SortedDictionaryAdd()
	{
		SortedDictionary<int, string> dict = [];
		foreach (KeyValuePair<int, string> kvp in keyValueData)
		{
			dict.TryAdd(kvp.Key, kvp.Value);
		}
		return dict;
	}

	/// <summary>
	/// Benchmark searching in SortedDictionary using ContainsKey.
	/// </summary>
	[Benchmark]
	public int SortedDictionaryContainsKey()
	{
		SortedDictionary<int, string> dict = [];
		foreach (KeyValuePair<int, string> kvp in keyValueData)
		{
			dict.TryAdd(kvp.Key, kvp.Value);
		}

		int found = 0;
		foreach (int key in searchData)
		{
			if (dict.ContainsKey(key))
			{
				found++;
			}
		}
		return found;
	}

	/// <summary>
	/// Benchmark SortedDictionary enumeration.
	/// </summary>
	[Benchmark]
	public int SortedDictionaryEnumerate()
	{
		SortedDictionary<int, string> dict = [];
		foreach (KeyValuePair<int, string> kvp in keyValueData)
		{
			dict.TryAdd(kvp.Key, kvp.Value);
		}

		int sum = 0;
		foreach (KeyValuePair<int, string> kvp in dict)
		{
			sum += kvp.Key;
		}
		return sum;
	}

	// ================================
	// Queue<T> Benchmarks
	// ================================

	/// <summary>
	/// Benchmark adding elements to Queue.
	/// </summary>
	[Benchmark]
	public Queue<int> QueueAdd()
	{
		Queue<int> queue = new();
		foreach (int item in testData)
		{
			queue.Enqueue(item);
		}
		return queue;
	}

	/// <summary>
	/// Benchmark Queue enumeration.
	/// </summary>
	[Benchmark]
	public int QueueEnumerate()
	{
		Queue<int> queue = new(testData);
		int sum = 0;
		foreach (int item in queue)
		{
			sum += item;
		}
		return sum;
	}

	/// <summary>
	/// Benchmark Queue dequeue operations.
	/// </summary>
	[Benchmark]
	public int QueueDequeue()
	{
		Queue<int> queue = new(testData);
		int sum = 0;
		while (queue.Count > 0)
		{
			sum += queue.Dequeue();
		}
		return sum;
	}

	// ================================
	// Stack<T> Benchmarks
	// ================================

	/// <summary>
	/// Benchmark adding elements to Stack.
	/// </summary>
	[Benchmark]
	public Stack<int> StackAdd()
	{
		Stack<int> stack = new();
		foreach (int item in testData)
		{
			stack.Push(item);
		}
		return stack;
	}

	/// <summary>
	/// Benchmark Stack enumeration.
	/// </summary>
	[Benchmark]
	public int StackEnumerate()
	{
		Stack<int> stack = new(testData);
		int sum = 0;
		foreach (int item in stack)
		{
			sum += item;
		}
		return sum;
	}

	/// <summary>
	/// Benchmark Stack pop operations.
	/// </summary>
	[Benchmark]
	public int StackPop()
	{
		Stack<int> stack = new(testData);
		int sum = 0;
		while (stack.Count > 0)
		{
			sum += stack.Pop();
		}
		return sum;
	}

	// ================================
	// ConcurrentDictionary<TKey, TValue> Benchmarks
	// ================================

	/// <summary>
	/// Benchmark adding elements to ConcurrentDictionary.
	/// </summary>
	[Benchmark]
	public ConcurrentDictionary<int, string> ConcurrentDictionaryAdd()
	{
		ConcurrentDictionary<int, string> dict = new();
		foreach (KeyValuePair<int, string> kvp in keyValueData)
		{
			dict.TryAdd(kvp.Key, kvp.Value);
		}
		return dict;
	}

	/// <summary>
	/// Benchmark searching in ConcurrentDictionary using ContainsKey.
	/// </summary>
	[Benchmark]
	public int ConcurrentDictionaryContainsKey()
	{
		ConcurrentDictionary<int, string> dict = new();
		foreach (KeyValuePair<int, string> kvp in keyValueData)
		{
			dict.TryAdd(kvp.Key, kvp.Value);
		}

		int found = 0;
		foreach (int key in searchData)
		{
			if (dict.ContainsKey(key))
			{
				found++;
			}
		}
		return found;
	}

	/// <summary>
	/// Benchmark ConcurrentDictionary enumeration.
	/// </summary>
	[Benchmark]
	public int ConcurrentDictionaryEnumerate()
	{
		ConcurrentDictionary<int, string> dict = new();
		foreach (KeyValuePair<int, string> kvp in keyValueData)
		{
			dict.TryAdd(kvp.Key, kvp.Value);
		}

		int sum = 0;
		foreach (KeyValuePair<int, string> kvp in dict)
		{
			sum += kvp.Key;
		}
		return sum;
	}

	// ================================
	// ConcurrentQueue<T> Benchmarks
	// ================================

	/// <summary>
	/// Benchmark adding elements to ConcurrentQueue.
	/// </summary>
	[Benchmark]
	public ConcurrentQueue<int> ConcurrentQueueAdd()
	{
		ConcurrentQueue<int> queue = new();
		foreach (int item in testData)
		{
			queue.Enqueue(item);
		}
		return queue;
	}

	/// <summary>
	/// Benchmark ConcurrentQueue enumeration.
	/// </summary>
	[Benchmark]
	public int ConcurrentQueueEnumerate()
	{
		ConcurrentQueue<int> queue = new();
		foreach (int item in testData)
		{
			queue.Enqueue(item);
		}

		int sum = 0;
		foreach (int item in queue)
		{
			sum += item;
		}
		return sum;
	}

	/// <summary>
	/// Benchmark ConcurrentQueue dequeue operations.
	/// </summary>
	[Benchmark]
	public int ConcurrentQueueDequeue()
	{
		ConcurrentQueue<int> queue = new();
		foreach (int item in testData)
		{
			queue.Enqueue(item);
		}

		int sum = 0;
		while (queue.TryDequeue(out int item))
		{
			sum += item;
		}
		return sum;
	}
}
