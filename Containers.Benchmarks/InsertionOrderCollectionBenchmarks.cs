// Copyright (c) ktsu.dev
// All rights reserved.
// Licensed under the MIT license.

namespace ktsu.Containers.Benchmarks;

using BenchmarkDotNet.Attributes;
using ktsu.Containers;

/// <summary>
/// Benchmarks for InsertionOrderCollection performance compared to built-in collections.
/// </summary>
[MemoryDiagnoser]
[SimpleJob]
public class InsertionOrderCollectionBenchmarks
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
	/// Benchmark adding elements to InsertionOrderCollection.
	/// </summary>
	[Benchmark]
	public InsertionOrderCollection<int> InsertionOrderCollection_Add()
	{
		InsertionOrderCollection<int> collection = [];
		foreach (int item in testData)
		{
			collection.Add(item);
		}
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
	/// Benchmark searching in InsertionOrderCollection using Contains.
	/// </summary>
	[Benchmark]
	public int InsertionOrderCollection_Contains()
	{
		InsertionOrderCollection<int> collection = [.. testData];
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
	/// Benchmark enumeration of InsertionOrderCollection.
	/// </summary>
	[Benchmark]
	public int InsertionOrderCollection_Enumerate()
	{
		InsertionOrderCollection<int> collection = [.. testData];
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
}
