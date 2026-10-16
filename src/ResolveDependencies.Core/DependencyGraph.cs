namespace ResolveDependencies.Core;

/// <summary>
/// Directed graph of the dependency rules: every package version maps to the package
/// versions it requires, in rule order. A version without rules has no requirements.
/// </summary>
public sealed class DependencyGraph
{
    private readonly IReadOnlyDictionary<PackageVersion, IReadOnlyList<PackageVersion>> _requirements;

    public DependencyGraph(IReadOnlyDictionary<PackageVersion, IReadOnlyList<PackageVersion>> requirements)
    {
        _requirements = requirements;
    }

    public IReadOnlyList<PackageVersion> RequirementsOf(PackageVersion packageVersion) =>
        _requirements.TryGetValue(packageVersion, out var requirements) ? requirements : [];
}
