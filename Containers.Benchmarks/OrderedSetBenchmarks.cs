// Copyright (c) ktsu.dev
// All rights reserved.
// Licensed under the MIT license.

using BenchmarkDotNet.Attributes;
using ktsu.Containers;

namespace ktsu.Containers.Benchmarks;

/// <summary>
/// Benchmarks for OrderedSet performance compared to built-in set collections.
/// </summary>
[MemoryDiagnoser]
[SimpleJob]
public class OrderedSetBenchmarks
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
		// Generate test data with some duplicates
		HashSet<int> uniqueValues = [];
		while (uniqueValues.Count < ElementCount)
		{
			uniqueValues.Add(random.Next(0, ElementCount * 2));
		}
		testData = [.. uniqueValues];

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

		// Generate union data
		HashSet<int> unionValues = [];
		while (unionValues.Count < ElementCount / 2)
		{
			unionValues.Add(random.Next(ElementCount, ElementCount * 3));
		}
		unionData = [.. unionValues];
	}

	/// <summary>
	/// Benchmark adding elements to OrderedSet.
	/// </summary>
	[Benchmark]
	public OrderedSet<int> OrderedSet_Add()
	{
		OrderedSet<int> set = new();
		foreach (int item in testData)
		{
			set.Add(item);
		}
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
	/// Benchmark searching in OrderedSet using Contains.
	/// </summary>
	[Benchmark]
	public int OrderedSet_Contains()
	{
		OrderedSet<int> set = new(testData);
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
	/// Benchmark enumeration of OrderedSet (sorted order).
	/// </summary>
	[Benchmark]
	public int OrderedSet_Enumerate()
	{
		OrderedSet<int> set = new(testData);
		int sum = 0;
		foreach (int item in set)
		{
			sum += item;
		}
		return sum;
	}

	/// <summary>
	/// Benchmark enumeration of HashSet (unordered).
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
	/// Benchmark enumeration of SortedSet (sorted order).
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
	/// Benchmark union operation on OrderedSet.
	/// </summary>
	[Benchmark]
	public OrderedSet<int> OrderedSet_Union()
	{
		OrderedSet<int> set = new(testData);
		set.UnionWith(unionData);
		return set;
	}

	/// <summary>
	/// Benchmark union operation on HashSet.
	/// </summary>
	[Benchmark]
	public HashSet<int> HashSet_Union()
	{
		HashSet<int> set = [.. testData];
		set.UnionWith(unionData);
		return set;
	}

	/// <summary>
	/// Benchmark union operation on SortedSet.
	/// </summary>
	[Benchmark]
	public SortedSet<int> SortedSet_Union()
	{
		SortedSet<int> set = [.. testData];
		set.UnionWith(unionData);
		return set;
	}

	/// <summary>
	/// Benchmark intersection operation on OrderedSet.
	/// </summary>
	[Benchmark]
	public OrderedSet<int> OrderedSet_Intersect()
	{
		OrderedSet<int> set = new(testData);
		set.IntersectWith(unionData);
		return set;
	}

	/// <summary>
	/// Benchmark intersection operation on HashSet.
	/// </summary>
	[Benchmark]
	public HashSet<int> HashSet_Intersect()
	{
		HashSet<int> set = [.. testData];
		set.IntersectWith(unionData);
		return set;
	}

	/// <summary>
	/// Benchmark intersection operation on SortedSet.
	/// </summary>
	[Benchmark]
	public SortedSet<int> SortedSet_Intersect()
	{
		SortedSet<int> set = [.. testData];
		set.IntersectWith(unionData);
		return set;
	}

	/// <summary>
	/// Benchmark removing elements from OrderedSet.
	/// </summary>
	[Benchmark]
	public OrderedSet<int> OrderedSet_Remove()
	{
		OrderedSet<int> set = new(testData);

		// Remove every 10th element
		for (int i = 0; i < testData.Length; i += 10)
		{
			set.Remove(testData[i]);
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

		// Remove every 10th element
		for (int i = 0; i < testData.Length; i += 10)
		{
			set.Remove(testData[i]);
		}

		return set;
	}

	/// <summary>
	/// Benchmark removing elements from SortedSet.
	/// </summary>
	[Benchmark]
	public SortedSet<int> SortedSet_Remove()
	{
		SortedSet<int> set = [.. testData];

		// Remove every 10th element
		for (int i = 0; i < testData.Length; i += 10)
		{
			set.Remove(testData[i]);
		}

		return set;
	}

	/// <summary>
	/// Benchmark mixed operations on OrderedSet.
	/// </summary>
	[Benchmark]
	public OrderedSet<int> OrderedSet_MixedOperations()
	{
		OrderedSet<int> set = new();

		// Add half the elements
		for (int i = 0; i < testData.Length / 2; i++)
		{
			set.Add(testData[i]);
		}

		// Search for some elements
		for (int i = 0; i < 10; i++)
		{
			set.Contains(searchData[i]);
		}

		// Union with some data
		set.UnionWith(unionData.Take(unionData.Length / 4));

		// Add remaining elements
		for (int i = testData.Length / 2; i < testData.Length; i++)
		{
			set.Add(testData[i]);
		}

		// Remove some elements
		for (int i = 0; i < testData.Length; i += 20)
		{
			set.Remove(testData[i]);
		}

		return set;
	}

	/// <summary>
	/// Benchmark mixed operations on HashSet.
	/// </summary>
	[Benchmark]
	public HashSet<int> HashSet_MixedOperations()
	{
		HashSet<int> set = [];

		// Add half the elements
		for (int i = 0; i < testData.Length / 2; i++)
		{
			set.Add(testData[i]);
		}

		// Search for some elements
		for (int i = 0; i < 10; i++)
		{
			set.Contains(searchData[i]);
		}

		// Union with some data
		set.UnionWith(unionData.Take(unionData.Length / 4));

		// Add remaining elements
		for (int i = testData.Length / 2; i < testData.Length; i++)
		{
			set.Add(testData[i]);
		}

		// Remove some elements
		for (int i = 0; i < testData.Length; i += 20)
		{
			set.Remove(testData[i]);
		}

		return set;
	}

	/// <summary>
	/// Benchmark mixed operations on SortedSet.
	/// </summary>
	[Benchmark]
	public SortedSet<int> SortedSet_MixedOperations()
	{
		SortedSet<int> set = [];

		// Add half the elements
		for (int i = 0; i < testData.Length / 2; i++)
		{
			set.Add(testData[i]);
		}

		// Search for some elements
		for (int i = 0; i < 10; i++)
		{
			set.Contains(searchData[i]);
		}

		// Union with some data
		set.UnionWith(unionData.Take(unionData.Length / 4));

		// Add remaining elements
		for (int i = testData.Length / 2; i < testData.Length; i++)
		{
			set.Add(testData[i]);
		}

		// Remove some elements
		for (int i = 0; i < testData.Length; i += 20)
		{
			set.Remove(testData[i]);
		}

		return set;
	}
}
