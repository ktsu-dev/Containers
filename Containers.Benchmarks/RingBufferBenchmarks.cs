// Copyright (c) ktsu.dev
// All rights reserved.
// Licensed under the MIT license.

using BenchmarkDotNet.Attributes;
using ktsu.Containers;

namespace ktsu.Containers.Benchmarks;

/// <summary>
/// Benchmarks for RingBuffer performance compared to built-in queue collections.
/// </summary>
[MemoryDiagnoser]
[SimpleJob]
public class RingBufferBenchmarks
{
	private readonly Random random = new(42); // Fixed seed for reproducible results
	private int[] testData = [];

	/// <summary>
	/// Gets or sets the buffer size to use in benchmarks.
	/// </summary>
	[Params(100, 1000, 10000)]
	public int BufferSize { get; set; }

	/// <summary>
	/// Setup method called before each benchmark iteration.
	/// </summary>
	[GlobalSetup]
	public void Setup()
	{
		// Generate test data (more than buffer size to test wrapping)
		testData = new int[BufferSize * 2];
		for (int i = 0; i < testData.Length; i++)
		{
			testData[i] = random.Next(0, 1000);
		}
	}

	/// <summary>
	/// Benchmark adding elements to RingBuffer.
	/// </summary>
	[Benchmark]
	public RingBuffer<int> RingBufferAdd()
	{
		RingBuffer<int> buffer = new(BufferSize);
		foreach (int item in testData)
		{
			buffer.PushBack(item);
		}
		return buffer;
	}

	/// <summary>
	/// Benchmark adding elements to Queue with size limit simulation.
	/// </summary>
	[Benchmark]
	public Queue<int> QueueAddWithLimit()
	{
		Queue<int> queue = new();
		foreach (int item in testData)
		{
			queue.Enqueue(item);
			if (queue.Count > BufferSize)
			{
				queue.Dequeue(); // Simulate ring buffer behavior
			}
		}
		return queue;
	}

	/// <summary>
	/// Benchmark adding elements to List with size limit simulation.
	/// </summary>
	[Benchmark]
	public List<int> ListAddWithLimit()
	{
		List<int> list = [];
		foreach (int item in testData)
		{
			list.Add(item);
			if (list.Count > BufferSize)
			{
				list.RemoveAt(0); // Simulate ring buffer behavior (expensive!)
			}
		}
		return list;
	}

	/// <summary>
	/// Benchmark accessing elements in RingBuffer by index.
	/// </summary>
	[Benchmark]
	public int RingBufferIndexAccess()
	{
		RingBuffer<int> buffer = new(testData.Take(BufferSize), BufferSize);
		int sum = 0;
		for (int i = 0; i < buffer.Count; i++)
		{
			sum += buffer[i];
		}
		return sum;
	}

	/// <summary>
	/// Benchmark accessing elements in Queue (requires enumeration).
	/// </summary>
	[Benchmark]
	public int QueueEnumerate()
	{
		Queue<int> queue = new(testData.Take(BufferSize));
		int sum = 0;
		foreach (int item in queue)
		{
			sum += item;
		}
		return sum;
	}

	/// <summary>
	/// Benchmark accessing elements in List by index.
	/// </summary>
	[Benchmark]
	public int ListIndexAccess()
	{
		List<int> list = [.. testData.Take(BufferSize)];
		int sum = 0;
		for (int i = 0; i < list.Count; i++)
		{
			sum += list[i];
		}
		return sum;
	}

	/// <summary>
	/// Benchmark enumeration of RingBuffer.
	/// </summary>
	[Benchmark]
	public int RingBufferEnumerate()
	{
		RingBuffer<int> buffer = new(testData.Take(BufferSize), BufferSize);
		int sum = 0;
		foreach (int item in buffer)
		{
			sum += item;
		}
		return sum;
	}

	/// <summary>
	/// Benchmark clearing RingBuffer.
	/// </summary>
	[Benchmark]
	public RingBuffer<int> RingBufferClear()
	{
		RingBuffer<int> buffer = new(testData.Take(BufferSize), BufferSize);
		buffer.Clear();
		return buffer;
	}

	/// <summary>
	/// Benchmark clearing Queue.
	/// </summary>
	[Benchmark]
	public Queue<int> QueueClear()
	{
		Queue<int> queue = new(testData.Take(BufferSize));
		queue.Clear();
		return queue;
	}

	/// <summary>
	/// Benchmark clearing List.
	/// </summary>
	[Benchmark]
	public List<int> ListClear()
	{
		List<int> list = [.. testData.Take(BufferSize)];
		list.Clear();
		return list;
	}

