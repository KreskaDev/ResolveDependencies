using ResolveDependencies.Console.Input;

namespace ResolveDependencies.Tests.Console;

/// <summary>
/// Format rules applied by the parser on sanitized lines: F2, F3, F7, F8 and the install
/// line field count. Negative cases assert the line number carried by the exception.
/// </summary>
public class InputParserTests
{
    private readonly InputParser _parser = new();

    [Fact]
    public void Parse_TaskExample_ReturnsPackagesAndDependencies()
    {
        string[] lines = ["2", "A,1", "B,1", "3", "A,1,B,1", "A,2,B,2", "C,1,B,1"];
        var expected = new InstallationRequestBuilder()
            .Install(("A", "1"))
            .Install(("B", "1"))
            .Requires(("A", "1"), ("B", "1"))
            .Requires(("A", "2"), ("B", "2"))
            .Requires(("C", "1"), ("B", "1"))
            .Build();

        var request = _parser.Parse(lines);

        Assert.Equal(expected.Packages, request.Packages);
        Assert.Equal(expected.Dependencies, request.Dependencies);
    }

    [Fact]
    public void Parse_ZeroPackages_ReturnsEmptyRequest()
    {
        string[] lines = ["0"];

        var request = _parser.Parse(lines);

        Assert.Empty(request.Packages);
        Assert.Empty(request.Dependencies);
    }

    [Fact]
    public void Parse_LineWithThreePairs_ReturnsOneDependencyPerPair()
    {
        string[] lines = ["1", "B,2", "1", "B,2,A,1,C,1"];
        var expected = new InstallationRequestBuilder()
            .Install(("B", "2"))
            .Requires(("B", "2"), ("A", "1"), ("C", "1"))
            .Build();

        var request = _parser.Parse(lines);

        Assert.Equal(expected.Dependencies, request.Dependencies);
    }

    [Fact]
    public void Parse_OddNumberOfFields_ThrowsFormatException()
    {
        string[] lines = ["1", "A,1", "1", "A,1,B"];

        var exception = Assert.Throws<InputFormatException>(() => _parser.Parse(lines));

        Assert.Equal(4, exception.LineNumber);
        Assert.Equal("expected an even number of fields, got 3", exception.Reason);
    }

    [Fact]
    public void Parse_DependencyLineWithTwoFields_ThrowsFormatException()
    {
        string[] lines = ["1", "A,1", "1", "A,1"];

        var exception = Assert.Throws<InputFormatException>(() => _parser.Parse(lines));

        Assert.Equal(4, exception.LineNumber);
        Assert.Equal("expected at least 4 fields, got 2", exception.Reason);
    }

    [Fact]
    public void Parse_MissingDependencySection_ReturnsNoDependencies()
    {
        string[] lines = ["1", "A,1"];

        var request = _parser.Parse(lines);

        Assert.Equal([new("A", "1")], request.Packages);
        Assert.Empty(request.Dependencies);
    }

    [Fact]
    public void Parse_EmptyLineInsideSection_ThrowsFormatException()
    {
        string[] lines = ["2", "A,1", "", "B,1"];

        var exception = Assert.Throws<InputFormatException>(() => _parser.Parse(lines));

        Assert.Equal(3, exception.LineNumber);
        Assert.Equal("empty line", exception.Reason);
    }

    [Fact]
    public void Parse_EmptyLineAsDependencyCount_ThrowsFormatException()
    {
        string[] lines = ["1", "A,1", "", "A,1,B,1"];

        var exception = Assert.Throws<InputFormatException>(() => _parser.Parse(lines));

        Assert.Equal(3, exception.LineNumber);
        Assert.Equal("empty line", exception.Reason);
    }

    [Fact]
    public void Parse_NoLines_ThrowsFormatException()
    {
        string[] lines = [];

        var exception = Assert.Throws<InputFormatException>(() => _parser.Parse(lines));

        Assert.Equal(1, exception.LineNumber);
        Assert.Equal("expected the number of packages", exception.Reason);
    }

    [Theory]
    [InlineData("x")]
    [InlineData("-1")]
    [InlineData("1.5")]
    [InlineData("A,1")]
    public void Parse_PackageCountNotNonNegativeInteger_ThrowsFormatException(string count)
    {
        string[] lines = [count, "A,1"];

        var exception = Assert.Throws<InputFormatException>(() => _parser.Parse(lines));

        Assert.Equal(1, exception.LineNumber);
        Assert.Equal($"expected a non-negative integer, got '{count}'", exception.Reason);
    }

