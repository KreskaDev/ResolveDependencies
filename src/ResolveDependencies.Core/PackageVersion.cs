namespace ResolveDependencies.Core;

/// <summary>
/// A package in one specific version. Value equality is what the whole algorithm relies on.
/// </summary>
public readonly record struct PackageVersion(string Name, string Version);
