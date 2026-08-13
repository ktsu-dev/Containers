// Copyright (c) 2023-2026 ktsu-dev contributors

namespace ktsu.Containers.Tests;

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
	public void Dequeue_ReferenceType_ReleasesReference()
	{
		SpscRingBuffer<string> buffer = new(4);
		buffer.TryEnqueue("hello");
		Assert.IsTrue(buffer.TryDequeue(out string? value));
		Assert.AreEqual("hello", value);
		Assert.IsTrue(buffer.IsEmpty);
	}
}
