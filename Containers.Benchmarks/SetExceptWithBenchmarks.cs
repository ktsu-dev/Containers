// Copyright (c) 2023-2026 ktsu-dev contributors

namespace ktsu.Containers.Benchmarks;

using BenchmarkDotNet.Attributes;
using ktsu.Containers;

/// <summary>
/// Benchmarks ExceptWith at sizes where removing one element at a time from list-backed storage would show
/// quadratic behaviour, against <see cref="HashSet{T}"/> as the linear baseline.
/// </summary>
[MemoryDiagnoser]
[SimpleJob]
public class SetExceptWithBenchmarks
{
	private int[] testData = [];
	private int[] exceptData = [];

	/// <summary>
	/// Gets or sets the number of elements in the set; half of them are excepted.
	/// </summary>
	[Params(10000, 80000)]
	public int ElementCount { get; set; }

	/// <summary>
	/// Builds the set contents and the even elements to except from them.
	/// </summary>
	[GlobalSetup]
	public void Setup()
	{
		Random random = new(42); // Fixed seed for reproducible results
		testData = [.. Enumerable.Range(0, ElementCount).OrderBy(_ => random.Next())];
		exceptData = [.. Enumerable.Range(0, ElementCount / 2).Select(i => i * 2)];
	}

	/// <summary>
	/// Benchmark ExceptWith on HashSet, the linear baseline.
	/// </summary>
	[Benchmark(Baseline = true)]
	public HashSet<int> HashSet_ExceptWith()
	{
		HashSet<int> set = [.. testData];
		set.ExceptWith(exceptData);
		return set;
	}

	/// <summary>
	/// Benchmark ExceptWith on InsertionOrderSet.
	/// </summary>
	[Benchmark]
	public InsertionOrderSet<int> InsertionOrderSet_ExceptWith()
	{
		InsertionOrderSet<int> set = [.. testData];
		set.ExceptWith(exceptData);
		return set;
	}

	/// <summary>
	/// Benchmark ExceptWith on ContiguousSet.
	/// </summary>
	[Benchmark]
	public ContiguousSet<int> ContiguousSet_ExceptWith()
	{
		ContiguousSet<int> set = [.. testData];
		set.ExceptWith(exceptData);
		return set;
	}

	/// <summary>
	/// Benchmark ExceptWith on OrderedSet.
	/// </summary>
	[Benchmark]
	public OrderedSet<int> OrderedSet_ExceptWith()
	{
		OrderedSet<int> set = [.. testData];
		set.ExceptWith(exceptData);
		return set;
	}
}
