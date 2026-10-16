using ResolveDependencies.Console.CommandLine;

namespace ResolveDependencies.Tests.Console;

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
        var exception = Assert.Throws<CommandLineException>(() => _parser.Parse([]));

        Assert.Equal("Input folder is required", exception.Message);
    }

    [Fact]
    public void Parse_OutputOptionWithoutInput_ThrowsCommandLineException()
    {
        var exception = Assert.Throws<CommandLineException>(() => _parser.Parse(["--output", "out"]));

        Assert.Equal("Input folder is required", exception.Message);
    }

    [Fact]
    public void Parse_ThreePositionalArguments_ThrowsCommandLineException()
    {
        var exception = Assert.Throws<CommandLineException>(() => _parser.Parse(["a", "b", "c"]));

        Assert.Equal("Unexpected argument: c", exception.Message);
    }

    [Fact]
    public void Parse_InputGivenTwice_ThrowsCommandLineException()
    {
        var exception = Assert.Throws<CommandLineException>(() => _parser.Parse(["in", "--input", "other"]));

        Assert.Equal("The input folder is given more than once", exception.Message);
    }

    [Fact]
    public void Parse_OutputGivenTwice_ThrowsCommandLineException()
    {
        var exception = Assert.Throws<CommandLineException>(() => _parser.Parse(["in", "out", "--output", "other"]));

        Assert.Equal("The output folder is given more than once", exception.Message);
    }

    [Theory]
    [InlineData("--input")]
    [InlineData("--output")]
    public void Parse_OptionWithoutValue_ThrowsCommandLineException(string option)
    {
        var exception = Assert.Throws<CommandLineException>(() => _parser.Parse(["in", option]));

        Assert.Equal($"Missing value for {option}", exception.Message);
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public void Parse_EmptyFolder_ThrowsCommandLineException(string folder)
    {
        foreach (var args in new string[][] { [folder], ["in", folder], ["in", "--output", folder] })
        {
            var exception = Assert.Throws<CommandLineException>(() => _parser.Parse(args));

            Assert.Equal("A folder must not be empty", exception.Message);
        }
    }

    [Fact]
    public void Parse_UnknownOption_ThrowsCommandLineException()
    {
        var exception = Assert.Throws<CommandLineException>(() => _parser.Parse(["in", "--verbose"]));

        Assert.Equal("Unknown option: --verbose", exception.Message);
    }
}
