namespace ResolveDependencies.Core;

/// <summary>
/// Domain service building the <see cref="DependencyGraph"/> from the dependency rules.
/// Several rules for the same dependent are read together as its requirement list, in
/// the order the rules appear.
/// </summary>
public sealed class DependencyGraphBuilder
{
    public DependencyGraph Build(IReadOnlyList<Dependency> dependencies)
    {
        var requirements = new Dictionary<PackageVersion, List<PackageVersion>>();
        foreach (var dependency in dependencies)
        {
            if (!requirements.TryGetValue(dependency.Dependent, out var list))
            {
                list = [];
                requirements[dependency.Dependent] = list;
            }

            list.Add(dependency.Requirement);
        }

        return new DependencyGraph(
            requirements.ToDictionary(pair => pair.Key, IReadOnlyList<PackageVersion> (pair) => pair.Value));
    }
}
