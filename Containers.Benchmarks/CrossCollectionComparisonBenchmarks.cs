// Copyright (c) ktsu.dev
// All rights reserved.
// Licensed under the MIT license.

namespace ktsu.Containers.Benchmarks;
using BenchmarkDotNet.Attributes;
using ktsu.Containers;

/// <summary>
/// Direct performance comparisons between ktsu.Containers implementations and standard .NET collections.
/// </summary>
[MemoryDiagnoser]
[SimpleJob]
public class CrossCollectionComparisonBenchmarks
{
	private readonly Random random = new(42); // Fixed seed for reproducible results
	private int[] testData = [];
	private int[] searchData = [];
	private int[] unionData = [];

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

		// Generate union data for set operations
		HashSet<int> unionValues = [];
		while (unionValues.Count < ElementCount / 2)
		{
			unionValues.Add(random.Next(ElementCount, ElementCount * 3));
		}
		unionData = [.. unionValues];
	}

	// ================================
	// Ordered Collection Scenarios
	// ================================

	/// <summary>
	/// Benchmark building an ordered collection - OrderedCollection approach.
	/// </summary>
	[Benchmark]
	public OrderedCollection<int> BuildOrderedCollectionIncremental()
	{
		OrderedCollection<int> collection = [.. testData];
		return collection;
	}

	/// <summary>
	/// Benchmark building an ordered collection - List + Sort approach.
	/// </summary>
	[Benchmark]
	public List<int> BuildOrderedCollectionBulkSort()
	{
		List<int> list = [.. testData];
		list.Sort();
		return list;
	}

	/// <summary>
	/// Benchmark building an ordered collection - SortedList approach.
	/// </summary>
	[Benchmark]
	public SortedList<int, int> BuildOrderedCollectionSortedList()
	{
		SortedList<int, int> sortedList = [];
		foreach (int item in testData)
		{
			sortedList.TryAdd(item, item);
		}
		return sortedList;
	}

	/// <summary>
	/// Benchmark search in ordered collections - OrderedCollection approach.
	/// </summary>
	[Benchmark]
	public int SearchOrderedCollectionBinary()
	{
		OrderedCollection<int> collection = [.. testData];
		int found = 0;
		foreach (int item in searchData)
		{
			if (collection.Contains(item))
			{
				found++;
			}
		}
		return found;
	}

	/// <summary>
	/// Benchmark search in ordered collections - List + BinarySearch approach.
	/// </summary>
	[Benchmark]
	public int SearchOrderedCollectionListBinary()
	{
		List<int> list = [.. testData];
		list.Sort();
		int found = 0;
		foreach (int item in searchData)
		{
			if (list.BinarySearch(item) >= 0)
			{
				found++;
			}
		}
		return found;
	}

	// ================================
	// Set Operations Scenarios
	// ================================

	/// <summary>
	/// Benchmark building a unique ordered set - OrderedSet approach.
	/// </summary>
	[Benchmark]
	public OrderedSet<int> BuildUniqueSetOrdered()
	{
		OrderedSet<int> set = [.. testData];
		return set;
	}

	/// <summary>
	/// Benchmark building a unique set - HashSet approach.
	/// </summary>
	[Benchmark]
	public HashSet<int> BuildUniqueSetHash()
	{
		HashSet<int> set = [];
		foreach (int item in testData)
		{
			set.Add(item);
		}
		return set;
	}

	/// <summary>
	/// Benchmark building a unique ordered set - SortedSet approach.
	/// </summary>
	[Benchmark]
	public SortedSet<int> BuildUniqueSetSorted()
	{
		SortedSet<int> set = [];
		foreach (int item in testData)
		{
			set.Add(item);
		}
		return set;
	}

	/// <summary>
	/// Benchmark set union operations - OrderedSet approach.
	/// </summary>
	[Benchmark]
	public OrderedSet<int> SetUnionOrdered()
	{
		OrderedSet<int> set1 = [.. testData];
		OrderedSet<int> set2 = [.. unionData];
		set1.UnionWith(set2);
		return set1;
	}

	/// <summary>
	/// Benchmark set union operations - HashSet approach.
	/// </summary>
	[Benchmark]
	public HashSet<int> SetUnionHash()
	{
		HashSet<int> set1 = [.. testData];
		HashSet<int> set2 = [.. unionData];
		set1.UnionWith(set2);
		return set1;
	}

	/// <summary>
	/// Benchmark set union operations - SortedSet approach.
	/// </summary>
	[Benchmark]
	public SortedSet<int> SetUnionSorted()
	{
		SortedSet<int> set1 = [.. testData];
		SortedSet<int> set2 = [.. unionData];
		set1.UnionWith(set2);
		return set1;
	}

	// ================================
	// Circular Buffer Scenarios
	// ================================

	/// <summary>
	/// Benchmark continuous data streaming - RingBuffer approach.
	/// </summary>
	[Benchmark]
	public RingBuffer<int> StreamingDataRingBuffer()
	{
		RingBuffer<int> buffer = new(ElementCount / 4); // Smaller buffer to force wrapping

		// Simulate continuous streaming
		foreach (int item in testData)
		{
			buffer.PushBack(item);
		}

		return buffer;
	}

	/// <summary>
	/// Benchmark continuous data streaming - Queue with manual size limiting.
	/// </summary>
	[Benchmark]
	public Queue<int> StreamingDataQueue()
	{
		Queue<int> queue = new();
		int maxSize = ElementCount / 4; // Same size as RingBuffer

		// Simulate continuous streaming with size limiting
		foreach (int item in testData)
		{
			queue.Enqueue(item);
			if (queue.Count > maxSize)
			{
				queue.Dequeue();
			}
		}

		return queue;
	}

	/// <summary>
	/// Benchmark continuous data streaming - List with manual size limiting.
	/// </summary>
	[Benchmark]
	public List<int> StreamingDataList()
	{
		List<int> list = [];
		int maxSize = ElementCount / 4; // Same size as RingBuffer

		// Simulate continuous streaming with size limiting (expensive!)
		foreach (int item in testData)
		{
			list.Add(item);
			if (list.Count > maxSize)
			{
				list.RemoveAt(0);
			}
		}

		return list;
	}

	/// <summary>
	/// Benchmark index-based access in circular buffer - RingBuffer approach.
	/// </summary>
	[Benchmark]
	public int IndexAccessRingBuffer()
	{
		RingBuffer<int> buffer = new(testData.Take(ElementCount / 2), ElementCount / 2);
		int sum = 0;
		for (int i = 0; i < buffer.Count; i++)
		{
			sum += buffer[i];
		}
		return sum;
	}

	/// <summary>
	/// Benchmark index-based access in queue - requires conversion to array.
	/// </summary>
	[Benchmark]
	public int IndexAccessQueue()
	{
		Queue<int> queue = new(testData.Take(ElementCount / 2));
		int[] array = [.. queue]; // Convert to array for index access
		int sum = 0;
		for (int i = 0; i < array.Length; i++)
		{
			sum += array[i];
		}
		return sum;
	}

	// ================================
	// Memory Efficiency Scenarios
	// ================================

	/// <summary>
	/// Benchmark memory-efficient frequent additions - OrderedCollection.
	/// </summary>
	[Benchmark]
	public OrderedCollection<int> MemoryEfficientOrderedCollection()
	{
		OrderedCollection<int> collection = new(ElementCount); // Pre-allocate capacity
		foreach (int item in testData)
		{
			collection.Add(item);
		}
		return collection;
	}

	/// <summary>
	/// Benchmark memory-efficient frequent additions - List with capacity.
	/// </summary>
	[Benchmark]
	public List<int> MemoryEfficientList()
	{
		List<int> list = new(ElementCount); // Pre-allocate capacity
		foreach (int item in testData)
		{
			list.Add(item);
		}
		list.Sort(); // Sort at the end
		return list;
	}

	/// <summary>
	/// Benchmark memory-efficient set operations - OrderedSet with capacity.
	/// </summary>
	[Benchmark]
	public OrderedSet<int> MemoryEfficientOrderedSet()
	{
		OrderedSet<int> set = new(ElementCount); // Pre-allocate capacity
		foreach (int item in testData)
		{
			set.Add(item);
		}
		return set;
	}

	/// <summary>
	/// Benchmark memory-efficient set operations - HashSet with capacity.
	/// </summary>
	[Benchmark]
	public HashSet<int> MemoryEfficientHashSet()
	{
		HashSet<int> set = new(ElementCount); // Pre-allocate capacity
		foreach (int item in testData)
		{
			set.Add(item);
		}
		return set;
	}

	// ================================
	// Real-World Usage Patterns
	// ================================

	/// <summary>
	/// Benchmark typical data processing workflow - build, search, modify.
	/// </summary>
	[Benchmark]
	public OrderedCollection<int> DataProcessingWorkflowOrderedCollection()
	{
		// Build collection
		OrderedCollection<int> collection = [.. testData.Take(ElementCount / 2)];

		// Search operations
		int found = 0;
		foreach (int item in searchData.Take(10))
		{
			if (collection.Contains(item))
			{
				found++;
			}
		}

		// Add more data
		foreach (int item in testData.Skip(ElementCount / 2))
		{
			collection.Add(item);
		}

		// Remove some items
		for (int i = 0; i < testData.Length; i += 20)
		{
			collection.Remove(testData[i]);
		}

		return collection;
	}

	/// <summary>
	/// Benchmark typical data processing workflow with List + manual sorting.
	/// </summary>
	[Benchmark]
	public List<int> DataProcessingWorkflowList()
	{
		// Build collection
		List<int> list = [];
		foreach (int item in testData.Take(ElementCount / 2))
		{
			list.Add(item);
		}
		list.Sort();

		// Search operations
		int found = 0;
		foreach (int item in searchData.Take(10))
		{
			if (list.BinarySearch(item) >= 0)
			{
				found++;
			}
		}

		// Add more data
		foreach (int item in testData.Skip(ElementCount / 2))
		{
			list.Add(item);
		}
		list.Sort(); // Re-sort after additions

		// Remove some items
		for (int i = 0; i < testData.Length; i += 20)
		{
			list.Remove(testData[i]);
		}

		return list;
	}

	/// <summary>
	/// Benchmark cache-like usage pattern with RingBuffer.
	/// </summary>
	[Benchmark]
	public RingBuffer<int> CacheUsagePatternRingBuffer()
	{
		RingBuffer<int> cache = new(100); // Fixed-size cache

		// Simulate cache access patterns
		foreach (int item in testData)
		{
			cache.PushBack(item);

			// Simulate occasional access to recent items
			if (cache.Count > 10 && random.Next(5) == 0)
			{
				int index = random.Next(Math.Min(cache.Count, 10));
				int value = cache[index]; // Access recent item
			}
		}

		return cache;
	}

	/// <summary>
	/// Benchmark cache-like usage pattern with Queue.
	/// </summary>
	[Benchmark]
	public Queue<int> CacheUsagePatternQueue()
	{
		Queue<int> cache = new();
		int maxSize = 100; // Fixed-size cache

		// Simulate cache access patterns
		foreach (int item in testData)
		{
			cache.Enqueue(item);
			if (cache.Count > maxSize)
			{
				cache.Dequeue();
			}

			// Simulate occasional access to items (requires enumeration)
			if (cache.Count > 10 && random.Next(5) == 0)
			{
				int count = 0;
				foreach (int value in cache)
				{
					if (count >= 10)
					{
						break; // Only check first 10 items
					}

					count++;
				}
			}
		}

		return cache;
	}
}
