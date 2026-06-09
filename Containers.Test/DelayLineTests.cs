// Copyright (c) ktsu.dev
// All rights reserved.
// Licensed under the MIT license.

namespace ktsu.Containers.Tests;

using Microsoft.VisualStudio.TestTools.UnitTesting;

[TestClass]
public class DelayLineTests
{
	[TestMethod]
	public void Constructor_NonPositiveCapacity_Throws()
	{
		Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => new DelayLine(0));
		Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => new DelayLine(-4));
	}

	[TestMethod]
	public void Capacity_MatchesRequestedMaxDelay()
	{
		DelayLine line = new(100);
		Assert.AreEqual(100, line.Capacity);
	}

	[TestMethod]
	public void NewDelayLine_ReadsZero()
	{
		DelayLine line = new(8);
		for (int delay = 0; delay <= line.Capacity; delay++)
		{
			Assert.AreEqual(0f, line.Read(delay));
		}
	}

	[TestMethod]
	public void Read_ReturnsSampleNSamplesInThePast()
	{
		DelayLine line = new(8);
		line.Write(1f);
		line.Write(2f);
		line.Write(3f);

		// Delay 0 == most recent, delay 2 == oldest of the three writes.
		Assert.AreEqual(3f, line.Read(0));
		Assert.AreEqual(2f, line.Read(1));
		Assert.AreEqual(1f, line.Read(2));
	}

	[TestMethod]
	public void Process_OutputsInputFromExactlyDelaySamplesEarlier()
	{
		DelayLine line = new(4);
		// Process(x, 4) yields x[n - 4]: zero until the line has been primed with 4 samples.
		Assert.AreEqual(0f, line.Process(10f, 4));
		Assert.AreEqual(0f, line.Process(20f, 4));
		Assert.AreEqual(0f, line.Process(30f, 4));
		Assert.AreEqual(0f, line.Process(40f, 4));
		Assert.AreEqual(10f, line.Process(50f, 4));
		Assert.AreEqual(20f, line.Process(60f, 4));
	}

	[TestMethod]
	public void Process_ZeroDelay_ReturnsInput()
	{
		DelayLine line = new(4);
		Assert.AreEqual(7f, line.Process(7f, 0));
		Assert.AreEqual(9f, line.Process(9f, 0));
	}

	[TestMethod]
	public void Read_OutOfRange_Throws()
	{
		DelayLine line = new(8);
		Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => line.Read(-1));
		Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => line.Read(9));
	}

	[TestMethod]
	public void ReadInterpolated_HalfwayBetweenSamples_AveragesNeighbours()
	{
		DelayLine line = new(8);
		line.Write(0f);
		line.Write(10f);

		// delay 0 -> 10 (newest), delay 1 -> 0 (older). 0.5 interpolates halfway.
		Assert.AreEqual(5f, line.ReadInterpolated(0.5f), 1e-6f);
	}

	[TestMethod]
	public void ReadInterpolated_IntegerDelay_MatchesRead()
	{
		DelayLine line = new(8);
		line.Write(3f);
		line.Write(7f);
		line.Write(11f);

		Assert.AreEqual(line.Read(1), line.ReadInterpolated(1f), 1e-6f);
	}

	[TestMethod]
	public void ReadInterpolated_OutOfRange_Throws()
	{
		DelayLine line = new(8);
		Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => line.ReadInterpolated(-0.5f));
		Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => line.ReadInterpolated(8.5f));
		Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => line.ReadInterpolated(float.NaN));
	}

	[TestMethod]
	public void Write_FlushesDenormalsToZero()
	{
		DelayLine line = new(4);
		float denormal = float.Epsilon; // smallest subnormal float, well below the threshold
		line.Write(denormal);
		Assert.AreEqual(0f, line.Read(0), "Denormal input must be flushed to zero.");
	}

	[TestMethod]
	public void Write_KeepsNormalSmallValues()
	{
		DelayLine line = new(4);
		const float audible = 1e-6f; // ~ -120 dBFS, a normal float that must be preserved
		line.Write(audible);
		Assert.AreEqual(audible, line.Read(0));
	}

	[TestMethod]
	public void Clear_ZeroesAllSamples()
	{
		DelayLine line = new(4);
		line.Write(1f);
		line.Write(2f);
		line.Clear();

		for (int delay = 0; delay <= line.Capacity; delay++)
		{
			Assert.AreEqual(0f, line.Read(delay));
		}
	}

	[TestMethod]
	public void WrapAround_LongStreamReadsCorrectDelay()
	{
		DelayLine line = new(3);
		float previous = 0f;
		for (int i = 1; i <= 1000; i++)
		{
			// Process(i, 1) returns the value written on the previous call (i - 1), 0 on the first.
			float output = line.Process(i, 1);
			Assert.AreEqual(i - 1, output);
			previous = output;
		}

		Assert.AreEqual(999f, previous);
	}
}
