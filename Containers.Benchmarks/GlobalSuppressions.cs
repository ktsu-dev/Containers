// Copyright (c) ktsu.dev
// All rights reserved.
// Licensed under the MIT license.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Globalization", "CA1303:Do not pass literals as localized parameters", Justification = "Benchmark console output doesn't need localization")]
[assembly: SuppressMessage("Design", "CA1002:Do not expose generic lists", Justification = "Benchmarks need to return specific collection types for measurement")]
[assembly: SuppressMessage("Naming", "CA1707:Identifiers should not contain underscores", Justification = "Benchmark method names use underscores for clarity")]
[assembly: SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes", Justification = "Benchmark classes are instantiated by BenchmarkDotNet via reflection")]
[assembly: SuppressMessage("Performance", "CA1852:Seal internal types", Justification = "Benchmark classes should not be sealed as they may be inherited by BenchmarkDotNet")]
[assembly: SuppressMessage("Design", "CA1515:Consider making public types internal", Justification = "Benchmark classes must be public for BenchmarkDotNet discovery")]
[assembly: SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "Benchmark methods cannot be static - they need instance context for BenchmarkDotNet")]
