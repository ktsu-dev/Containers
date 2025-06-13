// Copyright (c) ktsu.dev
// All rights reserved.
// Licensed under the MIT license.

namespace ktsu.Containers.Benchmarks;
using BenchmarkDotNet.Attributes;
using ktsu.Containers;

/// <summary>
/// Specialized performance analysis benchmarks focusing on scalability, edge cases, and advanced scenarios.
/// </summary>
[MemoryDiagnoser]
[SimpleJob]
public class PerformanceAnalysisBenchmarks
{
	private readonly Random random = new(42); // Fixed seed for reproducible results
	private int[] smallDataset = [];
	private int[] mediumDataset = [];
	private int[] largeDataset = [];
	private int[] duplicateHeavyData = [];
	private int[] sortedData = [];
	private int[] reverseSortedData = [];

	/// <summary>
	/// Gets or sets the scenario type for parameterized benchmarks.
	/// </summary>
	[Params("Small", "Medium", "Large", "DuplicateHeavy", "Sorted", "ReverseSorted")]
	public string Scenario { get; set; } = "Small";

	/// <summary>
	/// Setup method called before each benchmark iteration.
	/// </summary>
	[GlobalSetup]
	public void Setup()
	{
		// Small dataset (1,000 elements)
		smallDataset = new int[1000];
		for (int i = 0; i < smallDataset.Length; i++)
		{
			smallDataset[i] = random.Next(0, 500);
		}

		// Medium dataset (10,000 elements)
		mediumDataset = new int[10000];
		for (int i = 0; i < mediumDataset.Length; i++)
		{
			mediumDataset[i] = random.Next(0, 5000);
		}

		// Large dataset (100,000 elements)
		largeDataset = new int[100000];
		for (int i = 0; i < largeDataset.Length; i++)
		{
			largeDataset[i] = random.Next(0, 50000);
		}

		// Duplicate-heavy data (lots of repeated values)
		duplicateHeavyData = new int[10000];
		for (int i = 0; i < duplicateHeavyData.Length; i++)
		{
			duplicateHeavyData[i] = random.Next(0, 100); // High probability of duplicates
		}

		// Already sorted data
		sortedData = new int[10000];
		for (int i = 0; i < sortedData.Length; i++)
		{
			sortedData[i] = i;
		}

		// Reverse sorted data
		reverseSortedData = new int[10000];
		for (int i = 0; i < reverseSortedData.Length; i++)
		{
			reverseSortedData[i] = reverseSortedData.Length - 1 - i;
		}
	}

	/// <summary>
	/// Gets the current test dataset based on the scenario.
	/// </summary>
	private int[] GetCurrentDataset() => Scenario switch
	{
		"Small" => smallDataset,
		"Medium" => mediumDataset,
		"Large" => largeDataset,
		"DuplicateHeavy" => duplicateHeavyData,
		"Sorted" => sortedData,
		"ReverseSorted" => reverseSortedData,
		_ => smallDataset
	};

	// ================================
	// Scalability Analysis
	// ================================

	/// <summary>
	/// Benchmark OrderedCollection scalability across different data sizes and patterns.
	/// </summary>
	[Benchmark]
	public OrderedCollection<int> OrderedCollectionScalability()
	{
		int[] data = GetCurrentDataset();
		OrderedCollection<int> collection = [.. data];
		return collection;
	}

	/// <summary>
	/// Benchmark OrderedSet scalability across different data sizes and patterns.
	/// </summary>
	[Benchmark]
	public OrderedSet<int> OrderedSetScalability()
	{
		int[] data = GetCurrentDataset();
		OrderedSet<int> set = [.. data];
		return set;
	}

	/// <summary>
	/// Benchmark List + Sort scalability across different data sizes and patterns.
	/// </summary>
	[Benchmark]
	public List<int> ListSortScalability()
	{
		int[] data = GetCurrentDataset();
		List<int> list = [.. data];
		list.Sort();
		return list;
	}

