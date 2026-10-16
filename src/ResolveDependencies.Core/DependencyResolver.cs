namespace ResolveDependencies.Core;

/// <summary>
/// Domain service. Checks whether an installation request is valid: after following the
/// dependency rules transitively from the install list, no package may be required in
/// more than one version. Assumes the request is structurally valid.
/// </summary>
public sealed class DependencyResolver
{
    public ValidationResult Resolve(InstallationRequest request)
    {
        throw new NotImplementedException();
    }
}
