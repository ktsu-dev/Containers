// Copyright (c) ktsu.dev
// All rights reserved.
// Licensed under the MIT license.

namespace ktsu.Containers.Benchmarks;
using BenchmarkDotNet.Attributes;
using ktsu.Containers;

/// <summary>
/// Benchmarks for ContiguousSet performance compared to built-in collections.
/// Focus on unique elements with contiguous memory layout for optimal cache performance.
/// </summary>
[MemoryDiagnoser]
[SimpleJob]
public class ContiguousSetBenchmarks
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
	/// Benchmark adding elements to ContiguousSet.
	/// </summary>
	[Benchmark]
	public ContiguousSet<int> ContiguousSet_Add()
	{
		ContiguousSet<int> set = [.. testData];
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
	/// Benchmark searching in ContiguousSet using Contains (cache-friendly linear search).
	/// </summary>
	[Benchmark]
	public int ContiguousSet_Contains()
	{
		ContiguousSet<int> set = [.. testData];
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
	/// Benchmark enumeration of ContiguousSet (optimal cache access).
	/// </summary>
	[Benchmark]
	public int ContiguousSet_Enumerate()
	{
		ContiguousSet<int> set = [.. testData];
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
	/// Benchmark span access in ContiguousSet (zero-copy operations).
	/// </summary>
	[Benchmark]
	public int ContiguousSet_SpanAccess()
	{
		ContiguousSet<int> set = [.. testData];
		ReadOnlySpan<int> span = set.AsReadOnlySpan();
		int sum = 0;
		for (int i = 0; i < span.Length; i++)
		{
			sum += span[i];
		}
		return sum;
	}

	/// <summary>
	/// Benchmark removing elements from ContiguousSet.
	/// </summary>
	[Benchmark]
	public ContiguousSet<int> ContiguousSet_Remove()
	{
		ContiguousSet<int> set = [.. testData];

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
	/// Benchmark memory-intensive operations showing contiguous memory benefits.
	/// </summary>
	[Benchmark]
	public long ContiguousSet_MemoryIntensiveOp()
	{
		ContiguousSet<int> set = [.. testData];
		long result = 0;

		// Simulate cache-friendly sequential access pattern
		for (int pass = 0; pass < 3; pass++)
		{
			foreach (int item in set)
			{
				result += item * (pass + 1);
			}
		}

		return result;
	}

	/// <summary>
	/// Benchmark memory-intensive operations on HashSet.
	/// </summary>
	[Benchmark]
	public long HashSet_MemoryIntensiveOp()
	{
		HashSet<int> set = [.. testData];
		long result = 0;

		// Simulate access pattern (less cache-friendly)
		for (int pass = 0; pass < 3; pass++)
		{
			foreach (int item in set)
			{
				result += item * (pass + 1);
			}
		}

		return result;
	}

	/// <summary>
	/// Benchmark set operations (union, intersection) on ContiguousSet.
	/// </summary>
	[Benchmark]
	public ContiguousSet<int> ContiguousSet_SetOperations()
	{
		ContiguousSet<int> set1 = [.. testData[..(ElementCount / 2)]];
		ContiguousSet<int> set2 = [.. testData[(ElementCount / 4)..]];

		// Simulate union operation
		ContiguousSet<int> result = [.. set1];
		foreach (int item in set2)
		{
			result.Add(item);
		}

		return result;
	}

	/// <summary>
	/// Benchmark set operations (union, intersection) on HashSet.
	/// </summary>
	[Benchmark]
	public HashSet<int> HashSet_SetOperations()
	{
		HashSet<int> set1 = [.. testData[..(ElementCount / 2)]];
		HashSet<int> set2 = [.. testData[(ElementCount / 4)..]];

		// Use built-in union operation
		set1.UnionWith(set2);
		return set1;
	}

	/// <summary>
	/// Benchmark mixed operations (add, remove, search) on ContiguousSet.
	/// </summary>
	[Benchmark]
	public ContiguousSet<int> ContiguousSet_MixedOperations()
	{
		ContiguousSet<int> set =
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
