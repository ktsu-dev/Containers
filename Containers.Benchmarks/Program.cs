// Copyright (c) ktsu.dev
// All rights reserved.
// Licensed under the MIT license.

using BenchmarkDotNet.Running;
using ktsu.Containers.Benchmarks;

namespace ktsu.Containers.Benchmarks;

/// <summary>
/// Main program entry point for running container benchmarks.
/// </summary>
internal static class Program
{
	/// <summary>
	/// Main entry point that runs all benchmarks.
	/// </summary>
	/// <param name="args">Command line arguments passed to BenchmarkDotNet.</param>
	public static void Main(string[] args)
	{
		Console.WriteLine("ktsu.Containers Performance Benchmarks");
		Console.WriteLine("======================================");
		Console.WriteLine();

		// Run all benchmarks
		BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);
	}
}