	/// <summary>
	/// Benchmark resizing RingBuffer.
	/// </summary>
	[Benchmark]
	public RingBuffer<int> RingBufferResize()
	{
		RingBuffer<int> buffer = new(testData.Take(BufferSize), BufferSize);
		buffer.Resize(BufferSize * 2);
		return buffer;
	}

	/// <summary>
	/// Benchmark copying Queue to larger Queue (resize simulation).
	/// </summary>
	[Benchmark]
	public Queue<int> QueueResize()
	{
		Queue<int> oldQueue = new(testData.Take(BufferSize));
		Queue<int> newQueue = new(BufferSize * 2);

		// Copy all elements
		foreach (int item in oldQueue)
		{
			newQueue.Enqueue(item);
		}

		return newQueue;
	}

	/// <summary>
	/// Benchmark resampling RingBuffer (upsampling).
	/// </summary>
	[Benchmark]
	public RingBuffer<int> RingBufferResampleUp()
	{
		RingBuffer<int> buffer = new(testData.Take(BufferSize), BufferSize);
		buffer.Resample(BufferSize * 2);
		return buffer;
	}

	/// <summary>
	/// Benchmark resampling RingBuffer (downsampling).
	/// </summary>
	[Benchmark]
	public RingBuffer<int> RingBufferResampleDown()
	{
		RingBuffer<int> buffer = new(testData.Take(BufferSize), BufferSize);
		buffer.Resample(BufferSize / 2);
		return buffer;
	}

	/// <summary>
	/// Benchmark mixed operations on RingBuffer (realistic usage).
	/// </summary>
	[Benchmark]
	public RingBuffer<int> RingBufferMixedOperations()
	{
		RingBuffer<int> buffer = new(BufferSize);

		// Fill buffer
		for (int i = 0; i < BufferSize; i++)
		{
			buffer.PushBack(testData[i]);
		}

		// Access some elements
		int sum = 0;
		for (int i = 0; i < Math.Min(10, buffer.Count); i++)
		{
			sum += buffer[i];
		}

		// Add more elements (causing wrapping)
		for (int i = BufferSize; i < BufferSize + (BufferSize / 4); i++)
		{
			buffer.PushBack(testData[i % testData.Length]);
		}

		// Enumerate
		foreach (int item in buffer)
		{
			sum += item;
		}

		// Resize
		buffer.Resize(BufferSize + (BufferSize / 2));

		return buffer;
	}

	/// <summary>
	/// Benchmark mixed operations on Queue with size limit.
	/// </summary>
	[Benchmark]
	public Queue<int> QueueMixedOperations()
	{
		Queue<int> queue = new();

		// Fill queue
		for (int i = 0; i < BufferSize; i++)
		{
			queue.Enqueue(testData[i]);
		}

		// Access some elements (requires enumeration)
		int sum = 0;
		int count = 0;
		foreach (int item in queue)
		{
			sum += item;
			count++;
			if (count >= 10) break;
		}

		// Add more elements with size limit
		for (int i = BufferSize; i < BufferSize + (BufferSize / 4); i++)
		{
			queue.Enqueue(testData[i % testData.Length]);
			if (queue.Count > BufferSize)
			{
				queue.Dequeue();
			}
		}

		// Enumerate
		foreach (int item in queue)
		{
			sum += item;
		}

		// Resize simulation (create new queue)
		Queue<int> newQueue = new(BufferSize + (BufferSize / 2));
		foreach (int item in queue)
		{
			newQueue.Enqueue(item);
		}

		return newQueue;
	}

	/// <summary>
	/// Benchmark memory allocation patterns.
	/// </summary>
	[Benchmark]
	public RingBuffer<int> RingBufferMemoryPattern()
	{
		RingBuffer<int> buffer = new(BufferSize);

		// Simulate continuous data streaming
		for (int cycle = 0; cycle < 5; cycle++)
		{
			for (int i = 0; i < BufferSize; i++)
			{
				buffer.PushBack(testData[i % testData.Length]);
			}
		}

		return buffer;
	}

	/// <summary>
	/// Benchmark memory allocation patterns for Queue.
	/// </summary>
	[Benchmark]
	public Queue<int> QueueMemoryPattern()
	{
		Queue<int> queue = new();

		// Simulate continuous data streaming with size limit
		for (int cycle = 0; cycle < 5; cycle++)
		{
			for (int i = 0; i < BufferSize; i++)
			{
				queue.Enqueue(testData[i % testData.Length]);
				if (queue.Count > BufferSize)
				{
					queue.Dequeue();
				}
			}
		}

		return queue;
	}
}
