using ResolveDependencies.Core;
using ResolveDependencies.Core.Validation;

namespace ResolveDependencies.Tests.Core;

/// <summary>
/// One case per semantic rule S1-S11 from docs/algorithm_spec.md plus the two examples
/// from docs/story/ResolveDependencies.md. Conflicts are asserted with their versions in
/// the order the breadth-first traversal meets them.
/// </summary>
public class DependencyResolverRuleTests
{
    private readonly DependencyResolver _resolver = new();

    [Fact]
    public void Resolve_TaskExampleValid_ReturnsValid()
    {
        var request = new InstallationRequestBuilder()
            .Install(("A", "1"))
            .Install(("B", "1"))
            .Requires(("A", "1"), ("B", "1"))
            .Requires(("A", "2"), ("B", "2"))
            .Requires(("C", "1"), ("B", "1"))
            .Build();

        var result = _resolver.Resolve(request);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Resolve_TaskExampleInvalid_ReturnsConflict()
    {
        var request = new InstallationRequestBuilder()
            .Install(("A", "1"))
            .Install(("B", "2"))
            .Requires(("A", "1"), ("B", "1"))
            .Requires(("A", "2"), ("B", "2"))
            .Requires(("C", "1"), ("B", "1"))
            .Build();

        var result = _resolver.Resolve(request);

        AssertConflicts(result, ("B", ["2", "1"]));
    }

    [Fact]
    public void Resolve_S1TransitiveRequirement_ReturnsConflict()
    {
        var request = new InstallationRequestBuilder()
            .Install(("A", "2"))
            .Install(("B", "2"))
            .Requires(("A", "2"), ("C", "3"))
            .Requires(("C", "3"), ("D", "4"))
            .Requires(("D", "4"), ("B", "1"))
            .Build();

        var result = _resolver.Resolve(request);

        AssertConflicts(result, ("B", ["2", "1"]));
    }

    [Fact]
    public void Resolve_S2Cycle_ReturnsValid()
    {
        var request = new InstallationRequestBuilder()
            .Install(("B", "2"))
            .Requires(("A", "1"), ("B", "2"))
            .Requires(("B", "2"), ("A", "1"))
            .Build();

        var result = _resolver.Resolve(request);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Resolve_S3SelfDependency_ReturnsValid()
    {
        var request = new InstallationRequestBuilder()
            .Install(("B", "1"))
            .Requires(("B", "1"), ("B", "1"))
            .Build();

        var result = _resolver.Resolve(request);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Resolve_S4RulesOfNotInstalledVersion_AreIgnored()
    {
        var request = new InstallationRequestBuilder()
            .Install(("A", "2"))
            .Install(("B", "2"))
            .Requires(("A", "1"), ("B", "1"))
            .Requires(("A", "1"), ("B", "2"))
            .Build();

        var result = _resolver.Resolve(request);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Resolve_S5SeveralRulesOfOneVersionDisagree_ReturnsConflict()
    {
        var request = new InstallationRequestBuilder()
            .Install(("A", "1"))
            .Requires(("A", "1"), ("B", "2"))
            .Requires(("A", "1"), ("B", "1"))
            .Build();

        var result = _resolver.Resolve(request);

        AssertConflicts(result, ("B", ["2", "1"]));
    }

    [Fact]
    public void Resolve_S6ConflictBetweenTwoRequirements_ReturnsConflict()
    {
        var request = new InstallationRequestBuilder()
            .Install(("A", "1"))
            .Install(("C", "1"))
            .Requires(("A", "1"), ("B", "1"))
            .Requires(("C", "1"), ("B", "2"))
            .Build();

        var result = _resolver.Resolve(request);

        AssertConflicts(result, ("B", ["1", "2"]));
    }

    [Fact]
    public void Resolve_S7PackageOnlyAsRequirement_ReturnsValid()
    {
        var request = new InstallationRequestBuilder()
            .Install(("A", "1"))
            .Requires(("A", "1"), ("B", "1"))
            .Build();

        var result = _resolver.Resolve(request);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Resolve_S8VersionsComparedExactly_ReturnsConflict()
    {
        var request = new InstallationRequestBuilder()
            .Install(("A", "1"))
            .Install(("B", "1"))
            .Requires(("A", "1"), ("B", "1.0"))
            .Build();

        var result = _resolver.Resolve(request);

        AssertConflicts(result, ("B", ["1", "1.0"]));
    }

    [Fact]
    public void Resolve_S8PackageNamesCaseSensitive_ReturnsValid()
    {
        var request = new InstallationRequestBuilder()
            .Install(("a", "1"))
            .Install(("A", "2"))
            .Build();

        var result = _resolver.Resolve(request);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Resolve_S9VersionWithoutRules_HasNoRequirements()
    {
        var request = new InstallationRequestBuilder()
            .Install(("C", "3"))
            .Requires(("A", "1"), ("B", "1"))
            .Build();

        var result = _resolver.Resolve(request);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Resolve_S10DuplicateOnInstallList_ReturnsValid()
    {
        var request = new InstallationRequestBuilder()
            .Install(("A", "1"))
            .Install(("A", "1"))
            .Build();

        var result = _resolver.Resolve(request);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Resolve_S10TwoVersionsOnInstallList_ReturnsConflict()
    {
        var request = new InstallationRequestBuilder()
            .Install(("A", "1"))
            .Install(("A", "2"))
            .Build();

        var result = _resolver.Resolve(request);

        AssertConflicts(result, ("A", ["1", "2"]));
    }

    [Fact]
    public void Resolve_S11EmptyInstallList_ReturnsValid()
    {
        var request = new InstallationRequestBuilder().Build();

        var result = _resolver.Resolve(request);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Resolve_SeveralConflictingPackages_ReportsAllInOrderMet()
    {
        var request = new InstallationRequestBuilder()
            .Install(("A", "1"))
            .Install(("B", "1"))
            .Install(("A", "2"))
            .Install(("B", "2"))
            .Install(("A", "3"))
            .Build();

        var result = _resolver.Resolve(request);

        AssertConflicts(result, ("A", ["1", "2", "3"]), ("B", ["1", "2"]));
    }

    [Fact]
    public void Resolve_ConflictingVersion_DoesNotFollowItsRequirements()
    {
        var request = new InstallationRequestBuilder()
            .Install(("A", "1"))
            .Install(("A", "2"))
            .Requires(("A", "2"), ("B", "9"))
            .Install(("B", "1"))
            .Build();

        var result = _resolver.Resolve(request);

        AssertConflicts(result, ("A", ["1", "2"]));
    }

    private static void AssertConflicts(ValidationResult result, params (string Package, string[] Versions)[] expected)
    {
        Assert.False(result.IsValid);
        Assert.Equal(expected.Select(conflict => conflict.Package), result.Conflicts.Select(conflict => conflict.Package));
        foreach (var (package, versions) in expected)
        {
            Assert.Equal(versions, result.Conflicts.Single(conflict => conflict.Package == package).Versions);
        }
    }
}
