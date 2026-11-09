// Copyright (c) 2023-2026 ktsu-dev contributors

namespace ktsu.Containers.Tests;

using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using Microsoft.VisualStudio.TestTools.UnitTesting;

/// <summary>
/// Guards the nullability annotation on every map's <c>TryGetValue</c>: its <c>out</c> value is
/// <c>[MaybeNullWhen(false)]</c>, as <see cref="IDictionary{TKey, TValue}.TryGetValue"/> is, so a caller
/// who reads it after a miss gets a warning rather than a <see cref="NullReferenceException"/>.
/// </summary>
/// <remarks>
/// <see cref="ContiguousMap{TKey, TValue}"/> had no annotation at all (#96), and the other two maps had
/// it only behind <c>#if NET5_0_OR_GREATER</c>, so the netstandard assets that .NET Framework and Unity
/// consumers resolve shipped without it (#105). The attribute is in netstandard2.1 and Polyfill supplies
/// it for netstandard2.0, so every target can carry it.
/// </remarks>
[TestClass]
public class MapTryGetValueNullabilityTests
{
	private const string MaybeNullWhenName = "System.Diagnostics.CodeAnalysis.MaybeNullWhenAttribute";

	private static readonly string[] MapTypeNames =
	[
		"ktsu.Containers.ContiguousMap`2",
		"ktsu.Containers.InsertionOrderMap`2",
		"ktsu.Containers.OrderedMap`2",
	];

	/// <summary>
	/// Tests that the assembly the tests run against annotates each map's <c>out</c> value.
	/// </summary>
	[TestMethod]
	[DataRow(typeof(ContiguousMap<,>))]
	[DataRow(typeof(InsertionOrderMap<,>))]
	[DataRow(typeof(OrderedMap<,>))]
	public void TryGetValue_OutValue_IsMaybeNullWhenFalse(Type mapType)
	{
		ArgumentNullException.ThrowIfNull(mapType);

		MethodInfo? method = mapType.GetMethods()
			.SingleOrDefault(candidate => candidate.Name == "TryGetValue" && candidate.GetParameters().Length == 2);
		Assert.IsNotNull(method, $"{mapType.Name} has no public two-parameter TryGetValue.");

		MaybeNullWhenAttribute? attribute = method.GetParameters()[1].GetCustomAttribute<MaybeNullWhenAttribute>();
		Assert.IsNotNull(attribute, $"{mapType.Name}.TryGetValue's out value is not [MaybeNullWhen(false)].");
		Assert.IsFalse(attribute.ReturnValue, $"{mapType.Name}.TryGetValue's out value is [MaybeNullWhen(true)].");
	}

	/// <summary>
	/// Tests that every target the library was built for, the netstandard ones included, annotates
	/// each map's <c>out</c> value.
	/// </summary>
	/// <remarks>
	/// The test project runs on one framework, so only the built assets can show what the others
	/// say. They are read as metadata rather than loaded, since a netstandard2.0 asset carries
	/// Polyfill's own copy of the attribute. A build of the test project alone builds only the
	/// library's matching target; a solution build, which is what CI runs, builds them all.
	/// </remarks>
	[TestMethod]
	public void TryGetValue_OutValue_IsMaybeNullWhenFalse_InEveryBuiltTarget()
	{
		List<string> assets = BuiltLibraryAssets();
		if (!assets.Any(path => Path.GetFileName(Path.GetDirectoryName(path))!.StartsWith("netstandard", StringComparison.Ordinal)))
		{
			Assert.Inconclusive("No netstandard build of the library was found; build the solution to check every target.");
			return;
		}

		List<string> missing = [];
		foreach (string asset in assets)
		{
			string target = Path.GetFileName(Path.GetDirectoryName(asset))!;
			using FileStream stream = File.OpenRead(asset);
			using PEReader pe = new(stream);
			MetadataReader reader = pe.GetMetadataReader();

			foreach (TypeDefinitionHandle typeHandle in reader.TypeDefinitions)
			{
				TypeDefinition type = reader.GetTypeDefinition(typeHandle);
				string typeName = $"{reader.GetString(type.Namespace)}.{reader.GetString(type.Name)}";
				if (!MapTypeNames.Contains(typeName))
				{
					continue;
				}

				foreach (MethodDefinitionHandle methodHandle in type.GetMethods())
				{
					MethodDefinition method = reader.GetMethodDefinition(methodHandle);
					if (reader.GetString(method.Name) != "TryGetValue")
					{
						continue;
					}

					bool annotated = method.GetParameters()
						.Select(reader.GetParameter)
						.Where(parameter => parameter.SequenceNumber == 2)
						.SelectMany(parameter => parameter.GetCustomAttributes())
						.Any(attribute => AttributeTypeName(reader, reader.GetCustomAttribute(attribute)) == MaybeNullWhenName);

					if (!annotated)
					{
						missing.Add($"{target}: {typeName}.TryGetValue");
					}
				}
			}
		}

		Assert.IsEmpty(missing, $"Unannotated out values:{Environment.NewLine}{string.Join(Environment.NewLine, missing)}");
	}

	/// <summary>
	/// Finds the library's built assets for the configuration the tests were built in.
	/// </summary>
	private static List<string> BuiltLibraryAssets()
	{
		// The test binary lives in Containers.Test/bin/{Configuration}/{TargetFramework}/.
		DirectoryInfo testTarget = new(AppContext.BaseDirectory.TrimEnd(Path.DirectorySeparatorChar));
		string configuration = testTarget.Parent!.Name;

		DirectoryInfo? root = testTarget;
		while (root is not null && !File.Exists(Path.Combine(root.FullName, "Containers.sln")))
		{
			root = root.Parent;
		}

		Assert.IsNotNull(root, $"Could not locate Containers.sln above '{AppContext.BaseDirectory}'.");

		DirectoryInfo libraryBin = new(Path.Combine(root.FullName, "Containers", "bin", configuration));
		Assert.IsTrue(libraryBin.Exists, $"The library has no build output at '{libraryBin.FullName}'.");

		return
		[
			.. libraryBin.EnumerateDirectories()
				.Select(target => Path.Combine(target.FullName, "ktsu.Containers.dll"))
				.Where(File.Exists)
				.OrderBy(path => path, StringComparer.Ordinal)
		];
	}

	/// <summary>
	/// Names the type of a custom attribute, whether it is defined in the assembly or referenced.
	/// </summary>
	private static string AttributeTypeName(MetadataReader reader, CustomAttribute attribute)
	{
		EntityHandle parent = attribute.Constructor.Kind switch
		{
			HandleKind.MemberReference => reader.GetMemberReference((MemberReferenceHandle)attribute.Constructor).Parent,
			HandleKind.MethodDefinition => reader.GetMethodDefinition((MethodDefinitionHandle)attribute.Constructor).GetDeclaringType(),
			_ => default,
		};

		(StringHandle space, StringHandle name) = parent.Kind switch
		{
			HandleKind.TypeReference => (reader.GetTypeReference((TypeReferenceHandle)parent).Namespace, reader.GetTypeReference((TypeReferenceHandle)parent).Name),
			HandleKind.TypeDefinition => (reader.GetTypeDefinition((TypeDefinitionHandle)parent).Namespace, reader.GetTypeDefinition((TypeDefinitionHandle)parent).Name),
			_ => (default, default),
		};

		return name.IsNil ? string.Empty : $"{reader.GetString(space)}.{reader.GetString(name)}";
	}
}