    [Fact]
    public void Parse_DependencyCountNotNonNegativeInteger_ThrowsFormatException()
    {
        string[] lines = ["1", "A,1", "x", "A,1,B,1"];

        var exception = Assert.Throws<InputFormatException>(() => _parser.Parse(lines));

        Assert.Equal(3, exception.LineNumber);
        Assert.Equal("expected a non-negative integer, got 'x'", exception.Reason);
    }

    [Fact]
    public void Parse_FewerPackageLinesThanCount_ThrowsFormatException()
    {
        string[] lines = ["2", "A,1"];

        var exception = Assert.Throws<InputFormatException>(() => _parser.Parse(lines));

        Assert.Equal(3, exception.LineNumber);
        Assert.Equal("expected 2 package lines, got 1", exception.Reason);
    }

    [Fact]
    public void Parse_PackageCountFarAboveLineCount_ThrowsFormatException()
    {
        string[] lines = [int.MaxValue.ToString(), "A,1"];

        var exception = Assert.Throws<InputFormatException>(() => _parser.Parse(lines));

        Assert.Equal(3, exception.LineNumber);
    }

    [Fact]
    public void Parse_MorePackageLinesThanCount_ThrowsFormatException()
    {
        string[] lines = ["1", "A,1", "B,1", "0"];

        var exception = Assert.Throws<InputFormatException>(() => _parser.Parse(lines));

        Assert.Equal(3, exception.LineNumber);
        Assert.Equal("expected a non-negative integer, got 'B,1'", exception.Reason);
    }

    [Fact]
    public void Parse_FewerDependencyLinesThanCount_ThrowsFormatException()
    {
        string[] lines = ["1", "A,1", "2", "A,1,B,1"];

        var exception = Assert.Throws<InputFormatException>(() => _parser.Parse(lines));

        Assert.Equal(5, exception.LineNumber);
        Assert.Equal("expected 2 dependency lines, got 1", exception.Reason);
    }

    [Fact]
    public void Parse_MoreDependencyLinesThanCount_ThrowsFormatException()
    {
        string[] lines = ["1", "A,1", "1", "A,1,B,1", "A,1,C,1"];

        var exception = Assert.Throws<InputFormatException>(() => _parser.Parse(lines));

        Assert.Equal(5, exception.LineNumber);
        Assert.Equal("unexpected line after the dependency section", exception.Reason);
    }

    [Theory]
    [InlineData("A,")]
    [InlineData(",1")]
    public void Parse_EmptyFieldInPackageLine_ThrowsFormatException(string line)
    {
        string[] lines = ["1", line];

        var exception = Assert.Throws<InputFormatException>(() => _parser.Parse(lines));

        Assert.Equal(2, exception.LineNumber);
        Assert.Equal("empty field", exception.Reason);
    }

    [Fact]
    public void Parse_EmptyFieldInDependencyLine_ThrowsFormatException()
    {
        string[] lines = ["1", "A,1", "1", "A,1,,1"];

        var exception = Assert.Throws<InputFormatException>(() => _parser.Parse(lines));

        Assert.Equal(4, exception.LineNumber);
        Assert.Equal("empty field", exception.Reason);
    }

    [Fact]
    public void Parse_PackageLineWithThreeFields_ThrowsFormatException()
    {
        string[] lines = ["1", "A,1,B"];

        var exception = Assert.Throws<InputFormatException>(() => _parser.Parse(lines));

        Assert.Equal(2, exception.LineNumber);
        Assert.Equal("expected 2 fields, got 3", exception.Reason);
    }

    [Fact]
    public void Parse_WhitespaceInsideField_KeepsIt()
    {
        string[] lines = ["1", "My Package,1 beta"];

        var request = _parser.Parse(lines);

        Assert.Equal([new("My Package", "1 beta")], request.Packages);
    }

    [Fact]
    public void Parse_FormatError_MessageHasLineNumberAndReason()
    {
        string[] lines = ["1", "A,1", "1", "A,1,B"];

        var exception = Assert.Throws<InputFormatException>(() => _parser.Parse(lines));

        Assert.Equal("line 4: expected an even number of fields, got 3", exception.Message);
        Assert.Equal("expected an even number of fields, got 3", exception.Reason);
    }
}
