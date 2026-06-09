// Copyright (c) ktsu.dev
// All rights reserved.
// Licensed under the MIT license.

namespace ktsu.Containers;

using System;

/// <summary>
/// A fixed-capacity, single-channel circular delay line for audio-rate <see cref="float"/> samples.
/// </summary>
/// <remarks>
/// A delay line stores the most recent samples and lets you read any of them back by their age
/// (the number of samples ago they were written). It is the fundamental building block of delays,
/// echoes, reverbs, comb/all-pass filters, choruses and flangers.
///
/// <para>
/// <b>Real-time safety.</b> The backing store is allocated once in the constructor. After that,
/// <see cref="Write(float)"/>, <see cref="Read(int)"/>, <see cref="ReadInterpolated(float)"/> and
/// <see cref="Process(float, int)"/> never allocate, never take a lock and never block, so they are
/// safe to call from an audio callback. The buffer is intended to be owned and used by a single
/// thread (typically the audio thread).
/// </para>
///
/// <para>
/// <b>Denormal awareness.</b> Feedback delay structures naturally decay towards zero, and on many
/// CPUs the resulting subnormal (denormal) floating-point values are tens to hundreds of times more
/// expensive to process, causing audible CPU spikes. Samples are flushed to zero once their
/// magnitude falls below <see cref="DenormalThreshold"/> on write, keeping the buffer free of
/// denormals without any audible effect.
/// </para>
///
/// <para>
/// The internal capacity is rounded up to a power of two so that index wrapping uses a bitwise AND
/// rather than a modulo.
/// </para>
/// </remarks>
public sealed class DelayLine
{
	/// <summary>
	/// Magnitude at or below which a sample is treated as denormal and flushed to zero on write.
	/// </summary>
	/// <remarks>
	/// The smallest normal <see cref="float"/> is approximately 1.18e-38; this threshold sits well
	/// above the subnormal range so all denormals are eliminated, while only flushing values far
	/// below audibility (roughly -600 dBFS).
	/// </remarks>
	public const float DenormalThreshold = 1e-30f;

	/// <summary>
	/// The backing store, whose length is always a power of two.
	/// </summary>
	private readonly float[] buffer;

	/// <summary>
	/// Bitmask equal to <c>buffer.Length - 1</c>, used to wrap indices.
	/// </summary>
	private readonly int mask;

	/// <summary>
	/// Index of the next slot to be written.
	/// </summary>
	private int writeIndex;

	/// <summary>
	/// Gets the maximum delay, in samples, that can be read back from this delay line.
	/// </summary>
	public int Capacity { get; }

	/// <summary>
	/// Initializes a new instance of the <see cref="DelayLine"/> class that can delay a signal by up
	/// to <paramref name="maxDelaySamples"/> samples.
	/// </summary>
	/// <param name="maxDelaySamples">The maximum delay, in samples, the line must support.</param>
	/// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="maxDelaySamples"/> is less than one.</exception>
	public DelayLine(int maxDelaySamples)
	{
		ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maxDelaySamples);

