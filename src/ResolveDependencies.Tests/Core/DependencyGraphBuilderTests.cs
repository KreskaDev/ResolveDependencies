using ResolveDependencies.Core.Graph;

namespace ResolveDependencies.Tests.Core;

public class DependencyGraphBuilderTests
{
    private readonly DependencyGraphBuilder _builder = new();

    [Fact]
    public void Build_NoRules_EveryVersionHasNoRequirements()
    {
        var graph = _builder.Build([]);

        Assert.Empty(graph.RequirementsOf(new("A", "1")));
    }

    [Fact]
    public void Build_SeveralRulesForOneVersion_ListsRequirementsInRuleOrder()
    {
        var request = new InstallationRequestBuilder()
            .Requires(("A", "1"), ("B", "2"))
            .Requires(("A", "1"), ("C", "1"), ("B", "1"))
            .Build();

        var graph = _builder.Build(request.Dependencies);

        Assert.Equal([new("B", "2"), new("C", "1"), new("B", "1")], graph.RequirementsOf(new("A", "1")));
    }

    [Fact]
    public void Build_RulesForDifferentVersions_KeepsThemApart()
    {
        var request = new InstallationRequestBuilder()
            .Requires(("A", "1"), ("B", "1"))
            .Requires(("A", "2"), ("B", "2"))
            .Build();

        var graph = _builder.Build(request.Dependencies);

        Assert.Equal([new("B", "1")], graph.RequirementsOf(new("A", "1")));
        Assert.Equal([new("B", "2")], graph.RequirementsOf(new("A", "2")));
        Assert.Empty(graph.RequirementsOf(new("A", "3")));
    }
}
