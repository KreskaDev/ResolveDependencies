namespace ResolveDependencies.Core.Validation;

/// <summary>
/// Outcome of validating an installation request. Valid when no conflict was found.
/// </summary>
public sealed record ValidationResult(IReadOnlyList<Conflict> Conflicts)
{
    public bool IsValid => Conflicts.Count == 0;
}
