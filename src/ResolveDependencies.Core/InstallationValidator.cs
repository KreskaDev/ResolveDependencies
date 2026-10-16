namespace ResolveDependencies.Core;

/// <summary>
/// Domain service checking the semantic rules: a breadth-first traversal of the
/// dependency graph from the install list, allowing one version per package. Every
/// conflicting package is reported with its versions in the order they were met.
/// </summary>
public sealed class InstallationValidator
{
    public ValidationResult Validate(IReadOnlyList<PackageVersion> packages, DependencyGraph graph)
    {
        var required = new Dictionary<string, string>();
        var visited = new HashSet<PackageVersion>();
        var conflicts = new Dictionary<string, List<string>>();
        var conflictOrder = new List<string>();
        var queue = new Queue<PackageVersion>(packages);

        while (queue.TryDequeue(out var current))
        {
            if (!visited.Add(current))
            {
                continue;
            }

            if (required.TryGetValue(current.Name, out var requiredVersion) && requiredVersion != current.Version)
            {
                RecordConflict(current, requiredVersion);
                continue;
            }

            required[current.Name] = current.Version;
            foreach (var requirement in graph.RequirementsOf(current))
            {
                queue.Enqueue(requirement);
            }
        }

        return new ValidationResult(
            conflictOrder.Select(package => new Conflict(package, conflicts[package])).ToArray());

        void RecordConflict(PackageVersion current, string requiredVersion)
        {
            if (!conflicts.TryGetValue(current.Name, out var versions))
            {
                versions = [requiredVersion];
                conflicts[current.Name] = versions;
                conflictOrder.Add(current.Name);
            }

            if (!versions.Contains(current.Version))
            {
                versions.Add(current.Version);
            }
        }
    }
}
