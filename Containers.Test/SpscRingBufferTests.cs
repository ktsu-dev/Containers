// Copyright (c) 2023-2026 ktsu-dev contributors

namespace ktsu.Containers.Tests;

using System.Diagnostics;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

[TestClass]
public class SpscRingBufferTests
{
	/// <summary>
	/// Gets or sets the test context, used to obtain the cancellation token for background tasks.
	/// </summary>
	public TestContext TestContext { get; set; } = null!;

	[TestMethod]
	public void Constructor_NonPositiveCapacity_Throws()
	{
		Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => new SpscRingBuffer<int>(0));
		Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => new SpscRingBuffer<int>(-1));
	}

	[TestMethod]
	public void Constructor_CapacityAbove2Pow30Minus1_Throws()
	{
		Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => new SpscRingBuffer<byte>(1 << 30));
		Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => new SpscRingBuffer<byte>(int.MaxValue));
	}

	[TestMethod]
	public void Capacity_IsAtLeastRequested()
	{
		SpscRingBuffer<int> buffer = new(5);
		Assert.IsGreaterThanOrEqualTo(5, buffer.Capacity, "Usable capacity must be at least the requested amount.");
	}

	[TestMethod]
	public void NewBuffer_IsEmpty()
	{
		SpscRingBuffer<int> buffer = new(4);
		Assert.IsTrue(buffer.IsEmpty);
		Assert.AreEqual(0, buffer.Count);
		Assert.IsFalse(buffer.TryDequeue(out _));
	}

	[TestMethod]
	public void EnqueueDequeue_PreservesFifoOrder()
	{
		SpscRingBuffer<int> buffer = new(8);
		for (int i = 0; i < 5; i++)
		{
			Assert.IsTrue(buffer.TryEnqueue(i));
		}

		for (int i = 0; i < 5; i++)
		{
			Assert.IsTrue(buffer.TryDequeue(out int value));
			Assert.AreEqual(i, value);
		}

		Assert.IsTrue(buffer.IsEmpty);
	}

	[TestMethod]
	public void TryEnqueue_WhenFull_ReturnsFalse()
	{
		SpscRingBuffer<int> buffer = new(4);
		int enqueued = 0;
		while (buffer.TryEnqueue(enqueued))
		{
			enqueued++;
		}

		Assert.IsGreaterThanOrEqualTo(4, enqueued, "Should accept at least the requested capacity before reporting full.");
		Assert.IsFalse(buffer.TryEnqueue(999));
	}

	[TestMethod]
	public void TryPeek_DoesNotRemoveElement()
	{
		SpscRingBuffer<int> buffer = new(4);
		buffer.TryEnqueue(42);

		Assert.IsTrue(buffer.TryPeek(out int peeked));
		Assert.AreEqual(42, peeked);
		Assert.AreEqual(1, buffer.Count);

		Assert.IsTrue(buffer.TryDequeue(out int dequeued));
		Assert.AreEqual(42, dequeued);
	}

	[TestMethod]
	public void WrapAround_ReusesSlotsCorrectly()
	{
		SpscRingBuffer<int> buffer = new(4);

		// Cycle through many more items than capacity to force repeated wraparound.
		for (int i = 0; i < 1000; i++)
		{
			Assert.IsTrue(buffer.TryEnqueue(i));
			Assert.IsTrue(buffer.TryDequeue(out int value));
			Assert.AreEqual(i, value);
		}

		Assert.IsTrue(buffer.IsEmpty);
	}

	[TestMethod]
	public async Task ConcurrentProducerConsumer_TransfersAllItemsInOrder()
	{
		const int itemCount = 1_000_000;
		SpscRingBuffer<int> buffer = new(1024);

		Task producer = Task.Run(() =>
		{
			int produced = 0;
			while (produced < itemCount)
			{
				if (buffer.TryEnqueue(produced))
				{
					produced++;
				}
				else
				{
					Thread.SpinWait(1);
				}
			}
		}, TestContext.CancellationToken);

		Task<bool> consumer = Task.Run(() =>
		{
			int expected = 0;
			while (expected < itemCount)
			{
				if (buffer.TryDequeue(out int value))
				{
					if (value != expected)
					{
						return false;
					}

					expected++;
				}
				else
				{
					Thread.SpinWait(1);
				}
			}

			return true;
		}, TestContext.CancellationToken);

		await Task.WhenAll(producer, consumer).ConfigureAwait(false);
		Assert.IsTrue(await consumer.ConfigureAwait(false), "All items must be received exactly once and in order.");
		Assert.IsTrue(buffer.IsEmpty);
	}

	[TestMethod]
	public async Task Count_ReadFromThirdThread_NeverExceedsTrueCount()
	{
		// The producer only enqueues into an empty buffer, so the true count is always 0 or 1.
		SpscRingBuffer<int> buffer = new(1023);
		TimeSpan duration = TimeSpan.FromSeconds(2);
		int stop = 0;

		Task producer = Task.Run(() =>
		{
			int next = 0;
			while (Volatile.Read(ref stop) == 0)
			{
				if (buffer.IsEmpty)
				{
					buffer.TryEnqueue(next++);
				}
			}
		}, TestContext.CancellationToken);

		Task consumer = Task.Run(() =>
		{
			while (Volatile.Read(ref stop) == 0)
			{
				buffer.TryDequeue(out _);
			}
		}, TestContext.CancellationToken);

		Task<int> observer = Task.Run(() =>
		{
			int maxObserved = 0;
			Stopwatch stopwatch = Stopwatch.StartNew();
			while (stopwatch.Elapsed < duration)
			{
				maxObserved = Math.Max(maxObserved, buffer.Count);
			}

			Volatile.Write(ref stop, 1);
			return maxObserved;
		}, TestContext.CancellationToken);

		await Task.WhenAll(producer, consumer, observer).ConfigureAwait(false);
		Assert.IsLessThanOrEqualTo(1, await observer.ConfigureAwait(false), "Count must never report more elements than the buffer held.");
	}

	[TestMethod]
	public void HeadAfterOneLap_DiffersFromHeadBefore()
	{
		// Count accepts a tail read between two equal head reads. If head repeats after one lap, a
		// reader paused for exactly a lap pairs a stale tail with the current head (ABA, #125).
		SpscRingBuffer<int> buffer = new(3);
		int before = ReadIndex(buffer, "head");
		for (int i = 0; i <= buffer.Capacity; i++)
		{
			Assert.IsTrue(buffer.TryEnqueue(i));
			Assert.IsTrue(buffer.TryDequeue(out _));
		}

		Assert.AreNotEqual(before, ReadIndex(buffer, "head"), "Head must not repeat after one lap of the buffer.");
	}

	[TestMethod]
	public void IndicesWrappingPastIntMaxValue_KeepFifoOrderCountAndFullness()
	{
		SpscRingBuffer<int> buffer = new(3);
		WriteIndex(buffer, "head", int.MaxValue - 1);
		WriteIndex(buffer, "tail", int.MaxValue - 1);

		Assert.IsTrue(buffer.IsEmpty);
		for (int i = 0; i < buffer.Capacity; i++)
		{
			Assert.IsTrue(buffer.TryEnqueue(i));
			Assert.AreEqual(i + 1, buffer.Count);
		}

		Assert.IsFalse(buffer.TryEnqueue(99), "A full buffer must reject an enqueue across the wrap.");
		Assert.IsTrue(buffer.TryPeek(out int peeked));
		Assert.AreEqual(0, peeked);
		for (int i = 0; i < buffer.Capacity; i++)
		{
			Assert.IsTrue(buffer.TryDequeue(out int value));
			Assert.AreEqual(i, value);
		}

		Assert.IsTrue(buffer.IsEmpty);
		Assert.AreEqual(0, buffer.Count);
	}

	[TestMethod]
	public async Task Count_SmallBufferWithSeveralObservers_NeverExceedsTrueCount()
	{
		// A small buffer laps quickly, so an observer paused between its two head reads is most
		// likely to come back exactly a lap later. The true count is always 0 or 1.
		SpscRingBuffer<int> buffer = new(3);
		TimeSpan duration = TimeSpan.FromSeconds(2);
		int stop = 0;

		Task producer = Task.Run(() =>
		{
			int next = 0;
			while (Volatile.Read(ref stop) == 0)
			{
				if (buffer.IsEmpty)
				{
					buffer.TryEnqueue(next++);
				}
			}
		}, TestContext.CancellationToken);

		Task consumer = Task.Run(() =>
		{
			while (Volatile.Read(ref stop) == 0)
			{
				buffer.TryDequeue(out _);
			}
		}, TestContext.CancellationToken);

		Task<int>[] observers = [.. Enumerable.Range(0, 4).Select(_ => Task.Run(() =>
		{
			int maxObserved = 0;
			Stopwatch stopwatch = Stopwatch.StartNew();
			while (stopwatch.Elapsed < duration)
			{
				maxObserved = Math.Max(maxObserved, buffer.Count);
			}

			return maxObserved;
		}, TestContext.CancellationToken))];

		int[] maxima = await Task.WhenAll(observers).ConfigureAwait(false);
		Volatile.Write(ref stop, 1);
		await Task.WhenAll(producer, consumer).ConfigureAwait(false);
		Assert.IsLessThanOrEqualTo(1, maxima.Max(), "Count must never report more elements than the buffer held.");
	}

	private static FieldInfo IndexField(string name) =>
		typeof(SpscRingBuffer<int>).GetField(name, BindingFlags.Instance | BindingFlags.NonPublic)
		?? throw new InvalidOperationException($"SpscRingBuffer has no field named {name}.");

	private static int ReadIndex(SpscRingBuffer<int> buffer, string name) =>
		((SpscPaddedIndex)IndexField(name).GetValue(buffer)!).Value;

	private static void WriteIndex(SpscRingBuffer<int> buffer, string name, int value) =>
		IndexField(name).SetValue(buffer, new SpscPaddedIndex { Value = value });

	[TestMethod]
	public void Dequeue_ReferenceType_ReleasesReference()
	{
		SpscRingBuffer<string> buffer = new(4);
		buffer.TryEnqueue("hello");
		Assert.IsTrue(buffer.TryDequeue(out string? value));
		Assert.AreEqual("hello", value);
		Assert.IsTrue(buffer.IsEmpty);
	}
}
