namespace ResolveDependencies.Core;

/// <summary>
/// One dependency rule: <paramref name="Dependent"/> requires <paramref name="Requirement"/>.
/// </summary>
public sealed record Dependency(PackageVersion Dependent, PackageVersion Requirement);