	/// <summary>
	/// Benchmark HashSet scalability across different data sizes and patterns.
	/// </summary>
	[Benchmark]
	public HashSet<int> HashSetScalability()
	{
		int[] data = GetCurrentDataset();
		HashSet<int> set = [];
		foreach (int item in data)
		{
			set.Add(item);
		}
		return set;
	}

	// ================================
	// Worst-Case Scenarios
	// ================================

	/// <summary>
	/// Benchmark worst-case insertion pattern for OrderedCollection (always insert at beginning).
	/// </summary>
	[Benchmark]
	public OrderedCollection<int> OrderedCollectionWorstCaseInsertion()
	{
		OrderedCollection<int> collection = [];
		// Insert in reverse order to always insert at the beginning
		for (int i = 1000; i >= 0; i--)
		{
			collection.Add(i);
		}
		return collection;
	}

	/// <summary>
	/// Benchmark worst-case insertion pattern for List (always insert at beginning).
	/// </summary>
	[Benchmark]
	public List<int> ListWorstCaseInsertion()
	{
		List<int> list = [];
		// Insert in reverse order, then sort
		for (int i = 1000; i >= 0; i--)
		{
			list.Add(i);
		}
		list.Sort();
		return list;
	}

	/// <summary>
	/// Benchmark worst-case search pattern (item not found, search entire collection).
	/// </summary>
	[Benchmark]
	public int OrderedCollectionWorstCaseSearch()
	{
		OrderedCollection<int> collection = [.. sortedData.Take(1000)];
		int notFound = 0;

		// Search for items that don't exist (worst case for binary search)
		for (int i = 0; i < 100; i++)
		{
			if (!collection.Contains(-1)) // Item definitely not in collection
			{
				notFound++;
			}
		}
		return notFound;
	}

	// ================================
	// Memory Pressure Analysis
	// ================================

	/// <summary>
	/// Benchmark memory allocation patterns under rapid add/remove cycles.
	/// </summary>
	[Benchmark]
	public OrderedCollection<int> OrderedCollectionMemoryPressure()
	{
		OrderedCollection<int> collection = [];

		// Simulate rapid growth and shrinkage
		for (int cycle = 0; cycle < 10; cycle++)
		{
			// Add phase
			for (int i = 0; i < 100; i++)
			{
				collection.Add(random.Next(0, 1000));
			}

			// Remove phase
			for (int i = 0; i < 50; i++)
			{
				if (collection.Count > 0)
				{
					collection.Remove(collection[0]);
				}
			}
		}

		return collection;
	}

	/// <summary>
	/// Benchmark memory allocation patterns for RingBuffer under continuous streaming.
	/// </summary>
	[Benchmark]
	public RingBuffer<int> RingBufferMemoryPressure()
	{
		RingBuffer<int> buffer = new(100);

		// Simulate continuous data streaming (should have minimal allocations)
		for (int i = 0; i < 10000; i++)
		{
			buffer.PushBack(random.Next(0, 1000));
		}

		return buffer;
	}

	// ================================
	// Edge Case Handling
	// ================================

	/// <summary>
	/// Benchmark performance with empty collections.
	/// </summary>
	[Benchmark]
	public bool EmptyCollectionOperations()
	{
		OrderedCollection<int> collection = [];
		OrderedSet<int> set = [];
		RingBuffer<int> buffer = new(10);

		// Test operations on empty collections
		bool result = collection.Count == 0;
		result &= set.Count == 0;
		result &= buffer.Count == 0;
		result &= !collection.Contains(42);
		result &= !set.Contains(42);

		return result;
	}

	/// <summary>
	/// Benchmark performance with single-element collections.
	/// </summary>
	[Benchmark]
	public bool SingleElementOperations()
	{
		OrderedCollection<int> collection = new([42]);
		OrderedSet<int> set = new([42]);
		RingBuffer<int> buffer = new([42], 10);

		// Test operations on single-element collections
		bool result = collection.Count == 1;
		result &= set.Count == 1;
		result &= buffer.Count == 1;
		result &= collection.Contains(42);
		result &= set.Contains(42);
		result &= buffer[0] == 42;

		return result;
	}

