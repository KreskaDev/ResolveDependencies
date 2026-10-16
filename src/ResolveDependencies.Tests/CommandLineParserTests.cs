using ResolveDependencies.Console;

namespace ResolveDependencies.Tests;

public class CommandLineParserTests
{
    private readonly CommandLineParser _parser = new();

    [Fact]
    public void Parse_PositionalInputOnly_OutputDefaultsToInput()
    {
        var options = _parser.Parse(["in"]);

        Assert.Equal(new CommandLineOptions("in", "in"), options);
    }

    [Fact]
    public void Parse_PositionalInputAndOutput_ReturnsBoth()
    {
        var options = _parser.Parse(["in", "out"]);

        Assert.Equal(new CommandLineOptions("in", "out"), options);
    }

    [Fact]
    public void Parse_InputOptionOnly_OutputDefaultsToInput()
    {
        var options = _parser.Parse(["--input", "in"]);

        Assert.Equal(new CommandLineOptions("in", "in"), options);
    }

    [Fact]
    public void Parse_InputAndOutputOptions_ReturnsBoth()
    {
        var options = _parser.Parse(["--input", "in", "--output", "out"]);

        Assert.Equal(new CommandLineOptions("in", "out"), options);
    }

    [Fact]
    public void Parse_OptionsInReverseOrder_ReturnsBoth()
    {
        var options = _parser.Parse(["--output", "out", "--input", "in"]);

        Assert.Equal(new CommandLineOptions("in", "out"), options);
    }

    [Fact]
    public void Parse_PositionalInputAndOutputOption_ReturnsBoth()
    {
        var options = _parser.Parse(["in", "--output", "out"]);

        Assert.Equal(new CommandLineOptions("in", "out"), options);
    }

    [Fact]
    public void Parse_InputOptionAndPositionalOutput_ReturnsBoth()
    {
        var options = _parser.Parse(["--input", "in", "out"]);

        Assert.Equal(new CommandLineOptions("in", "out"), options);
    }

    [Fact]
    public void Parse_NoArguments_ThrowsCommandLineException()
    {
        Assert.Throws<CommandLineException>(() => _parser.Parse([]));
    }

    [Fact]
    public void Parse_OutputOptionWithoutInput_ThrowsCommandLineException()
    {
        Assert.Throws<CommandLineException>(() => _parser.Parse(["--output", "out"]));
    }

    [Fact]
    public void Parse_ThreePositionalArguments_ThrowsCommandLineException()
    {
        Assert.Throws<CommandLineException>(() => _parser.Parse(["a", "b", "c"]));
    }

    [Fact]
    public void Parse_InputGivenTwice_ThrowsCommandLineException()
    {
        Assert.Throws<CommandLineException>(() => _parser.Parse(["in", "--input", "other"]));
    }

    [Fact]
    public void Parse_OutputGivenTwice_ThrowsCommandLineException()
    {
        Assert.Throws<CommandLineException>(() => _parser.Parse(["in", "out", "--output", "other"]));
    }

    [Theory]
    [InlineData("--input")]
    [InlineData("--output")]
    public void Parse_OptionWithoutValue_ThrowsCommandLineException(string option)
    {
        Assert.Throws<CommandLineException>(() => _parser.Parse(["in", option]));
    }

    [Fact]
    public void Parse_UnknownOption_ThrowsCommandLineException()
    {
        Assert.Throws<CommandLineException>(() => _parser.Parse(["in", "--verbose"]));
    }
}
