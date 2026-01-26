# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

ktsu.Containers is a high-performance .NET collection library providing specialized container implementations: `OrderedCollection<T>`, `OrderedSet<T>`, `OrderedMap<TKey, TValue>`, `RingBuffer<T>`, `InsertionOrderCollection<T>`, `InsertionOrderSet<T>`, `InsertionOrderMap<TKey, TValue>`, `ContiguousCollection<T>`, `ContiguousSet<T>`, and `ContiguousMap<TKey, TValue>`.

The library uses the ktsu.Sdk MSBuild SDK for standardized build configuration and multi-targets .NET 9.0, 8.0, 7.0, 6.0, and netstandard2.1.

## Build and Test Commands

```bash
# Build
dotnet build

# Run all tests
dotnet test

# Run a single test by filter
dotnet test --filter "FullyQualifiedName~ClassName.MethodName"

# Run benchmarks (quick development feedback)
.\scripts\run-benchmarks.ps1 -Target Quick

# Run specific container benchmarks
.\scripts\run-benchmarks.ps1 -Target OrderedSet -Export Html

# Run full benchmark suite
.\scripts\run-benchmarks.ps1 -Target All -Export Html

# Manual benchmark with filter
dotnet run --project Containers.Benchmarks --configuration Release -- --filter "*Add*"
```

## Architecture

### Project Structure

- `Containers/` - Main library with all container implementations
- `Containers.Test/` - MSTest-based unit tests (targets net9.0 only)
- `Containers.Benchmarks/` - BenchmarkDotNet performance benchmarks

### Container Categories

**Sorted Containers** (maintain elements in sorted order via binary search):
- `OrderedCollection<T>` - Sorted collection with duplicates allowed
- `OrderedSet<T>` - Sorted unique elements with `ISet<T>` support
- `OrderedMap<TKey, TValue>` - Sorted key-value pairs

**Insertion-Order Containers** (preserve insertion order):
- `InsertionOrderCollection<T>`, `InsertionOrderSet<T>`, `InsertionOrderMap<TKey, TValue>`

**Contiguous Memory Containers** (optimized for cache efficiency):
- `ContiguousCollection<T>`, `ContiguousSet<T>`, `ContiguousMap<TKey, TValue>`

**Specialized**:
- `RingBuffer<T>` - Fixed-size circular buffer with O(1) operations

## Coding Standards

- Use explicit types instead of `var`
- Use `[]` for collection initialization instead of `new List<T>()`
- Use expression bodies for simple methods
- Use `ArgumentOutOfRangeException.ThrowIfNegative()` and `ThrowIfNegativeOrEqual()` instead of manual throws
- Types overriding `Equals` must implement `IEquatable<T>` and use `HashCode.Combine()` for `GetHashCode()`
- Add XML documentation for all public members

## Testing Conventions

- Use MSTest framework (`[TestClass]`, `[TestMethod]`)
- Use `CollectionAssert.AreEqual` for collection equality (use `.ToArray()` if type inference issues arise)
- Test edge cases: empty containers, boundary conditions, constructor validation

## Benchmarking Standards

- Benchmark classes must be public with `[MemoryDiagnoser]` and `[SimpleJob]` attributes
- Use `[Params(100, 1000, 10000)]` for scalability testing
- Use fixed random seed (`new Random(42)`) for reproducibility
- Always return results from benchmark methods to prevent dead code elimination
- Compare against equivalent .NET collections (List, HashSet, SortedSet, Queue, etc.)