	/// <summary>
	/// Benchmark performance with all duplicate elements.
	/// </summary>
	[Benchmark]
	public OrderedSet<int> DuplicateElementHandling()
	{
		OrderedSet<int> set = [];

		// Add the same element many times
		for (int i = 0; i < 1000; i++)
		{
			set.Add(42);
		}

		return set;
	}

	// ================================
	// Specialized Use Cases
	// ================================

	/// <summary>
	/// Benchmark sliding window pattern with RingBuffer.
	/// </summary>
	[Benchmark]
	public double SlidingWindowAnalysisRingBuffer()
	{
		RingBuffer<double> window = new(100);
		double[] values = new double[1000];

		// Generate sample data
		for (int i = 0; i < values.Length; i++)
		{
			values[i] = random.NextDouble() * 100;
		}

		double sum = 0;
		foreach (double value in values)
		{
			window.PushBack(value);

			// Calculate running average of window
			double windowSum = 0;
			foreach (double windowValue in window)
			{
				windowSum += windowValue;
			}
			sum += windowSum / window.Count;
		}

		return sum;
	}

	/// <summary>
	/// Benchmark sliding window pattern with Queue (manual size limiting).
	/// </summary>
	[Benchmark]
	public double SlidingWindowAnalysisQueue()
	{
		Queue<double> window = new();
		double[] values = new double[1000];
		int windowSize = 100;

		// Generate sample data
		for (int i = 0; i < values.Length; i++)
		{
			values[i] = random.NextDouble() * 100;
		}

		double sum = 0;
		foreach (double value in values)
		{
			window.Enqueue(value);
			if (window.Count > windowSize)
			{
				window.Dequeue();
			}

			// Calculate running average of window
			double windowSum = 0;
			foreach (double windowValue in window)
			{
				windowSum += windowValue;
			}
			sum += windowSum / window.Count;
		}

		return sum;
	}

	/// <summary>
	/// Benchmark priority queue simulation using OrderedCollection.
	/// </summary>
	[Benchmark]
	public int PriorityQueueSimulationOrderedCollection()
	{
		OrderedCollection<int> priorityQueue = [];
		int processed = 0;

		// Simulate priority queue operations
		for (int i = 0; i < 1000; i++)
		{
			// Add task with random priority
			priorityQueue.Add(random.Next(1, 100));

			// Process highest priority task (first element in sorted collection)
			if (priorityQueue.Count > 10)
			{
				int highestPriority = priorityQueue[0];
				priorityQueue.Remove(highestPriority);
				processed++;
			}
		}

		return processed;
	}

	/// <summary>
	/// Benchmark batch operations efficiency.
	/// </summary>
	[Benchmark]
	public OrderedSet<int> BatchOperationsEfficiency()
	{
		OrderedSet<int> set = [];
		int[] batch1 = [.. Enumerable.Range(0, 500)];
		int[] batch2 = [.. Enumerable.Range(250, 500)]; // Overlapping batch

		// Add first batch
		foreach (int item in batch1)
		{
			set.Add(item);
		}

		// Add overlapping batch (tests duplicate handling)
		foreach (int item in batch2)
		{
			set.Add(item);
		}

		// Perform set operations
		int[] batch3 = [.. Enumerable.Range(400, 300)];
		set.UnionWith(batch3);

		return set;
	}

	/// <summary>
	/// Benchmark iteration performance under modification.
	/// </summary>
	[Benchmark]
	public int SafeIterationDuringModification()
	{
		OrderedCollection<int> collection = [.. Enumerable.Range(0, 1000)];
		int sum = 0;

		// Create a snapshot for safe iteration while modifying original
		OrderedCollection<int> snapshot = [.. collection];

		int index = 0;
		foreach (int item in snapshot)
		{
			sum += item;

			// Modify original collection during iteration of snapshot
			if (index % 10 == 0 && collection.Count > 0)
			{
				collection.Remove(collection[0]);
			}
			index++;
		}

		return sum;
	}
}
