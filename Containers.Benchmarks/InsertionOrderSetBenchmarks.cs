// Copyright (c) ktsu.dev
// All rights reserved.
// Licensed under the MIT license.

namespace ktsu.Containers.Benchmarks;
using BenchmarkDotNet.Attributes;
using ktsu.Containers;

/// <summary>
/// Benchmarks for InsertionOrderSet performance compared to built-in collections.
/// </summary>
[MemoryDiagnoser]
[SimpleJob]
public class InsertionOrderSetBenchmarks
{
	private readonly Random random = new(42); // Fixed seed for reproducible results
	private int[] testData = [];
	private int[] searchData = [];

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
		// Generate test data with some duplicates
		testData = new int[ElementCount];
		for (int i = 0; i < ElementCount; i++)
		{
			testData[i] = random.Next(0, ElementCount / 2); // Creates duplicates
		}

		// Generate search data (mix of existing and non-existing values)
		searchData = new int[100];
		for (int i = 0; i < 50; i++)
		{
			searchData[i] = testData[random.Next(testData.Length)]; // Existing values
		}
		for (int i = 50; i < 100; i++)
		{
			searchData[i] = random.Next(ElementCount, ElementCount * 2); // Non-existing values
		}
	}

	/// <summary>
	/// Benchmark adding elements to InsertionOrderSet.
	/// </summary>
	[Benchmark]
	public InsertionOrderSet<int> InsertionOrderSet_Add()
	{
		InsertionOrderSet<int> set = [.. testData];
		return set;
	}

	/// <summary>
	/// Benchmark adding elements to HashSet.
	/// </summary>
	[Benchmark]
	public HashSet<int> HashSet_Add()
	{
		HashSet<int> set = [];
		foreach (int item in testData)
		{
			set.Add(item);
		}
		return set;
	}

	/// <summary>
	/// Benchmark adding elements to SortedSet.
	/// </summary>
	[Benchmark]
	public SortedSet<int> SortedSet_Add()
	{
		SortedSet<int> set = [];
		foreach (int item in testData)
		{
			set.Add(item);
		}
		return set;
	}

	/// <summary>
	/// Benchmark searching in InsertionOrderSet using Contains.
	/// </summary>
	[Benchmark]
	public int InsertionOrderSet_Contains()
	{
		InsertionOrderSet<int> set = [.. testData];
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
	/// Benchmark searching in HashSet using Contains.
	/// </summary>
	[Benchmark]
	public int HashSet_Contains()
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
	/// Benchmark searching in SortedSet using Contains.
	/// </summary>
	[Benchmark]
	public int SortedSet_Contains()
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
	/// Benchmark enumeration of InsertionOrderSet.
	/// </summary>
	[Benchmark]
	public int InsertionOrderSet_Enumerate()
	{
		InsertionOrderSet<int> set = [.. testData];
		int sum = 0;
		foreach (int item in set)
		{
			sum += item;
		}
		return sum;
	}

	/// <summary>
	/// Benchmark enumeration of HashSet.
	/// </summary>
	[Benchmark]
	public int HashSet_Enumerate()
	{
		HashSet<int> set = [.. testData];
		int sum = 0;
		foreach (int item in set)
		{
			sum += item;
		}
		return sum;
	}

	/// <summary>
	/// Benchmark enumeration of SortedSet.
	/// </summary>
	[Benchmark]
	public int SortedSet_Enumerate()
	{
		SortedSet<int> set = [.. testData];
		int sum = 0;
		foreach (int item in set)
		{
			sum += item;
		}
		return sum;
	}

	/// <summary>
	/// Benchmark removing elements from InsertionOrderSet.
	/// </summary>
	[Benchmark]
	public InsertionOrderSet<int> InsertionOrderSet_Remove()
	{
		InsertionOrderSet<int> set = [.. testData];

		// Remove every 10th unique element
		int[] uniqueItems = [.. set];
		for (int i = 0; i < uniqueItems.Length; i += 10)
		{
			set.Remove(uniqueItems[i]);
		}

		return set;
	}

	/// <summary>
	/// Benchmark removing elements from HashSet.
	/// </summary>
	[Benchmark]
	public HashSet<int> HashSet_Remove()
	{
		HashSet<int> set = [.. testData];

		// Remove every 10th unique element
		int[] uniqueItems = [.. set];
		for (int i = 0; i < uniqueItems.Length; i += 10)
		{
			set.Remove(uniqueItems[i]);
		}

		return set;
	}

	/// <summary>
	/// Benchmark mixed operations (add, remove, search) on InsertionOrderSet.
	/// </summary>
	[Benchmark]
	public InsertionOrderSet<int> InsertionOrderSet_MixedOperations()
	{
		InsertionOrderSet<int> set =
		[
			// Add elements
			.. testData[..^100],
		];

		// Remove some elements
		int[] toRemove = [.. set.Take(10)];
		foreach (int item in toRemove)
		{
			set.Remove(item);
		}

		// Add more elements
		foreach (int item in testData[^100..])
		{
			set.Add(item);
		}

		return set;
	}

	/// <summary>
	/// Benchmark mixed operations (add, remove, search) on HashSet.
	/// </summary>
	[Benchmark]
	public HashSet<int> HashSet_MixedOperations()
	{
		HashSet<int> set = [];

		// Add elements
		foreach (int item in testData[..^100])
		{
			set.Add(item);
		}

		// Remove some elements
		int[] toRemove = [.. set.Take(10)];
		foreach (int item in toRemove)
		{
			set.Remove(item);
		}

		// Add more elements
		foreach (int item in testData[^100..])
		{
			set.Add(item);
		}

		return set;
	}
}
