// Copyright (c) ktsu.dev
// All rights reserved.
// Licensed under the MIT license.

using BenchmarkDotNet.Attributes;
using ktsu.Containers;

namespace ktsu.Containers.Benchmarks;

/// <summary>
/// Benchmarks for OrderedCollection performance compared to built-in collections.
/// </summary>
[MemoryDiagnoser]
[SimpleJob]
public class OrderedCollectionBenchmarks
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
	/// Benchmark adding elements to OrderedCollection.
	/// </summary>
	[Benchmark]
	public OrderedCollection<int> OrderedCollection_Add()
	{
		OrderedCollection<int> collection = new();
		foreach (int item in testData)
		{
			collection.Add(item);
		}
		return collection;
	}

	/// <summary>
	/// Benchmark adding elements to List and then sorting.
	/// </summary>
	[Benchmark]
	public List<int> List_AddAndSort()
	{
		List<int> list = [];
		foreach (int item in testData)
		{
			list.Add(item);
		}
		list.Sort();
		return list;
	}

	/// <summary>
	/// Benchmark adding elements to SortedList.
	/// </summary>
	[Benchmark]
	public SortedList<int, int> SortedList_Add()
	{
		SortedList<int, int> sortedList = [];
		foreach (int item in testData)
		{
			sortedList.TryAdd(item, item);
		}
		return sortedList;
	}

	/// <summary>
	/// Benchmark searching in OrderedCollection using Contains.
	/// </summary>
	[Benchmark]
	public int OrderedCollection_Contains()
	{
		OrderedCollection<int> collection = new(testData);
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
	/// Benchmark searching in sorted List using BinarySearch.
	/// </summary>
	[Benchmark]
	public int List_BinarySearch()
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

	/// <summary>
	/// Benchmark searching in SortedList using ContainsKey.
	/// </summary>
	[Benchmark]
	public int SortedList_ContainsKey()
	{
		SortedList<int, int> sortedList = [];
		foreach (int item in testData)
		{
			sortedList.TryAdd(item, item);
		}

		int found = 0;
		foreach (int item in searchData)
		{
			if (sortedList.ContainsKey(item))
			{
				found++;
			}
		}
		return found;
	}

	/// <summary>
	/// Benchmark enumeration of OrderedCollection.
	/// </summary>
	[Benchmark]
	public int OrderedCollection_Enumerate()
	{
		OrderedCollection<int> collection = new(testData);
		int sum = 0;
		foreach (int item in collection)
		{
			sum += item;
		}
		return sum;
	}

	/// <summary>
	/// Benchmark enumeration of sorted List.
	/// </summary>
	[Benchmark]
	public int List_Enumerate()
	{
		List<int> list = [.. testData];
		list.Sort();
		int sum = 0;
		foreach (int item in list)
		{
			sum += item;
		}
		return sum;
	}

	/// <summary>
	/// Benchmark removing elements from OrderedCollection.
	/// </summary>
	[Benchmark]
	public OrderedCollection<int> OrderedCollection_Remove()
	{
		OrderedCollection<int> collection = new(testData);

		// Remove every 10th element
		for (int i = 0; i < testData.Length; i += 10)
		{
			collection.Remove(testData[i]);
		}

		return collection;
	}

	/// <summary>
	/// Benchmark removing elements from List (requires re-sorting).
	/// </summary>
	[Benchmark]
	public List<int> List_Remove()
	{
		List<int> list = [.. testData];
		list.Sort();

		// Remove every 10th element
		for (int i = 0; i < testData.Length; i += 10)
		{
			list.Remove(testData[i]);
		}

		return list;
	}

	/// <summary>
	/// Benchmark mixed operations (add, search, remove) on OrderedCollection.
	/// </summary>
	[Benchmark]
	public OrderedCollection<int> OrderedCollection_MixedOperations()
	{
		OrderedCollection<int> collection = new();

		// Add half the elements
		for (int i = 0; i < testData.Length / 2; i++)
		{
			collection.Add(testData[i]);
		}

		// Search for some elements
		for (int i = 0; i < 10; i++)
		{
			collection.Contains(searchData[i]);
		}

		// Add remaining elements
		for (int i = testData.Length / 2; i < testData.Length; i++)
		{
			collection.Add(testData[i]);
		}

		// Remove some elements
		for (int i = 0; i < testData.Length; i += 20)
		{
			collection.Remove(testData[i]);
		}

		return collection;
	}

	/// <summary>
	/// Benchmark mixed operations on List with sorting.
	/// </summary>
	[Benchmark]
	public List<int> List_MixedOperations()
	{
		List<int> list = [];

		// Add half the elements
		for (int i = 0; i < testData.Length / 2; i++)
		{
			list.Add(testData[i]);
		}
		list.Sort();

		// Search for some elements
		for (int i = 0; i < 10; i++)
		{
			list.BinarySearch(searchData[i]);
		}

		// Add remaining elements
		for (int i = testData.Length / 2; i < testData.Length; i++)
		{
			list.Add(testData[i]);
		}
		list.Sort();

		// Remove some elements
		for (int i = 0; i < testData.Length; i += 20)
		{
			list.Remove(testData[i]);
		}

		return list;
	}
}
