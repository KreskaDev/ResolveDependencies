using ResolveDependencies.Console;

namespace ResolveDependencies.Tests;

/// <summary>
/// Golden cases from docs/story/goldenData. Each test holds the exact text of one
/// inputNNN.txt, including its line endings and trailing blank lines, and asserts the
/// clean lines the parser receives. The last three tests cover the sanitizing rules the
/// golden files do not exercise.
/// </summary>
public class InputSanitizerTests
{
    private readonly InputSanitizer _sanitizer = new();

    [Fact]
    public void Sanitize_Golden000CrlfWithoutTrailingNewline_ReturnsLines()
    {
        const string text =
            "1\r\n" +
            "P1,42\r\n" +
            "1\r\n" +
            "P1,42,P2,Beta-1";

        var lines = _sanitizer.Sanitize(text);

        Assert.Equal(["1", "P1,42", "1", "P1,42,P2,Beta-1"], lines);
    }

    [Fact]
    public void Sanitize_Golden001CrlfWithTrailingNewline_ReturnsLines()
    {
        const string text =
            "1\r\n" +
            "A,1\r\n" +
            "2\r\n" +
            "A,1,B,2\r\n" +
            "A,1,B,1\r\n";

        var lines = _sanitizer.Sanitize(text);

        Assert.Equal(["1", "A,1", "2", "A,1,B,2", "A,1,B,1"], lines);
    }

    [Fact]
    public void Sanitize_Golden002LineWithThreePairs_ReturnsLines()
    {
        const string text =
            "1\r\n" +
            "B,2\r\n" +
            "2\r\n" +
            "B,2,A,1,C,1\r\n" +
            "C,1,A,2\r\n";

        var lines = _sanitizer.Sanitize(text);

        Assert.Equal(["1", "B,2", "2", "B,2,A,1,C,1", "C,1,A,2"], lines);
    }

    [Fact]
    public void Sanitize_Golden003TrailingBlankLine_DropsIt()
    {
        const string text =
            "1\r\n" +
            "B,1\r\n" +
            "1\r\n" +
            "B,1,B,1\r\n" +
            "\r\n";

        var lines = _sanitizer.Sanitize(text);

        Assert.Equal(["1", "B,1", "1", "B,1,B,1"], lines);
    }

    [Fact]
    public void Sanitize_Golden004CrlfWithTrailingNewline_ReturnsLines()
    {
        const string text =
            "2\r\n" +
            "A,2\r\n" +
            "B,2\r\n" +
            "3\r\n" +
            "A,1,B,1\r\n" +
            "A,1,B,2\r\n" +
            "A,2,C,3\r\n";

        var lines = _sanitizer.Sanitize(text);

        Assert.Equal(["2", "A,2", "B,2", "3", "A,1,B,1", "A,1,B,2", "A,2,C,3"], lines);
    }

    [Fact]
    public void Sanitize_Golden005CrlfWithoutTrailingNewline_ReturnsLines()
    {
        const string text =
            "2\r\n" +
            "A,2\r\n" +
            "B,2\r\n" +
            "5\r\n" +
            "A,1,B,1\r\n" +
            "A,1,B,2\r\n" +
            "A,2,C,3\r\n" +
            "C,3,D,4\r\n" +
            "D,4,B,1";

        var lines = _sanitizer.Sanitize(text);

        Assert.Equal(
            ["2", "A,2", "B,2", "5", "A,1,B,1", "A,1,B,2", "A,2,C,3", "C,3,D,4", "D,4,B,1"],
            lines);
    }

    [Fact]
    public void Sanitize_Golden006CrlfWithTrailingNewline_ReturnsLines()
    {
        const string text =
            "1\r\n" +
            "B,2\r\n" +
            "2\r\n" +
            "A,1,B,2\r\n" +
            "B,2,A,1\r\n";

        var lines = _sanitizer.Sanitize(text);

        Assert.Equal(["1", "B,2", "2", "A,1,B,2", "B,2,A,1"], lines);
    }

    [Fact]
    public void Sanitize_Golden007TrailingBlankLine_DropsIt()
    {
        const string text =
            "2\r\n" +
            "A,1\r\n" +
            "C,1\r\n" +
            "2\r\n" +
            "A,1,B,1\r\n" +
            "C,1,B,2\r\n" +
            "\r\n";

        var lines = _sanitizer.Sanitize(text);

        Assert.Equal(["2", "A,1", "C,1", "2", "A,1,B,1", "C,1,B,2"], lines);
    }

    [Fact]
    public void Sanitize_Golden008MissingDependencySection_ReturnsLines()
    {
        const string text =
            "1\r\n" +
            "A,1";

        var lines = _sanitizer.Sanitize(text);

        Assert.Equal(["1", "A,1"], lines);
    }

    [Fact]
    public void Sanitize_Golden009CrlfWithoutTrailingNewline_ReturnsLines()
    {
        const string text =
            "3\r\n" +
            "A,2\r\n" +
            "B,2\r\n" +
            "G,1\r\n" +
            "5\r\n" +
            "A,1,B,2\r\n" +
            "A,2,C,3\r\n" +
            "C,3,D,4\r\n" +
            "D,4,G,1\r\n" +
            "G,1,B,2";

        var lines = _sanitizer.Sanitize(text);

        Assert.Equal(
            ["3", "A,2", "B,2", "G,1", "5", "A,1,B,2", "A,2,C,3", "C,3,D,4", "D,4,G,1", "G,1,B,2"],
            lines);
    }

    [Fact]
    public void Sanitize_LineFeedEndings_ReturnsLines()
    {
        const string text = "1\nA,1\n1\nA,1,B,1\n";

        var lines = _sanitizer.Sanitize(text);

        Assert.Equal(["1", "A,1", "1", "A,1,B,1"], lines);
    }

    [Fact]
    public void Sanitize_SurroundingWhitespace_TrimsEachLine()
    {
        const string text = " 1 \r\n\tA,1\r\n1\r\nA,1,B,1  \r\n";

        var lines = _sanitizer.Sanitize(text);

        Assert.Equal(["1", "A,1", "1", "A,1,B,1"], lines);
    }

    [Fact]
    public void Sanitize_BlankLineInsideFile_KeepsIt()
    {
        const string text = "1\r\nA,1\r\n\r\n1\r\nA,1,B,1\r\n";

        var lines = _sanitizer.Sanitize(text);

        Assert.Equal(["1", "A,1", "", "1", "A,1,B,1"], lines);
    }
}
