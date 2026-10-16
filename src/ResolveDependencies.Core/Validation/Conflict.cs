namespace ResolveDependencies.Core.Validation;

/// <summary>
/// A package that is required in more than one version. <paramref name="Versions"/> lists
/// the distinct versions in the order they were met.
/// </summary>
public sealed record Conflict(string Package, IReadOnlyList<string> Versions);
