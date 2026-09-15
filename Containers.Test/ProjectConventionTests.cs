// Copyright (c) 2023-2026 ktsu-dev contributors

namespace ktsu.Containers.Tests;

using System.Xml.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

/// <summary>
/// Guards the project conventions behind the estate-wide naming audit: every project in the
/// repository imports ktsu.Sdk and lets it derive the assembly name and root namespace from the
/// solution-relative folder path, instead of hand-writing either value.
/// </summary>
/// <remarks>
/// Containers.Benchmarks drifted off ktsu.Sdk and hand-wrote both, which left its assembly named
/// <c>Containers.Benchmarks</c> while its namespace was <c>ktsu.Containers.Benchmarks</c>.
/// </remarks>
[TestClass]
public class ProjectConventionTests
{
	/// <summary>
	/// Walks up from the test binary until the directory holding the solution file is found.
	/// </summary>
	private static DirectoryInfo FindRepositoryRoot()
	{
		DirectoryInfo? directory = new(AppContext.BaseDirectory);
		while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "Containers.sln")))
		{
			directory = directory.Parent;
		}

		Assert.IsNotNull(directory, $"Could not locate Containers.sln above '{AppContext.BaseDirectory}'.");
		return directory;
	}

	/// <summary>
	/// Every project file in the repository, excluding build output.
	/// </summary>
	private static List<FileInfo> EnumerateProjects()
	{
		string binSegment = $"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}";
		string objSegment = $"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}";

		List<FileInfo> projects =
		[
			.. FindRepositoryRoot()
				.EnumerateFiles("*.csproj", SearchOption.AllDirectories)
				.Where(file => !file.FullName.Contains(binSegment, StringComparison.Ordinal))
				.Where(file => !file.FullName.Contains(objSegment, StringComparison.Ordinal))
				.OrderBy(file => file.FullName, StringComparer.Ordinal)
		];

		Assert.IsGreaterThan(0, projects.Count, "Expected at least one project file in the repository.");
		return projects;
	}

	private static bool ImportsKtsuSdk(XDocument project)
	{
		string? sdkAttribute = project.Root?.Attribute("Sdk")?.Value;
		if (sdkAttribute is not null && sdkAttribute.Contains("ktsu.Sdk", StringComparison.Ordinal))
		{
			return true;
		}

		return project.Root?
			.Elements()
			.Where(element => element.Name.LocalName == "Sdk")
			.Select(element => element.Attribute("Name")?.Value)
			.Any(name => name is not null && name.StartsWith("ktsu.Sdk", StringComparison.Ordinal)) ?? false;
	}

	[TestMethod]
	public void EveryProject_ImportsKtsuSdk()
	{
		List<string> violations = [];

		foreach (FileInfo project in EnumerateProjects())
		{
			if (!ImportsKtsuSdk(XDocument.Load(project.FullName)))
			{
				violations.Add(project.Name);
			}
		}

		Assert.IsEmpty(violations, $"These projects do not import ktsu.Sdk, so their identity is not derived: {string.Join(", ", violations)}");
	}

	[TestMethod]
	public void NoProject_HandWritesAssemblyNameOrRootNamespace()
	{
		List<string> violations = [];

		foreach (FileInfo project in EnumerateProjects())
		{
			XDocument document = XDocument.Load(project.FullName);
			List<string> overrides =
			[
				.. document.Descendants()
					.Select(element => element.Name.LocalName)
					.Where(name => name is "AssemblyName" or "RootNamespace")
					.Distinct(StringComparer.Ordinal)
					.OrderBy(name => name, StringComparer.Ordinal)
			];

			if (overrides.Count > 0)
			{
				violations.Add($"{project.Name} ({string.Join(" and ", overrides)})");
			}
		}

		Assert.IsEmpty(violations, $"ktsu.Sdk derives both values from the folder path; these projects override them: {string.Join(", ", violations)}");
	}
}
