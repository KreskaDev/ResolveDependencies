namespace ResolveDependencies.Core;

/// <summary>
/// Domain service answering whether an installation request is valid: builds the
/// dependency graph from the rules and validates the install list against it. Assumes
/// the request is structurally valid.
/// </summary>
public sealed class DependencyResolver(DependencyGraphBuilder graphBuilder, InstallationValidator validator)
{
    public DependencyResolver()
        : this(new DependencyGraphBuilder(), new InstallationValidator())
    {
    }

    public ValidationResult Resolve(InstallationRequest request)
    {
        var graph = graphBuilder.Build(request.Dependencies);
        return validator.Validate(request.Packages, graph);
    }
}