		// One extra slot is needed so the oldest readable sample (delay == maxDelaySamples) is not
		// the slot about to be overwritten.
		int length = NextPower2(maxDelaySamples + 1);
		buffer = new float[length];
		mask = length - 1;
		Capacity = maxDelaySamples;
	}

	/// <summary>
	/// Writes a new sample into the delay line, advancing the write position by one.
	/// </summary>
	/// <param name="sample">The sample to store. Denormal magnitudes are flushed to zero.</param>
	public void Write(float sample)
	{
		buffer[writeIndex] = FlushDenormal(sample);
		writeIndex = (writeIndex + 1) & mask;
	}

	/// <summary>
	/// Reads the sample that is <paramref name="delaySamples"/> samples in the past.
	/// </summary>
	/// <param name="delaySamples">The delay in samples; <c>0</c> is the most recently written sample and <see cref="Capacity"/> is the oldest.</param>
	/// <returns>The delayed sample.</returns>
	/// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="delaySamples"/> is not in the range <c>[0, Capacity]</c>.</exception>
	/// <remarks>With this convention a delay of <c>d</c> returns <c>x[n - d]</c>, so <see cref="Process(float, int)"/> with a delay of <c>d</c> outputs the input from exactly <c>d</c> samples earlier.</remarks>
	public float Read(int delaySamples)
	{
		ArgumentOutOfRangeException.ThrowIfNegative(delaySamples);
		ArgumentOutOfRangeException.ThrowIfGreaterThan(delaySamples, Capacity);

		// The most recently written sample sits one slot behind the write cursor (delay 0).
		int index = (writeIndex - 1 - delaySamples) & mask;
		return buffer[index];
	}

	/// <summary>
	/// Reads a fractionally delayed sample using linear interpolation between adjacent samples.
	/// </summary>
	/// <param name="delaySamples">The fractional delay in samples; must lie in the range <c>[0, Capacity]</c>.</param>
	/// <returns>The linearly interpolated delayed sample.</returns>
	/// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="delaySamples"/> is not in the range <c>[0, Capacity]</c>.</exception>
	/// <remarks>
	/// Linear interpolation is the standard choice for modulated delays (chorus/flanger). For static
	/// integer delays prefer <see cref="Read(int)"/>, which avoids the interpolation and the high-frequency
	/// roll-off it introduces.
	/// </remarks>
	public float ReadInterpolated(float delaySamples)
	{
		if (float.IsNaN(delaySamples) || delaySamples < 0f || delaySamples > Capacity)
		{
			throw new ArgumentOutOfRangeException(nameof(delaySamples), delaySamples, $"Delay must be in the range [0, {Capacity}].");
		}

		int delayInt = (int)delaySamples;
		float frac = delaySamples - delayInt;

		int index0 = (writeIndex - 1 - delayInt) & mask;
		// The interpolation partner is one sample older; clamp so we never read past Capacity.
		int olderDelay = Math.Min(delayInt + 1, Capacity);
		int index1 = (writeIndex - 1 - olderDelay) & mask;

		float s0 = buffer[index0];
		float s1 = buffer[index1];
		return s0 + ((s1 - s0) * frac);
	}

	/// <summary>
	/// Writes <paramref name="input"/> and returns the sample delayed by <paramref name="delaySamples"/> in a single call.
	/// </summary>
	/// <param name="input">The new sample to store.</param>
	/// <param name="delaySamples">The delay in samples; must lie in the range <c>[0, Capacity]</c>.</param>
	/// <returns>The delayed output sample, equal to the input from <paramref name="delaySamples"/> samples earlier.</returns>
	/// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="delaySamples"/> is not in the range <c>[0, Capacity]</c>.</exception>
	/// <remarks>This is the common per-sample idiom: push the input, then tap the output.</remarks>
	public float Process(float input, int delaySamples)
	{
		Write(input);
		return Read(delaySamples);
	}

	/// <summary>
	/// Resets the delay line, zeroing all stored samples and the write position.
	/// </summary>
	public void Clear()
	{
		Array.Clear(buffer, 0, buffer.Length);
		writeIndex = 0;
	}

	/// <summary>
	/// Flushes denormal magnitudes to zero.
	/// </summary>
	/// <param name="value">The value to sanitise.</param>
	/// <returns>Zero if the magnitude is at or below <see cref="DenormalThreshold"/>; otherwise the value unchanged.</returns>
	private static float FlushDenormal(float value) =>
		value is < DenormalThreshold and > -DenormalThreshold ? 0f : value;

	/// <summary>
	/// Calculates the next power of two greater than or equal to the specified value.
	/// </summary>
	/// <param name="v">The value to round up.</param>
	/// <returns>The next power of two.</returns>
	private static int NextPower2(int v)
	{
		v--;
		v |= v >> 1;
		v |= v >> 2;
		v |= v >> 4;
		v |= v >> 8;
		v |= v >> 16;
		v++;
		return v;
	}
}
