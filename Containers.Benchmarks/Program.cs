// Copyright (c) 2023-2026 ktsu-dev contributors

namespace ktsu.Containers.Benchmarks;

using BenchmarkDotNet.Running;

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
