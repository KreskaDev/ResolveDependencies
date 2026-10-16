using ResolveDependencies.Console;
using ResolveDependencies.Core;

namespace ResolveDependencies.Tests;

public class FileReportTests
{
    [Fact]
    public void ToOutputFileContent_Pass_ReturnsPassLine()
    {
        var report = FileReport.Pass();

        Assert.Equal("PASS\n", report.ToOutputFileContent());
    }

    [Fact]
    public void ToOutputFileContent_Fail_ReturnsFailLineWithoutConflicts()
    {
        var report = FileReport.Fail([new Conflict("B", ["2", "1"])]);

        Assert.Equal("FAIL\n", report.ToOutputFileContent());
    }

    [Fact]
    public void ToOutputFileContent_Error_ReturnsErrorLineAndReason()
    {
        var report = FileReport.Error("line 4: expected an even number of fields, got 3");

        Assert.Equal("ERROR\nline 4: expected an even number of fields, got 3\n", report.ToOutputFileContent());
    }

    [Fact]
    public void ToConsoleLines_Pass_ReturnsSummaryOnly()
    {
        var report = FileReport.Pass();

        Assert.Equal(["input000.txt: PASS"], report.ToConsoleLines("input000.txt"));
    }

    [Fact]
    public void ToConsoleLines_Fail_ReturnsSummaryAndOneLinePerConflict()
    {
        var report = FileReport.Fail([new Conflict("B", ["2", "1"]), new Conflict("C", ["1", "3"])]);

        Assert.Equal(
            ["input001.txt: FAIL", "  B: 2, 1", "  C: 1, 3"],
            report.ToConsoleLines("input001.txt"));
    }

    [Fact]
    public void ToConsoleLines_Error_ReturnsSummaryAndReason()
    {
        var report = FileReport.Error("line 4: expected an even number of fields, got 3");

        Assert.Equal(
            ["input002.txt: ERROR", "  line 4: expected an even number of fields, got 3"],
            report.ToConsoleLines("input002.txt"));
    }
}
