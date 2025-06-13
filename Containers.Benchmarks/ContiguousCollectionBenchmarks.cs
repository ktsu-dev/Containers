// Copyright (c) ktsu.dev
// All rights reserved.
// Licensed under the MIT license.

namespace ktsu.Containers.Benchmarks;
using BenchmarkDotNet.Attributes;
using ktsu.Containers;

/// <summary>
/// Benchmarks for ContiguousCollection performance compared to built-in collections.
/// Focus on cache efficiency and contiguous memory access patterns.
/// </summary>
[MemoryDiagnoser]
[SimpleJob]
public class ContiguousCollectionBenchmarks
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
	}

	/// <summary>
	/// Benchmark adding elements to ContiguousCollection.
	/// </summary>
	[Benchmark]
	public ContiguousCollection<int> ContiguousCollection_Add()
	{
		ContiguousCollection<int> collection = [.. testData];
		return collection;
	}

	/// <summary>
	/// Benchmark adding elements to List (baseline comparison).
	/// </summary>
	[Benchmark]
	public List<int> List_Add()
	{
		List<int> list = [];
		foreach (int item in testData)
		{
			list.Add(item);
		}
		return list;
	}

	/// <summary>
	/// Benchmark adding elements to Array (fixed size comparison).
	/// </summary>
	[Benchmark]
	public int[] Array_Fill()
	{
		int[] array = new int[testData.Length];
		for (int i = 0; i < testData.Length; i++)
		{
			array[i] = testData[i];
		}
		return array;
	}

	/// <summary>
	/// Benchmark searching in ContiguousCollection using Contains (cache-friendly).
	/// </summary>
	[Benchmark]
	public int ContiguousCollection_Contains()
	{
		ContiguousCollection<int> collection = [.. testData];
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
	/// Benchmark searching in List using Contains.
	/// </summary>
	[Benchmark]
	public int List_Contains()
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
	/// Benchmark enumeration of ContiguousCollection (optimal cache access).
	/// </summary>
	[Benchmark]
	public int ContiguousCollection_Enumerate()
	{
		ContiguousCollection<int> collection = [.. testData];
		int sum = 0;
		foreach (int item in collection)
		{
			sum += item;
		}
		return sum;
	}

	/// <summary>
	/// Benchmark enumeration of List.
	/// </summary>
	[Benchmark]
	public int List_Enumerate()
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
	/// Benchmark indexed access in ContiguousCollection (cache-friendly random access).
	/// </summary>
	[Benchmark]
	public int ContiguousCollection_IndexedAccess()
	{
		ContiguousCollection<int> collection = [.. testData];
		int sum = 0;
		for (int i = 0; i < collection.Count; i++)
		{
			sum += collection[i];
		}
		return sum;
	}

	/// <summary>
	/// Benchmark indexed access in List.
	/// </summary>
	[Benchmark]
	public int List_IndexedAccess()
	{
		List<int> list = [.. testData];
		int sum = 0;
		for (int i = 0; i < list.Count; i++)
		{
			sum += list[i];
		}
		return sum;
	}

	/// <summary>
	/// Benchmark span access in ContiguousCollection (zero-copy operations).
	/// </summary>
	[Benchmark]
	public int ContiguousCollection_SpanAccess()
	{
		ContiguousCollection<int> collection = [.. testData];
		ReadOnlySpan<int> span = collection.AsReadOnlySpan();
		int sum = 0;
		for (int i = 0; i < span.Length; i++)
		{
			sum += span[i];
		}
		return sum;
	}

	/// <summary>
	/// Benchmark removing elements from ContiguousCollection.
	/// </summary>
	[Benchmark]
	public ContiguousCollection<int> ContiguousCollection_Remove()
	{
		ContiguousCollection<int> collection = [.. testData];

		// Remove every 10th element
		for (int i = 0; i < testData.Length; i += 10)
		{
			collection.Remove(testData[i]);
		}

		return collection;
	}

	/// <summary>
	/// Benchmark removing elements from List.
	/// </summary>
	[Benchmark]
	public List<int> List_Remove()
	{
		List<int> list = [.. testData];

		// Remove every 10th element
		for (int i = 0; i < testData.Length; i += 10)
		{
			list.Remove(testData[i]);
		}

		return list;
	}

	/// <summary>
	/// Benchmark memory-intensive operations showing contiguous memory benefits.
	/// </summary>
	[Benchmark]
	public long ContiguousCollection_MemoryIntensiveOp()
	{
		ContiguousCollection<int> collection = [.. testData];
		long result = 0;

		// Simulate cache-friendly sequential access pattern
		for (int pass = 0; pass < 3; pass++)
		{
			for (int i = 0; i < collection.Count; i++)
			{
				result += collection[i] * (pass + 1);
			}
		}

		return result;
	}

	/// <summary>
	/// Benchmark memory-intensive operations on List.
	/// </summary>
	[Benchmark]
	public long List_MemoryIntensiveOp()
	{
		List<int> list = [.. testData];
		long result = 0;

		// Simulate cache-friendly sequential access pattern
		for (int pass = 0; pass < 3; pass++)
		{
			for (int i = 0; i < list.Count; i++)
			{
				result += list[i] * (pass + 1);
			}
		}

		return result;
	}

	/// <summary>
	/// Benchmark mixed operations (add, remove, access) on ContiguousCollection.
	/// </summary>
	[Benchmark]
	public ContiguousCollection<int> ContiguousCollection_MixedOperations()
	{
		ContiguousCollection<int> collection =
		[
			// Add elements
			.. testData[..^100],
		];

		// Access elements (cache-friendly)
		int sum = 0;
		for (int i = 0; i < collection.Count; i++)
		{
			sum += collection[i];
		}

		// Remove some elements
		for (int i = 0; i < 50; i++)
		{
			collection.Remove(testData[i]);
		}

		// Add more elements
		foreach (int item in testData[^100..])
		{
			collection.Add(item);
		}

		return collection;
	}

	/// <summary>
	/// Benchmark mixed operations (add, remove, access) on List.
	/// </summary>
	[Benchmark]
	public List<int> List_MixedOperations()
	{
		List<int> list = [];

		// Add elements
		foreach (int item in testData[..^100])
		{
			list.Add(item);
		}

		// Access elements
		int sum = 0;
		for (int i = 0; i < list.Count; i++)
		{
			sum += list[i];
		}

		// Remove some elements
		for (int i = 0; i < 50; i++)
		{
			list.Remove(testData[i]);
		}

		// Add more elements
		foreach (int item in testData[^100..])
		{
			list.Add(item);
		}

		return list;
	}
}
