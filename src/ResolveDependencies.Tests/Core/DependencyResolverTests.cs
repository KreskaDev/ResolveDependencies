using ResolveDependencies.Core;

namespace ResolveDependencies.Tests.Core;

/// <summary>
/// Golden cases from docs/story/goldenData. Each test builds the request exactly as the
/// parser would read it from inputNNN.txt and asserts the verdict found in outputNNN.txt.
/// </summary>
public class DependencyResolverTests
{
    private readonly DependencyResolver _resolver = new();

    [Fact]
    public void Resolve_Golden000VersionAsArbitraryString_ReturnsValid()
    {
        var request = new InstallationRequestBuilder()
            .Install(("P1", "42"))
            .Requires(("P1", "42"), ("P2", "Beta-1"))
            .Build();

        var result = _resolver.Resolve(request);

        Assert.True(result.IsValid);
        Assert.Empty(result.Conflicts);
    }

    [Fact]
    public void Resolve_Golden001ConflictingRulesOfOneVersion_ReturnsInvalid()
    {
        var request = new InstallationRequestBuilder()
            .Install(("A", "1"))
            .Requires(("A", "1"), ("B", "2"))
            .Requires(("A", "1"), ("B", "1"))
            .Build();

        var result = _resolver.Resolve(request);

        Assert.False(result.IsValid);
    }

    [Fact]
    public void Resolve_Golden002LineWithThreePairs_ReturnsInvalid()
    {
        var request = new InstallationRequestBuilder()
            .Install(("B", "2"))
            .Requires(("B", "2"), ("A", "1"), ("C", "1"))
            .Requires(("C", "1"), ("A", "2"))
            .Build();

        var result = _resolver.Resolve(request);

        Assert.False(result.IsValid);
    }

    [Fact]
    public void Resolve_Golden003SelfDependency_ReturnsValid()
    {
        var request = new InstallationRequestBuilder()
            .Install(("B", "1"))
            .Requires(("B", "1"), ("B", "1"))
            .Build();

        var result = _resolver.Resolve(request);

        Assert.True(result.IsValid);
        Assert.Empty(result.Conflicts);
    }

    [Fact]
    public void Resolve_Golden004RulesOfNotInstalledVersion_ReturnsValid()
    {
        var request = new InstallationRequestBuilder()
            .Install(("A", "2"))
            .Install(("B", "2"))
            .Requires(("A", "1"), ("B", "1"))
            .Requires(("A", "1"), ("B", "2"))
            .Requires(("A", "2"), ("C", "3"))
            .Build();

        var result = _resolver.Resolve(request);

        Assert.True(result.IsValid);
        Assert.Empty(result.Conflicts);
    }

    [Fact]
    public void Resolve_Golden005ChainOfDepthThree_ReturnsInvalid()
    {
        var request = new InstallationRequestBuilder()
            .Install(("A", "2"))
            .Install(("B", "2"))
            .Requires(("A", "1"), ("B", "1"))
            .Requires(("A", "1"), ("B", "2"))
            .Requires(("A", "2"), ("C", "3"))
            .Requires(("C", "3"), ("D", "4"))
            .Requires(("D", "4"), ("B", "1"))
            .Build();

        var result = _resolver.Resolve(request);

        Assert.False(result.IsValid);
    }

    [Fact]
    public void Resolve_Golden006Cycle_ReturnsValid()
    {
        var request = new InstallationRequestBuilder()
            .Install(("B", "2"))
            .Requires(("A", "1"), ("B", "2"))
            .Requires(("B", "2"), ("A", "1"))
            .Build();

        var result = _resolver.Resolve(request);

        Assert.True(result.IsValid);
        Assert.Empty(result.Conflicts);
    }

    [Fact]
    public void Resolve_Golden007ConflictBetweenTwoRequirements_ReturnsInvalid()
    {
        var request = new InstallationRequestBuilder()
            .Install(("A", "1"))
            .Install(("C", "1"))
            .Requires(("A", "1"), ("B", "1"))
            .Requires(("C", "1"), ("B", "2"))
            .Build();

        var result = _resolver.Resolve(request);

        Assert.False(result.IsValid);
    }

    [Fact]
    public void Resolve_Golden008NoDependencies_ReturnsValid()
    {
        var request = new InstallationRequestBuilder()
            .Install(("A", "1"))
            .Build();

        var result = _resolver.Resolve(request);

        Assert.True(result.IsValid);
        Assert.Empty(result.Conflicts);
    }

    [Fact]
    public void Resolve_Golden009LongChainWithoutConflict_ReturnsValid()
    {
        var request = new InstallationRequestBuilder()
            .Install(("A", "2"))
            .Install(("B", "2"))
            .Install(("G", "1"))
            .Requires(("A", "1"), ("B", "2"))
            .Requires(("A", "2"), ("C", "3"))
            .Requires(("C", "3"), ("D", "4"))
            .Requires(("D", "4"), ("G", "1"))
            .Requires(("G", "1"), ("B", "2"))
            .Build();

        var result = _resolver.Resolve(request);

        Assert.True(result.IsValid);
        Assert.Empty(result.Conflicts);
    }
}
