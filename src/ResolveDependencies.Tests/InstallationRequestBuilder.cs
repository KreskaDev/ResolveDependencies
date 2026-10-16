using ResolveDependencies.Core;

namespace ResolveDependencies.Tests;

/// <summary>
/// Fluent helper for building an <see cref="InstallationRequest"/> in tests. A call mirrors
/// one line of an input file: <c>Install</c> is a package line, <c>Requires</c> is a
/// dependency line with the dependent first and its requirements after it.
/// </summary>
public sealed class InstallationRequestBuilder
{
    private readonly List<PackageVersion> _packages = [];
    private readonly List<Dependency> _dependencies = [];

    public InstallationRequestBuilder Install((string Name, string Version) package)
    {
        _packages.Add(new PackageVersion(package.Name, package.Version));
        return this;
    }

    public InstallationRequestBuilder Requires(
        (string Name, string Version) dependent,
        params (string Name, string Version)[] requirements)
    {
        var dependentVersion = new PackageVersion(dependent.Name, dependent.Version);
        foreach (var (name, version) in requirements)
        {
            _dependencies.Add(new Dependency(dependentVersion, new PackageVersion(name, version)));
        }

        return this;
    }

    public InstallationRequest Build() => new(_packages.ToArray(), _dependencies.ToArray());
}
