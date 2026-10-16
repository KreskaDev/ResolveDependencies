using ResolveDependencies.Console;

namespace ResolveDependencies.Tests;

public class InputFileProcessorTests
{
    private readonly InputFileProcessor _processor = new();

    [Fact]
    public void Process_ValidInstallation_ReturnsPass()
    {
        const string text = "2\r\nA,1\r\nB,1\r\n3\r\nA,1,B,1\r\nA,2,B,2\r\nC,1,B,1\r\n";

        var report = _processor.Process(text);

        Assert.Equal(Verdict.Pass, report.Verdict);
    }

    [Fact]
    public void Process_InvalidInstallation_ReturnsFailWithConflicts()
    {
        const string text = "2\r\nA,1\r\nB,2\r\n3\r\nA,1,B,1\r\nA,2,B,2\r\nC,1,B,1\r\n";

        var report = _processor.Process(text);

        Assert.Equal(Verdict.Fail, report.Verdict);
        Assert.NotEmpty(report.Conflicts);
    }

    [Fact]
    public void Process_MalformedText_ReturnsErrorWithReason()
    {
        const string text = "1\r\nA,1\r\n1\r\nA,1,B\r\n";

        var report = _processor.Process(text);

        Assert.Equal(Verdict.Error, report.Verdict);
        Assert.Equal("line 4: expected an even number of fields, got 3", report.Reason);
    }
}
