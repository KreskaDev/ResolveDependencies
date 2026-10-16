namespace ResolveDependencies.Core.Model;

/// <summary>
/// Parsed input: the packages to install plus the dependency rules.
/// </summary>
public sealed record InstallationRequest(
    IReadOnlyList<PackageVersion> Packages,
    IReadOnlyList<Dependency> Dependencies);
